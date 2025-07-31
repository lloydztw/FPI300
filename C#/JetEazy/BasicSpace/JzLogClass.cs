using System;
using System.Drawing;
using System.Windows.Forms;

namespace JetEazy.BasicSpace
{
    public class CommonLogClass
    {
        bool m_IsRecordLog = true;
        RichTextBox richTextBoxRemote = null;
        #region 日志记录、支持其他线程访问  
        public delegate void LogAppendDelegate(Color color, string text);

        public void SetRichTextBox(RichTextBox ertb, bool eIsRecord = true)
        {
            richTextBoxRemote = ertb;
            m_IsRecordLog = eIsRecord;
        }
        /// <summary>  
        /// 追加显示文本  
        /// </summary>  
        /// <param name="color">文本颜色</param>  
        /// <param name="text">显示文本</param>  
        public void LogAppend(Color color, string text)
        {
            if (richTextBoxRemote == null)
                return;
            if (richTextBoxRemote.TextLength >= 20000)
                richTextBoxRemote.Text = "";

            //richTextBoxRemote.AppendText("/n");
            richTextBoxRemote.SelectionColor = color;
            richTextBoxRemote.AppendText(text + Environment.NewLine);

            richTextBoxRemote.SelectionStart = richTextBoxRemote.TextLength;
            richTextBoxRemote.ScrollToCaret();

        }
        /// <summary>  
        /// 显示错误日志  
        /// </summary>  
        /// <param name="text"></param>  
        public void LogError(string text)
        {
            if (m_IsRecordLog)
                _log(text);
            if (richTextBoxRemote == null)
                return;
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, Color.Red, DateTime.Now.ToString("HH:mm:ss ") + text);
        }
        /// <summary>  
        /// 显示警告信息  
        /// </summary>  
        /// <param name="text"></param>  
        public void LogWarning(string text)
        {
            if (m_IsRecordLog)
                _log(text);
            if (richTextBoxRemote == null)
                return;
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, Color.Violet, DateTime.Now.ToString("HH:mm:ss ") + text);
        }
        /// <summary>  
        /// 显示信息  
        /// </summary>  
        /// <param name="text"></param>  
        public void LogMessage(string text)
        {
            if (m_IsRecordLog)
                _log(text);
            if (richTextBoxRemote == null)
                return;
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, Color.Black, DateTime.Now.ToString("HH:mm:ss ") + text);
        }
        /// <summary>  
        /// 显示信息  
        /// </summary>  
        /// <param name="text"></param>  
        public void LogMessage(string text, Color eColor)
        {
            if (m_IsRecordLog)
                _log(text);
            if (richTextBoxRemote == null)
                return;
            text = LanguageExClass.Instance.ToTraditionalChinese(text);
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, eColor, DateTime.Now.ToString("HH:mm:ss.fff ") + text);
        }
        public void LogMessageOK(string text)
        {
            if (m_IsRecordLog)
                _log(text);
            if (richTextBoxRemote == null)
                return;
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, Color.Lime, DateTime.Now.ToString("HH:mm:ss ") + text);
        }
        #endregion

        ListBox m_LstLogMessage = null;
        public void SetListBox(ListBox eLstLogMessage)
        {
            m_LstLogMessage = eLstLogMessage;
        }

        private static readonly CommonLogClass m_log = new CommonLogClass();
        public static CommonLogClass Instance
        {
            get { return m_log; }
        }
        private string m_log_path = Application.StartupPath + @"\log";

        public string LogPath
        {
            set { m_log_path = value; }
        }
        private string m_log_filename = "log_filename";
        public string LogFilename
        {
            set { m_log_filename = value; }
        }

        int ilogCount = 10;
        public void LogListbox(string _message, string strMode = "1")
        {
            //return;//先不记录

            if (m_LstLogMessage != null)
            {
                switch (strMode)
                {
                    default:

                        //SkinListBoxItem _skitem = new SkinListBoxItem("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] " + _message);
                        string _skitem = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] " + _message;

                        m_LstLogMessage.Invoke(new Action(() =>
                        {
                            m_LstLogMessage.Items.Add(_skitem);
                            if (m_LstLogMessage.Items.Count > 5)
                                m_LstLogMessage.SelectedIndex = m_LstLogMessage.Items.Count - 1;
                            if (m_LstLogMessage.Items.Count > ilogCount)
                            {
                                int i = 0;
                                while (i < ilogCount / 2)
                                {
                                    m_LstLogMessage.Items.RemoveAt(0);
                                    i++;
                                }
                            }
                        }));

                        break;
                }
            }

            _log(_message);
        }
        public void Log(string _message, string strExt = ".log")
        {
            _log(_message, strExt);
        }
        private void _log(string strMsg, string strExt = ".log")
        {
            //lock (LAST_RECIPE_NAME)
            //{
            string strPath = m_log_path + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!System.IO.Directory.Exists(strPath))
                System.IO.Directory.CreateDirectory(strPath);

            string strFileName = strPath + DateTime.Now.ToString("yyyyMMdd_HH") + strExt;
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(strFileName, true, System.Text.Encoding.UTF8);
                stm.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                stm.Write(",");
                stm.Write(Application.ProductVersion);
                stm.Write(", ");
                stm.WriteLine(strMsg);
                stm.Flush();
                stm.Close();
                stm.Dispose();
                stm = null;
            }
            catch
            {
            }

            if (stm != null)
                stm.Dispose();
            //}
        }

        public void Log2(string _message, string strExt = ".log")
        {
            _log2(_message, strExt);
        }
        private void _log2(string strMsg, string strExt = ".log")
        {
            //lock (LAST_RECIPE_NAME)
            //{
            string strPath = m_log_path + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!System.IO.Directory.Exists(strPath))
                System.IO.Directory.CreateDirectory(strPath);

            string strFileName = strPath + m_log_filename + strExt;
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(strFileName, true, System.Text.Encoding.UTF8);
                stm.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                stm.Write(",");
                stm.Write(Application.ProductVersion);
                stm.Write(", ");
                stm.WriteLine(strMsg);
                stm.Flush();
                stm.Close();
                stm.Dispose();
                stm = null;
            }
            catch
            {
            }

            if (stm != null)
                stm.Dispose();
            //}
        }
    }
}
