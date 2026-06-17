using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.Security;
using System.Security.Cryptography;
using System.Text;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.Security
{
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16; //128-bit
        private const int KeySize = 32; //256-bit
        private static readonly HashAlgorithmName Algo = HashAlgorithmName.SHA256;

        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algo, KeySize);
            return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string passwordHash)
        {
            if (IsLegacyHash(passwordHash))
            {
                using var sha = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = Convert.ToBase64String(sha.ComputeHash(bytes));
                return SlowEquals(hash, passwordHash);
            }

            var parts = passwordHash.Split('$');
            if (parts.Length != 4 || parts[0] != "pbkdf2") return false;
            var iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var stored = Convert.FromBase64String(parts[3]);
            var computed = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algo, stored.Length);
            return SlowEquals(stored, computed);
        }

        public bool IsLegacyHash(string passwordHash) => !passwordHash.StartsWith("pbkdf2$");

        private static bool SlowEquals(string a, string b) => SlowEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
        private static bool SlowEquals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            if (a.Length != b.Length) return false;
            var diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}