using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
//using GlacialComponents.Controls;


namespace JetEazy.Lang
{
    /// <summary>
    /// 以下改成直接用 string 
    /// </summary>
    //public enum Language : int
    //{
    //    [Description("English")]
    //    en,
    //    [Description("正體中文")]
    //    zh_TW,
    //    [Description("简体中文")]
    //    zh_CN,
    //    [Description("Tiếng Việt")]
    //    vi,
    //}

    public class QxLang
    {
        public const string DEFAULT_PATH = @"C:\Program Files\Common Files\JetEazy\ini";
        public string PATH = "";

        #region PATH_FUNCTION
        private void init_path(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && System.IO.Directory.Exists(path))
                {
                    PATH = path;
                }
                else
                {
                    path = AppDomain.CurrentDomain.BaseDirectory;
                    var folder = System.IO.Path.GetFileName(path);
                    if (folder.ToLower() == "bin")
                    {
                        path = path.Replace(folder, "");
                    }
                    path = System.IO.Path.Combine(path, "Ini");
                    PATH = path;
                }
            }
            catch
            {
                PATH = DEFAULT_PATH;
            }
        }
        #endregion

        #region STATIC_DATA
        static Dictionary<string, QxLang> s_instances = new Dictionary<string, QxLang>();
        static int _languageID = 0;
        #endregion

        #region PRIVATE_DATA
        string _jsonFile = "language.json";
        Dictionary<string, string[]> _dict = null;
        string[] _availableLangs = null;
        #endregion

        #region PROTECTED_CONSTRUCTOR
        protected QxLang(string jsonFile)
        {
            if (jsonFile != null)
                _jsonFile = jsonFile;
        }
        #endregion

        public static QxLang Instance(string package = null, string path = null)
        {
            string key = (package != null)
                        ? package
                        : "default";

            if (!s_instances.TryGetValue(key, out QxLang lang))
            {
                var jsonFile = (package != null)
                    ? $"language_{package}.json"
                    : "language.json";
                lang = new QxLang(jsonFile);
                lang.init_path(path);
                lang._loadIni();
                s_instances[key] = lang;
            }

            return lang;
        }
        public event EventHandler LanguageChanged;
        
        public string[] GetAvailableLanguages(bool reload = true)
        {
            if(!reload &&  _availableLangs != null)
                return _availableLangs;

            if (_dict == null)
                _dict = _loadDict();
            
            return _loadLangsHeader(_dict);
        }
        public string GetLanguageName(int id)
        {
            if (_availableLangs != null && id < _availableLangs.Length)
                return _availableLangs[id];
            return "English";
        }
        public int LanguageID
        {
            get
            {
                return _languageID;
            }
            set
            {
                if (_languageID != value)
                {
                    _languageID = value;
                    _saveIni();
                    _fireLanguageChanged();
                    ReleaseDict();
                }
            }
        }

        public string Translate(string str, string defaultStr = null)
        {
            string result;

            if (string.IsNullOrEmpty(str))
            {
                result = str;
            }
            else
            {
                if (_dict == null)
                    _dict = _loadDict();

                result = _lookUp(_dict, str);
            }

            if (string.IsNullOrEmpty(result))
            {
                if (defaultStr != null)
                    return defaultStr;
                else
                    return str;
            }

            return result;
        }
        public string Translate(string prefix, int code, Enum e)
        {
            // 查表
            string key = prefix != null ? $"{prefix}_{code}" : e.ToString();
            string result = Translate(key, defaultStr: "");

            if (!string.IsNullOrEmpty(result))
                return result;

            // Default
            string langName = GetLanguageName(LanguageID);
            if (langName == "正體中文")
                return JetEazy.QxNums.GetEnumDescription(e);
            else if (langName == "简体中文")
                return JzLangKernel32.ToSimplified(JetEazy.QxNums.GetEnumDescription(e));
            else
                return e.ToString();
        }
        public string Translate(Enum e)
        {
            return Translate(null, -1, e);
        }

        public void Translate(Control gui, bool reload = true)
        {
            if (_dict == null || reload)
                _dict = _loadDict();

            if (_dict != null)
                _translate(gui, _dict);

            if (reload)
                ReleaseDict();
        }
        public void Dump(Control gui)
        {
            string dstPath = "d:\\paso.log\\lang";
            JetEazy.IO.QxPathUtility.InitDirectory(dstPath);
            var frm = gui.FindForm();
            var langName = GetLanguageName(LanguageID);
            var fileName = System.IO.Path.Combine(dstPath, frm.Name + $".{langName}.json");
            var jsonStr = _dumps(gui);
            System.IO.File.WriteAllText(fileName, jsonStr);
            //var dict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonStr);
            //System.Diagnostics.Debug.WriteLine(dict);
        }

        public void ReleaseDict()
        {
            if (_dict != null)
            {
                var old = _dict;
                _dict = null;
                old.Clear();
            }
        }
        public void ReloadDict()
        {
            _dict = _loadDict();
        }

        #region PRIVATE_EVENT_FUNCTIONS
        void _fireLanguageChanged()
        {
            foreach (var lang in s_instances.Values)
            {
                lang.LanguageChanged?.Invoke(lang, null);
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        Dictionary<string, string[]> _loadDict()
        {
            try
            {
                string fullFileName = System.IO.Path.Combine(PATH, _jsonFile);
                if (!System.IO.File.Exists(fullFileName))
                    fullFileName = System.IO.Path.Combine(DEFAULT_PATH, _jsonFile);

                string jsonStr = System.IO.File.ReadAllText(fullFileName);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonStr);
                return dict;
            }
            catch
            {
                return null;
            }
        }
        string[] _loadLangsHeader(Dictionary<string, string[]> dict)
        {
            if (dict != null && dict.ContainsKey("__HEADER__"))
            {
                _availableLangs = dict["__HEADER__"];
            }
            else
            {
                _availableLangs = new string[] { "English" };
            }
            return _availableLangs;
        }
        string _lookUp(Dictionary<string, string[]> dict, string key)
        {
            if (dict != null && dict.ContainsKey(key))
            {
                int idx = (int)this.LanguageID;
                var strs = dict[key];
                if (strs.Length > 0)
                {
                    var result = idx < strs.Length ? strs[idx] : strs[0];
                    if (true || string.IsNullOrEmpty(result))
                    {
                        if (_availableLangs == null)
                            GetAvailableLanguages();

                        if (_availableLangs != null && _availableLangs.Length > 0)
                        {
                            var langName = GetLanguageName(LanguageID);
                            if (langName == "简体中文")
                            {
                                //int idx2 = (int)Language.zh_TW;
                                int idx2 = Array.IndexOf(_availableLangs, "正體中文");
                                if (idx2 < strs.Length && idx2 > 0)
                                {
                                    result = JzLangKernel32.ToSimplified(strs[idx2]);
                                }
                            }
                        }
                    }
                    return !string.IsNullOrEmpty(result) ? result : strs[0];
                }
            }
            return null;
        }
        string _accumPrefix(string prefix, Control gui)
        {
            if (prefix == null)
                prefix = gui.Name;
            else
                prefix = prefix + "." + gui.Name;
            return prefix;
        }
        string _getKey(string prefix, Control gui)
        {
            // className
            string className = gui.GetType().Name.ToLower();

            // 特別處理 GlacialList
            //////if (className == "glaciallist")
            //////{
            //////    return _getKey(prefix, gui.Parent) + "." + gui.Name;
            //////}

            // 菜市場名字, 使用全長度 KEY.
            bool useFullKey = gui.Name.ToLower().StartsWith(className);
            var key = useFullKey ? prefix : gui.Name;
            return key;
        }
        void _translate(Control gui, Dictionary<string, string[]> dict, string prefix = null, int recursive = 0)
        {
            if (gui == null)
                return;
            
            prefix = _accumPrefix(prefix, gui);
            var key = _getKey(prefix, gui);

            // 特別處理 GlacialList
            //////if (gui is GlacialList glist)
            //////{
            //////    for (int j = 0; j < glist.Columns.Count; j++)
            //////    {
            //////        string key2 = $"{key}.cols[{j}]";
            //////        string colStr = _lookUp(dict, key2);
            //////        if (!string.IsNullOrEmpty(colStr))
            //////            glist.Columns[j].Text = colStr;
            //////    }
            //////}
            //////else
            {
                var text = _lookUp(dict, key);
                if (text != null)
                {
                    gui.Text = text;
                }
            }

            foreach (Control c in gui.Controls)
                _translate(c, dict, prefix, recursive + 1);
        }
        string _dumps(Control gui, string jsonStr = "", string prefix = null, int recursive = 0)
        {
            if (gui == null)
                return jsonStr;

            if (gui is RichTextBox)
                return jsonStr;

            prefix = _accumPrefix(prefix, gui);

            var text = gui.Text.Trim();

            if (text.Contains("lloydz.tw"))
                return jsonStr;

            if (recursive == 0)
                jsonStr = "{";

            // 特別處理 GlacialList
            ////if (gui is GlacialList glist)
            ////{
            ////    var key = _getKey(prefix, gui);
            ////    for (int c = 0; c < glist.Columns.Count; c++)
            ////    {
            ////        text = glist.Columns[c].Text.Trim();
            ////        jsonStr += $"\n\"{key}.cols[{c}]\" : [\"{text}\"],";
            ////    }
            ////}
            ////else
            {
                if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(gui.Name))
                {
                    var key = _getKey(prefix, gui);
                    jsonStr += $"\n\"{key}\" : [\"{text}\"],";
                }
            }

            foreach (Control c in gui.Controls)
                jsonStr = _dumps(c, jsonStr, prefix, recursive + 1);

            if (recursive == 0)
                jsonStr = jsonStr.Trim(',') + "\n}";

            return jsonStr;
        }
        #endregion

        #region PRIVATE_INI_FUNCTIONS
        string _getSafeIniFileName()
        {
            //if (_iniFileName != null)
            //    return _iniFileName;
            var fileName = System.IO.Path.Combine(
                        System.IO.Path.GetTempPath(),
                        Application.ProductName + ".lang.ini"
                    );
            return fileName;
        }
        void _loadIni()
        {
            var iniFileName = _getSafeIniFileName();
            int value = (int)_languageID;
            Win32.Win32Ini.Load(ref value, iniFileName, "QxLang", "language");
            _languageID = value;
        }
        void _saveIni()
        {
            var iniFileName = _getSafeIniFileName();
            Win32.Win32Ini.Save((int)_languageID, iniFileName, "QxLang", "language");
        }
        #endregion
    }
}
