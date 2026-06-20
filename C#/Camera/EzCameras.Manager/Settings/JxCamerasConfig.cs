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

using LeTian.JxProps;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace EzCamera.Settings.Old
{
    /// <summary>
    /// 相機組態
    /// </summary>
    public class JxCamerasConfig : JxContainer
    {
        #region PRIVATE_DATA
        List<JxCamDeviceInfo> _deviceInfos = new List<JxCamDeviceInfo>();
        #endregion

        public JxBase<string> TotalNumber = new JxBase<string>("TotalNumber", "相機數量", hasDetailButton: true);
        public JxCamerasConfig() : base("CamConfig", "相機組態")
        {
            HasDetailButton = true;
        }

        #region JX_DYNAMIC_MEMBERS
        public IProp[] _DynamicItems_
        {
            get
            {
                return _deviceInfos.ToArray();
            }
            set
            {
                DisposeItems();
                _deviceInfos.Clear();
                AddRange(iterate(value));
            }
        }
        public override void OnBindingSubItems()
        {
            DisposeItems();
            syncTotalNumber();
            var props = new List<IProp>() { TotalNumber };
            props.AddRange(_deviceInfos);
            BindItems(props);
            base.OnBindingSubItems();
        }
        public override void CopyFrom(IProp src)
        {
            base.CopyFrom(src);
            if (src is JxCamerasConfig cfg)
            {
                var newInfos = new List<JxCamDeviceInfo>(cfg._deviceInfos);
                ResetDeviceInfos(newInfos);
            }
        }
        IEnumerable<JxCamDeviceInfo> iterate(IProp[] src)
        {
            foreach (var prop in src)
            {
                if (prop is JxCamDeviceInfo dev)
                    yield return dev;
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        void addOneItem(JxCamDeviceInfo item, bool rebind)
        {
            if (item != null && !_deviceInfos.Contains(item))
            {
                string name = getUniqueName(item.Name);
                item.Name = name;
                _deviceInfos.Add(item);

                this[name] = item;

                if (rebind)
                {
                    syncTotalNumber();
                    OnBindingSubItems();
                }

                Modified = true;
            }
        }
        string getUniqueName(string name)
        {
            string stemName;
            int serialId;
            while (true)
            {
                var existProp = this[name];
                if (existProp == null)
                    return name;
                splitPropName(existProp.Name, out stemName, out serialId);
                name = combinePropName(stemName, serialId + 1);
            }
        }
        string combinePropName(string stemName, int id)
        {
            if (id > 0)
                return stemName + "_" + id;
            else
                return stemName;
        }
        void splitPropName(string name, out string stemName, out int id)
        {
            int pos = name.LastIndexOf('_');
            if (pos >= 0)
            {
                var ext = name.Substring(pos + 1);
                if (int.TryParse(ext, out id))
                {
                    stemName = name.Substring(0, pos);
                    return;
                }
            }
            id = 0;
            stemName = name;
        }
        void syncTotalNumber()
        {
            //if (TotalNumber.Value != _deviceInfos.Count)
            //    TotalNumber.Value = _deviceInfos.Count;
            if (int.TryParse(TotalNumber.Value, out int number) || number != _deviceInfos.Count)
            {
                TotalNumber.Value = _deviceInfos.Count.ToString();
            }
        }
        #endregion

        public void ResetDeviceInfos(IEnumerable<JxCamDeviceInfo> deviceInfos)
        {
            DisposeItems();
            _deviceInfos = new List<JxCamDeviceInfo>(deviceInfos);
            int id = 0;
            foreach (var devInfo in _deviceInfos)
                devInfo.ID = id++; 
            syncTotalNumber();
            OnBindingSubItems();
            Modified = true;
        }

        public JxCamDeviceInfo[] GetDeviceInfos()
        {
            return _deviceInfos.ToArray();
        }
        public JxCamDeviceInfo GetItem(int index)
        {
            if (index < _deviceInfos.Count)
                return _deviceInfos[index];
            return null;
        }
        public IEnumerable<JxCamDeviceInfo> IterateItem()
        {
            foreach (var filter in _deviceInfos)
                yield return filter;
        }

        public void AddRange(IEnumerable<JxCamDeviceInfo> items)
        {
            if (items != null)
            {
                foreach (var f in items)
                    addOneItem(f, false);
                syncTotalNumber();
                OnBindingSubItems();
            }
        }
        public void Add(JxCamDeviceInfo item, bool rebind = true)
        {
            addOneItem(item, rebind);
        }
    }


    /// <summary>
    /// 相機設備資訊
    /// </summary>
    public class JxCamDeviceInfo : JxBase<string>
    {
        #region PRIVATE_DATA
        int _id;
        void setID(int camID)
        {
            Name = $"Cam_{camID}";
            Description = $"相機_#{camID}";
        }
        #endregion

        public JxCamDeviceInfo()
        {
            //HasDetailButton = true;
        }
        public JxCamDeviceInfo(int id, string vendorInfo)
        {
            //HasDetailButton = true;
            setID(id);
            VendorInfo = vendorInfo;
        }

        [JsonIgnore]
        public int ID
        {
            get => _id;
            set => setID(_id = value);
        }
        [JsonIgnore]
        public string VendorInfo
        {
            get => Value;
            set => Value = value;
        }
    }
}
