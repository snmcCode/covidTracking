﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;
using Common.Models;
using System.Data;
using Common.Utilities.Exceptions;
using common.Models;
using System.Threading.Tasks;

namespace Common.Utilities
{
    /// <summary>
    /// This micro class is another model to replace the monolothic big databasManager class
    /// This class should only handle the database operations not act as a controller
    /// </summary>

    public class SqlDbManager
    {
        //private vars
        private readonly string _connectionString;
        private readonly LoggerHelper _helper = null;

        public enum DatabaseType
        {
            SQL,
            NoSQL
        }

        private async Task<SqlConnection> getSQLConnection()
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
            return conn;
        }

        public SqlDbManager(DatabaseType dbType, string connectionString, LoggerHelper helper)
        {
            _helper = helper;
            _connectionString = connectionString;
        }

        public async Task<Setting> Settings_Get(Setting s)
        {

            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("settings_Get", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("domain", System.Data.SqlDbType.VarChar, 100);
                    param.Value = s.domain;
                    param = cmd.Parameters.Add("key", System.Data.SqlDbType.VarChar, 50);
                    param.Value = s.key;

                    using (cmd)
                    {
                        SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                        using (reader)
                        {
                            while (reader.Read())
                            {
                                if (reader.IsDBNull("Value"))
                                    s.value = string.Empty;
                                else
                                    s.value = reader.GetString("Value");
                            }
                            reader.Close();
                        }
                    }

                    return s;
                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    _helper.DebugLogger.LogCustomError(e.Message);
                    throw new SqlDatabaseException("A Database Error Occurred",e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }
        }

        public async Task<List<StatusInfo>> GetStatuses()
        {
            List<StatusInfo> statuses = new List<StatusInfo>();
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("status_Get", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();
                    using (sqlDataReader)
                    {
                        var id = sqlDataReader.GetOrdinal("Id");
                        var name = sqlDataReader.GetOrdinal("Name");
                        var bitValue = sqlDataReader.GetOrdinal("BitValue");
               
                        while (await sqlDataReader.ReadAsync())
                        {
                            StatusInfo status = new StatusInfo();


                            // Set Mandatory Values
                            status.Id = sqlDataReader.GetByte(id);
                            status.Name = sqlDataReader.GetString(name);
                            status.BitValue = sqlDataReader.GetInt32(bitValue);
                            statuses.Add(status);
                        }
                    }
                    return statuses;

                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }
        }

        public async Task<Event> addEvent(Event myEvent)
        {

            using (var sqldbConnection = await getSQLConnection())
            {

                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_Create", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;


                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("orgId", System.Data.SqlDbType.Int);
                    param.Value = myEvent.OrgId;
                    param = cmd.Parameters.Add("eventName", System.Data.SqlDbType.NVarChar, 100);
                    param.Value = myEvent.Name;
                    param = cmd.Parameters.Add("eventDateTime", System.Data.SqlDbType.DateTime2, 7);
                    param.Value = myEvent.DateTime;
                    param = cmd.Parameters.Add("hall", System.Data.SqlDbType.NVarChar, 50);
                    param.Value = myEvent.Hall;
                    param = cmd.Parameters.Add("capacity", System.Data.SqlDbType.SmallInt);
                    param.Value = myEvent.Capacity;
                    param = cmd.Parameters.Add("isprivate", System.Data.SqlDbType.Bit);
                    param.Value = myEvent.IsPrivate;
                    param = cmd.Parameters.Add("targetAudience", System.Data.SqlDbType.SmallInt);
                    param.Value = myEvent.TargetAudience;
                    using (cmd)
                    {
                        cmd.Connection = sqldbConnection;
                        await cmd.ExecuteNonQueryAsync();
                        return myEvent;
                    }


                }


                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }

        }

