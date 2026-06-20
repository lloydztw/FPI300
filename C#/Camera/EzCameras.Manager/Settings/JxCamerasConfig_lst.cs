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


namespace EzCamera.Settings
{
    /// <summary>
    /// 相機組態
    /// </summary>
    public class JxCamerasConfig : JxListContainer<JxCamDeviceInfo>
    {
        public JxBase<string> TotalNumber = new JxBase<string>("TotalNumber", "相機數量", hasDetailButton: true);
        
        public JxCamerasConfig() : base("Camera.Config", "相機組態")
        {
            HasDetailButton = true;
        }

        #region JX_DYNAMIC_MEMBERS
        public override void OnBindingSubItems()
        {
            BindItems(new IProp[]
            {
                TotalNumber,
            });
            base.OnBindingSubItems();
            syncTotalNumber();
        }
        void syncTotalNumber()
        {
            if (int.TryParse(TotalNumber.Value, out int number) || number != this.ListCount)
            {
                TotalNumber.Value = this.ListCount.ToString();
            }
        }
        #endregion

        #region HELPER_FUNCTIONS
        public void ResetDeviceInfos(IEnumerable<JxCamDeviceInfo> deviceInfos)
        {
            var list = this;
            list.Clear();
            list.AddRange(deviceInfos);
            int id = 0;
            foreach (var devInfo in list)
                devInfo.ID = id++; 
            syncTotalNumber();
            // OnBindingSubItems();
            Modified = true;
        }
        public JxCamDeviceInfo GetItem(int index)
        {
            return this[index];
        }
        public JxCamDeviceInfo[] GetDeviceInfos()
        {
            var arr = this.ToArray();
            return arr;
        }
        public IEnumerable<JxCamDeviceInfo> IterateItem()
        {
            var list = this;
            foreach (var item in list)
                yield return item;
        }
        #endregion
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
