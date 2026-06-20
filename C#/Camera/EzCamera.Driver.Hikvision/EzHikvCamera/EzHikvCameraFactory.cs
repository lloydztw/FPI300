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

using EzCamera.Interface;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EzCamera.Driver.Hikvision
{
    //using EzCamera = EzHikvCameraTR;    // TR版 (支援 line trigger)
    using EzCamera = EzHikvCamera;

    public class EzHikvCameraFactory : IEzCameraFactory
    {
        #region PRIVATE_DATA
        IEzDeviceInfo[] _ezInfos = null;
        MyCamera.MV_CC_DEVICE_INFO_LIST m_hikDeviceInfoList;
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

            handleError(new HikvError(-102, $"LoadCamera({camID}) 超出範圍 ({_ezInfos.Length})!"));
            return null;
        }

        #region MV_PRIVATE_FUNCTIONS
        IEzDeviceInfo[] fetchAllDevices(bool reset = false)
        {
            try
            {
                if (_ezInfos != null && !reset)
                    return _ezInfos;

                var ezInfosList = new List<IEzDeviceInfo>();

                GC.Collect();

                m_hikDeviceInfoList.nDeviceNum = 0;

                int nRet = MyCamera.MV_CC_EnumDevices_NET(
                                MyCamera.MV_GIGE_DEVICE |
                                MyCamera.MV_USB_DEVICE,
                                ref m_hikDeviceInfoList);

                if (0 != nRet)
                {
                    handleError(new HikvError(-100, "遍历相机失败!"));
                    return null;
                }

                // 歷遍所有設備, 將之轉換到 EzDeviceInfo
                for (uint i = 0, number = m_hikDeviceInfoList.nDeviceNum; i < number; i++)
                {
                    var mvccDeviceInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                                            m_hikDeviceInfoList.pDeviceInfo[i],
                                            typeof(MyCamera.MV_CC_DEVICE_INFO));

                    // 取得想要顯示的 model (+ serial) 字串
                    string modelName = getDisplayModelName(ref mvccDeviceInfo);

                    if (string.IsNullOrEmpty(modelName))
                        continue;

                    // 將 海康 mvccDevice 轉換到 EzDeviceInfo
                    int index = ezInfosList.Count;
                    var ezInfo = EzCamera.DefaultDeviceInfo(index);
                    ezInfo.Model = modelName;

                    // 記住 mvccDeviceInfo 於 Tag
                    // 讓後續於 EzHikvisionCamera 內部
                    // 生成 MyCamera 之使用.
                    ezInfo.Tag = mvccDeviceInfo;
                    ezInfosList.Add(ezInfo);
                }

                _ezInfos = ezInfosList.ToArray();
                return _ezInfos;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine($"[DEBUG] {GetType().Name}.fetchAllDevice : {ex.Message}");
                MessageBox.Show(ex.Message, GetType().Name + " (海康)", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                handleError(new HikvError(-101, ex.Message, "無法生成 EzHikvisionCamera"));
                return null;
            }
        }
        string getDisplayModelName(ref MyCamera.MV_CC_DEVICE_INFO deviceInfo)
        {
            if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                var gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(
                                    deviceInfo.SpecialInfo.stGigEInfo,
                                    typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                //if (gigeInfo.chUserDefinedName != "")
                //    return $"GEV_{gigeInfo.chUserDefinedName}_{gigeInfo.chSerialNumber}";
                //else
                //    return $"GEV_{gigeInfo.chManufacturerName}_{gigeInfo.chModelName}_{gigeInfo.chSerialNumber}";

                return $"GEV_{gigeInfo.chModelName}_{gigeInfo.chSerialNumber}";
            }
            else if (deviceInfo.nTLayerType == MyCamera.MV_USB_DEVICE)
            {
                var usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(
                                    deviceInfo.SpecialInfo.stUsb3VInfo,
                                    typeof(MyCamera.MV_USB3_DEVICE_INFO));

                // 【根据 Huang 的代碼】
                //   如果 USB 檢查序號 不是 "00DA1455246", 則跳過該設備不予採用.
                if (usbInfo.chSerialNumber != "00DA1455246")
                {
                    return null;
                }

                //if (usbInfo.chUserDefinedName != "")
                //    return ("U3V_" + usbInfo.chUserDefinedName + "_" + usbInfo.chSerialNumber );
                //else
                //    return ("U3V_" + usbInfo.chManufacturerName + "_" + usbInfo.chModelName + "_" + usbInfo.chSerialNumber );

                return $"U3V_{usbInfo.chModelName}_{usbInfo.chSerialNumber}";
            }
            return null;
        }
        void handleError(HikvError err)
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
