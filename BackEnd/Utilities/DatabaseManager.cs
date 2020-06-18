using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Services.AppAuthentication;

using BackEnd.Models;

namespace BackEnd.Utilities
{
    public class DatabaseManager
    {
        public DatabaseManager(Visitor visitor, ILogger logger, IConfigurationRoot config)
        {
            Visitor = visitor;
            Logger = logger;
            Config = config;
        }

        private Visitor Visitor;

        private ILogger Logger;

        private IConfigurationRoot Config;

        private SqlConnection SqlConnection;

        private void CreateSqlConnection()
        {
            SqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString"));
            SqlConnection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
        }

        private void OpenSqlConnection()
        {
            try
            {
                SqlConnection.Open();
            }

            catch (SqlException e)
            {
                Logger.LogError($"Error opening SQL connection: {e}");
                throw new ApplicationException("Problem in Connecting to Database", e);
            }
        }


        private void AddVisitor()
        {
            SqlCommand command = new SqlCommand("RegisterUser", SqlConnection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@FirstName", Visitor.FirstName);
            command.Parameters.AddWithValue("@LastName", Visitor.LastName);
            command.Parameters.AddWithValue("@RegistrationOrg", Visitor.RegistrationOrg);
            command.Parameters.AddWithValue("@Email", Visitor.EmailAddress);
            command.Parameters.AddWithValue("@phoneNumber", Visitor.PhoneNumber);
            command.Parameters.AddWithValue("@Address", Visitor.Address);
            command.Parameters.AddWithValue("@IsMale", Visitor.IsMale);
            command.Parameters.AddWithValue("@FamilyId", Visitor.FamilyID);

            Logger.LogInformation(
                    "\nVisitor Information\n" +
                    $"Visitor\n" +
                    $"RegistrationOrg: {Visitor.RegistrationOrg}\n" +
                    $"FirstName: {Visitor.FirstName}\n" +
                    $"LastName: {Visitor.LastName}\n" +
                    $"EmailAddress: {Visitor.EmailAddress}\n" +
                    $"PhoneNumber: {Visitor.PhoneNumber}\n" +
                    $"Address: {Visitor.Address}\n" +
                    $"FamilyID: {Visitor.FamilyID}\n" +
                    $"IsMale: {Visitor.IsMale}\n"
                    );

            SqlParameter outputValue = command.Parameters.Add("@recordID", SqlDbType.UniqueIdentifier);
            outputValue.Direction = ParameterDirection.Output;

            try
            {
                Logger.LogInformation(
                    "\nCommand Information\n" +
                    $"Visitor\n" +
                    $"RegistrationOrg: {command.Parameters["@RegistrationOrg"].Value}\n" +
                    $"FirstName: {command.Parameters["@FirstName"].Value}\n" +
                    $"LastName: {command.Parameters["@LastName"].Value}\n" +
                    $"EmailAddress: {command.Parameters["@Email"].Value}\n" +
                    $"PhoneNumber: {command.Parameters["@phoneNumber"].Value}\n" +
                    $"Address: {command.Parameters["@Address"].Value}\n" +
                    $"FamilyID: {command.Parameters["@FamilyId"].Value}\n" +
                    $"IsMale: {command.Parameters["@IsMale"].Value}\n"
                    );
                command.ExecuteNonQuery();
                Visitor.Id = Guid.Parse(Convert.ToString(outputValue.Value));
            }

            catch (SqlException e)
            {
                Logger.LogError($"Error running Stored Procedure: {e}");
                throw new ApplicationException("Problem in Writing to Database", e);
            }

            catch (Exception e)
            {
                Logger.LogError($"Error in Code: {e}");
                throw new ApplicationException("Problem with Code", e);
            }

            command.Dispose();
        }

        private void CloseSqlConnection()
        {
            SqlConnection.Close();
        }

        public void CreateVisitor()
        {
            CreateSqlConnection();
            OpenSqlConnection();
            AddVisitor();
            CloseSqlConnection();
        }

        public Guid GetVisitorId()
        {
            return Visitor.Id;
        }
    }
}
