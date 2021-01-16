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
        public DatabaseManager(LoggerHelper helper, IConfiguration config)
        {
            Helper = helper;
            Config = config;
        }

        public DatabaseManager(Visitor visitor, LoggerHelper helper, IConfiguration config)
        {
            Visitor = visitor;
            Helper = helper;
            Config = config;
        }

        public DatabaseManager(Visit visit, LoggerHelper helper, IConfiguration config)
        {
            Visit = visit;
            Helper = helper;
            Config = config;
        }

        public DatabaseManager(Organization organization, LoggerHelper helper, IConfiguration config)
        {
            Organization = organization;
            Helper = helper;
            Config = config;
        }

        public DatabaseManager(ScannerLogin scannerLogin, LoggerHelper helper, IConfiguration config)
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

        private LoggerHelper Helper;

        private readonly IConfiguration Config;

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

        private async Task Check_Visitor(Guid Id)
        {

            Visitor visitor = new Visitor();

            // Set ID
            visitor.Id = Id;



            // Add Mandatory Parameters

            // Manage SQL Connection and Write to DB
            using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
            {
                try
                {
                    // Make SQL Command
                    using (SqlCommand command = new SqlCommand("GetUser"))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@userId", visitor.Id);

                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                        sqlConnection.Open();
                        command.Connection = sqlConnection;
                        SqlDataReader sqlDataReader = command.ExecuteReader();
                        using (sqlDataReader)
                        {
                            if (sqlDataReader.Read())
                            {
                                // Set Mandatory Values
                                visitor.RegistrationOrg = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("RegistrationOrg"));
                                visitor.FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName"));
                                visitor.LastName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("LastName"));
                                visitor.PhoneNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PhoneNumber")).Trim();
                                visitor.IsMale = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsMale"));
                                visitor.IsVerified = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsVerified"));

                                // Set Optional Values
                                var email = sqlDataReader.GetOrdinal("Email");
                                if (!sqlDataReader.IsDBNull(email))
                                {
                                    visitor.Address = sqlDataReader.GetString(email);
                                }
                                var address = sqlDataReader.GetOrdinal("Address");
                                if (!sqlDataReader.IsDBNull(address))
                                {
                                    visitor.Address = sqlDataReader.GetString(address);
                                }

                                var familyId = sqlDataReader.GetOrdinal("FamilyID");
                                if (!sqlDataReader.IsDBNull(familyId))
                                {
                                    visitor.FamilyID = sqlDataReader.GetGuid(familyId);
                                }
                            }
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


            // Check if visitor was found
            if (visitor.FirstName == null && visitor.LastName == null && visitor.Email == null && visitor.PhoneNumber == null)
            {
                throw new SqlDatabaseDataNotFoundException("Visitor Not Found");
            }
        }

        private async Task Get_Visitor_Lite(Guid Id)
        {
            // Set ID
            Visitor.Id = Id;



            // Add Mandatory Parameters

            // Manage SQL Connection and Write to DB
            using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
            {
                // Make SQL Command
                // TODO: If this doesn't work, change the name to getUserForLogVisit with lowercase first letter G

                try
                {
                    using (SqlCommand command = new SqlCommand("GetUserForLogVisit"))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@userId", Visitor.Id);

                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                        sqlConnection.Open();
                        command.Connection = sqlConnection;
                        SqlDataReader sqlDataReader = command.ExecuteReader();
                        using (sqlDataReader)
                        {
                            if (sqlDataReader.Read())
                            {
                                // Set Mandatory Values
                                Visitor.FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName"));
                                Visitor.LastName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("LastName"));
                                Visitor.PhoneNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PhoneNumber"));
                                Visitor.IsMale = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsMale"));
                                Visitor.IsVerified = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsVerified"));
                            }
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
        }

        private async Task Get_Visitor_Full(Guid Id)
        {
            // Set ID
            Visitor.Id = Id;



            // Manage SQL Connection and Write to DB
            using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
            {
                // Make SQL Command


                try
                {
                    using (SqlCommand command = new SqlCommand("GetUser"))
                    {

                        command.CommandType = CommandType.StoredProcedure;

                        // Add Mandatory Parameters
                        command.Parameters.AddWithValue("@userId", Visitor.Id);
                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                        sqlConnection.Open();
                        command.Connection = sqlConnection;
                        SqlDataReader sqlDataReader = command.ExecuteReader();
                        using (sqlDataReader)
                        {
                            if (sqlDataReader.Read())
                            {
                                // Set Mandatory Values
                                Visitor.RegistrationOrg = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("RegistrationOrg"));
                                Visitor.FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName"));
                                Visitor.LastName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("LastName"));
                                Visitor.PhoneNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PhoneNumber"));
                                Visitor.IsMale = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsMale"));
                                Visitor.IsVerified = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsVerified"));


                                var email = sqlDataReader.GetOrdinal("Email");
                                var address = sqlDataReader.GetOrdinal("Address");
                                var familyID = sqlDataReader.GetOrdinal("FamilyID");

                                // Set Optional Values
                                if (!sqlDataReader.IsDBNull(email))
                                {
                                    Visitor.Address = sqlDataReader.GetString(email);
                                }
                                if (!sqlDataReader.IsDBNull(address))
                                {
                                    Visitor.Address = sqlDataReader.GetString(address);
                                }
                                if (!sqlDataReader.IsDBNull(familyID))
                                {
                                    Visitor.FamilyID = sqlDataReader.GetGuid(familyID);
                                }
                            }
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

        }

        private async Task Get_Visitors(VisitorSearch visitorSearch)
        {
            // Make SQL Command
            using (SqlCommand command = new SqlCommand("GetUser"))
            {
                command.CommandType = CommandType.StoredProcedure;
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
                            sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                            sqlConnection.Open();
                            command.Connection = sqlConnection;
                            SqlDataReader sqlDataReader = command.ExecuteReader();
                            using (sqlDataReader)
                            {

                                var id = sqlDataReader.GetOrdinal("Id");
                                var registrationOrg = sqlDataReader.GetOrdinal("RegistrationOrg");
                                var firstname = sqlDataReader.GetOrdinal("FirstName");
                                var lastname = sqlDataReader.GetOrdinal("LastName");
                                var phoneNumber = sqlDataReader.GetOrdinal("PhoneNumber");
                                var isMale = sqlDataReader.GetOrdinal("IsMale");
                                var isVerified = sqlDataReader.GetOrdinal("IsVerified");
                                var email = sqlDataReader.GetOrdinal("Email");
                                var address = sqlDataReader.GetOrdinal("Address");
                                var familyId = sqlDataReader.GetOrdinal("FamilyID");

                                while (sqlDataReader.Read())
                                {
                                    // Create New Visitor Object and Set Mandatory Values
                                    Visitor visitor = new Visitor();
                                    visitor.Id = sqlDataReader.GetGuid(id);
                                    visitor.RegistrationOrg = sqlDataReader.GetInt32(registrationOrg);
                                    visitor.FirstName = sqlDataReader.GetString(firstname);
                                    visitor.LastName = sqlDataReader.GetString(lastname);
                                    visitor.PhoneNumber = sqlDataReader.GetString(phoneNumber);
                                    visitor.IsMale = sqlDataReader.GetBoolean(isMale);
                                    visitor.IsVerified = sqlDataReader.GetBoolean(isVerified);

                                    // Set Optional Values
                                    if (!sqlDataReader.IsDBNull(email))
                                    {
                                        visitor.Address = sqlDataReader.GetString(email);
                                    }
                                    if (!sqlDataReader.IsDBNull(address))
                                    {
                                        visitor.Address = sqlDataReader.GetString(address);
                                    }
                                    if (!sqlDataReader.IsDBNull(familyId))
                                    {
                                        visitor.FamilyID = sqlDataReader.GetGuid(familyId);
                                    }

                                    Visitors.Add(visitor);
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
                }
                else
                {
                    throw new BadRequestBodyException("No Searchable Information Found in Request");
                }

            }
        }

        private async Task Add_Visitor()
        {
            // Make SQL Command
            using (SqlCommand command = new SqlCommand("RegisterUser"))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add Mandatory Parameters
                command.Parameters.AddWithValue("@FirstName", Visitor.FirstName.Trim());
                command.Parameters.AddWithValue("@LastName", Visitor.LastName.Trim());
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
                if (Visitor.Email != null && Visitor.Email != "")
                {
                    command.Parameters.AddWithValue("@Email", Visitor.Email.Trim());
                }
                else
                {
                    command.Parameters.AddWithValue("@Email", DBNull.Value);
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
                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
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
                    Visitor.Id = Guid.Parse(Convert.ToString(outputValue.Value));
                }

            }
        }

        private async Task Update_Visitor()
        {
            // Make SQL Command
            using (SqlCommand command = new SqlCommand("UpdateUser"))
            {
                command.CommandType = CommandType.StoredProcedure;

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
                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
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

            }
        }

        private async Task Delete_Visitor(Guid Id)
        {
            // Set ID
            Visitor.Id = Id;

            // Make SQL Command
            using (SqlCommand command = new SqlCommand("DeleteVisitor"))
            {

                command.CommandType = CommandType.StoredProcedure;

                // Add Mandatory Parameters
                command.Parameters.AddWithValue("@Id", Visitor.Id);

                // Manage SQL Connection and Write to DB
                using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
                {
                    try
                    {
                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
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

            }
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

        private async Task Check_Organization(int Id)
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
                    sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                    sqlConnection.Open();
                    command.Connection = sqlConnection;
                    SqlDataReader sqlDataReader = command.ExecuteReader();
                    using (sqlDataReader)
                    {
                        if (sqlDataReader.Read())
                        {
                            // Set Mandatory Values
                            organization.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"));

                            // Set Optional Values
                            var address = sqlDataReader.GetOrdinal("Address");
                            if (!sqlDataReader.IsDBNull(address))
                            {
                                organization.Address = sqlDataReader.GetString(address);
                            }
                            var contactName = sqlDataReader.GetOrdinal("ContactName");

                            if (!sqlDataReader.IsDBNull(contactName))
                            {
                                organization.ContactName = sqlDataReader.GetString(contactName);
                            }
                            var contactNumber = sqlDataReader.GetOrdinal("ContactNumber");

                            if (!sqlDataReader.IsDBNull(contactNumber))
                            {
                                organization.ContactNumber = sqlDataReader.GetString(contactNumber);
                            }
                            var contactEmail = sqlDataReader.GetOrdinal("ContactEmail");

                            if (!sqlDataReader.IsDBNull(contactEmail))
                            {
                                organization.ContactEmail = sqlDataReader.GetString(contactEmail);
                            }
                            var loginName = sqlDataReader.GetOrdinal("loginName");

                            if (!sqlDataReader.IsDBNull(loginName))
                            {
                                organization.LoginName = sqlDataReader.GetString(loginName);
                            }
                            var loginSecretHash = sqlDataReader.GetOrdinal("loginSecretHash");

                            if (!sqlDataReader.IsDBNull(loginSecretHash))
                            {
                                organization.LoginSecretHash = sqlDataReader.GetString(loginSecretHash);
                            }
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



            // Check if result came back empty
            if (organization.Name == null)
            {
                throw new SqlDatabaseDataNotFoundException("Organization Not Found");
            }
        }

        private async Task Get_Organization(int Id)
        {
            // Set ID
            Organization.Id = Id;

            // Make SQL Command
            using (SqlCommand command = new SqlCommand("GetOrganization"))
            {

                command.CommandType = CommandType.StoredProcedure;

                // Add Mandatory Parameters
                command.Parameters.AddWithValue("@Id", Organization.Id);

                // Manage SQL Connection and Write to DB
                using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
                {
                    try
                    {
                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                        sqlConnection.Open();
                        command.Connection = sqlConnection;
                        SqlDataReader sqlDataReader = command.ExecuteReader();
                        using (sqlDataReader)
                        {
                            if (sqlDataReader.Read())
                            {
                                // Set Mandatory Values
                                Organization.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"));

                                // Set Optional Values
                                var address = sqlDataReader.GetOrdinal("Address");
                                if (!sqlDataReader.IsDBNull(address))
                                {
                                    Organization.Address = sqlDataReader.GetString(address);
                                }

                                var contactName = sqlDataReader.GetOrdinal("ContactName");

                                if (!sqlDataReader.IsDBNull(contactName))
                                {
                                    Organization.ContactName = sqlDataReader.GetString(contactName);
                                }

                                var contactNumber = sqlDataReader.GetOrdinal("ContactNumber");

                                if (!sqlDataReader.IsDBNull(contactNumber))
                                {
                                    Organization.ContactNumber = sqlDataReader.GetString(contactNumber);
                                }

                                var contactEmail = sqlDataReader.GetOrdinal("ContactEmail");

                                if (!sqlDataReader.IsDBNull(contactEmail))
                                {
                                    Organization.ContactEmail = sqlDataReader.GetString(contactEmail);
                                }

                                var loginName = sqlDataReader.GetOrdinal("loginName");

                                if (!sqlDataReader.IsDBNull(loginName))
                                {
                                    Organization.LoginName = sqlDataReader.GetString(loginName);
                                }
                                var loginSecretHash = sqlDataReader.GetOrdinal("loginSecretHash");

                                if (!sqlDataReader.IsDBNull(loginSecretHash))
                                {
                                    Organization.LoginSecretHash = sqlDataReader.GetString(loginSecretHash);
                                }
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

            }
        }

        private async Task Add_Organization()
        {
            // Make SQL Command
            using (SqlCommand command = new SqlCommand("InsertOrganization"))
            {
                command.CommandType = CommandType.StoredProcedure;

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
                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
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

            }
        }

        private async Task Update_Organization()
        {
            // Make SQL Command
            using (SqlCommand command = new SqlCommand("UpdateOrganization"))
            {
                command.CommandType = CommandType.StoredProcedure;

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
                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
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

            }
        }

        private async Task Delete_Organization(int Id)
        {
            // Set ID
            Organization.Id = Id;

            // Make SQL Command
            using (SqlCommand command = new SqlCommand("DeleteOrganization"))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add Mandatory Parameters
                command.Parameters.AddWithValue("@Id", Organization.Id);

                // Manage SQL Connection and Write to DB
                using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
                {
                    try
                    {
                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
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

            }
        }

        private async Task Login_Scanner()
        {
            if (ScannerLogin.Username != null && ScannerLogin.Password != null)
            {
                // Make SQL Command
                using (SqlCommand command = new SqlCommand("orgCheckCredentials"))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    // Add Mandatory Parameters
                    command.Parameters.AddWithValue("@loginName", ScannerLogin.Username.Trim());
                    command.Parameters.AddWithValue("@loginSecretHash", ScannerLogin.Password);

                    // Manage SQL Connection and Write to DB
                    using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
                    {
                        try
                        {
                            sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                            sqlConnection.Open();
                            command.Connection = sqlConnection;
                            SqlDataReader sqlDataReader = command.ExecuteReader();
                            using (sqlDataReader)
                            {
                                if (sqlDataReader.Read())
                                {
                                    // Set Mandatory Values
                                    Organization.Id = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Id"));
                                    Organization.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"));
                                }
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

                }
            }
            else
            {
                throw new BadRequestBodyException("No Searchable Information Found in Request");
            }
        }
        private async Task Update_OrganizationCredentials(OrganizationCredentialInfo organizationCredentialInfo)
        {
            if (organizationCredentialInfo.LoginName != null && organizationCredentialInfo.LoginSecretHash != null)
            {
                // Make SQL Command
                using (SqlCommand command = new SqlCommand("orgAddCredentials"))
                {

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
                            sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                            sqlConnection.Open();
                            command.Connection = sqlConnection;
                            command.ExecuteNonQuery();
                            int returnValue = (int)returnParameter.Value;

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

                }
            }
            else
            {
                throw new BadRequestBodyException("No Searchable Information Found in Request");
            }
        }
        private async Task Get_OrganizationDoors(int Id)
        {
            // Make SQL Command
            using (SqlCommand command = new SqlCommand("orgGetDoors"))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Search Parameters
                command.Parameters.AddWithValue("@orgId", Id);

                // Manage SQL Connection and Write to DB
                using (SqlConnection sqlConnection = new SqlConnection(Config.GetConnectionString("SQLConnectionString")))
                {
                    try
                    {
                        sqlConnection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                        sqlConnection.Open();
                        command.Connection = sqlConnection;
                        SqlDataReader sqlDataReader = command.ExecuteReader();
                        using (sqlDataReader)
                        {
                            var door = sqlDataReader.GetOrdinal("Door");
                            while (sqlDataReader.Read())
                            {
                                // Create New Visitor Object and Set Mandatory Values
                                OrganizationDoor organizationDoor = new OrganizationDoor
                                {
                                    OrganizationId = Id,
                                    DoorName = sqlDataReader.GetString(door)
                                };

                                OrganizationDoors.Add(organizationDoor);
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

            }
        }

        public Guid GetVisitorId()
        {
            return Visitor.Id;
        }

        public async Task<Visitor> GetVisitorLite(Guid Id)
        {
            Visitor = new Visitor();
            await Get_Visitor_Lite(Id);

            if (!Visitor_Found())
            {
                throw new SqlDatabaseDataNotFoundException("Visitor Not Found");
            }

            return Visitor;
        }

        public async Task<Visitor> GetVisitorFull(Guid Id)
        {
            Visitor = new Visitor();
            await Get_Visitor_Full(Id);

            if (!Visitor_Found())
            {
                throw new SqlDatabaseDataNotFoundException("Visitor Not Found");
            }

            return Visitor;
        }

        public async Task<List<Visitor>> GetVisitors(VisitorSearch visitorSearch)
        {
            await Get_Visitors(visitorSearch);

            if (!Visitors_Found())
            {
                throw new SqlDatabaseDataNotFoundException("No Visitors Found");
            }

            return Visitors;
        }

        public async Task AddVisitor()
        {
            await Add_Visitor();
        }

        public async Task UpdateVisitor()
        {
            await Check_Visitor(Visitor.Id);
            await Update_Visitor();
        }

        public async Task DeleteVisitor(Guid Id)
        {
            await Check_Visitor(Id);
            await Delete_Visitor(Id);
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

        public async Task<Organization> GetOrganization(int Id)
        {
            Organization = new Organization();
            await Get_Organization(Id);

            if (!Organization_Found())
            {
                throw new SqlDatabaseDataNotFoundException("Organization Not Found");
            }

            return Organization;
        }

        public async Task AddOrganization()
        {
            Organization.HashLoginSecret();
            await Add_Organization();
        }

        public async Task UpdateOrganization()
        {
            Organization.HashLoginSecret();
            await Check_Organization(Organization.Id);
            await Update_Organization();
        }

        public async Task DeleteOrganization(int Id)
        {
           await Check_Organization(Id);
           await Delete_Organization(Id);
        }

        public async Task<Organization> LoginScanner()
        {
            ScannerLogin.HashPassword();

            Organization = new Organization();

            await Login_Scanner();

            if (!Organization_Found())
            {
                throw new SqlDatabaseDataNotFoundException("Organization Not Found");
            }

            return Organization;
        }
        public async Task UpdateOrganizationCredentials(OrganizationCredentialInfo organizationCredentialInfo)
        {
            organizationCredentialInfo.HashLoginSecret();
            await Check_Organization(organizationCredentialInfo.Id);
            await Update_OrganizationCredentials(organizationCredentialInfo);
        }
        public async Task<List<OrganizationDoor>> GetOrganizationDoors(int Id)
        {
           await Get_OrganizationDoors(Id);

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
