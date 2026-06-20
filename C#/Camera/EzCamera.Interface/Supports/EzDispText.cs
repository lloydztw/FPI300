#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using EzCamera.Interface;
using System.Reflection;

namespace EzCamera
{
    public class EzDispText
    {
        public const char C_GLOBAL = '#';
        public const char C_LOCAL = '~';

        public static string Format(IEzCamera camera)
        {
            if (camera != null)
                return Format(camera.GlobalCamID, camera.DeviceInfo);
            return "";
        }
        public static string Format(int globalCamID, IEzDeviceInfo devInfo)
        {
            return $"{C_GLOBAL}{globalCamID} {devInfo}";
        }
        public static string Format(int globalCamID, string devInfo)
        {
            return $"{C_GLOBAL}{globalCamID} {devInfo}";
        }
        public static string Format(int globalCamID)
        {
            return $"{C_GLOBAL}{globalCamID}";
        }
        public static string Format(string dev, int index)
        {
            if (index > 0)
                return $"{dev} {C_LOCAL}{index}";
            else
                return dev;
        }

        public static bool Parse(string str, out int globalCamID, out string devInfo)
        {
            globalCamID = -1;
            devInfo = "";

            if (string.IsNullOrEmpty(str))
                return false;

            str = str.Trim();
            int pos = str.IndexOf(' ');
            if (pos > 0)
            {
                if (str[0] == C_GLOBAL)
                {
                    string tmp = str.Substring(0, pos).TrimStart(C_GLOBAL, ' ');
                    int.TryParse(tmp, out globalCamID);
                }
                devInfo = str.Substring(pos + 1).Trim();
            }

            return globalCamID >= 0 && !string.IsNullOrEmpty(devInfo);
        }
        public static bool Parse(string devInfo, out int index)
        {
            index = -1;
            
            if (string.IsNullOrEmpty(devInfo))
                return false;

            int pos = devInfo.LastIndexOf(C_LOCAL);
            if (pos >= 0)
                int.TryParse(devInfo.Substring(pos + 1).Trim(), out index);
            else
                index = 0;
            
            return index >= 0;
        }

        public static string Format(string vendorID, string vendorName, string model, int index)
        {
            string dev = !string.IsNullOrEmpty(model) ?
                            $"{vendorID} ({vendorName}) {model}" :
                            $"{vendorID} ({vendorName})";
            return EzDispText.Format(dev, index);
        }
        public static bool Parse(string str, out string vendorID, out string vendorName, out string model, out int index)
        {
            string[] strs = str.Split(C_LOCAL);

            index = 0;
            if (strs.Length > 1)
                int.TryParse(strs[1], out index);

            str = strs[0];
            strs = str.Split('(', ')', C_LOCAL);
            vendorID = strs[0].Trim();
            vendorName = strs.Length > 1 ? strs[1].Trim() : vendorID;
            model = strs.Length > 2 ? strs[2].Trim() : null;

            return true;
        }
    }
}
