using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using OpenCvSharp;

namespace JetEazy.BasicSpace
{
    public class JzLanguageClass
    {
        class LanguageItemClass
        {
            public string Name = "";
            string Chinese = "";
            string English = "";

            public LanguageItemClass(string lanstr)
            {
                string[] strs = lanstr.Split('@');

                Name = strs[0];
                Chinese = strs[1];
                English = strs[2];
            }

            public string GetLanguage(int LanguageIndex)
            {
                string Str = "";

                switch (LanguageIndex)
                {
                    case 0:
                        Str = Chinese;
                        break;
                    case 1:
                        Str = English;
                        break;
                }

                return Str;
            }

            public bool SetLanguage(Control K, int LanguageIndex)
            {
                int i = 0;
                string ControlName = K.Name;
                string LanguageStrnig = "";

                bool ret = false;

                LanguageStrnig = GetLanguage(LanguageIndex);

                string[] stmp;

                if ((ControlName.IndexOf("button") > -1) || (ControlName.IndexOf("groupBox") > -1) || (ControlName.IndexOf("tabPage") > -1) || (ControlName.IndexOf("radioButton") > -1) || (ControlName.IndexOf("checkBox") > -1) || (ControlName.IndexOf("pictureBox") > -1 || (ControlName.IndexOf("label") > -1) || (ControlName.IndexOf("comboBox") > -1) || (ControlName.IndexOf("tabControl") > -1)))
                {
                    if (Name.Equals(ControlName))
                    {
                        if (ControlName.IndexOf("comboBox") > -1)
                        {
                            stmp = LanguageStrnig.Split(',');

                            i = 0;
                            ComboBox combo = (ComboBox)K;
                            combo.Items.Clear();
                            while (i < stmp.Length)
                            {
                                combo.Items.Add(stmp[i]);
                                i++;
                            }
                        } 
                        else if (ControlName.IndexOf("tabControl") > -1)
                        {
                            stmp = LanguageStrnig.Split(',');

                            i = 0;
                            TabControl tabcontrol = (TabControl)K;
                                
                            foreach (string str in stmp)
                            {
                                tabcontrol.TabPages[i].Text = str;

                                i++;
                            }

                        }
                        else
                        {
                            K.Text = LanguageStrnig;
                        }

                        ret = true;
                    }

                    //else if (ScreenItem[i].ItemName.Equals("form"))
                    //{
                    //    MyForm.Text = ScreenItem[i].Language[INI.LANGUAGE];
                    //}
                }


                return ret;

            }
        }

        const int ToolTipAutoDelay = 300;

        char mySeparator = '\x1E';
        List<LanguageItemClass> LanguageList = new List<LanguageItemClass>();
        ToolTip Tips = new ToolTip();

        public JzLanguageClass()
        {

        }

        public void Initial(string uidbfile, int LanguageIndex, UserControl myControl)
        {
            if (Initial(uidbfile, LanguageIndex))
                SetControlLanguage(myControl, LanguageIndex);
        }
        public void Initial(string uidbfile, int LanguageIndex, Form myForm)
        {
            if (Initial(uidbfile, LanguageIndex))
                SetControlLanguage(myForm, LanguageIndex);
        }

        public bool Initial(string uidbfile, int LanguageIndex)
        {
            JzToolsClass myJzTools = new JzToolsClass();

            if (!File.Exists(uidbfile))
                return false;

            string Str = "";
            string[] strs;

            LanguageList.Clear();

            myJzTools.ReadData(ref Str, uidbfile);

            Str = Str.Replace(Environment.NewLine, mySeparator.ToString());
            strs = Str.Split(mySeparator);

            foreach (string str in strs)
            {
                LanguageList.Add(new LanguageItemClass(str));
            }
            return true;
        }

