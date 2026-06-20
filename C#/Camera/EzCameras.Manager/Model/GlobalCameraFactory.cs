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

using EzCamera.Driver;
using EzCamera.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace EzCamera.Manager
{
    /// <summary>
    /// 將所有會用到 各廠家的 CameraFactory 納入於此 Class 提供統一的生成接口. 
    /// <br/>【建議】不要使用 Singleton 才能節省內存.
    /// <br/> 查詢與生成camera之後, 內部的 _infos, _dict 才會自動釋放.
    /// </summary>
    internal abstract class CameraFactoryComposite : IEzCameraFactoryEx
    {
        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        NLog.ILogger _LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        #region KEYS
        string _FACTORY_KEY(string str)
        {
            var strs = str.Split(' ');
            return strs[0].ToUpper();
        }
        string _DEV_KEY(IEzDeviceInfo dev)
        {
            return dev != null ? dev.ToString() : "";
        }
        #endregion

        #region PRIVATE_CACHE_DATA
        /// <summary>
        /// 擴增 IEzDeviceInfo 以方便查表生成對應的 IEzCamera
        /// </summary>
        internal class DeviceInfoEx
        {
            #region DATA
            public int globalID;
            public IEzDeviceInfo info;
            public IEzCameraFactory factory;
            public string vendorKey;
            #endregion

            public DeviceInfoEx(int globalID, IEzDeviceInfo info, IEzCameraFactory factory)
            {
                this.globalID = globalID;
                this.info = info;
                this.factory = factory;
            }
            public IEzCamera LoadCamera()
            {
                var cam = factory.LoadCamera(info);
                if (cam != null)
                {
                    cam.GlobalCamID = globalID;
                }
                return cam;
            }
        }
        Dictionary<string, IEzCameraFactory> _factoriesDict = new Dictionary<string, IEzCameraFactory>();
        Dictionary<string, DeviceInfoEx> _devicesDict;
        protected IEzDeviceInfo[] _cache = new IEzDeviceInfo[0];
        int _lastCamID = 0;
        #endregion

        public CameraFactoryComposite()
        {
            LoadIni();
        }

        public IEzDeviceInfo[] GetAvailableCameraInfos()
        {
            fetchAllDevicesToCache(false);
            return _cache;
        }
        public IEzDeviceInfo GetDeviceInfo(int globalCamID)
        {
            fetchAllDevicesToCache(false);
            if (0 <= globalCamID && globalCamID < _cache.Length)
                return _cache[globalCamID];
            return null;
        }
        public IEzDeviceInfo GetDeviceInfo(string infoStr)
        {
            if (_devicesDict.TryGetValue(infoStr, out DeviceInfoEx devEx))
                return devEx.info;
            return null;
        }

        public IEzCamera LoadCamera(IEzDeviceInfo info)
        {
            return load_camera(info);
        }
        public IEzCamera LoadCamera(int globalCamID)
        {
            fetchAllDevicesToCache(false);

            if (globalCamID < 0)
                globalCamID = _lastCamID;

            var info = GetDeviceInfo(globalCamID);
            var cam = load_camera(info);

            //保留: 默認自動生成 對應的 Sim Camera
            //if (cam == null)
            //    cam = LoadCamera(_lastCamID = 0);

            return cam;
        }
        public IEzCamera LoadCamera(string infoStr)
        {
            var devInfo = GetDeviceInfo(infoStr);
            return load_camera(devInfo);
        }

        #region UNIV_LOAD_FUNCTION
        /// <summary>
        /// 所有 LoadCamera 的函式, 最終都 【必須】 須透過此函式生成 Camera
        /// </summary>
        IEzCamera load_camera(IEzDeviceInfo info)
        {
            if (info == null)
                return null;

            fetchAllDevicesToCache(false);

            if (!_devicesDict.TryGetValue(_DEV_KEY(info), out DeviceInfoEx devEx) || devEx == null)
                return null;

            // 如果之前已經生成, 使用既有的 camera
            // 否則生成新的 camera
            var cam = AllCreatedCameras.FindCamera(devEx.info);
            if (cam == null)
                cam = devEx.LoadCamera();

            // 紀錄 _lastCamID (於實務上幫助不大, 有時候反而會混淆操作 !!!)
            if (cam != null && cam.GlobalCamID != _lastCamID)
            {
                _lastCamID = cam.GlobalCamID;
                SaveIni();
            }

            return cam;
        }
        #endregion

        #region INTERNAL_FUNCTIONS
        internal void ImportVendor(string vendorKeyName, int simNumber = 0, bool autoSave = true, bool silent = false)
        {
            string factoryKey = _FACTORY_KEY(vendorKeyName);

            if (_factoriesDict.ContainsKey(factoryKey))
            {
                if (!silent)
                    _WARNING($"{vendorKeyName} 已經存在!");
                return;
            }

            try
            {
                var newFactory = AllSupportedCameraVendors.LoadCameraFactory(factoryKey, simNumber);

                // 調用 newFactory.GetAvailableCameraInfos()
                // 來測試各家相機所需的 DLLs 是否有完整安裝,
                // 如果測試失敗則跳過此 newFactory
                var trial = newFactory?.GetAvailableCameraInfos();
                if (trial == null || trial.Length == 0)
                {
                    if (!silent)
                        _WARNING($"系統沒有 {factoryKey} 相機!");
                    return;
                }

                _factoriesDict.Add(factoryKey, newFactory);
                fetchAllDevicesToCache(true);

                if (autoSave)
                {
                    SaveIni();
                }

                if (!silent)
                    _WARNING($"請確認 EzCamera.Driver.{factoryKey}.dll 已經正確引用.", null, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (!silent)
                    _WARNING(ex.Message, ex);
            }
        }
        internal void RemoveVendor(string vendorKeyName, bool autoSave = true)
        {
            string factoryKey = _FACTORY_KEY(vendorKeyName);
            if (!_factoriesDict.ContainsKey(factoryKey))
                return;

            _factoriesDict.Remove(factoryKey);
            fetchAllDevicesToCache(true);

            if (autoSave)
            {
                SaveIni();
            }
        }
        internal bool AutoAdjustGlobalSimDeviceInfos(List<int> usedSimIDs, bool syncCurrentSimNumber = false)
        {
            int targetSimNumber = 0;
            if (usedSimIDs != null && usedSimIDs.Count > 0)
            {
                usedSimIDs.Sort();
                var lackSimIDs = new List<int>();
                int id = 0;
                foreach (int usedID in usedSimIDs)
                {
                    for (; id + 1 < usedID; id++)
                    {
                        lackSimIDs.Add(id);
                    }
                    id = usedID;
                    targetSimNumber = Math.Max(targetSimNumber, id + 1);
                }
                if (lackSimIDs.Count == 0)
                    targetSimNumber++;
            }

            int currentSimNumber = 0;
            foreach (var devEx in _devicesDict.Values)
            {
                if (devEx.info.ToString().ToUpper().Contains("SIM"))
                {
                    currentSimNumber = Math.Max(currentSimNumber, devEx.info.Index + 1);
                }
            }

            // SPECIAL_CASE
            if (syncCurrentSimNumber)
            {
                targetSimNumber = currentSimNumber;
                currentSimNumber = -1;
            }

            if (targetSimNumber != currentSimNumber)
            {
                this.RemoveVendor("SIM", false);
                this.ImportVendor("SIM", simNumber: targetSimNumber);
                return true;
            }

            return false;
        }
        #endregion

        public static string IniPath
        {
            get;
            set;
        }

        #region INI_FILE_FUNCTIONS
        //>>> protected virtual bool _isGlobalSharedIni => true;
        internal string getIniFileName(string ext = null)
        {
            //string tag = GetType().Name;
            //if (!_isGlobalSharedIni)
            //    tag = System.IO.Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) + "_" + tag;
            //var fileName = Driver.Sim.EzFileUtil.GetIniFileName(tag, ext);

            if (string.IsNullOrEmpty(ext))
                ext = ".ini";
            
            string fname = GetType().Name + ext;
            
            string fileName;

            if(!string.IsNullOrEmpty(IniPath))
            {
                fileName = System.IO.Path.Combine(IniPath, fname);
                if (System.IO.File.Exists(fileName))
                    return fileName;
            }

            fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            return fileName;
        }
        void LoadDefaults()
        {
            foreach (var vendorKeyName in AllSupportedCameraVendors.VendorIDs)
            {
                ImportVendor(vendorKeyName, autoSave: false, silent: true);
            }
        }
        internal void SaveIni(string iniFileName = null)
        {
            if (string.IsNullOrEmpty(iniFileName))
                iniFileName = getIniFileName();

            string sectName = GetType().Name;

            JetEazy.Win32.Win32Ini.Save(_lastCamID, iniFileName, sectName, "lastCamID");

            var infoStrs = toStringArray(_cache);
            var str = string.Join(",", infoStrs);
            JetEazy.Win32.Win32Ini.Save(str, iniFileName, sectName, "deviceInfos");
        }
        internal void LoadIni(string iniFileName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(iniFileName))
                    iniFileName = getIniFileName();

                string sectName = GetType().Name;

                JetEazy.Win32.Win32Ini.Load(ref _lastCamID, iniFileName, sectName, "lastCamID");

                string str = "";
                JetEazy.Win32.Win32Ini.Load(ref str, iniFileName, sectName, "deviceInfos");
                if (string.IsNullOrEmpty(str))
                {
                    LoadDefaults();
                    SaveIni();
                    return;
                }

                var infoStrs = str.Split(',');
                rebuildCache(infoStrs);
            }
            catch (Exception ex)
            {
                _WARNING(ex.Message, ex);
            }
        }
        #endregion

        #region JSON_FILE_FUNCTIONS
#if (OPT_USE_JSON)
        internal void SaveJson()
        {
            var dto = new CacheDto()
            {
                infoStrs = toStringArray(_cache),
                lastCamID = _lastCamID,
            };
            string jsonFile = getIniFileName(".json");
            string jsonStr = JsonConvert.SerializeObject(dto);
            System.IO.File.WriteAllText(jsonFile, jsonStr, System.Text.Encoding.UTF8);
        }
        internal void LoadJson()
        {
            try
            {
                string jsonFile = getIniFileName(".json");
                if (System.IO.File.Exists(jsonFile))
                {
                    string jsonStr = System.IO.File.ReadAllText(jsonFile, System.Text.Encoding.UTF8);
                    var dto = JsonConvert.DeserializeObject<CacheDto>(jsonStr);
                    if (dto != null)
                    {
                        _lastCamID = dto.lastCamID;
                        rebuildCache(dto.infoStrs);
                        return;
                    }
                }
                LoadDefaults();
                SaveJson();
            }
            catch (Exception ex)
            {
                _WARNING(ex.Message);
            }
        }
        public class CacheDto
        {
            public string[] infoStrs;
            public int lastCamID;
        }
#endif
        #endregion

        #region PRIVATE_CACHE_FUNCTIONS
        protected void rebuildCache(string[] infoStrs)
        {
            // FACTORIES
            _factoriesDict.Clear();

            //// VERIFY (檢查格式)
            //if (infoStrs != null)
            //{
            //    var list = new List<string>(infoStrs);
            //    list.RemoveAll((s) => s == null || !EzDispText.Parse(s, out int index));
            //    if (infoStrs.Length != list.Count)
            //        infoStrs = list.ToArray();
            //}

            // Get simNumber
            int simNumber = 1;
            foreach (var infoStr in infoStrs)
            {
                if (infoStr != null && infoStr.ToUpper().Contains("SIM"))
                {
                    //var strs = infoStr.Split(C_LOCAL);
                    //if (strs.Length > 1 && int.TryParse(strs[strs.Length - 1], out int id))
                    //    simNumber = Math.Max(simNumber, id + 1);
                    if (EzDispText.Parse(infoStr, out int index))
                        simNumber = Math.Max(simNumber, index + 1);
                }
            }

            // IMPORT
            foreach (var infoStr in infoStrs)
            {
                string vkey = _FACTORY_KEY(infoStr);
                ImportVendor(vkey, autoSave: false, silent: true, simNumber: simNumber);
            }

            // TRIM CACHE (IMPORT 之後, deviceDict 應該會大於 infoStrs)
            trimCache(infoStrs);
        }
        void trimCache(string[] infoStrs)
        {
            // 因為除了數量, 還有順序也要比對
            // 偷懶直接用大字串比對
            bool dirty = string.Join("", toStringArray(_cache)) != string.Join("", infoStrs);

            // 除去多餘的 infoStrs
            var targetList = new List<string>(infoStrs);
            if (_devicesDict == null)
                _devicesDict = new Dictionary<string, DeviceInfoEx>();
            int n = targetList.RemoveAll(key => !_devicesDict.ContainsKey(key));
            dirty |= n > 0;

            // 除去 _devicesDict 多餘的 keys
            var redundentKeys = _devicesDict.Keys.ToList();
            redundentKeys.RemoveAll(key => targetList.Contains(key));
            dirty |= redundentKeys.Count > 0;
            foreach (var key in redundentKeys)
                _devicesDict.Remove(key);

            if (dirty)
            {
                // 重新設定 globalID (以 targetList 的順序為基準)
                var newCache = new IEzDeviceInfo[targetList.Count];
                int camID = 0;
                foreach (var key in targetList)
                {
                    var devEx = _devicesDict[key];
                    devEx.globalID = camID;
                    newCache[camID] = devEx.info;
                    camID++;
                }
                _cache = newCache;
            }
        }
        void fetchAllDevicesToCache(bool reset)
        {
            if (_devicesDict != null && !reset)
                return;

            _devicesDict = new Dictionary<string, DeviceInfoEx>();
            foreach (var kv in _factoriesDict)
            {
                var vendorKey = kv.Key;
                var factory = kv.Value;
                appendDevicesToCache(_devicesDict, vendorKey, factory);
            }

            var list = _devicesDict.Values.ToList();
            sortAndResetGlobalID(list);
            _cache = toArray(list);
        }
        void appendDevicesToCache(Dictionary<string, DeviceInfoEx> cache, string vendorKey, IEzCameraFactory factory)
        {
            if (factory != null)
            {
                var infos = factory.GetAvailableCameraInfos();
                foreach (var info in infos)
                {
                    var globalID = cache.Count;
                    var dev = new DeviceInfoEx(globalID, info, factory)
                    {
                        vendorKey = vendorKey,
                    };

                    cache.Add(_DEV_KEY(info), dev);
                    //_devicesCacheList.Add(dev);
                }
            }
        }
        void sortAndResetGlobalID(List<DeviceInfoEx> devices)
        {
            devices.Sort((a, b) =>
            {
                var s1 = a.vendorKey.ToUpper();
                var s2 = b.vendorKey.ToUpper();
                if (s1.Contains("SIM"))
                    s1 = "z" + s1;
                if (s2.Contains("SIM"))
                    s2 = "z" + s2;
                return String.Compare(s1, s2);
            });

            // 重新設定 global id
            int id = 0;
            foreach (var dev in devices)
            {
                dev.globalID = id++;
            }
        }
        IEzDeviceInfo[] toArray(List<DeviceInfoEx> devices)
        {
            IEzDeviceInfo[] arr;
            if (devices == null)
            {
                arr = new IEzDeviceInfo[0];
            }
            else
            {
                arr = new IEzDeviceInfo[devices.Count];
                for (int i = 0; i < devices.Count; i++)
                    arr[i] = devices[i].info;
            }
            return arr;
        }
        protected string[] toStringArray(IEnumerable<IEzDeviceInfo> infos)
        {
            var list = new List<string>();
            if (infos != null)
            {
                foreach (var info in infos)
                    list.Add(info.ToString());
            }
            return list.ToArray();
        }
        #endregion

        #region ERROR_MESSAGE
        void _WARNING(string message, Exception ex = null, MessageBoxIcon icon = MessageBoxIcon.Exclamation)
        {
            if (ex != null)
            {
                message = ex.Message + "\n\r" + ex.StackTrace;
            }
            MessageBox.Show(message, "JX : " + GetType().Name, MessageBoxButtons.OK, icon);
        }
        #endregion
    }


    internal class GlobalCameraFactory : CameraFactoryComposite
    {
        #region PRIVATE_MEMBERS
        //protected override bool _isGlobalSharedIni => false;
        static GlobalCameraFactory _instance;
        GlobalCameraFactory() { }
        #endregion
        public static GlobalCameraFactory Instance
        {
            get
            {
                if(_instance == null )
                    _instance = new GlobalCameraFactory();
                return _instance;
            }
        }
    }


    internal class AppCameraFactory : CameraFactoryComposite
    {
        #region PROTECTED_DATA
        //protected override bool _isGlobalSharedIni => false;
        static AppCameraFactory _instance;
        AppCameraFactory() { }
        #endregion
        
        public static AppCameraFactory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AppCameraFactory();
                return _instance;
            }
        }

        internal void RebuildCache(IEnumerable<IEzDeviceInfo> devices)
        {
            string[] infoStrs = toStringArray(devices);
            rebuildCache(infoStrs);
        }

