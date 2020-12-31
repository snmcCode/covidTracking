using System;
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

    class DbManager
    {
        //private vars
        private SqlConnection sqldbConnection = null;
        private Helper _helper = null;

        public enum DatabaseType
        {
            SQL,
            NoSQL
        }

        private SqlConnection getSQLConnection(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
            return conn;
        }

        public DbManager(DatabaseType dbType, string connectionString, Helper helper)
        {
            _helper = helper;
            if (dbType == DatabaseType.SQL)
            {
                sqldbConnection = getSQLConnection(connectionString);
            }

        }

        public async Task<Setting> Settings_Get(Setting s)
        {
            try
            {
                using (sqldbConnection)
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
                        while (reader.Read())
                        {
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
                throw new SqlDatabaseException("A Database Error Occurred");
            }
            finally
            {
                if (sqldbConnection.State == ConnectionState.Open)
                {
                    sqldbConnection.Close();
                }
            }
        }




        public async Task<Event> addEvent(Event myEvent)
        {
            try
            {
                using (sqldbConnection)
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
                    param = cmd.Parameters.Add("capacity", System.Data.SqlDbType.TinyInt);
                    param.Value = myEvent.Capacity;
                    param = cmd.Parameters.Add("isprivate", System.Data.SqlDbType.Bit);
                    param.Value = myEvent.IsPrivate;

                    using (cmd)
                    {
                        cmd.Connection = sqldbConnection;
                        await cmd.ExecuteNonQueryAsync();
                        return myEvent;
                    }


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

        public async Task<Event> updateEvent(Event myEvent)
        {

            try
            {
                using (sqldbConnection)
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
                    param = cmd.Parameters.Add("capacity", System.Data.SqlDbType.TinyInt);
                    param.Value = myEvent.Capacity;
                    param = cmd.Parameters.Add("isprivate", System.Data.SqlDbType.Bit);
                    param.Value = myEvent.IsPrivate;

                    using (cmd)
                    {
                        cmd.Connection = sqldbConnection;
                        await cmd.ExecuteNonQueryAsync();
                        return myEvent;
                    }

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


        public async Task<Ticket> PreregisterToEvent(Ticket myticket)
        {
            try
            {
                using (sqldbConnection)
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

            }
            catch(SqlException e)
            {
                
                if(e.Number== 51983)
                {
                    ApplicationException ex = new ApplicationException("BOOKED_SAME_GROUP");
                    throw ex;
                }
                else if(e.Number== 51982)
                {
                    ApplicationException ex = new ApplicationException("EVENT_FULL");
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


        public async Task<List<Event>> GetEventsByOrg(int Id, string startDate, string endDate)
        {
            List<Event> Events = new List<Event>();

            try
            {
                using (sqldbConnection)
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

                    while (await sqlDataReader.ReadAsync())
                    {
                        Event myevent = new Event();


                        // Set Mandatory Values
                        myevent.Id = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Id"));
                        myevent.OrgId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("OrgId"));
                        myevent.Capacity = sqlDataReader.GetByte(sqlDataReader.GetOrdinal("Capacity"));
                        myevent.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"));
                        myevent.DateTime = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("DateTime"));
                        myevent.Hall = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Hall"));
                        myevent.IsPrivate = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsPrivate"));
                        myevent.BookingCount = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("BookingCount"));
                        myevent.GroupId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("Groupid"));
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




        public async Task<List<ShortEvent>> GetEventsByOrgToday(int Id)
        {
            List<ShortEvent> Events = new List<ShortEvent>();

            try
            {
                using (sqldbConnection)
                {
                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_GetByOrgToday", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("orgId", System.Data.SqlDbType.Int);
                    param.Value = Id;


                    SqlDataReader sqlDataReader =await cmd.ExecuteReaderAsync();

                    while (await sqlDataReader.ReadAsync())
                    {
                        ShortEvent myevent = new ShortEvent();


                        // Set Mandatory Values
                        myevent.Id = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Id"));
                        myevent.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"));
                        myevent.Hall = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Hall"));
                        DateTime fieldDateTime = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("DateTime"));
                        myevent.MinuteOfTheDay = fieldDateTime.Hour * 60 + fieldDateTime.Minute;
                        myevent.Capacity= sqlDataReader.GetByte(sqlDataReader.GetOrdinal("Capacity"));
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




        public async Task<List<UserEvent>> GetEventsByUser(Guid visitorId)
        {
            List<UserEvent> Events = new List<UserEvent>();

            try
            {
                using (sqldbConnection)
                {
                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_GetByUser", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("visitorId", System.Data.SqlDbType.UniqueIdentifier);
                    param.Value = visitorId;


                    SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();

                    while (await sqlDataReader.ReadAsync())
                    {
                        UserEvent myevent = new UserEvent();

                          
                        // Set Mandatory Values
                        myevent.Organization = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Organization"));
                        myevent.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Event"));
                        myevent.DateTime = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("EventDate"));
                        myevent.BookingCount= sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("BookingCount"));
                        myevent.Id = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Id"));
                        myevent.orgId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("OrgId"));
                        myevent.groupId= sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("Groupid")).ToString();
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



        public async void DeleteEvent(int eventId)
        {
            try
            {
                using (sqldbConnection)
                {
                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_delete", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;



                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("id", System.Data.SqlDbType.Int);
                    param.Value =eventId;
                    


                    using (cmd)
                    {
                        cmd.Connection = sqldbConnection;
                        await cmd.ExecuteNonQueryAsync();
                       
                    }

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

        public async void UnregisterFromEvent(Guid visitorId, int  eventId)
        {
            try
            {
                using (sqldbConnection)
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


        public async void GroupEvents(List<int> ids)
        {
            try
            {
                using (sqldbConnection)
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


     public async Task<List<Visitor>> GetUsersByEvent(int eventId)
        {
            List<Visitor> visitors = new List<Visitor>();

            try
            {
                using (sqldbConnection)
                {
                    sqldbConnection.Open();
                    SqlCommand cmd = new SqlCommand("event_GetBookingByEvent", sqldbConnection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //parameters
                    SqlParameter param = null;
                    param = cmd.Parameters.Add("id", System.Data.SqlDbType.Int);
                    param.Value = eventId;

                    SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();

                    while (await sqlDataReader.ReadAsync())
                    {
                        Visitor visitor = new Visitor();

                        // Set Mandatory Values
                        visitor.VisitorIdShort = sqlDataReader.GetString(sqlDataReader.GetOrdinal("VisitorIdShort"));
                        visitor.FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName"));
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

}
