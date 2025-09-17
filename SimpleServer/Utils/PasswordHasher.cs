using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Cryptography;
using System.Text;

namespace SimpleServer.Utils
{
    public static class PasswordHasher
    {
        // returns: {iterations}.{base64(salt)}.{base64(hash)}
        public static string Hash(string password, int iterations = 100_000)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[16];
                rng.GetBytes(salt);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var hash = pbkdf2.GetBytes(32);
                    return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
                }
            }
        }

        public static bool Verify(string password, string stored)
        {
            var parts = stored.Split('.');
            if (parts.Length != 3) return false;
            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                var actual = pbkdf2.GetBytes(32);
                // constant-time compare
                var diff = 0;
                for (int i = 0; i < actual.Length; i++) diff |= actual[i] ^ expected[i];
                return diff == 0;
            }
        }
    }
}