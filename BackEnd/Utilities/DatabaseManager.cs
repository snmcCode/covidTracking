using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
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

        public DatabaseManager(Visit visit, ILogger logger, IConfigurationRoot config)
        {
            Visit = visit;
            Logger = logger;
            Config = config;
        }

        private Visitor Visitor;

        private readonly Visit Visit;

        private readonly List<Visitor> Visitors = new List<Visitor>();

        private readonly ILogger Logger;

        private readonly IConfigurationRoot Config;

        private bool AsyncSuccess;

        private void Get_Visitor(Guid Id)
        {
            // Set ID
            Visitor.Id = Id;

            // Make SQL Command
            SqlCommand command = new SqlCommand("GetUser")
            {
                CommandType = CommandType.StoredProcedure
            };

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

        private void Get_Visitors(VisitorSearch visitorSearch)
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("GetUser")
            {
                CommandType = CommandType.StoredProcedure
            };

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
                            // Create New Visitor Object and Set Mandatory Values
                            Visitor visitor = new Visitor
                            {
                                Id = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("Id")),
                                RegistrationOrg = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("RegistrationOrg")),
                                FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName")),
                                LastName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("LastName")),
                                Email = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Email")),
                                PhoneNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PhoneNumber")),
                                IsMale = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsMale"))
                            };

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

        private void Add_Visitor()
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("RegisterUser")
            {
                CommandType = CommandType.StoredProcedure
            };

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

        private void Update_Visitor()
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("UpdateUser")
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@recordID", Visitor.Id);

            // Add Optional Parameters
            if (Visitor.FirstName != null)
            {
                command.Parameters.AddWithValue("@FirstName", Visitor.FirstName);
            }
            else
            {
                command.Parameters.AddWithValue("@FirstName", DBNull.Value);
            }
            if (Visitor.LastName != null)
            {
                command.Parameters.AddWithValue("@LastName", Visitor.LastName);
            }
            else
            {
                command.Parameters.AddWithValue("@LastName", DBNull.Value);
            }
            if (Visitor.Email != null)
            {
                command.Parameters.AddWithValue("@Email", Visitor.Email);
            }
            else
            {
                command.Parameters.AddWithValue("@Email", DBNull.Value);
            }
            if (Visitor.PhoneNumber != null)
            {
                command.Parameters.AddWithValue("@phoneNumber", Visitor.PhoneNumber);
            }
            else
            {
                command.Parameters.AddWithValue("@phoneNumber", DBNull.Value);
            }
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
            if (Visitor.IsMale.HasValue)
            {
                command.Parameters.AddWithValue("@IsMale", Visitor.IsMale);
            }
            else
            {
                command.Parameters.AddWithValue("@IsMale", DBNull.Value);
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
            if (outputValue.Value != null)
            {
                Visitor.Id = Guid.Parse(Convert.ToString(outputValue.Value));
            }

            command.Dispose();
        }

        private async Task Log_Visit()
        {
            if (Visit.VisitorId != null && Visit.OrganizationId != null && Visit.Date != null && Visit.Time != null)
            {
                Visit.GenerateId();

                AsyncSuccess = false;

                using (CosmosClient cosmosClient = new CosmosClient(Config.GetConnectionString("NoSQLConnectionString")))
                {
                    try
                    {
                        Database database = cosmosClient.GetDatabase("AttendanceTracking");
                        Container container = database.GetContainer("visits");
                        await container.CreateItemAsync(Visit, new PartitionKey(Visit.PartitionKey));
                        AsyncSuccess = true;
                    }

                    catch (CosmosException e)
                    {
                        Logger.LogError($"Database Error: {e}");
                        AsyncSuccess = false;
                        throw new ApplicationException("Database Error");
                    }
                    finally
                    {
                        cosmosClient.Dispose();
                    }
                }
            }
            else
            {
                throw new DataException("No Searchable Information Found in Request");
            }
        }

        public Guid GetVisitorId()
        {
            return Visitor.Id;
        }

        public Visitor GetVisitor(Guid Id)
        {
            Visitor = new Visitor();
            Get_Visitor(Id);
            return Visitor;
        }

        public List<Visitor> GetVisitors(VisitorSearch visitorSearch)
        {
            Get_Visitors(visitorSearch);
            return Visitors;
        }

        public void AddVisitor()
        {
            Add_Visitor();
        }

        public void UpdateVisitor()
        {
            Update_Visitor();
        }

        public async Task<string> LogVisit()
        {
            await Log_Visit();

            if (AsyncSuccess)
            {
                return Visit.id;
            }
            else
            {
                return null;
            }
        }
    }
}
