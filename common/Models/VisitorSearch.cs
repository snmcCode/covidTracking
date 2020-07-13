using System;
using System.Text;

namespace Common.Models
{
    public class VisitorSearch
    {
        public string FirstName;

        public string LastName;

        public string Email;

        public string PhoneNumber;

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder()
                .AppendLine($"{(FirstName != null ? FirstName : "")}")
                .AppendLine($"{(LastName != null ? LastName : "")}")
                .AppendLine($"{(Email != null ? Email : "")}")
                .AppendLine($"{(PhoneNumber != null ? PhoneNumber : "")}");

            return stringBuilder.ToString();
        }
    }
}
