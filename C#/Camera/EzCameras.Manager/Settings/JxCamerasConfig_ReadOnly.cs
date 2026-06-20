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

using EzCamera.Manager;
using LeTian.JxProps;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace EzCamera.Settings
{
    /// <summary>
    /// 相機組態 (唯讀)
    /// </summary>
    public class JxCamerasConfig : JxContainer
    {
        public JxCamerasConfig() : base("CamConfig", "相機組態")
        {
            HasDetailButton = true;
            UpdateDeviceInfos();
            Modified = false;
        }

        [JsonIgnore]
        public JxBase<string> TotalNumber = new JxBase<string>("TotalNumber", "相機數量", hasDetailButton: true);

        [JsonIgnore]
        public JxCamDeviceInfo[] DeviceInfos
        {
            get;
            private set;
        } = new JxCamDeviceInfo[0];

        public override void OnBindingSubItems()
        {
            var props = new List<IProp>() { TotalNumber };
            props.AddRange(DeviceInfos);
            BindItems(props);
            base.OnBindingSubItems();
        }

        public void UpdateDeviceInfos(AppCameraFactory factory = null)
        {
            if (factory == null)
                factory = new AppCameraFactory();

            var infos = factory.GetAvailableCameraInfos();

            int N = infos.Length;
            var jxItems = new JxCamDeviceInfo[N];
            for (int id = 0; id < N; id++)
                jxItems[id] = new JxCamDeviceInfo(id, infos[id].ToString());

            DisposeItems();
            DeviceInfos = jxItems;
            TotalNumber.Value = N.ToString();
            OnBindingSubItems();
            Modified = true;
        }
    }


    /// <summary>
    /// 相機設備資訊 (唯讀)
    /// </summary>
    public class JxCamDeviceInfo : JxBase<string>
    {
        #region PRIVATE_DATA
        int _id;
        void setID(int camID)
        {
            Name = $"Cam_{camID}";
            Description = $"相機_{camID}";
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
