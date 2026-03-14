namespace MeuSitePessoal.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(string username);
}
