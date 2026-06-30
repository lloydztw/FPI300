using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JetEazy.BasicSpace
{
    public class JzLanguageClass
    {
        class LanguageItemClass
        {
            public string Name = "";
            //string Chinese = "";
            //string English = "";

            public LanguageItemClass(string lanstr)
            {
                //string[] strs = lanstr.Split('@');

                //Name = strs[0];
                //Chinese = strs[1];
                //English = strs[2];
            }

            public string GetLanguage(int LanguageIndex)
            {
                //string Str = "";

                //switch (LanguageIndex)
                //{
                //    case 0:
                //        Str = Chinese;
                //        break;
                //    case 1:
                //        Str = English;
                //        break;
                //}

                //return Str;
                return "Chinese";
            }

            public bool SetLanguage(Control K, int LanguageIndex)
            {
                //int i = 0;
                //string ControlName = K.Name;
                //string LanguageStrnig = "";

                //bool ret = false;

                //LanguageStrnig = GetLanguage(LanguageIndex);

                //string[] stmp;

                //if ((ControlName.IndexOf("button") > -1) || (ControlName.IndexOf("groupBox") > -1) || (ControlName.IndexOf("tabPage") > -1) || (ControlName.IndexOf("radioButton") > -1) || (ControlName.IndexOf("checkBox") > -1) || (ControlName.IndexOf("pictureBox") > -1 || (ControlName.IndexOf("label") > -1) || (ControlName.IndexOf("comboBox") > -1) || (ControlName.IndexOf("tabControl") > -1)))
                //{
                //    if (Name.Equals(ControlName))
                //    {
                //        if (ControlName.IndexOf("comboBox") > -1)
                //        {
                //            stmp = LanguageStrnig.Split(',');

                //            i = 0;
                //            ComboBox combo = (ComboBox)K;
                //            combo.Items.Clear();
                //            while (i < stmp.Length)
                //            {
                //                combo.Items.Add(stmp[i]);
                //                i++;
                //            }
                //        } 
                //        else if (ControlName.IndexOf("tabControl") > -1)
                //        {
                //            stmp = LanguageStrnig.Split(',');

                //            i = 0;
                //            TabControl tabcontrol = (TabControl)K;

                //            foreach (string str in stmp)
                //            {
                //                tabcontrol.TabPages[i].Text = str;

                //                i++;
                //            }

                //        }
                //        else
                //        {
                //            K.Text = LanguageStrnig;
                //        }

                //        ret = true;
                //    }

                //    //else if (ScreenItem[i].ItemName.Equals("form"))
                //    //{
                //    //    MyForm.Text = ScreenItem[i].Language[INI.LANGUAGE];
                //    //}
                //}


                //return ret;

                return false;
            }
        }

        public JzLanguageClass()
        {

        }

        public void Initial(string uidbfile, int LanguageIndex, UserControl myControl)
        {
        }
        public void Initial(string uidbfile, int LanguageIndex, Form myForm)
        {
        }
        public bool Initial(string uidbfile, int LanguageIndex)
        {
            return false;
        }
        public void SetControlLanguage(UserControl myControl,int languageindex)
        {         
        }
        public void SetControlLanguage(Form myForm, int languageindex)
        {
        }
        void CheckLanguage(Control K,int LanguageIndex)
        {
        }
        void CheckLanguage(Form K, int LanguageIndex)
        {
        }
        public string Messages(string MsgName,int languageindex)
        {
            return MsgName;
        }
        
    }

    public class LogClass
    {
        private static readonly LogClass m_log = new LogClass();
        public static LogClass Instance
        {
            get { return m_log; }
        }
        public string LogPath
        {
            get;
            set;
        }
        public void Log(string _message, string strExt = ".log")
        {
        }
        public void LogUserDir(string eLogPath, string _message)
        {
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
        public string GetLanguageText(string eCurrentName)
        {
            return eCurrentName;
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
            //int iret = 0;
            //string _filename = eLanguageFilePath + "\\language.csv";

            ////文件不存在
            //if (!File.Exists(_filename))
            //    return -1;

            //StreamReader sr = new StreamReader(_filename, Encoding.Default);
            //string strReadLine = sr.ReadLine();
            //while (!sr.EndOfStream)
            //{
            //    strReadLine = sr.ReadLine();
            //    string[] strs = strReadLine.Split(',');

            //    LanguageExItemClass _item = new LanguageExItemClass();

            //    int i = 0;

            //    if (strs.Length > 3)
            //    {
            //        while (i < strs.Length)
            //        {
            //            if (!string.IsNullOrEmpty(strs[0]))
            //            {
            //                if (i == 0)
            //                    _item.Index = int.Parse(strs[i]);
            //                else if (i == 1)
            //                    _item.Name = strs[i];
            //                else if (i == 2)
            //                    _item.IsShowCode = strs[i] == "1";
            //                else
            //                {
            //                    _item.LanguageList.Add(strs[i]);
            //                }

            //                i++;
            //            }
            //        }

            //        ControlLanguageList.Add(_item);
            //    }

            //}

            //sr.Close();
            //sr.Dispose();

            //return iret;
            return -1;
        }

        public void EnumControls(Control eContainer, bool fromCsv = true)
        {
            //if (eContainer is NumericUpDown)
            //    return;
            //if (eContainer is ComboBox)
            //    return;
            //if (eContainer is TextBox)
            //    return;
            //EnumControl(eContainer, fromCsv);
            //foreach (Control c in eContainer.Controls)
            //{
            //    EnumControl(eContainer, fromCsv);
            //    EnumControls(c, fromCsv);//递归的方法
            //}
        }
        public string GetLanguageText(string eInputText)
        {
            return eInputText;
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
