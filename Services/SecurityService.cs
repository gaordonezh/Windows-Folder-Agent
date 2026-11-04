using System.Security.Cryptography;

namespace WindowsFolderAgent.Services;

public class SecurityService
{
    private readonly string _token;

    public SecurityService()
    {
        _token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32)
        );
    }

    public string Token => _token;

    public bool ValidateToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var expected = System.Text.Encoding.UTF8.GetBytes(_token);
        var received = System.Text.Encoding.UTF8.GetBytes(token);

        return expected.Length == received.Length &&
               CryptographicOperations.FixedTimeEquals(
                   expected,
                   received
               );
    }
}
