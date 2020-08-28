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
using Common.Utilities.Exceptions;

namespace Common.Utilities
{
    public class DatabaseManager
    {
        public DatabaseManager(Helper helper, IConfigurationRoot config)
        {
            Helper = helper;
            Config = config;
        }

        public DatabaseManager(Visitor visitor, Helper helper, IConfigurationRoot config)
        {
            Visitor = visitor;
            Helper = helper;
            Config = config;
        }

        public DatabaseManager(Visit visit, Helper helper, IConfigurationRoot config)
        {
            Visit = visit;
            Helper = helper;
            Config = config;
        }

        public DatabaseManager(Organization organization, Helper helper, IConfigurationRoot config)
        {
            Organization = organization;
            Helper = helper;
            Config = config;
        }

        public DatabaseManager(ScannerLogin scannerLogin, Helper helper, IConfigurationRoot config)
        {
            ScannerLogin = scannerLogin;
            Helper = helper;
            Config = config;
        }

        private Visitor Visitor;

        private Visit Visit;

        private Organization Organization;

        private ScannerLogin ScannerLogin;

        private readonly List<Visitor> Visitors = new List<Visitor>();

        private readonly List<OrganizationDoor> OrganizationDoors = new List<OrganizationDoor>();

        private Helper Helper;

        private readonly IConfigurationRoot Config;

        private bool AsyncSuccess;

        private bool Visitor_Found()
        {
            if (Visitor.FirstName == null && Visitor.LastName == null && Visitor.Email == null && Visitor.PhoneNumber == null)
            {
                return false;
            }

            return true;
        }

        private bool Visitor_Verified()
        {
            if (Visitor.IsVerified == false)
            {
                return false;
            }

            return true;
        }

        private bool Visitors_Found()
        {
            if (Visitors == null || Visitors.Count == 0)
            {
                return false;
            }

            return true;
        }

        private bool OrganizationDoors_Found()
        {
            if (OrganizationDoors == null || OrganizationDoors.Count == 0)
            {
                return false;
            }

            return true;
        }

        private void Check_Visitor(Guid Id)
        {

            Visitor visitor = new Visitor();

            // Set ID
            visitor.Id = Id;

            // Make SQL Command
            SqlCommand command = new SqlCommand("GetUser")
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@userId", visitor.Id);

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
                        visitor.RegistrationOrg = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("RegistrationOrg"));
                        visitor.FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName"));
                        visitor.LastName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("LastName"));
                        visitor.Email = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Email"));
                        visitor.PhoneNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PhoneNumber")).Trim();
                        visitor.IsMale = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsMale"));
                        visitor.IsVerified = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsVerified"));

