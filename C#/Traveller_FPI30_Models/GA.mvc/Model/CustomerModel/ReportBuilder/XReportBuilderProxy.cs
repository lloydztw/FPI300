#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

namespace LaserAlignDX.Model
{
    /// <summary>
    /// 報表生成代理者
    /// </summary>
    public class XReportBuilderProxy : IxReportBuilder
    {
        string APP_PATH => Traveller106.Universal.MAINPATH;
        string INI_FILE => System.IO.Path.Combine(APP_PATH, "customizations.ini");
        static IxReportBuilder _imp;

        public XReportBuilderProxy()
        {
            if(_imp == null)
            {
                _imp = LoadImplement();
            }
        }

        public string GenerateReport(string stripId, string fileName, bool optOutputFinalText = false)
        {
            return _imp.GenerateReport(stripId, fileName, optOutputFinalText);
        }

        IxReportBuilder LoadImplement()
        {
            string custom = "";
            JetEazy.Win32.Win32Ini.Load(ref custom, INI_FILE, "Reports", "Customer");
            switch (custom)
            {
                case "PowerTech":
                    return new PowerTechReportBuilder();
                case "Thai":
                    return new ThaiReportBuilder();
                default:
                    return new JcetReportBuilder();
            }
        }
    }
}