using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Common.Models;
using System.Data;
using System.Data.SqlClient;

using MobileAuthenticate.Utilities.Exceptions;
using Microsoft.Azure.Services.AppAuthentication;

namespace MobileAuthenticate.Utilities
{
    public class DatabaseManager
    {
        public DatabaseManager(ScannerLogin scannerLogin, ILogger logger, IConfigurationRoot config)
        {
            ScannerLogin = scannerLogin;
            Logger = logger;
            Config = config;
        }

        ScannerLogin ScannerLogin;

        Organization Organization;

        private readonly ILogger Logger;

        private readonly IConfigurationRoot Config;

        private void Login_Scanner()
        {
            if (ScannerLogin.Username != null && ScannerLogin.Password != null)
            {
                // Make SQL Command
                SqlCommand command = new SqlCommand("LoginScanner")
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add Mandatory Parameters
                command.Parameters.AddWithValue("@username", ScannerLogin.Username.Trim());
                command.Parameters.AddWithValue("@password", ScannerLogin.Password.Trim());
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
                            Organization.Id = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Id"));
                        }
                    }
                    catch (SqlException e)
                    {
                        Logger.LogError($"Database Error: {e}");
                        throw new SqlDatabaseException("Database Error");
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
            else
            {
                throw new BadRequestBodyException("No Searchable Information Found in Request");
            }
        }

        private void Get_Organization(int Id)
        {
            // Set ID
            Organization.Id = Id;

            // Make SQL Command
            SqlCommand command = new SqlCommand("GetOrganization")
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@Id", Organization.Id);

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
                        Organization.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"));

                        // Set Optional Values
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("Address")))
                        {
                            Organization.Address = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Address"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ContactName")))
                        {
                            Organization.ContactName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ContactName"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ContactNumber")))
                        {
                            Organization.ContactNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ContactNumber"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ContactEmail")))
                        {
                            Organization.ContactEmail = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ContactEmail"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("loginName")))
                        {
                            Organization.LoginName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("loginName"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("loginSecretHash")))
                        {
                            Organization.LoginSecretHash = sqlDataReader.GetString(sqlDataReader.GetOrdinal("loginSecretHash"));
                        }
                    }
                }
                catch (SqlException e)
                {
                    Logger.LogError($"Database Error: {e}");
                    throw new SqlDatabaseException("Database Error");
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

        private bool Organization_Found()
        {
            if (Organization.Name == null)
            {
                return false;
            }

            return true;
        }

        public Organization LoginScanner()
        {
            ScannerLogin.HashPassword();

            Organization = new Organization();

            Login_Scanner();

            Get_Organization(Organization.Id);

            if (!Organization_Found())
            {
                throw new SqlDatabaseDataNotFoundException("Organization Not Found");
            }

            return Organization;
        }
    }
}
