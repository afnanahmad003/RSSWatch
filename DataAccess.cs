using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace RESSWATCH
{
    public abstract class DataAccess
    {
        private static string conStr = string.Empty;

        public static string CON_STR
        {
            get
            {
                return conStr;
            }
            set
            {

                conStr = value;
            }
        }

        public static bool DatabaseAvailable
        {
            get
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    try
                    {
                        bool isAvailable = false;
                        conn.Open();
                        isAvailable = true;
                        conn.Close();
                        return isAvailable;
                    }
                    catch (SqlException ex)
                    {
                        return false;
                    }
                }
            }
        }

        public static int ExecuteNonQuery(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();
            ApplicationLogger.Logger.WriteLog(commandText, ApplicationLogger.LogLevel.DBLOG);
            using (SqlConnection conn = new SqlConnection(conStr))
            {
                try
                {
                    PrepareCommand(cmd, conn, null, commandType, commandText, commandParameters);
                    int val = cmd.ExecuteNonQuery();

                    cmd.Parameters.Clear();
                    return val;
                }
                catch (SqlException ex)
                {
                    throw ex;
                }
            }
        }

        public static int ExecuteNonQuery(SqlConnection conn, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            ApplicationLogger.Logger.WriteLog(commandText, ApplicationLogger.LogLevel.DBLOG);
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, conn, null, commandType, commandText, commandParameters);

            try
            {
                int val = cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int ExecuteNonQuery(SqlTransaction trans, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            ApplicationLogger.Logger.WriteLog(commandText, ApplicationLogger.LogLevel.DBLOG);
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, commandType, commandText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        public static SqlDataReader ExecuteReader(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(conStr);

            try
            {
                PrepareCommand(cmd, conn, null, commandType, commandText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch// (Exception ex)
            {
                conn.Close();
                throw;//ex;
            }
        }

        public static object ExecuteScalar(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            using (SqlConnection conn = new SqlConnection(conStr))
            {
                PrepareCommand(cmd, conn, null, commandType, commandText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        public static object ExecuteDataSet(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            ApplicationLogger.Logger.WriteLog(commandText, ApplicationLogger.LogLevel.DBLOG);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adpt = new SqlDataAdapter();
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(conStr))
            {
                PrepareCommand(cmd, conn, null, commandType, commandText, commandParameters);
                adpt.SelectCommand = cmd;
                adpt.Fill(ds);
                cmd.Parameters.Clear();
                return ds;
            }
        }

        public static object ExecuteScalar(SqlConnection conn, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();

            PrepareCommand(cmd, conn, null, commandType, commandText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {

            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        public static SqlParameter CreateParameter(System.String name, System.Data.SqlDbType dbType, System.Object value)
        {

            SqlParameter prm = new SqlParameter(name, dbType);
            prm.Value = value == null ? DBNull.Value : value;
            return prm;

        }
    }
}