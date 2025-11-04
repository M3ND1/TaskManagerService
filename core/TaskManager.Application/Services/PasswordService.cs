using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services
{
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int DegreeOfParallelism = 1; // Number of threads to use
        private const int Iterations = 4; // Number of iterations
        private const int MemorySize = 65536; // 64 MB

        public string SecurePassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = HashPassword(password, salt);
            var combinedBytes = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, combinedBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(combinedBytes);
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize
            };

            return argon2.GetBytes(HashSize);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] combinedBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SaltSize];
            byte[] hash = new byte[HashSize];
            Buffer.BlockCopy(combinedBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(combinedBytes, SaltSize, hash, 0, HashSize);

            byte[] newHash = HashPassword(password, salt);

            return CryptographicOperations.FixedTimeEquals(hash, newHash);
        }
    }
}