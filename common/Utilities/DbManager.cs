﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;
using Common.Models;
using System.Data;
using Common.Utilities.Exceptions;
using common.Models;

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

        public Setting Settings_Get(Setting s)
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
                        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
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




        public Event addEvent(Event myEvent)
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
                        cmd.ExecuteNonQuery();
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

        public Event updateEvent(Event myEvent)
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
                        cmd.ExecuteNonQuery();
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


        public Ticket PreregisterToEvent(Ticket myticket)
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
                        cmd.ExecuteNonQuery();
                        return myticket;
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


        public List<Event> GetEventsByOrg(int Id)
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


                    SqlDataReader sqlDataReader = cmd.ExecuteReader();

                    while (sqlDataReader.Read())
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




        public List<ShortEvent> GetEventsByOrgToday(int Id)
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


                    SqlDataReader sqlDataReader = cmd.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        ShortEvent myevent = new ShortEvent();


                        // Set Mandatory Values
                        myevent.Id = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Id"));
                        myevent.Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"));
                        myevent.Hall = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Hall"));
                       

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
}
