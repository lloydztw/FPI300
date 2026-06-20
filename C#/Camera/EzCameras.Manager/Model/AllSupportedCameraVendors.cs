#region AUTHOR
/*
 * LeTian.JxProps
 * Copyright (C) 2025
 * 2025-03-10 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

//using EzCamera.Driver.DVP;
//using EzCamera.Driver.Hikvision;
using EzCamera.Driver.Sim;
using EzCamera.Driver.WebCam;

//using EzCamera.Driver.WebCam;
using EzCamera.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;


namespace EzCamera.Manager
{
    public class AllSupportedCameraVendors
    {
        #region PRIVATE_DATA
        class Context
        {
            public string VendorID;
            public string DllName;
            public string ClassName;
            public IEzCameraFactory factory;
            public Context (string vid, string dll, string className)
            {
                VendorID = vid;
                DllName = dll;
                ClassName = className;
            }
        }
        /// <summary>
        /// KEY 格式: 前面的字母必須與 VendorID 一致
        /// </summary>
        static Dictionary<string, Context> _dllFileDict = new Dictionary<string, Context>()
        {
            { "SIM", new Context("Sim 模擬相機", "EzCamera.Driver.Sim", "EzSimCameraFactory") },
            { "WEBCAM", new Context("WebCam USB", "EzCamera.Driver.WebCam", "EzUsbWebCameraFactory") },
            { "DVP", new Context("DVP 度申", "EzCamera.Driver.DVP", "EzDvpCameraFactory") },
            { "MV", new Context("MV 海康", "EzCamera.Driver.Hikvision", "EzHikvCameraFactory") },
            { "EPIX", new Context("EPIX", "EzCamera.Driver.Epix", "EzEpixCameraFactory") },
        };
        #endregion

        /// <summary>
        /// 目前已經實作 IEzCamera 之廠牌
        /// </summary>
        public static string[] VendorIDs
        {
            get
            {
                var list = new List<string>();
                foreach (var v in _dllFileDict.Values)
                    list.Add(v.VendorID);
                return list.ToArray();
            }
        }

        /// <summary>
        /// 根據廠牌載入其相關 DLLs
        /// </summary>
        public static IEzCameraFactory LoadCameraFactory(string venderKeyName, int simNumber = 0)
        {
            if (venderKeyName == null)
                return null;
            
            venderKeyName = venderKeyName.ToUpper();
            var strs = venderKeyName.Split(' ');
            string key = strs[0].Trim();

            if (key == "SIM")
            {
                simNumber = Math.Max(simNumber, 1);
                return new EzSimCameraFactory(simNumber);
            }
            if (key == "WEBCAM")
            {
                return new EzUsbWebCameraFactory();
            }
            if (_dllFileDict.TryGetValue(key, out var context))
            {
                if (context.factory != null)
                    return context.factory;

                IEzCameraFactory factory = dynamicLoadDll(context.DllName, context.ClassName);
                context.factory = factory;
                return factory;
            }
            return null;
        }

        #region PRIVATE_FUNCTIONS
        static IEzCameraFactory dynamicLoadDll(string dllName, string className)
        {
            string[] paths = new string[] {
                                GetExePath(),
                                "C:\\Program Files\\Common Files\\JetEazy",
                            };

            foreach (string path in paths)
            {
                string fullFileName = System.IO.Path.Combine(path, dllName + ".dll");
                if (System.IO.File.Exists(fullFileName))
                {
                    string fname = System.IO.Path.GetFileName(fullFileName);

                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(fullFileName);
                        if (assembly == null)
                        {
                            _FAILED_DLL("無法載入 Assembly", fullFileName);
                            continue;
                        }

                        // 取得類別型別
                        string fullClassName = dllName + "." + className;
                        Type type = assembly.GetType(fullClassName);

                        // 建立類別實例
                        object instance = Activator.CreateInstance(type);

                        return (IEzCameraFactory)instance;
                    }
                    catch (Exception ex)
                    {
                        _FAILED_DLL("無法載入 DLL", fullFileName, ex: ex);
                    }
                }
            }
            return null;
        }
        static string GetExePath()
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            return System.IO.Path.GetDirectoryName(exePath);
        }

        static void _FAILED_DLL(string errMsg, string fullFileName, Exception ex = null)
        {
            string fname = System.IO.Path.GetFileName(fullFileName);
            errMsg += " : " + fname;
            errMsg += "\n\r\n\r完整路徑 : " + fullFileName;
            if (ex != null)
                errMsg += "\n\r\n\r" + ex.StackTrace;
            _ERROR(errMsg);
        }
        static void _ERROR(string message)
        {
            MessageBox.Show(message, "AllSupportedCameraVendors", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion
    }
}
