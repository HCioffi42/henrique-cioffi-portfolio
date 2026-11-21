using System.Buffers;
using System.Net;
using System.Net.Sockets;
using MeuSitePessoal.Infrastructure.Configuration;
using MimeKit;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.ComponentModel;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace MeuSitePessoal.Tests.Integration.Infrastructure.Services;

/// <summary>
/// An SMTP server running inside the test process, on a loopback port, for the duration of one
/// test. It replaces the third-party provider the SMTP tests used to dial, so they exercise a
/// real connect, authenticate and send without depending on network access, DNS, or anyone
/// else's uptime (audit T5).
/// </summary>
internal sealed class SmtpTestServer : IAsyncDisposable
{
    internal const string ValidUsername = "test-user";
    internal const string ValidPassword = "test-password";

    private readonly SmtpServer.SmtpServer _server;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly RecordingMessageStore _store = new();
    private readonly Task _running;

    private SmtpTestServer(int port)
    {
        Port = port;

        var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost")
            .Endpoint(endpoint => endpoint
                .Port(port)
                .IsSecure(false)
                // The transport is loopback-only and lives for one test, so credentials are
                // offered in the clear. Requiring authentication is what makes the rejection
                // path reachable at all.
                .AllowUnsecureAuthentication(true)
                .AuthenticationRequired(true))
            .Build();

        var services = new ServiceProvider();
        services.Add(_store);
        services.Add(new FixedCredentialAuthenticator());

        _server = new SmtpServer.SmtpServer(options, services);
        _running = _server.StartAsync(_cancellation.Token);
    }

    /// <summary>The loopback port the server is listening on.</summary>
    internal int Port { get; }

    /// <summary>The messages the server has accepted, in arrival order.</summary>
    internal IReadOnlyList<MimeMessage> ReceivedMessages => _store.Messages;

    /// <summary>
    /// Starts a server on a free port and returns once it is accepting connections.
    /// </summary>
    internal static async Task<SmtpTestServer> StartAsync()
    {
        var server = new SmtpTestServer(FindFreePort());
        await server.WaitUntilListeningAsync();
        return server;
    }

    /// <summary>
    /// Settings addressing this server, for the component under test to be constructed with.
    /// </summary>
    internal EmailSettings BuildSettings(string? username = null, string? password = null) => new()
    {
        Host = "localhost",
        Port = Port,
        Username = username ?? ValidUsername,
        Password = password ?? ValidPassword,
        SenderEmail = "no-reply@test.local",
        SenderName = "Integration Test",

        // Selects SecureSocketOptions.None in the service under test. The server holds no
        // certificate, so an implicit or opportunistic TLS handshake has nothing to negotiate.
        EnableSsl = false
    };

    public async ValueTask DisposeAsync()
    {
        await _cancellation.CancelAsync();

        try
        {
            await _running;
        }
        catch (OperationCanceledException)
        {
            // The expected outcome of shutting the listener down.
        }

        _cancellation.Dispose();
    }

    /// <summary>
    /// Asks the operating system for an unused port. Binding to port zero and reading back the
    /// assignment avoids a fixed port that a developer's machine may already have in use.
    /// </summary>
    private static int FindFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    /// <summary>
    /// Polls until the listener accepts a connection. StartAsync returns before the socket is
    /// necessarily bound, so a test connecting immediately would otherwise race it.
    /// </summary>
    private async Task WaitUntilListeningAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        while (true)
        {
            try
            {
                using var probe = new TcpClient();
                await probe.ConnectAsync(IPAddress.Loopback, Port, timeout.Token);
                return;
            }
            catch (SocketException)
            {
                if (timeout.IsCancellationRequested)
                {
                    throw new InvalidOperationException(
                        $"The test SMTP server did not start listening on port {Port} within ten seconds.");
                }

                await Task.Delay(25, CancellationToken.None);
            }
        }
    }

    /// <summary>
    /// Accepts one known credential pair and rejects everything else, so a test can drive either
    /// outcome by choosing which credentials the service is configured with.
    /// </summary>
    private sealed class FixedCredentialAuthenticator : IUserAuthenticator
    {
        public Task<bool> AuthenticateAsync(
            ISessionContext context,
            string user,
            string password,
            CancellationToken cancellationToken)
            => Task.FromResult(user == ValidUsername && password == ValidPassword);
    }

    /// <summary>
    /// Parses each accepted message and keeps it, so a test can assert on what actually crossed
    /// the wire rather than only on the absence of an exception.
    /// </summary>
    private sealed class RecordingMessageStore : MessageStore
    {
        private readonly List<MimeMessage> _messages = [];

        internal IReadOnlyList<MimeMessage> Messages => _messages;

        public override async Task<SmtpResponse> SaveAsync(
            ISessionContext context,
            IMessageTransaction transaction,
            ReadOnlySequence<byte> buffer,
            CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();

            var position = buffer.GetPosition(0);

            while (buffer.TryGet(ref position, out var segment))
            {
                await stream.WriteAsync(segment, cancellationToken);
            }

            stream.Position = 0;

            _messages.Add(await MimeMessage.LoadAsync(stream, cancellationToken));

            return SmtpResponse.Ok;
        }
    }
}
