using System.Security.Cryptography;
using System.Text;

namespace Interview.Domain.Invitations.Utils;

public class TokenUtils
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz";
    private static readonly Random SRANDOM = new Random();

    public static string GenerateToken(int length)
    {
        return GenerateToken(Alphabet, length);
    }

    public static bool CompareHash(string hash, string text)
    {
        return GenerateHash(text) == hash;
    }

    private static string GenerateToken(string characters, int length)
    {
        return new string(Enumerable
            .Range(0, length)
            .Select(num => characters[SRANDOM.Next() % characters.Length])
            .ToArray());
    }

    private static string GenerateHash(string text)
    {
        using var hash = SHA256.Create();
        return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));
    }
}
