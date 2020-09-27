
using System;
using System.IO;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace twilioSendSMS
{
    class Program
    {
        static void Main(string[] args)
        {
            // DANGER! This is insecure. See http://twil.io/secure
            const string accountSid = "";
            const string authToken = "";
            const string messagingServiceSid = "";

            TwilioClient.Init(accountSid, authToken);

            using (StreamWriter w = File.AppendText("log.txt"))
            using (StreamReader sr = new StreamReader("testFNLNPH.csv")) // TODO add the file reference. Expected format is firstname, lastName, phoneNumber (in format: 1613#######)\n
            {
                string currentLine;
                LogInfo($"Starting program.", w);

                var countSent = 0;
                var countAccepted = 0;

                while ((currentLine = sr.ReadLine()) != null)
                {
                    
                    string[] vistorInfo = currentLine.Split(',');
                    var firstName = vistorInfo[0];
                    var lastName = vistorInfo[1];
                    var phoneNumber = vistorInfo[2];


                    var textBody = $"Salam {firstName} {lastName}, this is a demo alert showcasing the ability to quickly send mass text messages to our attendees if needed -SNMC."; // TODO mod if needed

                    try
                    {
                        var message = MessageResource.Create(
                        body: textBody,
                        messagingServiceSid: messagingServiceSid,
                        to: new Twilio.Types.PhoneNumber("+" + phoneNumber)
                    );


                        if (!message.ErrorCode.HasValue)
                            LogInfo($"Sent {firstName} {lastName} '{message.Body}' at {phoneNumber}; Price: {message.Price} {message.PriceUnit}; Status: {message.Status}", w);
                        else
                            LogError($"Error {message.ErrorCode} - {message.ErrorMessage} when sending {firstName} {lastName} '{message.Body}' at {phoneNumber}; Price: {message.Price} {message.PriceUnit}; Status: {message.Status}", w);

                        if (message.Status.ToString() == "accepted")
                        {
                            countAccepted++;
                        }
                        else
                        {
                            LogError($"Error, {firstName}, {lastName}, {phoneNumber}, did not accept  '{message.Body}', Status, {message.Status}", w);
                        }
                    }
                    catch (Twilio.Exceptions.ApiException e)
                    {
                        LogError($"Exception caught {e.Code} - {e.Message} - {e.StackTrace}", w);
                    }

                    countSent++;


                    
                    /*

                    var textBody2 = $"Start using it by having it on your mobile device OR printing it OR picking it up from SNMC this week anytime on Monday to Thursday, from Asr to Maghrib -SNMC"; // TODO mod if needed

                    try
                    {
                        var message = MessageResource.Create(
                            body: textBody2,
                            messagingServiceSid: messagingServiceSid,
                            to: new Twilio.Types.PhoneNumber("+" + phoneNumber)
                        );


                        if (!message.ErrorCode.HasValue)
                            LogInfo($"Sent {firstName} {lastName} '{message.Body}' at {phoneNumber}; Price: {message.Price} {message.PriceUnit}; Status: {message.Status}", w);
                        else
                            LogError($"Error {message.ErrorCode} - {message.ErrorMessage} when sending {firstName} {lastName} '{message.Body}' at {phoneNumber}; Price: {message.Price} {message.PriceUnit}; Status: {message.Status}", w);

                        if (message.Status.ToString() == "accepted")
                        {
                            countAccepted++;
                        }
                        else
                        {
                            LogError($"Error, {firstName}, {lastName}, {phoneNumber}, did not accept  '{message.Body}', Status, {message.Status}", w);
                        }

                    }
                    catch (Twilio.Exceptions.ApiException e)
                    {
                        LogError($"Exception caught {e.Code} - {e.Message} - {e.StackTrace}", w);
                    }

                    countSent++;
                    */

                    
                }
                LogInfo($"Sent total of {countSent} text messages. {countAccepted} messages were accepted.", w);

            }

        }

        public static void LogInfo(string logMessage, TextWriter w)
        {
            w.Write("\r\nINFO: ");
            w.WriteLine(DateTime.Now.ToString() + " - " + logMessage);
        }

        public static void LogError(string logMessage, TextWriter w)
        {
            w.Write("\r\nERROR: ");
            w.WriteLine(DateTime.Now.ToString() + " - " + logMessage);
        }
    }
}
