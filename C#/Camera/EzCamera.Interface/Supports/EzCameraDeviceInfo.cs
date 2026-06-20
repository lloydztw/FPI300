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


using System;

namespace EzCamera.Interface
{
    /// <summary>
    /// 範例: 
    /// <br/> Sim (模擬)
    /// <br/> WebCam (常規網路相機)
    /// <br/> MV (海康)
    /// <br/> DVP (度申) 
    /// </summary>
    public class EzCameraDeviceInfo : ICloneable, IEzDeviceInfo
    {
        //static char C_LOCAL => EzDispText.C_LOCAL;

        public int Index { get; set; }
        public string VendorID { get; set; }   
        public string VendorName { get; set; }
        public string Model { get; set; }
        public object Tag { get; set; }         // 保留擴充用

        public EzCameraDeviceInfo(string vendorID, string vendorName, string model = null, int index=0)
        {
            VendorID = vendorID;
            VendorName = vendorName;
            Model = model;
            Index = index;
            Tag = null;
        }
        public EzCameraDeviceInfo(object obj)
        {
            // 如果 class 命名格式為 "Ez{Vendor}Camera"
            // 可默認自動解析出 vendorID
            // 例如:
            //      EzEpixCamera -> "Epix"
            //      EzMVCamera -> "MV
            VendorName = obj.GetType().Name;
            VendorID = VendorName.Replace("Ez", "").Replace("Camera", "");
            Model = null;
        }
        
        public static EzCameraDeviceInfo Parse(string str)
        {
            //string[] strs = str.Split(C_LOCAL);

            //int index = 0;
            //if (strs.Length > 1)
            //    int.TryParse(strs[1], out index);

            //str = strs[0];    
            //strs = str.Split('(', ')', C_LOCAL);
            //string vendorID = strs[0].Trim();
            //string vendorName = strs.Length > 1 ? strs[1].Trim() : vendorID;
            //string model = strs.Length > 2 ? strs[2].Trim() : null;

            EzDispText.Parse(str, out string vendorID, out string vendorName, out string model, out int index);
            var info = new EzCameraDeviceInfo(vendorID, vendorName, model);
            info.Index = index;
            return info;
        }
        public override string ToString()
        {
            //return !string.IsNullOrEmpty(Model) ? 
            //    $"{VendorID} ({VendorName}) {Model} {C_LOCAL}{Index}" :
            //    $"{VendorID} ({VendorName}) {C_LOCAL}{Index}" ;
            return EzDispText.Format(VendorID, VendorName, Model, Index);
        }

        public EzCameraDeviceInfo Clone()
        {
            return (EzCameraDeviceInfo)MemberwiseClone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
    }
}
