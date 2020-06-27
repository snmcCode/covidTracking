using System;
using System.Data;
using Twilio;
using Twilio.Rest.Verify.V2.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Common.Models;

namespace BackEnd.Utilities
{
    public class TwilioManager
    {

        public TwilioManager(VisitorPhoneNumberInfo visitorPhoneNumberInfo, ILogger logger, IConfigurationRoot config)
        {
            VisitorPhoneNumberInfo = visitorPhoneNumberInfo;
            Logger = logger;
            Config = config;
        }

        private VisitorPhoneNumberInfo VisitorPhoneNumberInfo;

        private readonly ILogger Logger;

        private readonly IConfigurationRoot Config;

        private void Send_SMS()
        {
            string accountSid = Config["TWILIO_ACCOUNT_SID"];
            string authToken = Config["TWILIO_AUTH_TOKEN"];
            string pathServiceSid = Config["TWILIO_SNMC_TRACKING_REGISTRATION_SERVICE_SID"];

            TwilioClient.Init(accountSid, authToken);

            VerificationResource verification = VerificationResource.Create(
                to: VisitorPhoneNumberInfo.PhoneNumber,
                channel: "sms",
                pathServiceSid: pathServiceSid
                );

            VisitorPhoneNumberInfo.IsValidPhoneNumber = verification.Valid.Value;
            VisitorPhoneNumberInfo.PhoneNumberType = verification.Lookup.ToString();
        }

        private void Verify_Phone_Number()
        {
            string accountSid = Config["TWILIO_ACCOUNT_SID"];
            string authToken = Config["TWILIO_AUTH_TOKEN"];
            string pathServiceSid = Config["TWILIO_SNMC_TRACKING_REGISTRATION_SERVICE_SID"];

            TwilioClient.Init(accountSid, authToken);

            VerificationCheckResource verificationCheck = VerificationCheckResource.Create(
                to: VisitorPhoneNumberInfo.PhoneNumber,
                code: VisitorPhoneNumberInfo.VerificationCode,
                pathServiceSid: pathServiceSid
                );

            VisitorPhoneNumberInfo.VerificationStatus = verificationCheck.Status;
        }

        public void SendSMS()
        {
            if (VisitorPhoneNumberInfo.PhoneNumber != null && VisitorPhoneNumberInfo.Id != Guid.Empty)
            {
                Send_SMS();
            }
            else
            {
                throw new DataException("Missing Information");
            }
        }

        public void VerifyPhoneNumber()
        {
            if (VisitorPhoneNumberInfo.PhoneNumber != null && VisitorPhoneNumberInfo.Id != Guid.Empty && VisitorPhoneNumberInfo.VerificationCode != null)
            {
                Verify_Phone_Number();
            }
            else
            {
                throw new DataException("Missing Information");
            }
        }

        public VisitorPhoneNumberInfo GetVisitorPhoneNumberInfo()
        {
            return VisitorPhoneNumberInfo;
        }
    }
}
