using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.IO;
using NLog;
using System.Security.Cryptography;

namespace FlexMold.Utility
{
    public static class FlexMoldDB
    {
        public static bool IsEditingEnabled;
        public static SQLiteConnection connectString;
        public static SQLiteCommand cmd;
        public static SQLiteDataAdapter adapter;
        public static DataSet ds = new DataSet();
        public static DataTable dt = new DataTable();
        public static int id;
        public static bool isDoubleClick = false;
        
        public static bool checkAdminPriv(string passkey) {
            //File.ReadAllLines(CSVHelper.GetCurrentDirectoryPath() + "\\FlexMoldUser.connect", Encoding.UTF8);

            //CREATE TABLE "FM_Settings"(
            //    "FM_SaltKey"    TEXT,
            //    "FM_UserName"   TEXT,
            //    "FM_Dept"   TEXT
            //)
            try
            {
                if (connectString.State != ConnectionState.Open)
                    connectString.Open();
                cmd = new SQLiteCommand();
                String sql = "";
                sql = "SELECT FM_SaltKey FROM FM_Settings";
                adapter = new SQLiteDataAdapter(sql, connectString);
                ds.Reset();
                adapter.Fill(ds);
                connectString.Close();

                string saltKey = "";
                if (ds != null)
                {
                    foreach (DataTable dt in ds.Tables)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            saltKey = dr["FM_SaltKey"].ToString();
                            if (saltKey != null && saltKey.Length > 1 && saltKey == passkey)
                            {
                                IsEditingEnabled = true;
                                return true;
                            }
                            else { IsEditingEnabled = false; }
                        }
                    }
                }
                return false;
            }
            catch(Exception ee) { }

