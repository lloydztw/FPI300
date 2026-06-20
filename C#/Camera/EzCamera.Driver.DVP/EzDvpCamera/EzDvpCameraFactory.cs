#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-05-05 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using DVPCameraType;
using EzCamera.Interface;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EzCamera.Driver.DVP
{
    using EzCamera = EzDvpCamera;
    using VendorDeviceInfo = dvpCameraInfo;

    public class EzDvpCameraFactory : IEzCameraFactory
    {
        #region PRIVATE_DATA
        IEzDeviceInfo[] _ezInfos = null;
        List<object> m_hikDeviceInfoList;
        #endregion

        public IEzDeviceInfo[] GetAvailableCameraInfos()
        {
            fetchAllDevices();
            return _ezInfos;
        }
        public IEzCamera LoadCamera(IEzDeviceInfo info)
        {
            fetchAllDevices();
            return createEzCamera(info);
        }
        public IEzCamera LoadCamera(int camID)
        {
            fetchAllDevices();

            if (camID < _ezInfos.Length)
            {
                var info = _ezInfos[camID];
                var camera = createEzCamera(info);
                return camera;
            }

            handleError(new Do3Error(-102, $"LoadCamera({camID}) 超出範圍 ({_ezInfos.Length})!"));
            return null;
        }

        #region MV_PRIVATE_FUNCTIONS
        IEzDeviceInfo[] fetchAllDevices(bool reset = false)
        {
            try
            {
                if (_ezInfos != null && !reset)
                    return _ezInfos;

                GC.Collect();

                var ezInfosList = new List<IEzDeviceInfo>();

                uint deviceCount = 0;
                DVPCamera.dvpRefresh(ref deviceCount);
                for (uint i = 0; i < deviceCount; i++)
                {
                    //获取设备信息
                    var dvpInfo = new VendorDeviceInfo();
                    var status = DVPCamera.dvpEnum(i, ref dvpInfo);

                    if (status == dvpStatus.DVP_STATUS_OK)
                    {
                        // 取得想要顯示的 model (+ serial) 字串
                        string modelName = getDisplayModelName(ref dvpInfo);

                        if (string.IsNullOrEmpty(modelName))
                            continue;

                        // 將 "度申" dvpInfo 轉換到 EzDeviceInfo
                        int index = ezInfosList.Count;
                        var ezInfo = EzCamera.DefaultDeviceInfo(index);
                        ezInfo.Model = modelName;

                        // 記住 dvpInfo 於 Tag
                        // 讓後續於 EzDvpCamera 內部之使用
                        ezInfo.Tag = dvpInfo;
                        ezInfosList.Add(ezInfo);
                    }
                }

                _ezInfos = ezInfosList.ToArray();
                return _ezInfos;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine($"[DEBUG] {GetType().Name}.fetchAllDevice : {ex.Message}");
                MessageBox.Show(ex.Message, GetType().Name + " (度申)", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _ezInfos = new EzCameraDeviceInfo[0];
                return _ezInfos;
            }
        }
        IEzCamera createEzCamera(IEzDeviceInfo info)
        {
            try
            {
                return new EzCamera(info);
            }
            catch (Exception ex)
            {
                handleError(new Do3Error(-101, ex.Message, "無法生成 EzDvpCamera"));
                return null;
            }
        }
        string getDisplayModelName(ref VendorDeviceInfo info)
        {
            return $"{info.Model}-{info.SerialNumber}@{info.UserID}";
        }
        void handleError(EzCameraError err)
        {
            // 暫時直接使用 MessageBox 
            MessageBox.Show($"異常: {err}",
                            GetType().Name,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
        }
        #endregion
    }
}
