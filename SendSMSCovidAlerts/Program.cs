using System;
using System.IO;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace SendSMSCovidAlerts
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO add the data in file referenced. Expected format is firstname, phoneNumber (in format: 12223334444)
            var contactsToNotify = "contactsToNotify.csv"; 

            // TODO change based off of contacts running against
            var groupLetter = "B"; 

            // TODO change these depending on postive case appearance
            var organizationCovidPositivePersonCheckedInAt = "ORG"; 
            var dateCovidPositivePersonCheckedIn = "Jan01";
            var localTimeCovidPositivePersonCheckedIn = "12:00pm";

            // TODO fill this up but DO NOT COMMIT as that would be insecure. See http://twil.io/secure
            const string accountSid = "";
            const string authToken = "";
            const string messagingServiceSid = "";


            Console.WriteLine("Running SendSMSCovidAlerts");

            TwilioClient.Init(accountSid, authToken);
            
            var numMessagesSent = 0;
            var numMessagesErrors = 0;

            var pathToContactsToNotify = Path.Combine(Environment.CurrentDirectory, contactsToNotify);

            using (StreamWriter w = File.AppendText("log.txt"))
            using (StreamReader sr = new StreamReader(pathToContactsToNotify))
            {
                LogInfo($"Starting to run script", w);
                string currentLine;
                while ((currentLine = sr.ReadLine()) != null)
                {

                    string[] vistorInfo = currentLine.Split(',');
                    var firstName = vistorInfo[0];
                    var phoneNumber = vistorInfo[1];

                    var textBody = $"{firstName}, COVID positive person attended {organizationCovidPositivePersonCheckedInAt} " +
                        $"on {dateCovidPositivePersonCheckedIn} at {localTimeCovidPositivePersonCheckedIn}.\n" +
                        $"You are in Group {groupLetter}. " +
                        $"Find information here: myonlinemasjid.ca/Covid.\n" +
                        $"-MasjidPass team";

                    var message = MessageResource.Create(
                        body: textBody,
                        messagingServiceSid: messagingServiceSid,
                        to: new Twilio.Types.PhoneNumber("+" + phoneNumber)
                    );

                    if (!message.ErrorCode.HasValue)
                    {
                        LogInfo($"Sent {firstName} '{message.Body}' at {phoneNumber}", w);
                        numMessagesSent++;
                    }
                    else
                    {
                        LogError($"Error {message.ErrorCode} - {message.ErrorMessage} when sending {firstName} '{message.Body}' at {phoneNumber}", w);
                        numMessagesErrors++;
                    }
                }
                Console.WriteLine("Sent " + numMessagesSent);
                LogInfo($"Sent {numMessagesSent} successfully", w);
                LogInfo($"Sent {numMessagesErrors} errorneously", w);
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