using JetEazy.Lang;
using System.Windows.Forms;

namespace JetEazy.BasicSpace
{
    public class JzLangPackage
    {
        #region PRIVATE_DATA
        QxLang _lang;
        #endregion

        public static string AutoLangDir(string path)
        {
            if (path != null)
            {
                while(path.Length > 3)
                {
                    var parent = System.IO.Path.GetDirectoryName(path);
                    if (parent.ToUpper().EndsWith("AUTOMATION"))
                    {
                        return System.IO.Path.Combine(path, "Ini", "language");
                    }
                    path = parent;
                }
            }
            return path;
        }

        public JzLangPackage(string package, string path)
        {
            _lang = QxLang.Instance(package, AutoLangDir(path));
        }
        public string Translate(string text, string defaultText = null)
        {
            if (_lang != null)
                return _lang.Translate(text, defaultText);
            return text;
        }
        public void Translate(Control wnd)
        {
            _lang?.Translate(wnd);
        }
    }
}
