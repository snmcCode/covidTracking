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
            }
        }


        private void AddVisitor()
        {
            SqlCommand command = new SqlCommand("RegisterUser", SqlConnection);
            command.CommandType = CommandType.StoredProcedure;
            SqlParameter outputValue = command.Parameters.Add("@recordID", SqlDbType.UniqueIdentifier);
            outputValue.Direction = ParameterDirection.Output;
            // Figure out how to get output value of parameter

            try
            {
                command.ExecuteNonQuery();
                Visitor.Id = Convert.ToString(outputValue.Value);
            }

            catch (Exception e)
            {
                Logger.LogError($"Error running Stored Procedure: {e}");
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

        public string GetVisitorId()
        {
            return Visitor.Id;
        }
    }
}