        public async Task<Event> updateEvent(Event myEvent)
        {

            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_Update", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;



                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("id", System.Data.SqlDbType.Int);
                    param.Value = myEvent.Id;
                    param = cmd.Parameters.Add("eventName", System.Data.SqlDbType.NVarChar, 100);
                    param.Value = myEvent.Name;
                    param = cmd.Parameters.Add("eventDateTime", System.Data.SqlDbType.DateTime2, 7);
                    param.Value = myEvent.DateTime;
                    param = cmd.Parameters.Add("hall", System.Data.SqlDbType.NVarChar, 50);
                    param.Value = myEvent.Hall;
                    param = cmd.Parameters.Add("capacity", System.Data.SqlDbType.SmallInt);
                    param.Value = myEvent.Capacity;
                    param = cmd.Parameters.Add("isprivate", System.Data.SqlDbType.Bit);
                    param.Value = myEvent.IsPrivate;
                    param = cmd.Parameters.Add("targetAudience", System.Data.SqlDbType.SmallInt);
                    param.Value = myEvent.TargetAudience;

                    using (cmd)
                    {
                        cmd.Connection = sqldbConnection;
                        await cmd.ExecuteNonQueryAsync();
                        return myEvent;
                    }

                }


                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }
        }


        public async Task<Ticket> PreregisterToEvent(Ticket myticket)
        {

            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_register_user", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;



                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("VisitorId", System.Data.SqlDbType.UniqueIdentifier);
                    param.Value = myticket.visitorId;
                    param = cmd.Parameters.Add("EventId", System.Data.SqlDbType.Int);
                    param.Value = myticket.eventId;


                    using (cmd)
                    {
                        cmd.Connection = sqldbConnection;
                        await cmd.ExecuteNonQueryAsync();
                        return myticket;
                    }

                }


                catch (SqlException e)
                {

                    if (e.Number == 51983)
                    {
                        ApplicationException ex = new ApplicationException("BOOKED_SAME_GROUP");
                        throw ex;
                    }
                    else if (e.Number == 51982)
                    {
                        ApplicationException ex = new ApplicationException("EVENT_FULL");
                        throw ex;
                    }
                    else if (e.Number == 51984)
                    {
                        ApplicationException ex = new ApplicationException("WRONG_AUDIENCE");
                        throw ex;

                    }
                    else if (e.Number== 51985)
                    {
                        ApplicationException ex = new ApplicationException("BLOCKED_USER");
                        throw ex;
                    }
                    else
                    {
                        _helper.DebugLogger.InnerException = e;
                        _helper.DebugLogger.InnerExceptionType = "SqlException";
                        throw new SqlDatabaseException("A Database Error Occurred :" + e);
                    }

                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "Exception";
                    throw new SqlDatabaseException("A non SQL Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }
        }


        public async Task<List<Event>> GetEventsByOrg(int Id, string startDate, string endDate)
        {
            List<Event> Events = new List<Event>();
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_GetByOrg", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("orgId", System.Data.SqlDbType.Int);
                    param.Value = Id;

                    if (!String.IsNullOrEmpty(startDate))
                    {
                        param = cmd.Parameters.Add("startDate", System.Data.SqlDbType.DateTime2);
                        var startDate1 = Convert.ToDateTime(startDate);
                        param.Value = startDate1;

                    }
                    if (!String.IsNullOrEmpty(endDate))
                    {

                        param = cmd.Parameters.Add("endDate", System.Data.SqlDbType.DateTime2);
                        var endDate1 = Convert.ToDateTime(endDate);
                        param.Value = endDate1;
                    }

                    SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();
                    using (sqlDataReader)
                    {
                        var id = sqlDataReader.GetOrdinal("Id");
                        var orgId = sqlDataReader.GetOrdinal("OrgId");
                        var capacity = sqlDataReader.GetOrdinal("Capacity");
                        var name = sqlDataReader.GetOrdinal("Name");
                        var datetime = sqlDataReader.GetOrdinal("DateTime");
                        var hall = sqlDataReader.GetOrdinal("Hall");
                        var isPrivate = sqlDataReader.GetOrdinal("IsPrivate");
                        var bookingCount = sqlDataReader.GetOrdinal("BookingCount");
                        var groupId = sqlDataReader.GetOrdinal("Groupid");
                        var targetAudience = sqlDataReader.GetOrdinal("TargetAudience");

                        while (await sqlDataReader.ReadAsync())
                        {
                            Event myevent = new Event();


                            // Set Mandatory Values
                            myevent.Id = sqlDataReader.GetInt32(id);
                            myevent.OrgId = sqlDataReader.GetInt32(orgId);
                            myevent.Capacity = sqlDataReader.GetInt16(capacity);
                            myevent.Name = sqlDataReader.GetString(name);
                            myevent.DateTime = sqlDataReader.GetDateTime(datetime);
                            myevent.Hall = sqlDataReader.GetString(hall);
                            myevent.IsPrivate = sqlDataReader.GetBoolean(isPrivate);
                            myevent.BookingCount = sqlDataReader.GetInt32(bookingCount);
                            myevent.GroupId = sqlDataReader.GetGuid(groupId);
                            if (sqlDataReader.IsDBNull(targetAudience))
                                myevent.TargetAudience = 0;
                            else
                                myevent.TargetAudience = sqlDataReader.GetInt16(targetAudience);
                            Events.Add(myevent);
                        }
                    }




                    return Events;


                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }

        }




        public async Task<List<ShortEvent>> GetEventsByOrgToday(int Id)
        {
            List<ShortEvent> Events = new List<ShortEvent>();
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_GetByOrgToday", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("orgId", System.Data.SqlDbType.Int);
                    param.Value = Id;


                    SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();
                    using (sqlDataReader)
                    {
                        var id = sqlDataReader.GetOrdinal("Id");
                        var capacity = sqlDataReader.GetOrdinal("Capacity");
                        var name = sqlDataReader.GetOrdinal("Name");
                        var datetime = sqlDataReader.GetOrdinal("DateTime");
                        var hall = sqlDataReader.GetOrdinal("Hall");

                        while (await sqlDataReader.ReadAsync())
                        {
                            ShortEvent myevent = new ShortEvent();


                            // Set Mandatory Values
                            myevent.Id = sqlDataReader.GetInt32(id);
                            myevent.Name = sqlDataReader.GetString(name);
                            myevent.Hall = sqlDataReader.GetString(hall);
                            DateTime fieldDateTime = sqlDataReader.GetDateTime(datetime);
                            myevent.MinuteOfTheDay = fieldDateTime.Hour * 60 + fieldDateTime.Minute;
                            myevent.Capacity = sqlDataReader.GetInt16(capacity);
                            Events.Add(myevent);
                        }
                    }



                    return Events;


                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }

            }
        }




        public async Task<List<UserEvent>> GetEventsByUser(Guid visitorId)
        {
            List<UserEvent> Events = new List<UserEvent>();

            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_GetByUser", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("visitorId", System.Data.SqlDbType.UniqueIdentifier);
                    param.Value = visitorId;


                    SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();
                    using (sqlDataReader)
                    {
                        var id = sqlDataReader.GetOrdinal("Id");
                        var orgId = sqlDataReader.GetOrdinal("OrgId");
                        var organization = sqlDataReader.GetOrdinal("Organization");
                        var eventIndex = sqlDataReader.GetOrdinal("Event");
                        var eventDate = sqlDataReader.GetOrdinal("EventDate");
                        var groupId = sqlDataReader.GetOrdinal("Groupid");
                        var targetAudience = sqlDataReader.GetOrdinal("TargetAudience");

                        while (await sqlDataReader.ReadAsync())
                        {
                            UserEvent myevent = new UserEvent();


                            // Set Mandatory Values
                            myevent.Organization = sqlDataReader.GetString(organization);
                            myevent.Name = sqlDataReader.GetString(eventIndex);
                            myevent.DateTime = sqlDataReader.GetDateTime(eventDate);
                            myevent.Id = sqlDataReader.GetInt32(id);
                            myevent.orgId = sqlDataReader.GetInt32(orgId);
                            myevent.groupId = sqlDataReader.GetGuid(groupId).ToString();
                            if (sqlDataReader.IsDBNull(targetAudience))
                                myevent.targetAudience = 0;
                            else
                                myevent.targetAudience = sqlDataReader.GetInt16(targetAudience);
                            Events.Add(myevent);
                        }
                    }



                    return Events;


                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }

        }



        public async Task DeleteEvent(int eventId)
        {
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_delete", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;



                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("id", System.Data.SqlDbType.Int);
                    param.Value = eventId;



                    using (cmd)
                    {
                        cmd.Connection = sqldbConnection;
                        await cmd.ExecuteNonQueryAsync();

                    }



                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }
        }

        public async Task UnregisterFromEvent(Guid visitorId, int eventId)
        {
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_unregister_user", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;



                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("eventId", System.Data.SqlDbType.Int);
                    param.Value = eventId;
                    param = cmd.Parameters.Add("visitorId", System.Data.SqlDbType.UniqueIdentifier);
                    param.Value = visitorId;



                    using (cmd)
                    {
                        cmd.Connection = sqldbConnection;
                        await cmd.ExecuteNonQueryAsync();

                    }



                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }
        }

        public async Task GroupEvents(List<int> ids)
        {
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_group", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;


                    //CReating Table    
                    DataTable GroupEvents = new DataTable();

                    // Adding Columns    
                    DataColumn COLUMN = new DataColumn();
                    COLUMN.ColumnName = "events";
                    COLUMN.DataType = typeof(int);
                    GroupEvents.Columns.Add(COLUMN);


                    foreach (int id in ids)
                    {
                        DataRow DR = GroupEvents.NewRow();
                        DR[0] = id;

                        GroupEvents.Rows.Add(DR);
                    }


                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("events", System.Data.SqlDbType.Structured);
                    param.Value = GroupEvents;




                    using (cmd)
                    {
                        cmd.Connection = sqldbConnection;
                        await cmd.ExecuteNonQueryAsync();

                    }



                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }
        }


        public async Task<List<Visitor>> GetUsersByEvent(int eventId)
        {
            List<Visitor> visitors = new List<Visitor>();
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_GetBookingByEvent", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("id", System.Data.SqlDbType.Int);
                    param.Value = eventId;

                    SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();
                    using (sqlDataReader)
                    {
                        var visitorIdShort = sqlDataReader.GetOrdinal("VisitorIdShort");
                        var firstName = sqlDataReader.GetOrdinal("FirstName");
                        var registrationTime = sqlDataReader.GetOrdinal("RegistrationTime");

                        while (await sqlDataReader.ReadAsync())
                        {
                            Visitor visitor = new Visitor();

                            // Set Mandatory Values
                            visitor.VisitorIdShort = sqlDataReader.GetString(visitorIdShort);
                            visitor.FirstName = sqlDataReader.GetString(firstName);
                            visitor.registrationTime = sqlDataReader.GetDateTime(registrationTime);
                            visitors.Add(visitor);
                        }
                    }

                    return visitors;
                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }

        }

        public async Task<bool> CheckUserBooking(int eventId, Guid visitorId)
        {
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {
                    bool dbResult = false;

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_CheckUserBooking", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("visitorId", System.Data.SqlDbType.UniqueIdentifier);
                    param.Value = visitorId;

                    param = cmd.Parameters.Add("eventId", System.Data.SqlDbType.Int);
                    param.Value = eventId;

                    param = cmd.Parameters.Add("isBooked", System.Data.SqlDbType.Bit);
                    param.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    dbResult = (bool)cmd.Parameters["isBooked"].Value;

                    return dbResult;
                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }
        }


        public async Task<List<Organization>> GetOrganizations(int? id)
        {
            List<Organization> organizations = new List<Organization>();
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {

                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("getOrganization", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter param = new SqlParameter("Id", SqlDbType.Int);
                    param.Value = id;
                    cmd.Parameters.Add(param);

                    SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();
                    using (sqlDataReader)
                    {
                        var OrgId = sqlDataReader.GetOrdinal("Id");
                        var name = sqlDataReader.GetOrdinal("Name");
                        var address = sqlDataReader.GetOrdinal("Address");
                        var contactName = sqlDataReader.GetOrdinal("ContactName");
                        var contactNumber = sqlDataReader.GetOrdinal("ContactNumber");
                        var contactEmail = sqlDataReader.GetOrdinal("ContactEmail");


                        while (await sqlDataReader.ReadAsync())
                        {
                            StatusInfo status = new StatusInfo();
                            // Set Mandatory Values
                            Organization organization = new Organization();
                            organization.Id = sqlDataReader.GetInt32(OrgId);
                            organization.Name = sqlDataReader.GetString(name);
                            if (!sqlDataReader.IsDBNull(address))
                                organization.Address = sqlDataReader.GetString(address);
                            if (!sqlDataReader.IsDBNull(contactName))
                                organization.ContactName = sqlDataReader.GetString(contactName);
                            if (!sqlDataReader.IsDBNull(contactNumber))
                                organization.ContactNumber = sqlDataReader.GetString(contactNumber);
                            if (!sqlDataReader.IsDBNull(contactEmail))
                                organization.ContactEmail = sqlDataReader.GetString(contactEmail);

                            organizations.Add(organization);
                        }
                    }
                    return organizations;

                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }

        }

        public async Task<bool> SetVisitorStatus(VisitorStatus visitorStatus)
        {
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {
                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("status_setUser", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //paramters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("visitorId", System.Data.SqlDbType.UniqueIdentifier);
                    param.Value = visitorStatus.visitorId;

                    param = cmd.Parameters.Add("orgId", System.Data.SqlDbType.Int);
                    param.Value = visitorStatus.orgId;

                    param = cmd.Parameters.Add("statusValue", System.Data.SqlDbType.Int);
                    param.Value = visitorStatus.statusValue;

                    await cmd.ExecuteNonQueryAsync();

                    return true;

                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }

        }

        public async Task<int> GetVisitorStatus(int orgId, Guid visitorId)
        {
            using (var sqldbConnection = await getSQLConnection())
            {
                try
                {
                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("status_getUser", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //paramters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("orgId", System.Data.SqlDbType.Int);
                    param.Value = orgId;

                    param = cmd.Parameters.Add("visitorId", System.Data.SqlDbType.UniqueIdentifier);
                    param.Value = visitorId;

                    SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();
                    int statusValue = 0;
                    using (sqlDataReader)
                    {
                        var statusValueOrd = sqlDataReader.GetOrdinal("statusValue");

                        while (await sqlDataReader.ReadAsync())
                        {
                            statusValue = sqlDataReader.GetInt32(statusValueOrd);
                        }

                    }
                    return statusValue;

                }
                catch (Exception e)
                {
                    _helper.DebugLogger.InnerException = e;
                    _helper.DebugLogger.InnerExceptionType = "SqlException";
                    throw new SqlDatabaseException("A Database Error Occurred :" + e);
                }
                finally
                {
                    if (sqldbConnection.State == ConnectionState.Open)
                    {
                        sqldbConnection.Close();
                    }
                }
            }
        }
    }

}
