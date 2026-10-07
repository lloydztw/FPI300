using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace JetEazy.BasicSpace
{
    [Serializable]
    public class JzCheckRepeatClass
    {
        protected JzCheckRepeatClass()
        {

        }
        private static JzCheckRepeatClass _instance = null;
        public static JzCheckRepeatClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new JzCheckRepeatClass();
                return _instance;
            }
        }

        public string mysql_server_ip { get; set; } = "localhost";
        public int mysql_server_port { get; set; } = 3306;

        public string mysql_server_user { get; set; } = "root";
        public string mysql_server_pwd { get; set; } = "12892414";
        public string mysql_server_db { get; set; } = "mainsd";

        /// <summary>
        /// 报表批号
        /// </summary>
        public string Report_LOT { get; set; } = "none";

        #region 重复码检查

        CommonLogClass m_log = new CommonLogClass();

        MySqlConnection sqlCnt = null;
        MySqlCommand cmd = null;

        private string getReportLot()
        {
            if (string.IsNullOrEmpty(Report_LOT))
                return "NONE";
            return Report_LOT;
        }
        /// <summary>
        /// 设定log档路径
        /// </summary>
        /// <param name="ePath">输入路径</param>
        public void SetLogPath(string ePath = "D:\\log\\logRepeatCode")
        {
            m_log.LogPath = ePath;
        }

        /*单个插入数据停止使用 可能影响性能
        public bool MySqlCheckTableExist()
        {
            bool iret = true;

            MySqlConnection sqlCnt = null;
            MySqlCommand cmd = null;

            try
            {
                sqlCnt = new MySqlConnection();
                string ConnectionString = "server=127.0.0.1;port=3306;user=root;password=12892414; database=mainsd;";
                ConnectionString = "server=" + mysql_server_ip +
                                                   ";port=" + mysql_server_port.ToString() +
                                                   ";user=" + mysql_server_user +
                                                   ";password=" + mysql_server_pwd +
                                                   ";database=" + mysql_server_db + ";";

                sqlCnt.ConnectionString = ConnectionString;
                sqlCnt.Open();

                string table_name = $"jztb_{getReportLot()}";
                string sql = $@" SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table_name}'";
                m_log.Log2("sql=" + sql);
                cmd = new MySqlCommand(sql, sqlCnt);
                //iret = cmd.ExecuteNonQuery();
                object result = cmd.ExecuteScalar(); // 执行查询并返回第一行的第一列
                iret = result != null;

                m_log.Log2("result=" + iret.ToString());

            }
            catch (Exception ex)
            {
                m_log.Log2(ex.Message);
                //m_log.Log2(ex.StackTrace);
                //m_log.Log2(ex.Source);

                iret = false;
            }

            if (sqlCnt != null)
            {
                sqlCnt.Close();
                sqlCnt.Dispose();
                sqlCnt = null;
            }
            if (cmd != null)
            {
                cmd.Dispose();
                cmd = null;
            }
            return iret;
        }
        /// <summary>
        /// 建立批号的数据表
        /// </summary>
        /// <returns>>=0则是建立成功  <0则是建立失败</returns>
        public int MySqlCreateTable()
        {
            int iret = 0;

            MySqlConnection sqlCnt = null;
            MySqlCommand cmd = null;

            try
            {
                sqlCnt = new MySqlConnection();
                string ConnectionString = "server=127.0.0.1;port=3306;user=root;password=12892414; database=mainsd;";
                ConnectionString = "server=" + mysql_server_ip +
                                                   ";port=" + mysql_server_port.ToString() +
                                                   ";user=" + mysql_server_user +
                                                   ";password=" + mysql_server_pwd +
                                                   ";database=" + mysql_server_db + ";";

                sqlCnt.ConnectionString = ConnectionString;
                sqlCnt.Open();

                string table_name = $"jztb_{getReportLot()}";
                string sql = $"CREATE TABLE IF NOT EXISTS {table_name} (id INT AUTO_INCREMENT COMMENT '序号'," +
                    $"b01 VARCHAR(30) NOT NULL COMMENT '条码'," +
                     $"b02 DATETIME COMMENT '时间'," +
  $"b03 TEXT COMMENT '备注'," +
  $"PRIMARY KEY(id) ," +
  $"UNIQUE INDEX(id)" +
  $");";
                m_log.Log2("sql=" + sql);
                cmd = new MySqlCommand(sql, sqlCnt);
                iret = cmd.ExecuteNonQuery();
                m_log.Log2("result=" + iret.ToString());

            }
            catch (Exception ex)
            {
                m_log.Log2(ex.Message);
                //m_log.Log2(ex.StackTrace);
                //m_log.Log2(ex.Source);

                iret = -1;
            }

            if (sqlCnt != null)
            {
                sqlCnt.Close();
                sqlCnt.Dispose();
                sqlCnt = null;
            }
            if (cmd != null)
            {
                cmd.Dispose();
                cmd = null;
            }
            return iret;
        }
        /// <summary>
        /// 插入数据库
        /// </summary>
        /// <param name="eBarcodeStr">插入的条码</param>
        /// <returns>>=0则是插入完成 <0则是插入失败 </returns>
        public int MySqlTableInsert(string eBarcodeStr)
        {
            int iret = 0;
            MySqlConnection sqlCnt = null;
            MySqlCommand cmd = null;

            try
            {
                sqlCnt = new MySqlConnection();
                string ConnectionString = "server=127.0.0.1;port=3306;user=root;password=12892414; database=mainsd;";
                ConnectionString = "server=" + mysql_server_ip +
                                                   ";port=" + mysql_server_port.ToString() +
                                                   ";user=" + mysql_server_user +
                                                   ";password=" + mysql_server_pwd +
                                                   ";database=" + mysql_server_db + ";";

                sqlCnt.ConnectionString = ConnectionString;
                sqlCnt.Open();

                string table_name = $"jztb_{getReportLot()}";
                string sql = "INSERT INTO " + table_name +
                                  "(" + "b01,b02) VALUES ('" + eBarcodeStr + "','" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "')";

                m_log.Log2("sql=" + sql);
                cmd = new MySqlCommand(sql, sqlCnt);
                iret = cmd.ExecuteNonQuery();
                m_log.Log2("result=" + iret.ToString());
            }
            catch (Exception ex)
            {
                m_log.Log2(ex.Message);
                //m_log.Log2(ex.StackTrace);
                //m_log.Log2(ex.Source);

                iret = -1;
            }

            if (sqlCnt != null)
            {
                sqlCnt.Close();
                sqlCnt.Dispose();
                sqlCnt = null;
            }
            if (cmd != null)
            {
                cmd.Dispose();
                cmd = null;
            }
            return iret;
        }
        /// <summary>
        /// 检查是否在数据表中有重复码
        /// </summary>
        /// <param name="eBarcodeStr">检查的条码</param>
        /// <returns>大于0则重复</returns>
        public int MySqlTableQuery(string eBarcodeStr)
        {
            int iret = 0;
            MySqlConnection sqlCnt = null;
            MySqlCommand cmd = null;

            try
            {
                sqlCnt = new MySqlConnection();
                string ConnectionString = "server=127.0.0.1;port=3306;user=root;password=12892414; database=mainsd;";
                ConnectionString = "server=" + mysql_server_ip +
                                                   ";port=" + mysql_server_port.ToString() +
                                                   ";user=" + mysql_server_user +
                                                   ";password=" + mysql_server_pwd +
                                                   ";database=" + mysql_server_db + ";";

                sqlCnt.ConnectionString = ConnectionString;
                sqlCnt.Open();

                string table_name = $"jztb_{getReportLot()}";
                string sql = $"SELECT count(*) FROM {table_name} WHERE b01='{eBarcodeStr}'";


                m_log.Log2("sql=" + sql);
                cmd = new MySqlCommand(sql, sqlCnt);
                object result = cmd.ExecuteScalar(); // 执行查询并返回第一行的第一列
                //iret = result != null;
                if (result != null)
                {
                    //iret = (int)result;
                    //m_log.Log2("result=" + result.ToString());
                    int.TryParse(result.ToString(), out iret);
                }
                //iret = cmd.ExecuteNonQuery();
                m_log.Log2("result=" + iret.ToString());
            }
            catch (Exception ex)
            {
                m_log.Log2(ex.Message);
                //m_log.Log2(ex.StackTrace);
                //m_log.Log2(ex.Source);

                iret = -1;
            }

            if (sqlCnt != null)
            {
                sqlCnt.Close();
                sqlCnt.Dispose();
                sqlCnt = null;
            }
            if (cmd != null)
            {
                cmd.Dispose();
                cmd = null;
            }
            return iret;
        }
        */

        #region 一次性插入检查重复码

        public bool OpenDB()
        {
            bool iret = true;
            if (sqlCnt == null)
                sqlCnt = new MySqlConnection();
            string ConnectionString = "server=127.0.0.1;port=3306;user=root;password=12892414; database=mainsd;";
            ConnectionString = "server=" + mysql_server_ip +
                                               ";port=" + mysql_server_port.ToString() +
                                               ";user=" + mysql_server_user +
                                               ";password=" + mysql_server_pwd +
                                               ";database=" + mysql_server_db + ";";

            sqlCnt.ConnectionString = ConnectionString;
            m_log.Log2($"open_db {ConnectionString}");
            try
            {
                sqlCnt.Open();
            }
            catch (Exception ex)
            {
                m_log.Log2(ex.Message);
                //m_log.Log2(ex.StackTrace);
                //m_log.Log2(ex.Source);

                iret = false;
            }
            return iret;
        }
        public void CloseDB()
        {
            if (sqlCnt != null)
            {
                sqlCnt.Close();
                sqlCnt.Dispose();
                sqlCnt = null;
            }
            if (cmd != null)
            {
                cmd.Dispose();
                cmd = null;
            }
        }
        public bool MySqlCheckTableExist()
        {
            bool iret = true;
            try
            {
                string table_name = $"jztb_{getReportLot()}";
                string sql = $@" SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table_name}'";
                m_log.Log2("sql=" + sql);
                cmd = new MySqlCommand(sql, sqlCnt);
                object result = cmd.ExecuteScalar(); // 执行查询并返回第一行的第一列
                iret = result != null;
                m_log.Log2("result=" + iret.ToString());
            }
            catch (Exception ex)
            {
                m_log.Log2(ex.Message);
                //m_log.Log2(ex.StackTrace);
                //m_log.Log2(ex.Source);

                iret = false;
            }
            return iret;
        }
        /// <summary>
        /// 建立批号的数据表
        /// </summary>
        /// <returns>>=0则是建立成功  <0则是建立失败</returns>
        public int MySqlCreateTable()
        {
            int iret = 0;
            try
            {
                string table_name = $"jztb_{getReportLot()}";
                string sql = $"CREATE TABLE IF NOT EXISTS {table_name} (" +
                    $"b01 VARCHAR(30) NOT NULL COMMENT '条码'," +
                     $"b02 TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '时间'," +
  $"PRIMARY KEY(b01) " +
  $");";
                m_log.Log2("sql=" + sql);
                cmd = new MySqlCommand(sql, sqlCnt);
                iret = cmd.ExecuteNonQuery();
                m_log.Log2("result=" + iret.ToString());

            }
            catch (Exception ex)
            {
                m_log.Log2(ex.Message);
                //m_log.Log2(ex.StackTrace);
                //m_log.Log2(ex.Source);

                iret = -1;
            }
            return iret;
        }
        /// <summary>
        /// 插入数据库
        /// </summary>
        /// <param name="eBarcodeStr">插入的条码</param>
        /// <returns>>=0则是插入完成 <0则是插入失败 </returns>
        public int MySqlTableInsert(List<string> eBarcodeList)
        {
            int iret = 0;

            try
            {
                if (eBarcodeList.Count == 0)
                    return iret;

                string _barcodeStr = string.Empty;
                foreach (string eBarcode in eBarcodeList)
                {
                    _barcodeStr += $"('{eBarcode}'),";
                }
                _barcodeStr = _barcodeStr.Remove(_barcodeStr.Length - 1, 1);

                string table_name = $"jztb_{getReportLot()}";
                string sql = "INSERT INTO " + table_name +
                                  "(b01) VALUES " + _barcodeStr;

                m_log.Log2("sql=" + sql);
                cmd = new MySqlCommand(sql, sqlCnt);
                iret = cmd.ExecuteNonQuery();
                m_log.Log2("result=" + iret.ToString());
            }
            catch (Exception ex)
            {
                m_log.Log2(ex.Message);
                //m_log.Log2(ex.StackTrace);
                //m_log.Log2(ex.Source);

                iret = -1;
            }
            return iret;
        }
        /// <summary>
        /// 检查是否在数据表中有重复码
        /// </summary>
        /// <param name="eBarcodeStr">检查的条码</param>
        /// <returns>大于0则重复</returns>
        public int MySqlTableQuery(List<string> eBarcodeList, ref List<string> refrepeatbarcode)
        {
            int iret = 0;

            try
            {
                refrepeatbarcode.Clear();
                string repeatStr = string.Empty;
                if (eBarcodeList.Count == 0)
                    return iret;

                string _barcodeStr = string.Empty;
                foreach (string eBarcode in eBarcodeList)
                {
                    _barcodeStr += $"'{eBarcode}',";
                }
                _barcodeStr = _barcodeStr.Remove(_barcodeStr.Length - 1, 1);

                string table_name = $"jztb_{getReportLot()}";
                string sql = $"SELECT b01 FROM {table_name} WHERE b01 IN ({_barcodeStr})";

                m_log.Log2("sql=" + sql);
                cmd = new MySqlCommand(sql, sqlCnt);
                //MySqlDataReader _dataReader = cmd.ExecuteReader(); // 执行查询并返回第一行的第一列
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) // 循环读取每一行数据
                    {
                        string name = reader["b01"].ToString();
                        //Console.WriteLine("Name: " + name); // 打印值或做其他处理
                        refrepeatbarcode.Add(name);
                        repeatStr += name + ",";
                    }
                }
                iret = refrepeatbarcode.Count;
                //iret = cmd.ExecuteNonQuery();
                m_log.Log2("result=" + iret.ToString());
                if (iret > 0)
                    m_log.Log2("repeat=" + repeatStr);
            }
            catch (Exception ex)
            {
                m_log.Log2(ex.Message);
                //m_log.Log2(ex.StackTrace);
                //m_log.Log2(ex.Source);

                iret = -1;
            }
            return iret;
        }

        #endregion

        #endregion
    }
}
