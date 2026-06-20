#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-07-03 appended by LeTian Chang.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using DirectShowLib;
using System;
using System.Collections.Generic;


namespace Camera_NET
{
    public class EzDevice : IDisposable
    {
        public const char SEP = '@';

        #region PRIVATE_DATA
        DsDevice _imp;
        #endregion

        public EzDevice(DsDevice imp)
        {
            _imp = imp;
        }
        public void Dispose()
        {
            _imp?.Dispose();
            _imp = null;
        }
        public string UniqueName
        {
            get
            {
                string name = _imp?.Name ?? "";
                if (Offset > 0)
                    name += $"{SEP}{Offset}";
                return name;
            }
        }
        public int Offset
        {
            get;
            internal set;
        }

        public static implicit operator DsDevice(EzDevice dev)
        {
            return dev?._imp;
        }
    }


    public class DsDeviceChoice : IDisposable
    {
        #region PROTECTED_DATA
        /// <summary>
        /// "video" 或 "audio"
        /// </summary>
        protected readonly Guid _category;

        /// <summary>
        /// DirectShow 絕對順序
        /// </summary>
        protected List<EzDevice> _pCapDevices = null;
        #endregion

        public DsDeviceChoice(string category)
        {
            _category = string.Compare(category, "audio", true)==0 
                        ? FilterCategory.AudioInputDevice
                        : FilterCategory.VideoInputDevice;
        }
        public virtual void Dispose()
        {
            var devices = _pCapDevices;
            if (devices != null)
            {
                _pCapDevices = null;
                foreach (DsDevice device in devices)
                {
                    device?.Dispose();
                }
                devices.Clear();
            }
        }

        public IEnumerable<string> IterUniqueDeviceNames()
        {
            _updateDeviceList();
            foreach(var device in _pCapDevices)
            {
                yield return device.UniqueName;
            }
        }
        public IEnumerable<EzDevice> IterDevices()
        {
            _updateDeviceList();
            foreach(var device in _pCapDevices)
                yield return device;
        }

        /// <summary>
        /// 比對 device.DevicePath 唯一值
        /// </summary>
        public int GetIndexInDevices(DsDevice device)
        {
            try
            {
                int device_index = -1;

                if (device == null)
                {
                    return -1;
                }
                else
                {
                    int index = 0;
                    foreach (DsDevice cmp in _pCapDevices)
                    {
                        if (0 == string.Compare(device.DevicePath, cmp.DevicePath))
                        {
                            device_index = index;
                            break;
                        }
                        index++;
                    }
                }

                return device_index;
            }
            catch
            {
                throw;
            }
        }
        public DsDevice GetDeviceByIndex(int index)
        {
            _updateDeviceList();
            if (index >= 0 && index < _pCapDevices.Count)
            {
                var device = _pCapDevices[index];
                return device;
            }
            return null;
        }
        public DsDevice RetrieveDeviceByIndex(int index)
        {
            _updateDeviceList();
            if (index >= 0 && index < _pCapDevices.Count)
            {
                var device = _pCapDevices[index];
                _pCapDevices.RemoveAt(index);
                return device;
            }
            return null;
        }

        public int GetIndexByName(string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
                return -1;

            _updateDeviceList();

            int index = 0;
            foreach (var device in _pCapDevices)
            {
                if (string.Compare(device.UniqueName, uniqueName, true) == 0)
                {
                    return index;
                }
                index++;
            }

            return -1;
        }
        public DsDevice GetDeviceByName(string uniqueName)
        {
            int index = GetIndexByName(uniqueName);
            if(index >= 0)
                return _pCapDevices[index];
            return null;
        }
        public DsDevice RetrieveDeviceByName(string uniqueName, out int index)
        {
            index = GetIndexByName(uniqueName);
            if (index >= 0)
            {
                var device = _pCapDevices[index];
                _pCapDevices.RemoveAt(index);
                return device;
            }
            return null;
        }

        #region PRIVATE_FUNCTIONS
        private void _updateDeviceList(bool force = false)
        {
            if (force)
                Dispose();

            if (_pCapDevices == null)
            {
                var devices = DsDevice.GetDevicesOfCat(_category);
                _pCapDevices = _convert(devices);
            }
        }
        private List<EzDevice> _convert(DsDevice[] devices)
        {
            var dict = new Dictionary<string, List<DsDevice>>();
            var list = new List<EzDevice>();
            foreach (DsDevice dev in devices)
            {
                var myDevice = new EzDevice(dev);
                if(!dict.ContainsKey(dev.Name))
                {
                    dict.Add(dev.Name, new List<DsDevice>() { dev });
                    myDevice.Offset = 0;
                }
                else 
                {
                    var exists = dict[dev.Name];
                    myDevice.Offset = exists.Count;
                    exists.Add(dev);
                }
                list.Add(myDevice);
            }
            return list;
        }
        #endregion
    }


    public class DsAudioChoice : DsDeviceChoice
    {
        public DsAudioChoice() : base ("audio")
        {
        }
    }


    public class DsVideoChoice : DsDeviceChoice
    {
        public DsVideoChoice() : base("video")
        {
        }
    }
}
