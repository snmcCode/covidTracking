using System;
using Twilio;
using Twilio.Rest.Verify.V2.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Twilio.Rest.Api.V2010.Account;
using Common.Models;
using Common.Utilities.Exceptions;
using Twilio.Exceptions;
using System.Threading.Tasks;

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

        public async Task<bool> SendText(string messageBody,string phoneNumber)
        {
            string accountSid = Config["TWILIO_ACCOUNT_SID"]; ;
            string authToken = Config["TWILIO_AUTH_TOKEN"]; ;
            string messagingServiceSid = Config["TWILIO_SNMC_TRACKING_REGISTRATION_SERVICE_SID"]; ;
            //initialize Twilio client
            TwilioClient.Init(accountSid, authToken);

            var message = await MessageResource.CreateAsync(
                        body: messageBody,
                        messagingServiceSid: messagingServiceSid,
                        to: new Twilio.Types.PhoneNumber(phoneNumber)
                        );

            

            if (!message.ErrorCode.HasValue)
            {
                Helper.DebugLogger.ApiName = "SendText";
                Helper.DebugLogger.LogCustomInformation($"Verification Message sent successfully to {phoneNumber}");
                return true;
            }
            else
            {
                Helper.DebugLogger.ApiName = "SendText";
                Helper.DebugLogger.LogCustomError($"Error { message.ErrorCode} - { message.ErrorMessage} when sending '{message.Body}' to {phoneNumber} ");
                return false;
            }

        }

        private void Send_VerficationCode()
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

        public void SendVerificationCode()
        {
            if (VisitorPhoneNumberInfo.PhoneNumber != null && VisitorPhoneNumberInfo.Id != Guid.Empty)
            {
                Send_VerficationCode();
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
