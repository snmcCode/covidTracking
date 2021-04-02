using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Models;

namespace common.Utilities
{
    class CommunicationManager
    {

        public async Task<bool> SendVerificationCode(string phoneNumber,string secret)
        {
            string strTimeStamp = string.Concat(DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString());
            var verification = await GenerateVerificationCode(phoneNumber, strTimeStamp, secret);


            return true;
        }

        public async Task<bool> VerifyVerificationCode(string phoneNumber,string secret,string receivedCode)
        {
            var strTimeStamp = string.Concat(DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString());
            var strTimeStamp2= string.Concat(DateTime.Now.Hour.ToString(), DateTime.Now.AddMinutes(-1).Minute.ToString());

            var verification = await GenerateVerificationCode(phoneNumber, strTimeStamp, secret);
            var verification2 = await GenerateVerificationCode(phoneNumber, strTimeStamp2, secret);

            if (receivedCode == verification | receivedCode == verification2)
            {
                return true;
            }
            else
            {
                return false;
            }

        }



        public async Task<string> GenerateVerificationCode(string phoneNumber,string strTimeStamp,string secret)
        {
            int sum = 0;
            phoneNumber = phoneNumber.Replace('+', '');
            var verificationString = string.Concat(phoneNumber, strTimeStamp, secret);
            foreach (var c in verificationString)
            {
                sum += int.Parse(c.ToString());
            }
            sum = sum ^ 2;
            return sum.ToString();
        }

    }
}