#if OPT_USE_JX
        private void FromJX(object src)
        {
            // APP 所有 cameras config
            // 以當下的 AppCameraFactory 為準則
            return;

            if (src != null && src is JxCamerasConfig jxConfig)
            {
                List<string> infoStrs = new List<string>();
                foreach (var jxInfo in jxConfig.IterateItem())
                {
                    infoStrs.Add(jxInfo.VendorInfo);
                }
                rebuildCache(infoStrs.ToArray());
            }
        }
        internal void ToJX(object dst)
        {
            if (dst != null && dst is JxCamerasConfig jxConfig)
            {
                var jxInfos = jxConfig.GetDeviceInfos();
                var infoStrs = toStringArray(_cache);
                bool dirty = false;

                if (infoStrs.Length != jxInfos.Length)
                {
                    dirty = true;
                }
                else
                {
                    for (int i = 0; i < infoStrs.Length; i++)
                    {
                        if (infoStrs[i] != jxInfos[i].VendorInfo)
                        {
                            dirty = true;
                            break;
                        }
                    }
                }
                
                if (!dirty)
                    return;

                int N = infoStrs.Length;
                jxInfos = new JxCamDeviceInfo[N];
                for (int i = 0; i < N; i++)
                    jxInfos[i] = new JxCamDeviceInfo(i, infoStrs[i]);

                jxConfig.ResetDeviceInfos(jxInfos);
            }
        }
#else
        internal void ToJX(object arg)
        {
        }
#endif
    }
}
