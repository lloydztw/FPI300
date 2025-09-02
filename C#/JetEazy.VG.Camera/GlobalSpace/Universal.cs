using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Data;
using System.Data.OleDb;
using System.Drawing.Imaging;

namespace JetEazy
{
    public class Universal
    {
        public static bool IsDebug = false;
        /// <summary>
        /// 是否开启多线程
        /// </summary>
        public static bool IsMultiThread = true;
        public static bool isRcpUIOKClick = false;
        public static LayoutEnum LAYOUT = LayoutEnum.L1440X900;
        public static int StaticStartNo = 80000;

        public static int MainTimerInterval = 20;
        
        public static string GlobalImageTypeString = ".png";
        public static ImageFormat GlobalImageFormat = ImageFormat.Png;

        //public static string JSONPATH = @"D:\JSON";
        //public static string BACKPATH = @"D:\JSONBAK";
        //public static string TESTPATH = @"D:\LOA";
        //public static string WORKPATH = @"D:\LOA";
        public static string MYDECODE = @"D:\\Jeteazy\\";

        protected static OleDbConnection DATACONNECTION;
        protected static OleDbCommand[] DATACOMMAND;
        protected static OleDbCommandBuilder[] DATACMDBUILDER;
        protected static OleDbDataAdapter[] DATAADAPTER;

        protected static DataSet DATASET;

        public static char SeperateCharA = '\x1e';
        public static char SeperateCharB = '\x1d';
        public static char SeperateCharC = '\x1f';
        public static char SeperateCharD = '\x03';
        public static char SeperateCharE = '\x04';
        public static char SeperateCharF = '\x05';
        public static char SeperateCharG = '\x06';

        public static char NewlineChar = '\x02';
        public static string TESTPATH;

        public static void UpdateTable(string tablename)
        {
            DATACONNECTION.Open();

            DATACMDBUILDER[(int)(DataTableEnum)Enum.Parse(typeof(DataTableEnum), tablename, false)].GetDeleteCommand();
            DATAADAPTER[(int)(DataTableEnum)Enum.Parse(typeof(DataTableEnum), tablename, false)].Update(DATASET, tablename);

            DATACONNECTION.Close();
        }
        public static void DeleteTableRow(string tablename,string deletecommand)
        {
            DATACONNECTION.Open();

            DATACOMMAND[(int)(DataTableEnum)Enum.Parse(typeof(DataTableEnum), tablename, false)].CommandText = deletecommand;
            DATACOMMAND[(int)(DataTableEnum)Enum.Parse(typeof(DataTableEnum), tablename, false)].ExecuteNonQuery();

            DATACONNECTION.Close();
        }
    }
}
