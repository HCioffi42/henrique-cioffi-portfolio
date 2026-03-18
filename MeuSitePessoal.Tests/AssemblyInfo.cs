using Xunit;

// Disables parallel test execution to prevent database race conditions during integration tests.
[assembly: CollectionBehavior(DisableTestParallelization = true)]