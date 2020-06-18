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

            // Mandatory Parameters
            command.Parameters.AddWithValue("@FirstName", Visitor.FirstName);
            command.Parameters.AddWithValue("@LastName", Visitor.LastName);
            command.Parameters.AddWithValue("@Email", Visitor.EmailAddress);
            command.Parameters.AddWithValue("@phoneNumber", Visitor.PhoneNumber);
            command.Parameters.AddWithValue("@IsMale", Visitor.IsMale);

            // Optional Parameters
            if (Visitor.RegistrationOrg != 0)
            {
                command.Parameters.AddWithValue("@RegistrationOrg", Visitor.RegistrationOrg);
            }
            else
            {
                command.Parameters.AddWithValue("@RegistrationOrg", DBNull.Value);
            }
            if (Visitor.Address != null)
            {
                command.Parameters.AddWithValue("@Address", Visitor.Address);
            }
            else
            {
                command.Parameters.AddWithValue("@Address", DBNull.Value);
            }
            if (Visitor.FamilyID != Guid.Empty)
            {
                command.Parameters.AddWithValue("@FamilyId", Visitor.FamilyID);
            }
            else
            {
                command.Parameters.AddWithValue("@FamilyId", DBNull.Value);
            }

            // Output Parameter (ID)
            SqlParameter outputValue = command.Parameters.Add("@recordID", SqlDbType.UniqueIdentifier);
            outputValue.Direction = ParameterDirection.Output;

            try
            {
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
