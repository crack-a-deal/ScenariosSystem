using System;
using System.Security.Cryptography;
using System.Text;

namespace PasswordHasher
{
    public class PasswordHasherService : IPasswordHasher
    {
        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(
                    "Password is required",
                    nameof(password));

            using var sha256 = SHA256.Create();

            byte[] passwordBytes =
                Encoding.UTF8.GetBytes(password);

            byte[] hash =
                sha256.ComputeHash(passwordBytes);

            return BitConverter
                .ToString(hash)
                .Replace("-", "");
        }

        public bool Verify(
            string password,
            string hash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(hash))
                return false;

            string passwordHash = Hash(password);

            return passwordHash == hash;
        }
    }
}