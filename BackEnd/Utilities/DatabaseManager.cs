using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Services.AppAuthentication;

using BackEnd.Models;

namespace BackEnd.Utilities
{
    public class DatabaseManager
    {
        public DatabaseManager(ILogger logger, IConfigurationRoot config)
        {
            Logger = logger;
            Config = config;
        }

        public DatabaseManager(Visitor visitor, ILogger logger, IConfigurationRoot config)
        {
            Visitor = visitor;
            Logger = logger;
            Config = config;
        }

        private Visitor Visitor;

        private List<Visitor> Visitors = new List<Visitor>();

        private ILogger Logger;

        private IConfigurationRoot Config;

        private void _GetVisitor(Guid Id)
        {
            // Set ID
            Visitor.Id = Id;

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

        private void _GetVisitors(VisitorSearch visitorSearch)
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("GetUser");
            command.CommandType = CommandType.StoredProcedure;

            // Search Parameters
            if (visitorSearch.FirstName != null)
            {
                command.Parameters.AddWithValue("@FirstName", visitorSearch.FirstName);
            }
            else
            {
                command.Parameters.AddWithValue("@FirstName", DBNull.Value);
            }
            if (visitorSearch.LastName != null)
            {
                command.Parameters.AddWithValue("@LastName", visitorSearch.LastName);
            }
            else
            {
                command.Parameters.AddWithValue("@LastName", DBNull.Value);
            }
            if (visitorSearch.Email != null)
            {
                command.Parameters.AddWithValue("@Email", visitorSearch.Email);
            }
            else
            {
                command.Parameters.AddWithValue("@Email", DBNull.Value);
            }
            if (visitorSearch.PhoneNumber != null)
            {
                command.Parameters.AddWithValue("@phoneNumber", visitorSearch.PhoneNumber);
            }
            else
            {
                command.Parameters.AddWithValue("@phoneNumber", DBNull.Value);
            }

            // Set ID to Null
            command.Parameters.AddWithValue("@userID", DBNull.Value);

            // Check if anything was provided
            if (visitorSearch.FirstName != null || visitorSearch.LastName != null || visitorSearch.Email != null || visitorSearch.PhoneNumber != null)
            {
                // Manage SQL Connection and Write to DB
                using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
                {
                    try
                    {
                        sqlConnection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
                        sqlConnection.Open();
                        command.Connection = sqlConnection;
                        SqlDataReader sqlDataReader = command.ExecuteReader();

                        while (sqlDataReader.Read())
                        {
                            // Create New Visitor Object
                            Visitor visitor = new Visitor();

                            // Set Mandatory Values
                            Logger.LogInformation($"Found Visitor ID: {visitor.Id}");
                            visitor.Id = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("Id"));
                            visitor.RegistrationOrg = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("RegistrationOrg"));
                            visitor.FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName"));
                            visitor.LastName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("LastName"));
                            visitor.Email = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Email"));
                            visitor.PhoneNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PhoneNumber"));
                            visitor.IsMale = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsMale"));

                            // Set Optional Values
                            if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("Address")))
                            {
                                visitor.Address = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Address"));
                            }
                            if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("FamilyID")))
                            {
                                visitor.FamilyID = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("FamilyID"));
                            }

                            Visitors.Add(visitor);
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
            }
            else
            {
                throw new DataException("No Searchable Information Found in Request");
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

        public Guid GetVisitorId()
        {
            return Visitor.Id;
        }

        public Visitor GetVisitor(Guid Id)
        {
            Visitor = new Visitor();
            _GetVisitor(Id);
            return Visitor;
        }

        public List<Visitor> GetVisitors(VisitorSearch visitorSearch)
        {
            _GetVisitors(visitorSearch);
            return Visitors;
        }

        public void AddVisitor()
        {
            _AddVisitor();
        }
    }
}
