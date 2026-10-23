using System;
using System.Security.Cryptography;
using System.Text;

namespace UP01_Morozov_FoodSubscription.Helpers
{
    /// <summary>Хэширование паролей пользователей (алгоритм SHA-256).</summary>
    public static class HashHelper
    {
        /// <summary>
        /// Возвращает SHA-256 хэш строки в формате Base64.
        /// В базе данных хранится только результат этого метода.
        /// </summary>
        public static string GetHash(string password)
        {
            if (password == null)
                password = string.Empty;

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>Сравнивает введённый пароль с хэшем, сохранённым в базе данных.</summary>
        public static bool Verify(string password, string storedHash)
        {
            return string.Equals(GetHash(password), storedHash, StringComparison.Ordinal);
        }
    }
}
