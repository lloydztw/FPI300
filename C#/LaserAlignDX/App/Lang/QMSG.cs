#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-22 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Windows.Forms;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;
using GlobalConfig = LaserAlignDX.GlobalConfig;

namespace JetEazy.Lang
{
    public partial class QMSG
    {
        public static string TITLE => GlobalConfig.TITLE;
        static string LANG_PATH
        {
            get
            {
                return System.IO.Path.Combine(GlobalConfig.APP_ROOT_PATH, "Ini", "language");
            }
        }

        #region LANGUAGE
        static QxLang _lang = QxLang.Instance("prompts", LANG_PATH);
        static QxLang _langGui = QxLang.Instance("gui", LANG_PATH);
        static QxLang _langRcp = QxLang.Instance("rcp", LANG_PATH);
        //static QMsgStateTranslator _stateTranslator = new QMsgStateTranslator();
        #endregion

        public static QxLang Lang(string langPack = "prompts")
        {
            return langPack == "prompts" ? _lang : QxLang.Instance(langPack);
        }
        public static void Dump(Control gui, int delay = 0)
        {
            if (delay <= 0)
            {
                _langGui.Dump(gui);
            }
            else
            {
                new Action(() =>
                {
                    System.Threading.Thread.Sleep(delay);
                    gui.BeginInvoke(new Action(() => _langGui.Dump(gui)));
                }).BeginInvoke(null, null);
            }
        }
        public static void Translate(Control gui, int delay = 0)
        {
            if (delay <= 0)
            {
                _langGui.Translate(gui);
            }
            else
            {
                new Action(() =>
                {
                    System.Threading.Thread.Sleep(delay);
                    gui.BeginInvoke(new Action(() => _langGui.Translate(gui)));
                }).BeginInvoke(null, null);
            }
        }

        /// <summary>
        /// 多語系翻譯 (to translate 'model state' to display text)
        /// </summary>
        public static string HardTranslate(object state, out bool isRecoveringFromErr)
        {
            //return _stateTranslator.Translate(state, out isRecoveringFromErr);
            isRecoveringFromErr = false;
            return state?.ToString();
        }
        public static string Text(string text, string langPack = "prompts")
        {
            var lang = Lang(langPack);
            return lang.Translate(text);
        }
        public static string Text(Enum prompt, params string[] args)
        {
            //>>> var _lang = QxLang.Instance("prompts");
            //>>> string msg = _lang.Translate("STR", (int)prompt, prompt);
            var msg = _lang.Translate(prompt);
            if (args.Length > 0)
                msg += SmartTranslate(_lang, args);
            return msg;
        }
        public static string Text(ErrorCodes err, params string[] args)
        {
            var _lang = QxLang.Instance("errors");
            string msg = _lang.Translate("ERR", (int)err, err);
            if (args.Length > 0)
                msg += SmartTranslate(_lang, args);
            return msg;
        }

        #region HELP_FUNCTIONS
        internal static string SmartTranslate(QxLang lang, params string[] args)
        {
            // 有 "%T:" 開頭標記, 自動翻譯
            if (args.Length > 0)
            {
                string tagT = "%T:";
                string msg = "";
                foreach (var arg in args)
                {
                    if (string.IsNullOrEmpty(arg))
                        continue;
                    if (arg.StartsWith(tagT))
                    {
                        var t = lang.Translate(arg.Replace(tagT, "").Trim());
                        msg += t;
                    }
                    else
                    {
                        msg += arg;
                    }
                }
                return msg;
            }
            return "";
        }
        internal static string SmartTranslate(string langPack, params string[] args)
        {
            // 有 "%T:" 開頭標記, 自動翻譯
            var lang = (langPack != null)
                ? QxLang.Instance(langPack)
                : _lang;
            return SmartTranslate(lang, args);
        }
        internal static string SmartTranslate(params string[] args)
        {
            // 有 "%T:" 開頭標記, 自動翻譯
            return SmartTranslate(_lang, args);
        }
        #endregion

        public static string MotorDisplayName(int axisID, bool postFix = true)
        {
            //var name = Drivers.HeadConfig.GetAttribName(null, axisID, postFix);
            //if (postFix)
            //{
            //    var names = name.Split(' ');
            //    if (names.Length > 1)
            //    {
            //        names[1] = Text(names[1]);
            //        name = names[0] + " " + names[1];
            //    }
            //}
            //return name;
            return $"Motor_#{axisID + 1}";
        }
        public static string DeviceDisplayName(params string[] names)
        {
            //>>> return Fierro.Drivers.HeadConfig.GetAttribName("Z", id);
            string result = "";
            foreach (var name in names)
                result += _lang.Translate(name);
            return result;
        }

        #region PRIVATE_FUNCTIONS
        static string _TAG(ErrorCodes err)
        {
            return $"{{%{(int)err}%}}";
        }
        #endregion

        public static string Pack(ErrorCodes err, bool includeDesc = true)
        {
            return includeDesc ? JetEazy.QxNums.GetEnumDescription(err) + _TAG(err) : _TAG(err);
        }
        public static bool UnPack(object arg, out string msg, out ErrorCodes err)
        {
            if (arg == null)
            {
                msg = "";
                err = ErrorCodes.OK;
                return false;
            }
            else
            {
                bool isFound = false;
                msg = (string)arg.ToString();
                err = ErrorCodes.OK;

                // 簡單暴力拆解
                foreach (ErrorCodes e in Enum.GetValues(typeof(ErrorCodes)))
                {
                    var tag = _TAG(e);
                    if (msg.Contains(tag))
                    {
                        err = e;
                        msg = msg.Replace(tag, "");
                        isFound = true;
                    }
                }

                return isFound;
            }
        }

        public static string FormatErrorMsg(Exception ex)
        {
            if (ex == null)
                return "";

            string sep = "於";

            string msg = ex.Message;
            if (ex.StackTrace != null)
            {
                msg += "\n\r";
                msg += ex.StackTrace.Replace(sep, "\n\r@");
            }
            //>System.Diagnostics.Trace.WriteLine(msg);
            return msg;
        }
        public static string TRACE_DETECT_AGAIN(string subject, string exception)
        {
            // 內部 LOG 使用
            return string.Format("{0}: 偵測到 : {1} [再次巡訪檢查]", subject, exception);
        }
        public static string PROMPT_INVALID_REGEX(string src, string arg = null)
        {
            //string err = QMSG.Text(ErrorCodes.ERR_REGEX_REF_INFO);
            //if (src == "lotItem")
            //{
            //    //string strE = "The Serial No must match the following \"Regular Expression\":";
            //    //string strC = "Serial No 必須符合以下 \"正規表示法\" 格式:\n\r\n\r" +
            //    //                INI.SERIAL_ID_REGEX.ToString() + "\n\r\n\r" +
            //    //                QMSG.ERR_REGEX_REF_INFO;
            //    //errMsg = strE + "\n\r\n\r" + strC;
            //    err = QMSG.Text("序號格式有誤") + "\n\r\n\r" + err;
            //}
            //else if (src == "iniFile")
            //{
            //    //"ini 檔內的 SERIAL_ID_PATTERN 設定值有誤!\n\r\n\r" +
            //    // QMSG.ERR_REGEX_REF_INFO;
            //    var fileName = System.IO.Path.GetFileName(arg);
            //    err = $"SERIAL_ID_PATTERN error\n\r\n\rini file: {fileName}\n\r\n\r" + err;
            //}
            //else
            //{
            //}
            //return err;
            return src;
        }
    }
}
