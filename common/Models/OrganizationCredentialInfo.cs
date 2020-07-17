using System;
using System.Text;
using System.Security.Cryptography;

namespace Common.Models
{
    public class OrganizationCredentialInfo
    {
        public int Id;

        public string LoginName;

        public string LoginSecretHash;

        public void HashLoginSecret()
        {
            if (LoginSecretHash != null)
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // Trim LoginSecret just in case
                    LoginSecretHash = LoginSecretHash.Trim();

                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(LoginSecretHash));

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));

                    }

                    LoginSecretHash = builder.ToString();
                }
            }
        }
    }
}
