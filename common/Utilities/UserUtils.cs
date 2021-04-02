using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;


namespace Common.Utilities
  {
    public class UserUtils
    {
        private readonly IConfiguration Config;
        private LoggerHelper Helper;
        private VisitorPhoneNumberInfo visitorPhoneNumberInfo;
        private string secret;

        public UserUtils(LoggerHelper helper, IConfiguration config, VisitorPhoneNumberInfo visitorInfo)
        {
            Helper = helper;
            Config = config;
            visitorPhoneNumberInfo = visitorInfo;
            secret=config["VERIFICATION_CODE_SECRET"];

        }

        public async Task<bool> SendVerificationCode()
        {
            if(visitorPhoneNumberInfo.PhoneNumber == null | visitorPhoneNumberInfo.PhoneNumber==string.Empty)
            {
                throw new ArgumentNullException("Visitor Phone Number is null or empty");
            }

            string strTimeStamp = string.Concat(DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString());
            var verification = await GenerateVerificationCode(visitorPhoneNumberInfo.PhoneNumber, strTimeStamp, secret);

            TwilioManager twilio = new TwilioManager(visitorPhoneNumberInfo,Helper, Config);
            string verificationMessage = $"{Config["VERIFICATION_TEXT"]} {verification} ";
            bool messageSent= await twilio.SendText(verificationMessage,visitorPhoneNumberInfo.PhoneNumber);
            if(messageSent)
            {
                visitorPhoneNumberInfo.IsValidPhoneNumber = true;
            return true;
            }else
            {
                Helper.DebugLogger.LogCustomError("Twilio returned failure in sending verification. check twilio errors");
                visitorPhoneNumberInfo.IsValidPhoneNumber = false;
                return false;
            }
        }

        public async Task<VisitorPhoneNumberInfo> VerifyVerificationCode()
        {
            const int minutesToWait = 5;
            string[] strTimeStamp=new string[minutesToWait];
            string[] verificationCode = new string[minutesToWait];
            for(int i=0; i < 5; i++)
            {
                strTimeStamp[i]= string.Concat(DateTime.Now.Hour.ToString(), DateTime.Now.AddMinutes(i*-1).Minute.ToString());
                verificationCode[i]= await GenerateVerificationCode(visitorPhoneNumberInfo.PhoneNumber, strTimeStamp[i], secret);
                if(verificationCode[i]==visitorPhoneNumberInfo.VerificationCode) //received code
                {
                    visitorPhoneNumberInfo.VerificationStatus = "approved";
                    return visitorPhoneNumberInfo;
                }
            }
            visitorPhoneNumberInfo.VerificationStatus = "fail";
            return visitorPhoneNumberInfo;
        }

        private async Task<string> GenerateVerificationCode(string phoneNumber,string strTimeStamp,string secret,int codeLenth=4)
        {
            try { 
            int generatedInt = 0;
            phoneNumber = phoneNumber.Replace("+", string.Empty);
            var verificationString = string.Concat(phoneNumber, strTimeStamp, secret);
            Byte[] verificationBytes = Encoding.UTF8.GetBytes(verificationString);

            var algorithm = SHA1.Create();
            var generatedByptes = algorithm.ComputeHash(verificationBytes);

            generatedInt = BitConverter.ToInt32(generatedByptes);

            if(generatedInt<0)
            { generatedInt = generatedInt * -1; }

            var generatedString = generatedInt.ToString();
            return generatedString.Substring(0, codeLenth);
            }
            catch(Exception e)
            {
                Helper.DebugLogger.InnerException = e;
                throw new ApplicationException("An Error Occurred with the User Verification function");
            }
        }

    }
}