                        // Set Optional Values
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("Address")))
                        {
                            visitor.Address = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Address"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("FamilyID")))
                        {
                            visitor.FamilyID = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("FamilyID"));
                        }
                    }
                }
                catch (SqlException e)
                {
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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

            // Check if visitor was found
            if (visitor.FirstName == null && visitor.LastName == null && visitor.Email == null && visitor.PhoneNumber == null)
            {
                throw new SqlDatabaseDataNotFoundException("Visitor Not Found");
            }
        }

        private void Get_Visitor_Lite(Guid Id)
        {
            // Set ID
            Visitor.Id = Id;

            // Make SQL Command
            // TODO: If this doesn't work, change the name to getUserForLogVisit with lowercase first letter G
            SqlCommand command = new SqlCommand("GetUserForLogVisit")
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
                        Visitor.FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName"));
                        Visitor.LastName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("LastName"));
                        Visitor.PhoneNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PhoneNumber"));
                        Visitor.IsMale = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsMale"));
                    }
                }
                catch (SqlException e)
                {
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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

        private void Get_Visitor_Full(Guid Id)
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
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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
                command.Parameters.AddWithValue("@FirstName", visitorSearch.FirstName.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@FirstName", DBNull.Value);
            }
            if (visitorSearch.LastName != null)
            {
                command.Parameters.AddWithValue("@LastName", visitorSearch.LastName.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@LastName", DBNull.Value);
            }
            if (visitorSearch.Email != null)
            {
                command.Parameters.AddWithValue("@Email", visitorSearch.Email.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@Email", DBNull.Value);
            }
            if (visitorSearch.PhoneNumber != null)
            {
                command.Parameters.AddWithValue("@phoneNumber", visitorSearch.PhoneNumber.Trim());
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
                        Helper.DebugLogger.InnerException = e;
                        Helper.DebugLogger.InnerExceptionType = "SqlException";
                        throw new SqlDatabaseException("A Database Error Occurred");
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
            command.Parameters.AddWithValue("@FirstName", Visitor.FirstName.Trim());
            command.Parameters.AddWithValue("@LastName", Visitor.LastName.Trim());
            command.Parameters.AddWithValue("@Email", Visitor.Email.Trim());
            command.Parameters.AddWithValue("@phoneNumber", Visitor.PhoneNumber.Trim());
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
            if (Visitor.IsVerified.HasValue)
            {
                command.Parameters.AddWithValue("@isVerified", Visitor.IsVerified);
            }
            else
            {
                command.Parameters.AddWithValue("@isVerified", DBNull.Value);
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
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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
                command.Parameters.AddWithValue("@FirstName", Visitor.FirstName.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@FirstName", DBNull.Value);
            }
            if (Visitor.LastName != null)
            {
                command.Parameters.AddWithValue("@LastName", Visitor.LastName.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@LastName", DBNull.Value);
            }
            if (Visitor.Email != null)
            {
                command.Parameters.AddWithValue("@Email", Visitor.Email.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@Email", DBNull.Value);
            }
            if (Visitor.PhoneNumber != null)
            {
                command.Parameters.AddWithValue("@phoneNumber", Visitor.PhoneNumber.Trim());
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
                command.Parameters.AddWithValue("@IsVerified", Visitor.IsVerified);
            }
            else
            {
                command.Parameters.AddWithValue("@IsVerified", DBNull.Value);
            }
            if (Visitor.LastInfectionDate != null)
            {
                command.Parameters.AddWithValue("@LastInfectionDate", Visitor.LastInfectionDate);
            }
            else
            {
                command.Parameters.AddWithValue("@LastInfectionDate", DBNull.Value);
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
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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
            // TODO: Test if DeviceLocation ever comes up as null, if not, add a non-null check for them here
            if (Visit.VisitorId != null && Visit.Organization != null && Visit.Date != null && Visit.Time != null && Visit.ScannerVersion != null)
            {
                // Prepare Visit Data for Writing to DB
                Visit.FinalizeData();

                AsyncSuccess = false;

                using (CosmosClient cosmosClient = new CosmosClient(Config.GetConnectionString("NoSQLConnectionString")))
                {
                    try
                    {
                        VisitInfo visitInfo = Visit.GetVisitInfo();
                        VisitorInfo visitorInfo = Visit.GetVisitorInfo();

                        Database database = cosmosClient.GetDatabase("AttendanceTracking");
                        Container container = database.GetContainer(Config["NoSQLDBCollection"]);
                        await container.CreateItemAsync(visitInfo, new PartitionKey(visitInfo.PartitionKey));
                        await container.CreateItemAsync(visitorInfo, new PartitionKey(visitorInfo.PartitionKey));
                        AsyncSuccess = true;
                    }

                    catch (CosmosException e)
                    {
                        AsyncSuccess = false;
                        Helper.DebugLogger.InnerException = e;
                        Helper.DebugLogger.InnerExceptionType = "CosmosException";
                        throw new NoSqlDatabaseException("A CosmosDB Database Error Occurred");
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

        private bool Organization_Found()
        {
            if (Organization.Name == null)
            {
                return false;
            }

            return true;
        }

        private void Check_Organization(int Id)
        {
            Organization organization = new Organization();

            // Set ID
            organization.Id = Id;

            // Make SQL Command
            SqlCommand command = new SqlCommand("GetOrganization")
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@Id", organization.Id);

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
                        organization.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"));

                        // Set Optional Values
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("Address")))
                        {
                            organization.Address = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Address"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ContactName")))
                        {
                            organization.ContactName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ContactName"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ContactNumber")))
                        {
                            organization.ContactNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ContactNumber"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ContactEmail")))
                        {
                            organization.ContactEmail = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ContactEmail"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("loginName")))
                        {
                            organization.LoginName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("loginName"));
                        }
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("loginSecretHash")))
                        {
                            organization.LoginSecretHash = sqlDataReader.GetString(sqlDataReader.GetOrdinal("loginSecretHash"));
                        }
                    }
                }
                catch (SqlException e)
                {
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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
            if (organization.Name == null)
            {
                throw new SqlDatabaseDataNotFoundException("Organization Not Found");
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
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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

        private void Add_Organization()
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("InsertOrganization")
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add Mandatory Parameters
            command.Parameters.AddWithValue("@Name", Organization.Name.Trim());

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
                command.Parameters.AddWithValue("@ContactName", Organization.ContactName.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@ContactName", DBNull.Value);
            }
            if (Organization.ContactNumber != null)
            {
                command.Parameters.AddWithValue("@ContactNumber", Organization.ContactNumber.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@ContactNumber", DBNull.Value);
            }
            if (Organization.ContactEmail != null)
            {
                command.Parameters.AddWithValue("@ContactEmail", Organization.ContactEmail.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@ContactEmail", DBNull.Value);
            }
            if (Organization.ContactNumber != null)
            {
                command.Parameters.AddWithValue("@loginName", Organization.LoginName.Trim());
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

            // Add Output Parameter (ID)
            SqlParameter outputValue = command.Parameters.Add("@recordID", SqlDbType.Int);
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
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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
                Organization.Id = (int)outputValue.Value;
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
                command.Parameters.AddWithValue("@Name", Organization.Name.Trim());
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
                command.Parameters.AddWithValue("@ContactName", Organization.ContactName.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@ContactName", DBNull.Value);
            }
            if (Organization.ContactNumber != null)
            {
                command.Parameters.AddWithValue("@ContactNumber", Organization.ContactNumber.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@ContactNumber", DBNull.Value);
            }
            if (Organization.ContactEmail != null)
            {
                command.Parameters.AddWithValue("@ContactEmail", Organization.ContactEmail.Trim());
            }
            else
            {
                command.Parameters.AddWithValue("@ContactEmail", DBNull.Value);
            }
            if (Organization.LoginName != null)
            {
                command.Parameters.AddWithValue("@loginName", Organization.LoginName.Trim());
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
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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

        private void Login_Scanner()
        {
            if (ScannerLogin.Username != null && ScannerLogin.Password != null)
            {
                // Make SQL Command
                SqlCommand command = new SqlCommand("orgCheckCredentials")
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add Mandatory Parameters
                command.Parameters.AddWithValue("@loginName", ScannerLogin.Username.Trim());
                command.Parameters.AddWithValue("@loginSecretHash", ScannerLogin.Password);

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
                            Organization.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"));
                        }
                    }
                    catch (SqlException e)
                    {
                        Helper.DebugLogger.InnerException = e;
                        Helper.DebugLogger.InnerExceptionType = "SqlException";
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
        private void Update_OrganizationCredentials(OrganizationCredentialInfo organizationCredentialInfo)
        {
            if (organizationCredentialInfo.LoginName != null && organizationCredentialInfo.LoginSecretHash != null)
            {
                // Make SQL Command
                SqlCommand command = new SqlCommand("orgAddCredentials")
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add Mandatory Parameters
                command.Parameters.AddWithValue("@orgId", organizationCredentialInfo.Id);
                command.Parameters.AddWithValue("@loginName", organizationCredentialInfo.LoginName.Trim());
                command.Parameters.AddWithValue("@loginSecretHash", organizationCredentialInfo.LoginSecretHash);

                // Add Return Value Parameter
                SqlParameter returnParameter = command.Parameters.Add("@ReturnVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;

                // Manage SQL Connection and Write to DB
                using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
                {
                    try
                    {
                        sqlConnection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
                        sqlConnection.Open();
                        command.Connection = sqlConnection;
                        command.ExecuteNonQuery();
                        int returnValue = (int) returnParameter.Value;

                        if (returnValue == -1)
                        {
                            throw new SqlDatabaseDataNotFoundException("Credentials Not Updated, Organization Was Likely Not Found");
                        }
                    }
                    catch (SqlException e)
                    {
                        Helper.DebugLogger.InnerException = e;
                        Helper.DebugLogger.InnerExceptionType = "SqlException";
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
        private void Get_OrganizationDoors(int Id)
        {
            // Make SQL Command
            SqlCommand command = new SqlCommand("orgGetDoors")
            {
                CommandType = CommandType.StoredProcedure
            };

            // Search Parameters
            command.Parameters.AddWithValue("@orgId", Id);

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
                        OrganizationDoor organizationDoor = new OrganizationDoor
                        {
                            OrganizationId = Id,
                            DoorName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Door"))
                        };

                        OrganizationDoors.Add(organizationDoor);
                    }
                }
                catch (SqlException e)
                {
                    Helper.DebugLogger.InnerException = e;
                    Helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred");
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

        public Visitor GetVisitorLite(Guid Id)
        {
            Visitor = new Visitor();
            Get_Visitor_Lite(Id);

            if (!Visitor_Found())
            {
                throw new SqlDatabaseDataNotFoundException("Visitor Not Found");
            }

            return Visitor;
        }

        public Visitor GetVisitorFull(Guid Id)
        {
            Visitor = new Visitor();
            Get_Visitor_Full(Id);

            if (!Visitor_Found())
            {
                throw new SqlDatabaseDataNotFoundException("Visitor Not Found");
            }

            return Visitor;
        }

        public List<Visitor> GetVisitors(VisitorSearch visitorSearch)
        {
            Get_Visitors(visitorSearch);

            if (!Visitors_Found())
            {
                throw new SqlDatabaseDataNotFoundException("No Visitors Found");
            }

            return Visitors;
        }

        public void AddVisitor()
        {
            Add_Visitor();
        }

        public void UpdateVisitor()
        {
            Check_Visitor(Visitor.Id);
            Update_Visitor();
        }

        public void DeleteVisitor(Guid Id)
        {
            Check_Visitor(Id);
            Delete_Visitor(Id);
        }

        public async Task<string> LogVisit()
        {
            await Log_Visit();

            if (AsyncSuccess)
            {
                if (!Visitor_Verified())
                {
                    throw new UnverifiedException($"Unverified Visitor: {Visitor.Id}");
                }

                return Visit.VisitorInfoId;
            }
            else
            {
                return null;
            }
        }

        public int GetOrganizationId()
        {
            return Organization.Id;
        }

        public Organization GetOrganization(int Id)
        {
            Organization = new Organization();
            Get_Organization(Id);

            if (!Organization_Found())
            {
                throw new SqlDatabaseDataNotFoundException("Organization Not Found");
            }

            return Organization;
        }

        public void AddOrganization()
        {
            Organization.HashLoginSecret();
            Add_Organization();
        }

        public void UpdateOrganization()
        {
            Organization.HashLoginSecret();
            Check_Organization(Organization.Id);
            Update_Organization();
        }

        public void DeleteOrganization(int Id)
        {
            Check_Organization(Id);
            Delete_Organization(Id);
        }

        public Organization LoginScanner()
        {
            ScannerLogin.HashPassword();

            Organization = new Organization();

            Login_Scanner();

            if (!Organization_Found())
            {
                throw new SqlDatabaseDataNotFoundException("Organization Not Found");
            }

            return Organization;
        }
        public void UpdateOrganizationCredentials(OrganizationCredentialInfo organizationCredentialInfo)
        {
            organizationCredentialInfo.HashLoginSecret();
            Check_Organization(organizationCredentialInfo.Id);
            Update_OrganizationCredentials(organizationCredentialInfo);
        }
        public List<OrganizationDoor> GetOrganizationDoors(int Id)
        {
            Get_OrganizationDoors(Id);

            if (!OrganizationDoors_Found())
            {
                throw new SqlDatabaseDataNotFoundException("No Organization Doors Found");
            }

            return OrganizationDoors;
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

        public void SetDataParameter(ScannerLogin scannerLogin)
        {
            ScannerLogin = scannerLogin;
        }
    }
}
