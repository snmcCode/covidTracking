using System;
using System.IO;
using System.Collections.Generic;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using LINQtoCSV;

namespace SendSMSCovidAlerts
{
    class ContactToNotify
    {
        [CsvColumn(Name = "Name", FieldIndex = 1)]
        public string Name { get; set; }
        
        [CsvColumn(Name = "PhoneNumber", FieldIndex = 2)]
        public string PhoneNumber { get; set; }

        [CsvColumn(Name = "SMSMessage", FieldIndex = 3)]
        public string SMSMessage { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO add the data in file referenced. Expected format is firstname, phoneNumber (in format: +12223334444), SMSMessage to send
            var contactsToNotify = "contactsToNotify.csv"; 

            // set up CSV file reader
            var pathToContactsToNotify = Path.Combine(Environment.CurrentDirectory, contactsToNotify);
            CsvFileDescription inputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',', 
                FirstLineHasColumnNames = false,
                EnforceCsvColumnAttribute = true
            };
            CsvContext cc = new CsvContext();
            IEnumerable<ContactToNotify> products =
                cc.Read<ContactToNotify>(pathToContactsToNotify, inputFileDescription);


            // set up Twilio. TODO make this pull from config
            // DANGER! DO NOT PUSH WITH SECRETS - This is insecure. See http://twil.io/secure
            const string accountSid = "";
            const string authToken = "";
            const string messagingServiceSid = "";
            
            TwilioClient.Init(accountSid, authToken);
            
            var numMessagesSent = 0;
            var numMessagesErrors = 0;

            using (StreamWriter w = File.AppendText("log.txt"))
            {
                LogInfo($"Starting to run script", w);
                foreach (ContactToNotify contact in products) 
                {
                    var message = MessageResource.Create(
                            body: contact.SMSMessage.TrimStart(',').TrimEnd(','),
                            messagingServiceSid: messagingServiceSid,
                            to: new Twilio.Types.PhoneNumber(contact.PhoneNumber)
                    );

                    if (!message.ErrorCode.HasValue)
                    {
                        var info = $"Sent {contact.Name} '{message.Body}' at {contact.PhoneNumber}";
                        Console.WriteLine(info);
                        LogInfo(info, w);
                        numMessagesSent++;
                    }
                    else
                    {
                        var errorInfo = $"Error {message.ErrorCode} - {message.ErrorMessage} when sending {contact.Name} '{message.Body}' at {contact.PhoneNumber}";
                        Console.WriteLine(errorInfo);
                        LogError(errorInfo, w);
                        numMessagesErrors++;
                    }
                }

                Console.WriteLine("Sent " + numMessagesSent);
                LogInfo($"Sent {numMessagesSent} successfully", w);
                LogInfo($"Sent {numMessagesErrors} erroneously", w);
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