        public void SetControlLanguage(UserControl myControl,int languageindex)
        {
            Tips.RemoveAll();
            Tips.AutoPopDelay = ToolTipAutoDelay;

            if (LanguageList.Count < 1)
                return;

            foreach (Control K in myControl.Controls)
            {
                if (!K.Name.IndexOf("UI").Equals(-1))
                    continue;
                CheckLanguage(K,languageindex);
                foreach (Control K1 in K.Controls)
                {
                    if (!K1.Name.IndexOf("UI").Equals(-1))
                        continue;
                    CheckLanguage(K1, languageindex);
                    foreach (Control K2 in K1.Controls)
                    {
                        if (!K2.Name.IndexOf("UI").Equals(-1))
                            continue;
                        CheckLanguage(K2, languageindex);
                        foreach (Control K3 in K2.Controls)
                        {
                            if (!K3.Name.IndexOf("UI").Equals(-1))
                                continue;
                            CheckLanguage(K3, languageindex);
                            foreach (Control K4 in K3.Controls)
                            {
                                if (!K4.Name.IndexOf("UI").Equals(-1))
                                    continue;
                                CheckLanguage(K4, languageindex);
                                foreach (Control K5 in K4.Controls)
                                {
                                    if (!K5.Name.IndexOf("UI").Equals(-1))
                                        continue;
                                    CheckLanguage(K5, languageindex);
                                    foreach (Control K6 in K5.Controls)
                                    {
                                        if (!K6.Name.IndexOf("UI").Equals(-1))
                                            continue;
                                        CheckLanguage(K6, languageindex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void SetControlLanguage(Form myForm, int languageindex)
        {
            Tips.RemoveAll();
            Tips.AutoPopDelay = ToolTipAutoDelay;

            if (LanguageList.Count < 1)
                return;

            CheckLanguage(myForm, languageindex);

            foreach (Control K in myForm.Controls)
            {
                if (!K.Name.IndexOf("UI").Equals(-1))
                    continue;
                CheckLanguage(K, languageindex);
                foreach (Control K1 in K.Controls)
                {
                    if (!K1.Name.IndexOf("UI").Equals(-1))
                        continue;
                    CheckLanguage(K1, languageindex);
                    foreach (Control K2 in K1.Controls)
                    {
                        if (!K2.Name.IndexOf("UI").Equals(-1))
                            continue;
                        CheckLanguage(K2, languageindex);
                        foreach (Control K3 in K2.Controls)
                        {
                            if (!K3.Name.IndexOf("UI").Equals(-1))
                                continue;
                            CheckLanguage(K3, languageindex);
                            foreach (Control K4 in K3.Controls)
                            {
                                if (!K4.Name.IndexOf("UI").Equals(-1))
                                    continue;
                                CheckLanguage(K4, languageindex);
                                foreach (Control K5 in K4.Controls)
                                {
                                    if (!K5.Name.IndexOf("UI").Equals(-1))
                                        continue;
                                    CheckLanguage(K5, languageindex);
                                    foreach (Control K6 in K5.Controls)
                                    {
                                        if (!K6.Name.IndexOf("UI").Equals(-1))
                                            continue;
                                        CheckLanguage(K6, languageindex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        void CheckLanguage(Control K,int LanguageIndex)
        {
            foreach (LanguageItemClass languageitem in LanguageList)
            {
                if (languageitem.SetLanguage(K, LanguageIndex))
                    break;
            }
        }
        void CheckLanguage(Form K, int LanguageIndex)
        {
            foreach (LanguageItemClass languageitem in LanguageList)
            {
                if(languageitem.Name.Equals("Form"))
                {
                    K.Text = languageitem.GetLanguage(LanguageIndex);
                    break;
                }
            }
        }

        public string Messages(string MsgName,int languageindex)
        {
            int i = 0;
            string retStr = "";

            while (i < LanguageList.Count)
            {
                if (LanguageList[i].Name == MsgName)
                {
                    retStr = LanguageList[i].GetLanguage(languageindex);
                    break;
                }
                i++;
            }

            return retStr;
        }
        
    }

    public class LogClass
    {
        private static readonly LogClass m_log = new LogClass();
        public static LogClass Instance
        {
            get { return m_log; }
        }
        private string m_log_path = "D:" + @"\log";
        public string LogPath
        {
            set { m_log_path = value; }
        }
        public void Log(string _message, string strExt = ".log")
        {
            _log(_message, strExt);
        }
        public void LogUserDir(string eLogPath, string _message)
        {
            _logdir(eLogPath, _message);
        }
        private void _logdir(string edir, string strMsg, string strExt = ".log")
        {
            //lock (LAST_RECIPE_NAME)
            //{
            string strPath = edir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\";
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
    }

    public class LanguageExItemClass
    {
        public LanguageExItemClass()
        {

        }

        public int Index = 0;
        public string Name = "ID_NAME";
        public List<string> LanguageList = new List<string>();
        public bool IsShowCode = false;

        /// <summary>
        /// 选择哪一种语言
        /// </summary>
        public int SelectLanguageIndex = 0;

        bool bExist = false;
        public string GetLanguageText(string eCurrentName)
        {
            string retStr = "";
            bExist = false;

            if (eCurrentName.Trim() == Name)
            {
                bExist = true;
            }

            if (!bExist)
            {
                foreach (string str in LanguageList)
                {
                    if (string.IsNullOrEmpty(str))
                        continue;

                    if (eCurrentName.Trim() == str.Trim())
                    {
                        //retStr = str;
                        bExist = true;
                        break;
                    }
                }
            }

            if (!bExist)
            {
                retStr = eCurrentName;
            }
            else
            {
                if (SelectLanguageIndex < LanguageList.Count)
                {
                    if (IsShowCode)
                        retStr = Name;
                    else
                        retStr = LanguageList[SelectLanguageIndex];
                }
                else
                {
                    retStr = Name;
                }
            }

            return retStr;
        }
    }

    public class LanguageExClass
    {
        private static LanguageExClass m_Language = new LanguageExClass();
        public static LanguageExClass Instance
        {
            get
            {
                return m_Language;
            }
        }

        public List<LanguageExItemClass> ControlLanguageList = new List<LanguageExItemClass>();

        private bool m_FirstCsv = false;
        public bool FirstCsv
        {
            get { return m_FirstCsv; }
            set { m_FirstCsv = value; }
        }
        /// <summary>
        /// 选择哪一种语言
        /// </summary>
        private int m_languageIndex = 0;
        public int LanguageIndex
        {
            get { return m_languageIndex; }
            set { m_languageIndex = value; }
        }
        public int Load(string eLanguageFilePath)
        {
            int iret = 0;
            string _filename = eLanguageFilePath + "\\language.csv";

            //文件不存在
            if (!File.Exists(_filename))
                return -1;

            StreamReader sr = new StreamReader(_filename, Encoding.Default);
            string strReadLine = sr.ReadLine();
            while (!sr.EndOfStream)
            {
                strReadLine = sr.ReadLine();
                string[] strs = strReadLine.Split(',');

                LanguageExItemClass _item = new LanguageExItemClass();

                int i = 0;

                if (strs.Length > 3)
                {
                    while (i < strs.Length)
                    {
                        if (!string.IsNullOrEmpty(strs[0]))
                        {
                            if (i == 0)
                                _item.Index = int.Parse(strs[i]);
                            else if (i == 1)
                                _item.Name = strs[i];
                            else if (i == 2)
                                _item.IsShowCode = strs[i] == "1";
                            else
                            {
                                _item.LanguageList.Add(strs[i]);
                            }

                            i++;
                        }
                    }

                    ControlLanguageList.Add(_item);
                }

            }

            sr.Close();
            sr.Dispose();

            return iret;
        }

        public void EnumControls(Control eContainer, bool fromCsv = true)
        {
            if (eContainer is NumericUpDown)
                return;
            if (eContainer is ComboBox)
                return;
            if (eContainer is TextBox)
                return;
            EnumControl(eContainer, fromCsv);
            foreach (Control c in eContainer.Controls)
            {
                EnumControl(eContainer, fromCsv);
                EnumControls(c, fromCsv);//递归的方法
            }
        }
        int m_LanguageIndex = 0;
        private void EnumControl(Control eContainer,bool fromCsv=true)
        {
            //if (m_FirstCsv)
            {
                //if (!eContainer.Visible)
                //    return;
                if (eContainer is NumericUpDown)
                    return;
                if (eContainer is ComboBox)
                    return;
                if (eContainer is TextBox)
                    return;
            }

            if (!string.IsNullOrEmpty(eContainer.Text))
            {
                if (fromCsv)
                {

                    foreach (LanguageExItemClass myItem in ControlLanguageList)
                    {
                        myItem.SelectLanguageIndex = m_languageIndex;
                        eContainer.Text = myItem.GetLanguageText(eContainer.Text);
                    }

                    if (m_FirstCsv)
                    {
                        //第一次生成文件用
                        if (!string.IsNullOrEmpty(eContainer.Text))
                        {
                            if (eContainer.Text[0] != 'A')
                            {
                                string _OrgText = eContainer.Text;
                                eContainer.Text = "A" + m_LanguageIndex.ToString() + "," + _OrgText;
                                m_LanguageIndex++;
                                JetEazy.BasicSpace.LogClass.Instance.Log(eContainer.Text + "," + eContainer.Name, ".csv");
                            }
                        }
                    }
                    ////第一次生成文件用
                    //if (eContainer.Text[0] != 'A')
                    //{
                    //    string _OrgText = eContainer.Text;
                    //    eContainer.Text = "A" + m_LanguageIndex.ToString() + "," + _OrgText;
                    //    m_LanguageIndex++;
                    //    JetEazy.BasicSpace.LogClass.Instance.Log(eContainer.Text, ".csv");
                    //}
                }
                else
                {
                    //繁体

                    eContainer.Text = ToTraditionalChinese(eContainer.Text);
                }


            }
        }

        public string GetLanguageText(string eInputText)
        {
            string ret = eInputText;
            if (!string.IsNullOrEmpty(eInputText))
            {
                foreach (LanguageExItemClass myItem in ControlLanguageList)
                {
                    myItem.SelectLanguageIndex = m_languageIndex;
                    ret = myItem.GetLanguageText(eInputText);
                    if (ret != eInputText)
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 字符串简体转繁体
        /// </summary>
        /// <param name="strSimple"></param>
        /// <returns></returns>
        public string ToTraditionalChinese(string strSimple)
        {

            return GetLanguageText(strSimple.Trim());
            ////string strTraditional = Microsoft.VisualBasic.Strings.StrConv(strSimple, Microsoft.VisualBasic.VbStrConv.TraditionalChinese, 0);
            ////return strTraditional;

            //return JzLangKernel32.ToTraditional(strSimple);
        }

        /// <summary>
        /// 字符串繁体转简体
        /// </summary>
        /// <param name="strTraditional"></param>
        /// <returns></returns>
        public string ToSimplifiedChinese(string strTraditional)
        {
            string strSimple = Microsoft.VisualBasic.Strings.StrConv(strTraditional, VbStrConv.SimplifiedChinese, 0);
            return strSimple;
        }

    }

}
