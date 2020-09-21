using System;
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

        public DbManager(DatabaseType dbType,string connectionString,Helper helper)
        {
            _helper = helper;
            if(dbType==DatabaseType.SQL)
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
                if(sqldbConnection.State==ConnectionState.Open)
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





    }
}