            IsEditingEnabled = false;
            return false;
        }


        public static void InitializeFlexMoldDB() {
            //IsEditingEnabled = false;
            var log = LogManager.GetLogger("Device Status");
            try
            {
                String connStr = "Data Source=" + Directory.GetCurrentDirectory() + "\\FlexMold.db;version=3";
                connectString = new SQLiteConnection(connStr);

                //checkAdminPriv("FM_Login");

                log.Log(LogLevel.Info, "Database connected successfully!"); 
            }
            catch
            { log.Log(LogLevel.Error, "Database connection error"); }            
        }

        public static DataSet ReadData(string TableName,string whereclause)
        {
            try
            {
                if (connectString == null)
                    InitializeFlexMoldDB();

                if(connectString.State != ConnectionState.Open)
                    connectString.Open();
                cmd = new SQLiteCommand();
                String sql = "";
                if(whereclause!=null && whereclause.Length>1)
                    sql = "SELECT * FROM " + TableName +" where "+ whereclause;
                else
                    sql = "SELECT * FROM " + TableName;

                adapter = new SQLiteDataAdapter(sql, connectString);
                ds.Reset();
                adapter.Fill(ds);
                //dt = ds.Tables[0];
                //dataGridView1.DataSource = dt;
                connectString.Close();

                return ds;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            return null;
        }

        public static bool UpdateData(string TableName, string whereclause, Dictionary<string, string> parms)
        {
            try
            {
                String parasStr = "";


                foreach (KeyValuePair<string, string> entry in parms)
                {
                    parasStr += entry.Key + "='" + entry.Value+"' ";
                    parasStr += " , ";
                }
                parasStr = parasStr.Remove(parasStr.Length- 2);

                if (connectString.State != ConnectionState.Open)
                    connectString.Open();
                
                cmd = new SQLiteCommand();
                String sql = "";
                if (whereclause != null && whereclause.Length > 1)
                    sql = "UPDATE " + TableName + " SET " + parasStr + " where " + whereclause;
                else
                    sql = "UPDATE " + TableName + " SET " + parasStr;

                adapter = new SQLiteDataAdapter(sql, connectString);
                ds.Reset();
                adapter.Fill(ds);
                //dt = ds.Tables[0];
                //dataGridView1.DataSource = dt;
                connectString.Close();

                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            return false;
        }

        public static bool InsertOrUpdateData(string TableName, string whereclause, Dictionary<string, string> parms)
        {
            try
            {
                if (connectString == null)
                    InitializeFlexMoldDB();

                if (connectString.State != ConnectionState.Open)
                    connectString.Open();
                cmd = new SQLiteCommand();
                String sqlQuery = "";
                if (whereclause != null && whereclause.Length > 1)
                    sqlQuery = "SELECT * FROM " + TableName + " where " + whereclause;
                else
                    sqlQuery = "SELECT * FROM " + TableName;

                adapter = new SQLiteDataAdapter(sqlQuery, connectString);
                ds.Reset();
                adapter.Fill(ds);
                //dt = ds.Tables[0];
                //dataGridView1.DataSource = dt;
                connectString.Close();

                //if record exist then update it else insert it
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    String parasStr = "";

                    foreach (KeyValuePair<string, string> entry in parms)
                    {
                        parasStr += entry.Key + "='" + entry.Value + "' ";
                        parasStr += " , ";
                    }
                    parasStr = parasStr.Remove(parasStr.Length - 2);

                    if (connectString.State != ConnectionState.Open)
                        connectString.Open();

                    cmd = new SQLiteCommand();
                    String sql = "";
                    if (whereclause != null && whereclause.Length > 1)
                        sql = "UPDATE " + TableName + " SET " + parasStr + " where " + whereclause;
                    else
                        sql = "UPDATE " + TableName + " SET " + parasStr;

                    adapter = new SQLiteDataAdapter(sql, connectString);
                    ds.Reset();
                    adapter.Fill(ds);
                    //dt = ds.Tables[0];
                    //dataGridView1.DataSource = dt;
                    connectString.Close();

                    return true;
                }
                else { //insert data
                    String parasStrKeys = "";
                    String parasStrVals = "";

                    foreach (KeyValuePair<string, string> entry in parms)
                    {
                        parasStrKeys += ""+entry.Key + "";
                        parasStrKeys += " , ";
                        
                        parasStrVals += "'" + entry.Value + "'";
                        parasStrVals += " , ";
                    }

                    parasStrKeys = parasStrKeys.Remove(parasStrKeys.Length - 2);
                    parasStrVals = parasStrVals.Remove(parasStrVals.Length - 2);

                    if (connectString.State != ConnectionState.Open)
                        connectString.Open();

                    cmd = new SQLiteCommand();
                    String sql = "";
                    //if (whereclause != null && whereclause.Length > 1)
                        sql = "INSERT INTO " + TableName + " (" + parasStrKeys + ") values( " + parasStrVals + ") ";
                    //else
                    //    sql = "UPDATE " + TableName + " SET " + parasStr;

                    adapter = new SQLiteDataAdapter(sql, connectString);
                    ds.Reset();
                    adapter.Fill(ds);
                    //dt = ds.Tables[0];
                    //dataGridView1.DataSource = dt;
                    connectString.Close();

                    return true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            return false;
        }


        //public static byte[] GetHashKey(string hashKey)
        //{

        //    // Initialize
        //    UTF8Encoding encoder = new UTF8Encoding();
        //    // Get the salt
        //    //string salt = !string.IsNullOrEmpty(Salt) ? Salt : "I am a nice little salt";
        //    string salt = "mykey1_FM";
        //    byte[] saltBytes = encoder.GetBytes(salt);
        //    // Setup the hasher
        //    Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(hashKey, saltBytes);
        //    // Return the key
        //    return rfc.GetBytes(16);
        //}
        //public static string Encrypt(byte[] key, string dataToEncrypt)
        //{
        //    // Initialize
        //    DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider();

        //    cryptic.Key = ASCIIEncoding.ASCII.GetBytes("mykey694");
        //    cryptic.IV = ASCIIEncoding.ASCII.GetBytes("mykey694");

        //    byte[] byteArray = Encoding.UTF8.GetBytes(dataToEncrypt);

        //    CryptoStream crStream = new CryptoStream(new MemoryStream(byteArray),
        //       cryptic.CreateEncryptor(), CryptoStreamMode.Write);


        //    byte[] data = ASCIIEncoding.ASCII.GetBytes(dataToEncrypt);

        //    crStream.Write(data, 0, data.Length);

        //    crStream.Close();
        //    string st = data.ToString();
        //    return st;
        //    //stream.Close();

        //}

        //public static string Decrypt(byte[] key, string encryptedString)
        //{

        //    DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider();

        //    cryptic.Key = ASCIIEncoding.ASCII.GetBytes("mykey694");
        //    cryptic.IV = ASCIIEncoding.ASCII.GetBytes("mykey694");

        //    byte[] byteArray = Encoding.UTF8.GetBytes(encryptedString);

        //    CryptoStream crStream = new CryptoStream(new MemoryStream(byteArray),cryptic.CreateDecryptor(), CryptoStreamMode.Read);
        //    StreamReader reader = new StreamReader(crStream);
        //    string data = reader.ReadToEnd();

        //    reader.Close();
        //    crStream.Close();
        //    return data;
        //}





    }
}
