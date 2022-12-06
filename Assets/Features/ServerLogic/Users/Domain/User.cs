using System;
using System.Security.Cryptography;

namespace Features.ServerLogic.Users.Domain
{
    public class User
    {
        public string Id;
        public string UserName;
        public string Password;
        public string FriendCode;

        /// <summary>
        /// Encrypt password for storage.
        /// </summary>
        public static string EncryptPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            string savedPasswordHash = Convert.ToBase64String(hashBytes);
            return savedPasswordHash;
        }

        /// <summary>
        /// Compare encrypted password to a normal one
        /// </summary>
        public static bool ComparePassword(string password, string hashedPassword)
        {
            byte[] hashBytes,
                   hash;
            /* Extract the bytes */
            hashBytes = Convert.FromBase64String(hashedPassword);
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;
            return true;
        }
    }
}