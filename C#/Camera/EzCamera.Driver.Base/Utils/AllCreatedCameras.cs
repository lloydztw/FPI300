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
using System;
using System.Collections.Generic;
using System.Linq;


namespace EzCamera.Driver
{
    public class AllCreatedCameras
    {
        #region PRIVATE_DATA
        static object _sync = new object();
        static Dictionary<string, IEzCamera> _table = new Dictionary<string, IEzCamera>();
        static string _KEY(IEzCamera camera)
        {
            return camera != null ? camera.FriendlyName : null;
        }
        #endregion

        public static int TotalCount
        {
            get
            {
                lock (_sync)
                {
                    return _table.Count;
                }
            }
        }
        public static int Register(IEzCamera camera, bool silent = false)
        {
            if (camera == null)
                return 0;

            lock (_sync)
            {
                string key = _KEY(camera);

                if (_table.ContainsKey(key))
                {
                    if (!silent)
                        throw new Exception($"[異常] 相機重複生成 {camera.FriendlyName}");
                    return 0;
                }

                _table.Add(key, camera);
                return _table.Count;
            }
        }
        public static void Unregister(IEzCamera camera)
        {
            if (camera == null)
                return;

            lock (_sync)
            {
                string key = _KEY(camera);
                if (_table.ContainsKey(key))
                    _table.Remove(key);
            }
        }
        public static bool Contains(IEzCamera camera)
        {
            if (camera == null)
                return false;

            lock (_sync)
            {
                string key = _KEY(camera);
                return _table.ContainsKey(key);
            }
        }
        public static bool Contains(IEzDeviceInfo deviceInfo)
        {
            if (deviceInfo == null)
                return false;

            lock( _sync)
            {
                string key = deviceInfo.ToString();
                return _table.ContainsKey(key);
            }
        }
        public static IEzCamera FindCamera(IEzDeviceInfo deviceInfo)
        {
            if (deviceInfo == null)
                return null;
            lock (_sync)
            {
                string key = deviceInfo.ToString();
                if( _table.TryGetValue(key, out IEzCamera camera))
                    return camera;
                return null;
            }
        }
        public static IEnumerable<IEzCamera> IterateCamera()
        {
            foreach(var camera in _table.Values)
                yield return camera;
        }
        public static IEzCamera[] ToArray(bool sort = true)
        {
            lock (_sync)
            {
                var arr = _table.Values.ToArray();
                if (sort)
                {
                    Array.Sort(arr, (c1, c2) =>
                    {
                        int n1 = c1 != null ? c1.GlobalCamID : int.MaxValue;
                        int n2 = c2 != null ? c2.GlobalCamID : int.MaxValue;
                        return n1 - n2;
                    });
                }
                return arr;
            }
        }
        public static void DisposeAll()
        {
            var cameras = _table.Values.ToList();
            foreach(var camera in cameras)
            {
                camera?.StopLiveMode();
                camera?.Dispose();
            }
        }
    }
}
