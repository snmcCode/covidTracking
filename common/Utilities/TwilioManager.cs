using System;
using Twilio;
using Twilio.Rest.Verify.V2.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Common.Models;
using Common.Utilities.Exceptions;
using Twilio.Exceptions;

namespace Common.Utilities
{
    public class TwilioManager
    {

        public TwilioManager(VisitorPhoneNumberInfo visitorPhoneNumberInfo, LoggerHelper helper, IConfiguration config)
        {
            VisitorPhoneNumberInfo = visitorPhoneNumberInfo;
            Helper = helper;
            Config = config;
        }

        private VisitorPhoneNumberInfo VisitorPhoneNumberInfo;

        private LoggerHelper Helper;

        private readonly IConfiguration Config;

        private void Send_SMS()
        {
            string accountSid = Config["TWILIO_ACCOUNT_SID"];
            string authToken = Config["TWILIO_AUTH_TOKEN"];
            string pathServiceSid = Config["TWILIO_SNMC_TRACKING_REGISTRATION_SERVICE_SID"];

            try
            {
                TwilioClient.Init(accountSid, authToken);
                VerificationResource verification = VerificationResource.Create(
                    to: VisitorPhoneNumberInfo.PhoneNumber,
                    channel: "sms",
                    pathServiceSid: pathServiceSid
                    );
                VisitorPhoneNumberInfo.IsValidPhoneNumber = verification.Valid.Value;
                VisitorPhoneNumberInfo.PhoneNumberType = verification.Lookup.ToString();
            }

            catch (TwilioException e)
            {
                Helper.DebugLogger.InnerException = e;
                Helper.DebugLogger.InnerExceptionType = "TwilioAPIException";
                throw new TwilioApiException("An Error Occurred with the Twilio API");
            }
        }

        private void Verify_Phone_Number()
        {
            string accountSid = Config["TWILIO_ACCOUNT_SID"];
            string authToken = Config["TWILIO_AUTH_TOKEN"];
            string pathServiceSid = Config["TWILIO_SNMC_TRACKING_REGISTRATION_SERVICE_SID"];

            try
            {
                TwilioClient.Init(accountSid, authToken);
                VerificationCheckResource verificationCheck = VerificationCheckResource.Create(
                    to: VisitorPhoneNumberInfo.PhoneNumber,
                    code: VisitorPhoneNumberInfo.VerificationCode,
                    pathServiceSid: pathServiceSid
                    );
                VisitorPhoneNumberInfo.IsValidPhoneNumber = verificationCheck.Valid.Value;
                VisitorPhoneNumberInfo.VerificationStatus = verificationCheck.Status;
            }

            catch (TwilioException e)
            {
                Helper.DebugLogger.InnerException = e;
                Helper.DebugLogger.InnerExceptionType = "TwilioAPIException";
                throw new TwilioApiException("An Error Occurred with the Twilio API");
            }

        }

        public void SendSMS()
        {
            if (VisitorPhoneNumberInfo.PhoneNumber != null && VisitorPhoneNumberInfo.Id != Guid.Empty)
            {
                Send_SMS();
            }
            else
            {
                throw new BadRequestBodyException("Missing Information");
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
                throw new BadRequestBodyException("Missing Information");
            }
        }

        public VisitorPhoneNumberInfo GetVisitorPhoneNumberInfo()
        {
            return VisitorPhoneNumberInfo;
        }
    }
}
