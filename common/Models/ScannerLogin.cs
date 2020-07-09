using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.Models
{
    public class ScannerLogin
    {
        public string Username;

        public string Password;

        public void HashPassword()
        {
            if (Password != null)
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // Trim password just in case
                    Password = Password.Trim();

                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(Password));

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));

                    }

                    Password = builder.ToString();
                }
            }
        }
    }
}
