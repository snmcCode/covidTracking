using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Services.AppAuthentication;

using Common.Models;
using BackEnd.Utilities.Exceptions;

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

        public DatabaseManager(Organization organization, ILogger logger, IConfigurationRoot config)
        {
            Organization = organization;
            Logger = logger;
            Config = config;
        }

        private Visitor Visitor;

        private Visit Visit;

        private Organization Organization;

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
                        Visitor.IsVerified = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsVerified"));

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

            // Check if result came back empty
            if (Visitor.FirstName == null && Visitor.LastName == null && Visitor.Email == null && Visitor.PhoneNumber == null && Visitor.Address == null)
            {
                throw new SqlDatabaseDataException("Visitor Not Found");
            }
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
                                IsMale = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsMale")),
                                IsVerified = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsVerified"))
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
            }
            else
            {
                throw new BadRequestBodyException("No Searchable Information Found in Request");
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
            command.Parameters.AddWithValue("@id", Visitor.Id);

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
                command.Parameters.AddWithValue("@FamilyID", Visitor.FamilyID);
            }
            else
            {
                command.Parameters.AddWithValue("@FamilyID", DBNull.Value);
            }
            if (Visitor.IsMale.HasValue)
            {
                command.Parameters.AddWithValue("@IsMale", Visitor.IsMale);
            }
            else
            {
                command.Parameters.AddWithValue("@IsMale", DBNull.Value);
            }
            if (Visitor.IsVerified.HasValue)
            {
                command.Parameters.AddWithValue("@isVerified", Visitor.IsVerified);
            }
            else
            {
                command.Parameters.AddWithValue("@isVerified", DBNull.Value);
            }

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

        private void Delete_Visitor(Guid Id)
        {
            // Set ID
            Visitor.Id = Id;

            // Make SQL Command
            SqlCommand command = new SqlCommand("DeleteVisitor")
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@Id", Visitor.Id);

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

        private async Task Log_Visit()
        {
            if (Visit.VisitorId != null && Visit.Organization != null && Visit.Date != null && Visit.Time != null)
            {
                AsyncSuccess = false;

                using (CosmosClient cosmosClient = new CosmosClient(Config.GetConnectionString("NoSQLConnectionString")))
                {
                    try
                    {
                        VisitInfo visitInfo = Visit.GetVisitInfo();
                        VisitorInfo visitorInfo = Visit.GetVisitorInfo();

                        Database database = cosmosClient.GetDatabase("AttendanceTracking");
                        Container container = database.GetContainer("visits");
                        await container.CreateItemAsync(visitInfo, new PartitionKey(visitInfo.PartitionKey));
                        await container.CreateItemAsync(visitorInfo, new PartitionKey(visitorInfo.PartitionKey));
                        AsyncSuccess = true;
                    }

                    catch (CosmosException e)
                    {
                        Logger.LogError($"Database Error: {e}");
                        AsyncSuccess = false;
                        throw new NoSqlDatabaseException("Database Error");
                    }
                    finally
                    {
                        cosmosClient.Dispose();
                    }
                }
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

            // Check if result came back empty
            if (Organization.Name == null && Organization.Address == null && Organization.ContactName == null && Organization.ContactNumber == null && Organization.ContactEmail == null && Organization.LoginName == null && Organization.LoginSecretHash == null)
            {
                throw new SqlDatabaseDataException("Organization Not Found");
            }
        }

        private void Add_Organization()
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("InsertOrganization")
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@Name", Organization.Name);

            // Add Optional Parameters
            if (Organization.Address != null)
            {
                command.Parameters.AddWithValue("@Address", Organization.Address);
            }
            else
            {
                command.Parameters.AddWithValue("@Address", DBNull.Value);
            }
            if (Organization.ContactName != null)
            {
                command.Parameters.AddWithValue("@ContactName", Organization.ContactName);
            }
            else
            {
                command.Parameters.AddWithValue("@ContactName", DBNull.Value);
            }
            if (Organization.ContactNumber != null)
            {
                command.Parameters.AddWithValue("@ContactNumber", Organization.ContactNumber);
            }
            else
            {
                command.Parameters.AddWithValue("@ContactNumber", DBNull.Value);
            }
            if (Organization.ContactEmail != null)
            {
                command.Parameters.AddWithValue("@ContactEmail", Organization.ContactEmail);
            }
            else
            {
                command.Parameters.AddWithValue("@ContactEmail", DBNull.Value);
            }
            if (Organization.ContactNumber != null)
            {
                command.Parameters.AddWithValue("@loginName", Organization.LoginName);
            }
            else
            {
                command.Parameters.AddWithValue("@loginName", DBNull.Value);
            }
            if (Organization.ContactNumber != null)
            {
                command.Parameters.AddWithValue("@loginSecretHash", Organization.LoginSecretHash);
            }
            else
            {
                command.Parameters.AddWithValue("@loginSecretHash", DBNull.Value);
            }


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

        private void Update_Organization()
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("UpdateOrganization")
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@Id", Organization.Id);

            // Add Optional Parameters
            if (Organization.Name != null)
            {
                command.Parameters.AddWithValue("@Name", Organization.Name);
            }
            else
            {
                command.Parameters.AddWithValue("@Name", DBNull.Value);
            }
            if (Organization.Address != null)
            {
                command.Parameters.AddWithValue("@Address", Organization.Address);
            }
            else
            {
                command.Parameters.AddWithValue("@Address", DBNull.Value);
            }
            if (Organization.ContactName != null)
            {
                command.Parameters.AddWithValue("@ContactName", Organization.ContactName);
            }
            else
            {
                command.Parameters.AddWithValue("@ContactName", DBNull.Value);
            }
            if (Organization.ContactNumber != null)
            {
                command.Parameters.AddWithValue("@ContactNumber", Organization.ContactNumber);
            }
            else
            {
                command.Parameters.AddWithValue("@ContactNumber", DBNull.Value);
            }
            if (Organization.ContactEmail != null)
            {
                command.Parameters.AddWithValue("@ContactEmail", Organization.ContactEmail);
            }
            else
            {
                command.Parameters.AddWithValue("@ContactEmail", DBNull.Value);
            }
            if (Organization.LoginName != null)
            {
                command.Parameters.AddWithValue("@loginName", Organization.LoginName);
            }
            else
            {
                command.Parameters.AddWithValue("@loginName", DBNull.Value);
            }
            if (Organization.LoginSecretHash != null)
            {
                command.Parameters.AddWithValue("@loginSecretHash", Organization.LoginSecretHash);
            }
            else
            {
                command.Parameters.AddWithValue("@loginSecretHash", DBNull.Value);
            }

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

        private void Delete_Organization(int Id)
        {
            // Set ID
            Organization.Id = Id;

            // Make SQL Command
            SqlCommand command = new SqlCommand("DeleteOrganization")
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
                    command.ExecuteNonQuery();
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

        public void DeleteVisitor(Guid Id)
        {
            Delete_Visitor(Id);
        }

        public async Task<string> LogVisit()
        {
            await Log_Visit();

            if (AsyncSuccess)
            {
                return Visit.VisitorInfoId;
            }
            else
            {
                return null;
            }
        }

        public void AddOrganization()
        {
            Add_Organization();
        }

        public int GetOrganizationId()
        {
            return Organization.Id;
        }

        public Organization GetOrganization(int Id)
        {
            Organization = new Organization();
            Get_Organization(Id);
            return Organization;
        }

        public void UpdateOrganization()
        {
            Update_Organization();
        }

        public void DeleteOrganization(int Id)
        {
            Delete_Organization(Id);
        }

        public void SetDataParameter(Visit visit)
        {
            Visit = visit;
        }

        public void SetDataParameter(Visitor visitor)
        {
            Visitor = visitor;
        }

        public void SetDataParameter(Organization organization)
        {
            Organization = organization;
        }
    }
}
