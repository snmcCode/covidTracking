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

        private void _GetVisitor()
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("GetUser");
            command.CommandType = CommandType.StoredProcedure;

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@userId", Visitor.Id);

            // Manage SQL Connection and Write to DB
            using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
            {
                try
                {
                    sqlConnection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
                    sqlConnection.Open();
                    command.Connection = sqlConnection;
                    SqlDataReader sqlDataReader = command.ExecuteReader();

                    if (sqlDataReader.Read())
                    {
                        // Set Mandatory Values
                        Visitor.RegistrationOrg = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("RegistrationOrg"));
                        Visitor.FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName"));
                        Visitor.LastName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("LastName"));
                        Visitor.Email = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Email"));
                        Visitor.PhoneNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PhoneNumber"));
                        Visitor.IsMale = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsMale"));

                        // Set Optional Values
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("Address")))
                        {
                            Visitor.Address = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Address"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("FamilyID")))
                        {
                            Visitor.FamilyID = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("FamilyID"));
                        }
                    }
                }
                catch (SqlException e)
                {
                    Logger.LogError($"Database Error: {e}");
                    throw new ApplicationException("Database Error");
                }
                finally
                {
                    // Close SQL Connection if it is still open
                    if (sqlConnection.State == ConnectionState.Open)
                    {
                        sqlConnection.Close();
                    }
                }
            }

            command.Dispose();
        }

        private void _AddVisitor()
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("RegisterUser");
            command.CommandType = CommandType.StoredProcedure;

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@FirstName", Visitor.FirstName);
            command.Parameters.AddWithValue("@LastName", Visitor.LastName);
            command.Parameters.AddWithValue("@Email", Visitor.Email);
            command.Parameters.AddWithValue("@phoneNumber", Visitor.PhoneNumber);
            command.Parameters.AddWithValue("@IsMale", Visitor.IsMale);

            // Add Optional Parameters
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

            // Add Output Parameter (ID)
            SqlParameter outputValue = command.Parameters.Add("@recordID", SqlDbType.UniqueIdentifier);
            outputValue.Direction = ParameterDirection.Output;

            // Manage SQL Connection and Write to DB
            using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
            {
                try
                {
                    sqlConnection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
                    sqlConnection.Open();
                    command.Connection = sqlConnection;
                    command.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    Logger.LogError($"Database Error: {e}");
                    throw new ApplicationException("Database Error");
                }
                finally
                {
                    // Close SQL Connection if it is still open
                    if (sqlConnection.State == ConnectionState.Open)
                    {
                        sqlConnection.Close();
                    }
                }
            }

            // Set ID from Output Parameter
            if (outputValue.Value != null) {
                Visitor.Id = Guid.Parse(Convert.ToString(outputValue.Value));
            }

            command.Dispose();
        }

        public Visitor GetVisitor()
        {
            _GetVisitor();

            return Visitor;
        }

        public void AddVisitor()
        {
            _AddVisitor();
        }

        public Guid GetVisitorId()
        {
            return Visitor.Id;
        }
    }
}
