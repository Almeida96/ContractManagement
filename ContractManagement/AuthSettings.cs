using System.Security.Cryptography;
using System.Text;

namespace ContractManagement;

public sealed record AuthSettings(string Key, string Username, string Password)
{
    public const string Issuer = "contract-management";
    public const string Audience = "contract-management-api";
    public static AuthSettings Read(IConfiguration configuration)
    {
        var key = configuration["Auth:Key"] ?? "";
        var username = configuration["Auth:Username"] ?? "";
        var password = configuration["Auth:Password"] ?? "";
        if (Encoding.UTF8.GetByteCount(key) < 32 || username.Length is < 1 or > 100 || password.Length is < 12 or > 128)
            throw new InvalidOperationException("Set Auth__Key (32+ UTF-8 bytes), Auth__Username and Auth__Password (12–128 characters).");
        return new(key, username, password);
    }
    public bool Matches(string? username, string? password)
    {
        if (username is null || password is null || username.Length > 100 || password.Length > 128) return false;
        return CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(username)), SHA256.HashData(Encoding.UTF8.GetBytes(Username)))
            & CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(password)), SHA256.HashData(Encoding.UTF8.GetBytes(Password)));
    }
}
