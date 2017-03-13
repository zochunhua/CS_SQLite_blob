using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using Finisar.SQLite;//http://einboch.pixnet.net/blog/post/248187728-c%23%E6%93%8D%E4%BD%9Csqlite%E8%B3%87%E6%96%99%E5%BA%AB


using System.IO;

namespace CS_SQLite_blob
{
    class SQLite
    {
        //--
        //SQLite
        public static String DBpath = "SQLite_Blob.db";//直接修改成相對路徑這樣就可克服中文路徑問題 at 20160905 public static String DBpath = System.Windows.Forms.Application.StartupPath + "\\SYRIS_V8.db";
        public static String StrModifyuid = "";
        public static SQLiteConnection m_icn = new SQLiteConnection();
        public static void initSQLiteDatabase()
        {

            if (!System.IO.File.Exists(DBpath))//偵測不到DB就建立新的
            {
                CreateSQLiteDatabase(DBpath);
                string createtablestring = "";

                createtablestring = "CREATE TABLE Blob_Data (uid INTEGER PRIMARY KEY,name TEXT UNIQUE, data BLOB not null);";
                CreateSQLiteTable(DBpath, createtablestring);

            }
        }
        public static SQLiteConnection OpenConn(string Database)//資料庫連線程式
        {
            string cnstr = string.Format("Data Source=" + Database + ";Version=3;New=False;Compress=True;");
            if (m_icn.State == ConnectionState.Closed)//if (m_icn.State != ConnectionState.Open)
            {
                m_icn.ConnectionString = cnstr;
                m_icn.Open();
            }
            return m_icn;
        }
        public static void CloseConn()
        {
            m_icn.Close();
        }
        public static void CreateSQLiteDatabase(string Database)//建立資料庫程式
        {
            string cnstr = string.Format("Data Source=" + Database + ";Version=3;New=True;Compress=True;");
            SQLiteConnection icn = new SQLiteConnection();
            icn.ConnectionString = cnstr;
            icn.Open();
            icn.Close();
        }
        public static void CreateSQLiteTable(string Database, string CreateTableString)//建立資料表程式
        {
            SQLiteConnection icn = OpenConn(Database);
            SQLiteCommand cmd = new SQLiteCommand(CreateTableString, icn);
            SQLiteTransaction mySqlTransaction = icn.BeginTransaction();
            try
            {
                cmd.Transaction = mySqlTransaction;
                cmd.ExecuteNonQuery();
                mySqlTransaction.Commit();
            }
            catch (Exception ex)
            {
                mySqlTransaction.Rollback();
                throw (ex);
            }
            if (icn.State == ConnectionState.Open) icn.Close();
        }
        public static void SQLiteInsertImge(string Database, string SqlSelectString, byte[] buffer)//新增資料程式
        {
            SQLiteConnection icn = OpenConn(Database);
            SQLiteCommand cmd = new SQLiteCommand(SqlSelectString, icn);
            //SQLiteTransaction mySqlTransaction = icn.BeginTransaction();
            try
            {
                //cmd.Transaction = mySqlTransaction;

                SQLiteParameter para = new SQLiteParameter("@data", DbType.Binary);
                para.Value = buffer;
                cmd.Parameters.Add(para);
                
                cmd.ExecuteNonQuery();
                //mySqlTransaction.Commit();
            }
            catch //(Exception ex)
            {
                //mySqlTransaction.Rollback();
                //throw (ex);
            }
            if (icn.State == ConnectionState.Open) icn.Close();
        }
        public static void SQLiteInsertUpdateDelete(string Database, string SqlSelectString)//新增資料程式
        {
            SQLiteConnection icn = OpenConn(Database);
            SQLiteCommand cmd = new SQLiteCommand(SqlSelectString, icn);
            SQLiteTransaction mySqlTransaction = icn.BeginTransaction();
            try
            {
                cmd.Transaction = mySqlTransaction;
                cmd.ExecuteNonQuery();
                mySqlTransaction.Commit();
            }
            catch //(Exception ex)
            {
                mySqlTransaction.Rollback();
                //throw (ex);
            }
            if (icn.State == ConnectionState.Open) icn.Close();
        }
        public static SQLiteDataReader GetDataReader(string Database, string SQLiteString)//讀取資料程式
        {
            SQLiteConnection icn = OpenConn(Database);
            SQLiteDataAdapter da = new SQLiteDataAdapter(SQLiteString, icn);

            SQLiteCommand sqlite_cmd;
            sqlite_cmd = icn.CreateCommand();// 要下任何命令先取得該連結的執行命令物件
            sqlite_cmd.CommandText = SQLiteString;

            SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();// 執行查詢塞入 sqlite_datareader
            return sqlite_datareader;
        }
        public static DataTable GetDataTable(string Database, string SQLiteString)//讀取資料程式
        {
            DataTable myDataTable = new DataTable();
            SQLiteConnection icn = OpenConn(Database);
            SQLiteDataAdapter da = new SQLiteDataAdapter(SQLiteString, icn);
            DataSet ds = new DataSet();
            ds.Clear();
            da.Fill(ds);

            myDataTable = ds.Tables[0];
            return myDataTable;
        }
        //--
        public static void SQLite_clearDB()//清空資料庫紀錄
        {
            SQLiteInsertUpdateDelete(DBpath, "DELETE FROM users;");
            SQLiteInsertUpdateDelete(DBpath, "DELETE FROM user_ext_group;");
            SQLiteInsertUpdateDelete(DBpath, "DELETE FROM dept;");
            SQLiteInsertUpdateDelete(DBpath, "DELETE FROM door;");
            if (System.IO.File.Exists("AutoSave0.dat"))
            {
                System.IO.File.Delete("AutoSave0.dat");
            }
            if (System.IO.File.Exists("AutoSave1.dat"))
            {
                System.IO.File.Delete("AutoSave1.dat");
            }
        }
        public static void SQLite_Json2DB()//把HTTP資料轉存到DB中
        {
            /*// 把之前的DB全部都先停用-2017/03/07
            if (System.IO.File.Exists("doors.dat"))//門資料料
            {
                StreamReader sr = new StreamReader("doors.dat");
                while (!sr.EndOfStream)// 每次讀取一行，直到檔尾
                {
                    string line = sr.ReadLine();// 讀取文字到 line 變數
                    if (line.Length > 0)
                    {
                        doors_All _lib = JsonConvert.DeserializeObject<doors_All>(line);
                        foreach (doors_data _book in _lib.doors)
                        {
                            //id INTEGER PRIMARY KEY,door_no INTEGER,door_name TEXT,controller_id INTEGER,unlock_time INTEGER,status INTEGER
                            if (string.IsNullOrEmpty(_book.id) == true) _book.id = "NULL";
                            if (string.IsNullOrEmpty(_book.door_no) == true) _book.door_no = "NULL";
                            if (string.IsNullOrEmpty(_book.door_name) == true) _book.door_name = "NULL";
                            if (string.IsNullOrEmpty(_book.controller_id) == true) _book.controller_id = "NULL";
                            if (string.IsNullOrEmpty(_book.unlock_time) == true) _book.unlock_time = "NULL";
                            if (string.IsNullOrEmpty(_book.status) == true) _book.status = "NULL";
                            String StrSQL = String.Format("INSERT INTO door VALUES ({0},{1},'{2}',{3},{4},{5});", _book.id, _book.door_no, _book.door_name, _book.controller_id, _book.unlock_time, _book.status);
                            SQLiteInsertUpdateDelete(DBpath, StrSQL);//"INSERT INTO dept VALUES (1,1,'工廠01',NULL,NULL,0);"
                        }
                    }
                }
                sr.Close();
                //System.IO.File.Delete("doors.dat");
            }

            if (System.IO.File.Exists("depts.dat"))//部門資料
            {
                StreamReader sr = new StreamReader("depts.dat");
                while (!sr.EndOfStream)// 每次讀取一行，直到檔尾
                {
                    string line = sr.ReadLine();// 讀取文字到 line 變數
                    if (line.Length > 0)
                    {
                        depts_All _lib = JsonConvert.DeserializeObject<depts_All>(line);
                        foreach (depts_data _book in _lib.depts)
                        {
                            //id INTEGER PRIMARY KEY,dep_id TEXT,dep_name TEXT,dep_desc TEXT,unit INTEGER,tree_level INTEGER
                            if (string.IsNullOrEmpty(_book.id) == true) _book.id = "NULL";
                            if (string.IsNullOrEmpty(_book.dep_id) == true) _book.dep_id = "NULL";//_book.dep_id = _book.id;//測試用at 2016/09/08 
                            if (string.IsNullOrEmpty(_book.dep_name) == true) _book.dep_name = "NULL";
                            if (string.IsNullOrEmpty(_book.dep_desc) == true) _book.dep_desc = "NULL";
                            if (string.IsNullOrEmpty(_book.unit) == true) _book.unit = "NULL";
                            if (string.IsNullOrEmpty(_book.tree_level) == true) _book.tree_level = "0";//防呆tree_level預設0 at 2016/09/08
                            String StrSQL = String.Format("INSERT INTO dept VALUES ({0},'{1}','{2}','{3}',{4},{5});", _book.id, _book.dep_id, _book.dep_name, _book.dep_desc, _book.unit, _book.tree_level);
                            SQLiteInsertUpdateDelete(DBpath, StrSQL);//"INSERT INTO dept VALUES (1,1,'工廠01',NULL,NULL,0);"
                        }
                    }
                }
                sr.Close();
                //System.IO.File.Delete("depts.dat");
            }

            if (System.IO.File.Exists("users.dat"))
            {
                StreamReader sr = new StreamReader("users.dat");
                while (!sr.EndOfStream)// 每次讀取一行，直到檔尾
                {
                    string line = sr.ReadLine();// 讀取文字到 line 變數
                    if (line.Length > 0)
                    {
                        users_All _lib = JsonConvert.DeserializeObject<users_All>(line);
                        foreach (users_data _book in _lib.users)
                        {
                            //id INTEGER PRIMARY KEY,username TEXT,password TEXT,real_name TEXT,emp_no TEXT,emp_type_id INTEGER,emp_title_id INTEGER,controller_group_id INTEGER,enable INTEGER,gender INTEGER,mobile TEXT,tel TEXT,email TEXT,pic_path TEXT,auth_group_id INTEGER,onboard_date INTEGER,language TEXT,last_login_time INTEGER,last_login_ip TEXT,del INTEGER
                            if (string.IsNullOrEmpty(_book.id) == true) _book.id = "NULL";
                            if (string.IsNullOrEmpty(_book.auth_group_id) == true) _book.auth_group_id = "NULL";
                            if (string.IsNullOrEmpty(_book.controller_group_id) == true) _book.controller_group_id = "NULL";
                            if (string.IsNullOrEmpty(_book.del) == true) _book.del = "NULL";
                            if (string.IsNullOrEmpty(_book.email) == true) _book.email = "NULL";
                            if (string.IsNullOrEmpty(_book.emp_no) == true) _book.emp_no = "NULL";
                            if (string.IsNullOrEmpty(_book.emp_title_id) == true) _book.emp_title_id = "NULL";
                            if (string.IsNullOrEmpty(_book.emp_type_id) == true) _book.emp_type_id = "NULL";
                            if (string.IsNullOrEmpty(_book.enable) == true) _book.enable = "NULL";
                            if (string.IsNullOrEmpty(_book.gender) == true) _book.gender = "NULL";
                            if (string.IsNullOrEmpty(_book.language) == true) _book.language = "NULL";
                            if (string.IsNullOrEmpty(_book.last_login_ip) == true) _book.last_login_ip = "NULL";
                            if (string.IsNullOrEmpty(_book.last_login_time) == true) _book.last_login_time = "NULL";
                            if (string.IsNullOrEmpty(_book.mobile) == true) _book.mobile = "NULL";
                            if (string.IsNullOrEmpty(_book.onboard_date) == true) _book.onboard_date = "NULL";
                            if (string.IsNullOrEmpty(_book.password) == true) _book.password = "NULL";
                            if (string.IsNullOrEmpty(_book.pic_path) == true) _book.pic_path = "NULL";
                            if (string.IsNullOrEmpty(_book.real_name) == true) _book.real_name = "NULL";
                            if (string.IsNullOrEmpty(_book.tel) == true) _book.tel = "NULL";
                            if (string.IsNullOrEmpty(_book.username) == true) _book.username = "NULL";
                            //"CREATE TABLE users (id INTEGER PRIMARY KEY,username TEXT,password TEXT,real_name TEXT,emp_no TEXT,emp_type_id INTEGER,emp_title_id INTEGER,controller_group_id INTEGER,enable INTEGER,gender INTEGER,mobile TEXT,tel TEXT,email TEXT,pic_path TEXT,auth_group_id INTEGER,onboard_date INTEGER,language TEXT,last_login_time INTEGER,last_login_ip TEXT,del INTEGER);";
                            String StrSQL = String.Format("INSERT INTO users VALUES ({0},'{1}','{2}','{3}','{4}',{5},{6},{7},{8},{9},'{10}','{11}','{12}','{13}',{14},{15},'{16}',{17},'{18}',{19});", _book.id, _book.username, _book.password, _book.real_name, _book.emp_no, _book.emp_type_id, _book.emp_title_id, _book.controller_group_id, _book.enable, _book.gender, _book.mobile, _book.tel, _book.email, _book.pic_path, _book.auth_group_id, _book.onboard_date, _book.language, _book.last_login_time, _book.last_login_ip, _book.del);
                            SQLiteInsertUpdateDelete(DBpath, StrSQL);//"INSERT INTO dept VALUES (1,1,'工廠01',NULL,NULL,0);"
                        }
                    }
                }
                sr.Close();
                //System.IO.File.Delete("users.dat");
            }

            if (System.IO.File.Exists("user_ext_group.dat"))
            {
                StreamReader sr = new StreamReader("user_ext_group.dat");
                while (!sr.EndOfStream)// 每次讀取一行，直到檔尾
                {
                    string line = sr.ReadLine();// 讀取文字到 line 變數
                    if (line.Length > 0)
                    {
                        user_ext_group_All _lib = JsonConvert.DeserializeObject<user_ext_group_All>(line);
                        foreach (user_ext_group_data _book in _lib.user_ext_group)
                        {
                            //id INTEGER PRIMARY KEY,user_id INTEGER,dept_id INTEGER,tree_level INTEGER
                            if (string.IsNullOrEmpty(_book.id) == true) _book.id = "NULL";
                            if (string.IsNullOrEmpty(_book.user_id) == true) _book.user_id = "NULL";
                            if (string.IsNullOrEmpty(_book.dept_id) == true) _book.dept_id = "NULL";
                            if (string.IsNullOrEmpty(_book.tree_level) == true) _book.tree_level = "0";//防呆tree_level預設0 at 2016/09/08
                            //"CREATE TABLE users (id INTEGER PRIMARY KEY,username TEXT,password TEXT,real_name TEXT,emp_no TEXT,emp_type_id INTEGER,emp_title_id INTEGER,controller_group_id INTEGER,enable INTEGER,gender INTEGER,mobile TEXT,tel TEXT,email TEXT,pic_path TEXT,auth_group_id INTEGER,onboard_date INTEGER,language TEXT,last_login_time INTEGER,last_login_ip TEXT,del INTEGER);";
                            String StrSQL = String.Format("INSERT INTO user_ext_group VALUES ({0},{1},{2},{3});", _book.id, _book.user_id, _book.dept_id, _book.tree_level);
                            SQLiteInsertUpdateDelete(DBpath, StrSQL);//"INSERT INTO dept VALUES (1,1,'工廠01',NULL,NULL,0);"
                        }
                    }
                }
                sr.Close();
                //System.IO.File.Delete("user_ext_group.dat");
            }
            */
        }
        public static void SQLite_testAdd()//將測試資料寫入DB
        {
            SQLiteInsertUpdateDelete(DBpath, "DELETE FROM users;");
            SQLiteInsertUpdateDelete(DBpath, "DELETE FROM user_ext_group;");
            SQLiteInsertUpdateDelete(DBpath, "DELETE FROM dept;");
            SQLiteInsertUpdateDelete(DBpath, "DELETE FROM door;");

            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO users VALUES (1,NULL,NULL,'小廖',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO users VALUES (2,NULL,NULL,'廖仔',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO users VALUES (3,NULL,NULL,'廖晅晢',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO user_ext_group VALUES (1,1,4,3);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO user_ext_group VALUES (2,1,5,3);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO user_ext_group VALUES (3,2,3,2);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO user_ext_group VALUES (4,3,2,1);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO user_ext_group VALUES (5,1,6,2);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO user_ext_group VALUES (6,3,4,3);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO dept VALUES (1,'1','工廠01',NULL,NULL,0);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO dept VALUES (2,'2','工廠02',NULL,NULL,0);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO dept VALUES (3,'3','研發課',NULL,1,1);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO dept VALUES (4,'4','第一室',NULL,3,2);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO dept VALUES (5,'5','第二室',NULL,3,2);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO dept VALUES (6,'6','會計課',NULL,2,1);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO door VALUES (1,1,'大門',1,NULL,NULL);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO door VALUES (2,2,'後門',1,NULL,NULL);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO door VALUES (3,3,'東門',1,NULL,NULL);");
            SQLiteInsertUpdateDelete(DBpath, "INSERT INTO door VALUES (4,4,'西門',1,NULL,NULL);");
        }
    }
}
