#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzCamera.Interface;
using EzCamera.Manager;
using LeTian.JxProps;
using System.Runtime.InteropServices;

namespace EzAoiChipLocQC
{
    public class JxLocalCameraConfig : JxContainer
    {
        static string DEFAULT_INI_FILE => System.IO.Path.Combine(Global.APP_PATH.IniPath, "camera_QC.ini");
        
        #region PRIVATE_DATA
        bool _initDone = false;
        #endregion

        public JxBase<string> DeviceInfo = new JxBase<string>("DeviceInfo", "相機設備", hasDetailButton: true);
        public JxPathFile IniFile = new JxPathFile("IniFile", DEFAULT_INI_FILE, "相機 Ini 檔案", isPathOnly: false);
        public JxLocalCameraConfig(string name, string description) : base(name, description)
        {
            Init();
        }

        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                DeviceInfo,
                IniFile,
            });
            base.OnBindingSubItems();
        }

        #region PRIVATE_FUNCTIONS
        private void Init()
        {
            if (!_initDone)
            {
                DeviceInfo.OnButtonClicked += (s, e) => BrowseCamera();
                
                var mgr = AppCamerasManager.Instance;
                var camera = mgr.LoadCamera();
                UpdateCameraInfo(camera);

                _initDone = true;
            }
        }
        private void BrowseCamera()
        {
            var mgr = AppCamerasManager.Instance;
            var camera = mgr.BrowseCameraConfig();
            UpdateCameraInfo(camera);
        }
        private void UpdateCameraInfo(IEzCamera camera)
        {
            string info = camera?.DeviceInfo?.ToString();
            DeviceInfo.Value = info != null ? info : "NA";
        }
        #endregion
    }
}
