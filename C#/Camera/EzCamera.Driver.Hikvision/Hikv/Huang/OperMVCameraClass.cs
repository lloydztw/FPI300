using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


namespace EzCamera.Driver.Hikvision.Huang
{
    /// <summary>
    /// 通过海康威视动态库操作海康威视相机
    /// </summary>
    public class OperMVCameraClass
    {
        /// <summary>
        /// 相机信息列表
        /// </summary>
        public MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        /// <summary>
        /// 相机对象数组
        /// </summary>
        public MyCamera[] m_pMyCamera = null;
        /// <summary>
        /// 相机能使用的个数
        /// </summary>
        public int m_nCanOpenDeviceNum = 0;
        /// <summary>
        /// 能使用相机的信息
        /// </summary>
        public MyCamera.MV_CC_DEVICE_INFO[] m_pDeviceInfo;
        /// <summary>
        /// 流里面图像
        /// </summary>
        public MyCamera.cbOutputExdelegate cbImage;
        /// <summary>
        /// 相机异常回调函数
        /// </summary>
        public MyCamera.cbExceptiondelegate pCallBackFunc = null;
        /// <summary>
        /// 相机帧数数组
        /// </summary>
        public int[] m_nFrames;
        /// <summary>
        /// 保存图像的缓存大小
        /// </summary>
        public UInt32 m_nBufSizeForSaveImage = 0;
        /// <summary>
        /// 用于保存图像的缓存
        /// </summary>
        public IntPtr m_pBufForSaveImage = IntPtr.Zero;
        /// <summary>
        /// 监控每个相机是否在采集状态
        /// </summary>
        public bool[] m_bGrabbing = null;
        /// <summary>
        /// 图像保存路径
        /// </summary>
        public string saveBmpFilePath = "C:\\";
        /// <summary>
        /// 图像保存名称
        /// </summary>
        public string saveBmpName = "temp.bmp";
        /// <summary>
        /// 监控每个相机是否要保存图像
        /// </summary>
        public bool[] m_bSaveImg = null;
        /// <summary>
        /// 显示句柄数组
        /// </summary>
        public IntPtr[] m_hDisplayHandle;
        /// <summary>
        /// 用于从驱动获取图像的缓存大小
        /// </summary>
        public UInt32 m_nBufSizeForDriver = 0;
        /// <summary>
        /// 用于从驱动获取图像的缓存
        /// </summary>
        public IntPtr m_BufForDriver;
        public Bitmap[] bmpNows = null;
        /// <summary>
        /// 相机初始化是否成功
        /// </summary>
        public bool InitialOK=false;

        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="csMessage">错误信息</param>
        /// <param name="nErrorNum">错误代码</param>
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }
            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += "错误或无效的句柄 "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " 不支持的功能  "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " 缓存已满  "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " 函数调用顺序有误  "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " 错误的参数 "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " 资源申请失败 "; break;
                case MyCamera.MV_E_NODATA: errorMsg += "无数据 "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += "前置条件有误，或运行环境已发生变化 "; break;
                case MyCamera.MV_E_VERSION: errorMsg += "版本不匹配 "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += "传入的内存空间不足 "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += "未知的错误 "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += "通用错误"; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += "节点访问条件有误 "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += "设备无访问权限 "; break;
                case MyCamera.MV_E_BUSY: errorMsg += "设备忙，或网络断开 "; break;
                case MyCamera.MV_E_NETER: errorMsg += "网络相关错误 "; break;
                default:
                    break;
            }
            MessageBox.Show(errorMsg, "错误原因", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        /// <summary>
        /// 遍历相机
        /// </summary>
        /// <returns>若未获取到相机返回null</returns>
        public string[] ErgodicCamera()
        {
           GC.Collect();
            //创建设备列表
            List<string> cameraList = new List<string>();

            m_pDeviceList.nDeviceNum = 0;

            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE |
                MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
           
            if (0 != nRet)
            {
                ShowErrorMsg("遍历相机失败!", 0);
                return null;
            }
            // 在窗体列表中显示设备名
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
               
                
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (gigeInfo.chUserDefinedName != "")
                    {
                        cameraList.Add("GEV_" + gigeInfo.chUserDefinedName + "_" + gigeInfo.chSerialNumber);
                    }
                    else
                    {
                        cameraList.Add("GEV_" + gigeInfo.chManufacturerName + "_" + gigeInfo.chModelName + "_" + gigeInfo.chSerialNumber);
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    //if (usbInfo.chUserDefinedName != "")
                    //{
                    //    cameraList.Add("U3V_" + usbInfo.chUserDefinedName + "_" + usbInfo.chSerialNumber );
                    //}
                    //else
                    //{
                    //    cameraList.Add("U3V_" + usbInfo.chManufacturerName + "_" + usbInfo.chModelName + "_" + usbInfo.chSerialNumber );
                    //}

                    if(usbInfo.chSerialNumber == "00DA1455246")
                    {
                         cameraList.Add("U3V_" + usbInfo.chManufacturerName + "_" + usbInfo.chModelName + "_" + usbInfo.chSerialNumber );
                        
                    }
                }
            }

            if (cameraList.Count > 0)
            {
                return cameraList.ToArray();
            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// 初始化相机列表中所有相机
        /// </summary>
        /// <returns>初始化相机是否成功</returns>
        public bool InitialCamera()
        {
            string[]ccdinfo= ErgodicCamera();
            if(ccdinfo==null||ccdinfo.Length==0)
                return false;

         
            bmpNows = new Bitmap[ccdinfo.Length];
            for (int i = 0; i < bmpNows.Length; i++)
            {
                bmpNows[i] = new Bitmap(1, 1);
            }

            // 获取使用设备的数量 
            uint m_nDevNum = m_pDeviceList.nDeviceNum;
            //相机对象数组
            m_pMyCamera = new MyCamera[m_nDevNum];
            //每个相机信息数组
            m_pDeviceInfo = new MyCamera.MV_CC_DEVICE_INFO[m_nDevNum];
            //绑定回调函数
            cbImage = new MyCamera.cbOutputExdelegate(ImageCallBack);
            //相机异常函数回调
            pCallBackFunc = new MyCamera.cbExceptiondelegate(cbExceptiondelegate);
            //每个相机是否正在采集
            m_bGrabbing = new bool[m_nDevNum];
            //每个相机是否要保存图像
            m_bSaveImg = new bool[m_nDevNum];
            //相机获取累计帧数数组
            m_nFrames = new int[m_nDevNum];
            //相机内存图像句柄数组
            m_hDisplayHandle = new IntPtr[m_nDevNum];
            int nRet = -1;
            for (uint i = 0, j = 0; j < m_nDevNum; ++i, ++j)
            {
                //获取选择的设备信息 
                MyCamera.MV_CC_DEVICE_INFO deviceInfo =
                    (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[j],
                                                              typeof(MyCamera.MV_CC_DEVICE_INFO));

                if (deviceInfo.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(deviceInfo.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));

                    if (usbInfo.chSerialNumber != "00DA1455246")
                    {
                        i--;
                        continue;
                    }
                }
                //打开设备 
                if (null == m_pMyCamera[i])
                {
                    m_pMyCamera[i] = new MyCamera();
                    if (null == m_pMyCamera[i])
                    {
                        ShowErrorMsg("相机 " + i.ToString() + "初始化错误", 0);
                        return false;
                    }
                }
                nRet = m_pMyCamera[i].MV_CC_CreateDevice_NET(ref deviceInfo);
                if (MyCamera.MV_OK != nRet)
                {
                    ShowErrorMsg("相机 " + i.ToString() + "创建错误", nRet);
                    return false;
                }
                nRet = m_pMyCamera[i].MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    ShowErrorMsg("相机 " + i.ToString() + "打开失败", nRet);
                    i--;
                }
                else
                {
                    m_nCanOpenDeviceNum++;
                    m_pDeviceInfo[i] = deviceInfo;
                    // 探测网络最佳包大小(只对GigE相机有效) 
                    if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        int nPacketSize = m_pMyCamera[i].MV_CC_GetOptimalPacketSize_NET();
                        if (nPacketSize > 0)
                        {
                            nRet = m_pMyCamera[i].MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                            if (nRet != MyCamera.MV_OK)
                            {
                                ShowErrorMsg("设置封包尺寸失败 ,数据包大小: " + nPacketSize.ToString("x8"), nRet);
                            }
                        }
                        else
                        {
                            ShowErrorMsg("获取封包尺寸失败" + nPacketSize.ToString("x8"), 0);
                        }
                    }
                    nRet = m_pMyCamera[i].MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
                    if (nRet != MyCamera.MV_OK)
                    {
                        ShowErrorMsg("设置触发模式失败!", nRet);
                    }
                    //开启触发模式  默认软触发
                    nRet = m_pMyCamera[i].MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                    if (nRet != MyCamera.MV_OK)
                    {
                        ShowErrorMsg("设置触发模式的触发源失败!", nRet);
                    }

                    //每个相机分别注册回调函数
                    m_pMyCamera[i].MV_CC_RegisterImageCallBackEx_NET(cbImage, (IntPtr)i);

                    //每个相机分别注册掉线回调函数
                    m_pMyCamera[i].MV_CC_RegisterExceptionCallBack_NET(pCallBackFunc, (IntPtr)i);
                    //默认相机没有采集图像
                    m_bGrabbing[i] = false;
                    //默认相机没有保存图像
                    m_bSaveImg[i] = false;
                    //开启采集模式
                    nRet = m_pMyCamera[i].MV_CC_StartGrabbing_NET();
                    if (nRet != MyCamera.MV_OK)
                    {
                        ShowErrorMsg("开启采集模式失败!", nRet);
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 获取相机丢失帧数
        /// </summary>
        /// <param name="cameraIndex">相机编号</param>
        /// <returns>返回相机丢失帧数数目</returns>
        public int GetLostFrame(int cameraIndex)
        {
            if (m_bGrabbing!=null&&m_bGrabbing[cameraIndex])
            {
                MyCamera.MV_ALL_MATCH_INFO pstInfo = new MyCamera.MV_ALL_MATCH_INFO();
                if (m_pDeviceInfo[cameraIndex].nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_MATCH_INFO_NET_DETECT MV_NetInfo = new MyCamera.MV_MATCH_INFO_NET_DETECT();
                    pstInfo.nInfoSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyCamera.MV_MATCH_INFO_NET_DETECT));
                    pstInfo.nType = MyCamera.MV_MATCH_TYPE_NET_DETECT;
                    int size = Marshal.SizeOf(MV_NetInfo);
                    pstInfo.pInfo = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(MV_NetInfo, pstInfo.pInfo, false);
                    m_pMyCamera[cameraIndex].MV_CC_GetAllMatchInfo_NET(ref pstInfo);
                    MV_NetInfo = (MyCamera.MV_MATCH_INFO_NET_DETECT)Marshal.PtrToStructure(pstInfo.pInfo, typeof(MyCamera.MV_MATCH_INFO_NET_DETECT));
                    int count = (int)MV_NetInfo.nLostFrameCount;
                    Marshal.FreeHGlobal(pstInfo.pInfo);
                    return count;
                }
                else if (m_pDeviceInfo[cameraIndex].nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_MATCH_INFO_USB_DETECT MV_NetInfo = new MyCamera.MV_MATCH_INFO_USB_DETECT();
                    pstInfo.nInfoSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyCamera.MV_MATCH_INFO_USB_DETECT));
                    pstInfo.nType = MyCamera.MV_MATCH_TYPE_USB_DETECT;
                    int size = Marshal.SizeOf(MV_NetInfo);
                    pstInfo.pInfo = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(MV_NetInfo, pstInfo.pInfo, false);
                    m_pMyCamera[cameraIndex].MV_CC_GetAllMatchInfo_NET(ref pstInfo);
                    MV_NetInfo = (MyCamera.MV_MATCH_INFO_USB_DETECT)Marshal.PtrToStructure(pstInfo.pInfo, typeof(MyCamera.MV_MATCH_INFO_USB_DETECT));
                    int count = (int)MV_NetInfo.nErrorFrameCount;
                    Marshal.FreeHGlobal(pstInfo.pInfo);
                    return count;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }
        /// <summary>
        /// 设定设备中图像有效负载大小
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        public void SetPayloadSize(int cameraIndex)
        {
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("获取有效负载大小失败", nRet);
                return;
            }
            UInt32 nPayloadSize = stParam.nCurValue;
            if (nPayloadSize > m_nBufSizeForDriver)
            {
                if (m_BufForDriver != IntPtr.Zero)
                {
                    Marshal.Release(m_BufForDriver);
                }
                m_nBufSizeForDriver = nPayloadSize;
                m_BufForDriver = Marshal.AllocHGlobal((Int32)m_nBufSizeForDriver);
            }
            if (m_BufForDriver == IntPtr.Zero)
            {
                return;
            }
        }
        /// <summary>
        /// 取流回调函数 
        /// </summary>
        /// <param name="pData">图像指针</param>
        /// <param name="pFrameInfo">帧信息</param>
        /// <param name="pUser"></param>
        private void ImageCallBack(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            int nIndex = (int)pUser;

            // 抓取的累计帧数 
            ++m_nFrames[nIndex];
           
            getOneBmp(pData, pFrameInfo, nIndex);

            #region 直接绑定显示图像控件句柄
            //MyCamera.MV_DISPLAY_FRAME_INFO stDisplayInfo = new MyCamera.MV_DISPLAY_FRAME_INFO();
            //stDisplayInfo.hWnd = m_hDisplayHandle[nIndex];
            //stDisplayInfo.pData = pData;
            //stDisplayInfo.nDataLen = pFrameInfo.nFrameLen;
            //stDisplayInfo.nWidth = pFrameInfo.nWidth;
            //stDisplayInfo.nHeight = pFrameInfo.nHeight;
            //stDisplayInfo.enPixelType = pFrameInfo.enPixelType;
            //m_pMyCamera[nIndex].MV_CC_DisplayOneFrame_NET(ref stDisplayInfo);
            #endregion


        }
        /// <summary>
        /// 获取一张图像
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="stFrameInfo"></param>
        private void getOneBmp(IntPtr pData, MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo, int CCDIndex)
        {
            Monitor.Enter(Universal.CameraLock[CCDIndex]);
            try
            {
                if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    bmpNows[CCDIndex] = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth, PixelFormat.Format8bppIndexed, pData);
                    ColorPalette cp = bmpNows[CCDIndex].Palette;
                    // init palette
                    for (int i = 0; i < 256; i++)
                    {
                        cp.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    // set palette back
                    bmpNows[CCDIndex].Palette = cp;
                    //旋转90度配合
                    bmpNows[CCDIndex].RotateFlip(RotateFlipType.Rotate90FlipNone);
                   
                }
                else if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_HB_BGR8_Packed ||
                stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                {
                    //RGB8
                    bmpNows[CCDIndex] = BGR2RGB(new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pData));
                }
                else if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_HB_RGBA8_Packed)
                {
                    //RGBA8
                    bmpNows[CCDIndex] = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 4, PixelFormat.Format32bppArgb, pData);
                }
            }
            finally
            {
                Monitor.Exit(Universal.CameraLock[CCDIndex]);
            }
        }


        /// <summary>
        /// 获取相机信息
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>对影相机信息</returns>
        private MyCamera.MV_CC_DEVICE_INFO getDeviceInfo(int cameraIndex)
        {
            // 获取选择的设备信息 
            MyCamera.MV_CC_DEVICE_INFO device =
                (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[cameraIndex],
                                                              typeof(MyCamera.MV_CC_DEVICE_INFO));
            return device;
        }
        /// <summary>
        /// 相机异常回调函数
        /// </summary>
        /// <param name="nMsgType"></param>
        /// <param name="pUser"></param>
        private void cbExceptiondelegate(uint nMsgType, IntPtr pUser)
        {
            if (nMsgType == MyCamera.MV_EXCEPTION_DEV_DISCONNECT)
            {
                int cameraIndex = (int)pUser;
                m_bGrabbing[cameraIndex] = false;
                // 停止采集
                m_pMyCamera[cameraIndex].MV_CC_StopGrabbing_NET();
                // 关闭设备
                m_pMyCamera[cameraIndex].MV_CC_CloseDevice_NET();
                m_pMyCamera[cameraIndex].MV_CC_DestroyDevice_NET();
                // 获取选择的设备信息 
                MyCamera.MV_CC_DEVICE_INFO device =
                    (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[cameraIndex],
                                                                  typeof(MyCamera.MV_CC_DEVICE_INFO));
                // 打开设备 
                while (true)
                {
                    int nRet = m_pMyCamera[cameraIndex].MV_CC_CreateDevice_NET(ref device);
                    if (MyCamera.MV_OK != nRet)
                    {
                        Thread.Sleep(5);
                        continue;
                    }
                    nRet = m_pMyCamera[cameraIndex].MV_CC_OpenDevice_NET();
                    if (MyCamera.MV_OK != nRet)
                    {
                        Thread.Sleep(5);
                        m_pMyCamera[cameraIndex].MV_CC_DestroyDevice_NET();
                        continue;
                    }
                    else
                    {
                        nRet = InitCamera(cameraIndex);
                        if (nRet!= MyCamera.MV_OK)
                        {
                            Thread.Sleep(5);
                            m_pMyCamera[cameraIndex].MV_CC_DestroyDevice_NET();
                            continue;
                        }
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 初始化掉线相机
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>返回初始化相机结果</returns>
        private int InitCamera(int cameraIndex)
        {
            //重新为相机注册异常回调函数
            int nRet = m_pMyCamera[cameraIndex].MV_CC_RegisterExceptionCallBack_NET(pCallBackFunc, (IntPtr)cameraIndex);
            GC.KeepAlive(pCallBackFunc);
            if (MyCamera.MV_OK != nRet)
            {
                return nRet;
            }
            ////重新为相机注册回调函数
            nRet =  m_pMyCamera[cameraIndex].MV_CC_RegisterImageCallBackEx_NET(cbImage, (IntPtr)cameraIndex);
            GC.KeepAlive(cbImage);
            if (MyCamera.MV_OK != nRet)
            {
                return nRet;
            }
            // 标志位置位true 
            m_bGrabbing[cameraIndex] = true;
            // 开始采集 
            nRet = m_pMyCamera[cameraIndex].MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                m_bGrabbing[cameraIndex] = false;
                return nRet;
            }
            return MyCamera.MV_OK;
        }
        /// <summary>
        /// 外部控件数组绑定相机图像句柄数组
        /// </summary>
        /// <param name="picBoxes">外部控件数组引用</param>
        public void controlShow(PictureBox[] picBoxes)
        {
            //m_hDisplayHandle[0] = pic.Handle;
            int count;

            count = Math.Min(picBoxes.Length, m_nCanOpenDeviceNum);
            //if (picBoxes.Length > m_nCanOpenDeviceNum)
            //    count = m_nCanOpenDeviceNum;
            //else
            //    count = picBoxes.Length;

            for (int i = 0; i < count; i++)
            {
                m_hDisplayHandle[i] = picBoxes[i].Handle;
            }
        }
        /// <summary>
        /// 外部控件绑定相机图像句柄
        /// </summary>
        /// <param name="pic">外部控件</param>
        /// <param name="index">索引</param>
        public void controlShow(PictureBox pic, int index)
        {
            if (m_nCanOpenDeviceNum == 0)
            {
                return;
            }
            if (index <= m_nCanOpenDeviceNum)
                m_hDisplayHandle[index] = pic.Handle;
        }
        // 保存图片 
        private void SaveImage(IntPtr pData, MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo, int nIndex)
        {
            if ((3 * stFrameInfo.nFrameLen + 2048) > m_nBufSizeForSaveImage)
            {
                m_nBufSizeForSaveImage = 3 * stFrameInfo.nFrameLen + 2048;
                m_pBufForSaveImage = Marshal.AllocHGlobal((Int32)m_nBufSizeForSaveImage);
            }
            MyCamera.MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX();
            //选择保存图像格式
            stSaveParam.enImageType = MyCamera.MV_SAVE_IAMGE_TYPE.MV_Image_Bmp;
            stSaveParam.enPixelType = stFrameInfo.enPixelType;
            stSaveParam.pData = pData;
            stSaveParam.nDataLen = stFrameInfo.nFrameLen;
            stSaveParam.nHeight = stFrameInfo.nHeight;
            stSaveParam.nWidth = stFrameInfo.nWidth;
            stSaveParam.pImageBuffer = m_pBufForSaveImage;
            stSaveParam.nBufferSize = m_nBufSizeForSaveImage;
            //stSaveParam.nJpgQuality = 80;//存Jpeg时有效
            int nRet = m_pMyCamera[nIndex].MV_CC_SaveImageEx_NET(ref stSaveParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 "+nIndex.ToString()+" 保存图像失败!", 0);
            }
            else
            {
                Byte[] bArrBufForSaveImage = new Byte[stSaveParam.nImageLen];
                Marshal.Copy(m_pBufForSaveImage, bArrBufForSaveImage, 0, (Int32)stSaveParam.nImageLen);
                Marshal.Release(m_pBufForSaveImage);
                FileStream file = new FileStream(saveBmpFilePath+ saveBmpName, FileMode.Create, FileAccess.Write);
                file.Write(bArrBufForSaveImage, 0, (int)stSaveParam.nImageLen);
                file.Close();
                //string temp = "No." + (nIndex + 1).ToString() + "Device Save Succeed!";
                ShowErrorMsg("相机 "+nIndex.ToString()+" 保存图像成功!路径 "+ saveBmpFilePath + saveBmpName, 0);
            }
        }
        /// <summary>
        /// 获取相机当前宽度
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>获取失败,返回0</returns>
        public uint getWidth(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return 0;
            }
            MyCamera.MVCC_INTVALUE widthValue = new MyCamera.MVCC_INTVALUE();
           int nRet= m_pMyCamera[cameraIndex].MV_CC_GetIntValue_NET("Width", ref widthValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取宽度失败!", nRet);
                return 0;
            }
            return widthValue.nCurValue;
        }
        /// <summary>
        /// 获取相机最大宽度
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>获取失败,返回0</returns>
        public uint getMaxWidth(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return 0;
            }
            MyCamera.MVCC_INTVALUE widthValue = new MyCamera.MVCC_INTVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetIntValue_NET("Width", ref widthValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取最大宽度失败!", nRet);
                return 0;
            }
            return widthValue.nMax;
        }
        /// <summary>
        /// 获取相机最小宽度
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>获取失败,返回0</returns>
        public uint getMinWidth(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return 0;
            }
            MyCamera.MVCC_INTVALUE widthValue = new MyCamera.MVCC_INTVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetIntValue_NET("Width", ref widthValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取最小宽度失败!", nRet);
                return 0;
            }
            return widthValue.nMin;
        }
        /// <summary>
        /// 获取相机当前高度
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>获取失败,返回0</returns>
        public uint getHeight(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return 0;
            }
            MyCamera.MVCC_INTVALUE heightValue = new MyCamera.MVCC_INTVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetIntValue_NET("Height", ref heightValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取高度失败!", nRet);
                return 0;
            }
            return heightValue.nCurValue;
        }
        /// <summary>
        /// 获取相机最大高度
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>获取失败,返回0</returns>
        public uint getMaxHeight(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return 0;
            }
            MyCamera.MVCC_INTVALUE heightValue = new MyCamera.MVCC_INTVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetIntValue_NET("Height", ref heightValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取最大高度失败!", nRet);
                return 0;
            }
            return heightValue.nMax;
        }
        /// <summary>
        /// 获取相机最小高度
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>获取失败,返回0</returns>
        public uint getMinHeight(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return 0;
            }
            MyCamera.MVCC_INTVALUE heightValue = new MyCamera.MVCC_INTVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetIntValue_NET("Height", ref heightValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取最小高度失败!", nRet);
                return 0;
            }
            return heightValue.nMin;
        }
        /// <summary>
        /// 设置相机宽度
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <param name="widthValue">相机宽度</param>
        public void setWidth(int cameraIndex, uint widthValue)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return;
            }
            int nRet = m_pMyCamera[cameraIndex].MV_CC_SetIntValue_NET("Height", widthValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 设置宽度失败!", nRet);
                return;
            }
        }
        /// <summary>
        /// 设置相机高度
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        ///  <param name="heightValue">相机高度</param>
        public void setHeight(int cameraIndex,uint heightValue)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return;
            }
            int nRet = m_pMyCamera[cameraIndex].MV_CC_SetIntValue_NET("Height", heightValue);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 设置高度失败!", nRet);
                return;
            }
        }
        /// <summary>
        /// 获取相机当前曝光时间
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>相机当前曝光时间</returns>
        public float GetExposure(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return -1;
            }
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取曝光时间失败!", nRet);
            }
            return stParam.fCurValue;
        }
        /// <summary>
        /// 获取相机当前增益值
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>相机当前增益值</returns>
        public float GetGain(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return -1;
            }
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("Gain", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取增益值失败!", nRet);
            }
            return stParam.fCurValue;
        }
        /// <summary>
        /// 获取相机当前帧率值
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>相机当前帧率值</returns>
        public float GetFPS(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return -1;
            }
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("ResultingFrameRate", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取帧率值失败!", nRet);
            }
            return stParam.fCurValue;
        }
        /// <summary>
        /// 获取相机最大FPS
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>相机最大FPS</returns>
        public float GetMaxFPS(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return -1;
            }
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("ResultingFrameRate", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取帧率值失败!", nRet);
            }
            return stParam.fMax;
        }
        /// <summary>
        /// 获取相机最小FPS
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>相机最小FPS</returns>
        public float GetMinFPS(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return -1;
            }
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("ResultingFrameRate", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取帧率值失败!", nRet);
            }
            return stParam.fMin;
        }
        /// <summary>
        /// 获取相机最大增益值
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>最大增益值</returns>
        public float GetMaxGain(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return -1;
            }
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("Gain", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取增益值失败!", nRet);
            }
            return stParam.fMax;
        }
        /// <summary>
        /// 获取相机最小增益值
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>最小增益值</returns>
        public float GetMinGain(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return -1;
            }
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("Gain", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取增益值失败!", nRet);
            }
            return stParam.fMin;
        }
        /// <summary>
        /// 获取相机最大曝光时间
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>相机最大曝光时间</returns>
        public float GetMaxExposure(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return -1;
            }
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取曝光时间失败!", nRet);
            }
            return stParam.fMax;
        }
        /// <summary>
        /// 获取相机最小曝光时间
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>相机最小曝光时间</returns>
        public float GetMinExposure(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return -1;
            }
            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 获取曝光时间失败!", nRet);
            }
            return stParam.fMin;
        }
        /// <summary>
        /// 设定相机曝光时间
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <param name="exposureDate">曝光时间(微秒)</param>
        public bool SetExposure(int cameraIndex, float exposureDate)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return false;
            }
            ////先把自动曝光关掉
            //int nRet = m_pMyCamera[cameraIndex].MV_CC_SetEnumValue_NET("ExposureAuto", 0);
            //if (nRet != MyCamera.MV_OK)
            //{
            //    ShowErrorMsg("相机 " + cameraIndex.ToString() + " 关闭自动曝光失败!", nRet);
            //}
            int nRet = m_pMyCamera[cameraIndex].MV_CC_SetFloatValue_NET("ExposureTime", exposureDate);
            if (nRet != MyCamera.MV_OK)
            {
                return false;
                //ShowErrorMsg("相机 " + cameraIndex.ToString() + " 设定曝光时间失败!", nRet);
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 曝光补偿(增益(白加黑减))
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <param name="gainValue">补偿值</param>
        public bool SetGain(int cameraIndex, float gainValue)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return false;
            }
            //先把自动增益关掉
            int nRet = m_pMyCamera[cameraIndex].MV_CC_SetEnumValue_NET("GainAuto", 0);
            if (nRet != MyCamera.MV_OK)
            {
                return false;
                //ShowErrorMsg("相机 " + cameraIndex.ToString() + " 关闭自动增益失败!", nRet);
            }
            MyCamera.MVCC_FLOATVALUE gain=new MyCamera.MVCC_FLOATVALUE();
            m_pMyCamera[cameraIndex].MV_CC_GetFloatValue_NET("Gain",ref gain);

            nRet = m_pMyCamera[cameraIndex].MV_CC_SetFloatValue_NET("Gain", gainValue);
            if (nRet != MyCamera.MV_OK)
            {
                return false;
                //ShowErrorMsg("相机 " + cameraIndex.ToString() + " 增益失败!", nRet);
            }


            return true;
        }
        /// <summary>
        /// 设定相机帧率
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <param name="FPS">要设定的帧率</param>
        public bool SetFPS(int cameraIndex, float FPS)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("索引不能超过可用相机", 0);
                return false;
            }
            int nRet = m_pMyCamera[cameraIndex].MV_CC_SetFloatValue_NET("AcquisitionFrameRate", FPS);
            if (nRet != MyCamera.MV_OK)
            {
                return false;
                //ShowErrorMsg("设置" + cameraIndex.ToString() + "相机帧率失败", nRet);
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 开始采集图像
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        public void StartGrabbing(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("相机索引超出可用相机数量", 0);
                return;
            }
            //设置SDK内部图像缓存节点个数
            //int nRet = m_pMyCamera[cameraIndex].MV_CC_SetImageNodeNum_NET(0);
            //if (MyCamera.MV_OK != nRet)
            //{
            //    ShowErrorMsg("相机" + cameraIndex.ToString() + "设置SDK内部图像缓存节点失败!", nRet);
            //    return;
            //}


            m_bGrabbing[cameraIndex] = true;
            int nRet = m_pMyCamera[cameraIndex].MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机" + cameraIndex.ToString() + "取图失败!", nRet);
                return;
            }
        }
      

        /// <summary>
        /// BGR2RGB
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private Bitmap BGR2RGB(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format24bppRgb) return bmp;


            int h = bmp.Height;

            int w = bmp.Width;

            Bitmap bmpOut = new Bitmap(w, h, PixelFormat.Format24bppRgb);    //每像素3字节

            BitmapData dataIn = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData dataOut = bmpOut.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {

                byte* pIn = (byte*)(dataIn.Scan0.ToPointer());      //指向源文件首地址
                byte* pOut = (byte*)(dataOut.Scan0.ToPointer());  //指向目标文件首地址
                for (int y = 0; y < dataIn.Height; y++)  //列扫描
                {

                    for (int x = 0; x < dataIn.Width; x++)   //行扫描
                    {

                        pOut[0] = pIn[2];     //R分量

                        pOut[1] = pIn[1];     //G分量

                        pOut[2] = pIn[0];     //B分量

                        pIn += 3;
                        pOut += 3;      //指针后移3个分量位置

                    }

                    pIn += dataIn.Stride - dataIn.Width * 3;
                    pOut += dataOut.Stride - dataOut.Width * 3;

                }

            }

            bmpOut.UnlockBits(dataOut);

            bmp.UnlockBits(dataIn);

            return bmpOut;

        }

        /// <summary>
        /// 旋转图像
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <param name="angle">旋转角度(只支持90,180,270)</param>
        public void RotationBmp(int cameraIndex, int angle, IntPtr pData, MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("相机索引超出可用相机数量", 0);
                return;
            }
        //       public MvGvspPixelType enPixelType;
        //public uint nWidth;
        //public uint nHeight;
        //public IntPtr pSrcData;
        //public uint nSrcDataLen;
        //public IntPtr pDstBuf;
        //public uint nDstBufLen;
        //public uint nDstBufSize;
        MyCamera.MV_CC_ROTATE_IMAGE_PARAM mV_CC_ROTATE_IMAGE_PARAM = new MyCamera.MV_CC_ROTATE_IMAGE_PARAM();
            mV_CC_ROTATE_IMAGE_PARAM.enPixelType=stFrameInfo.enPixelType;
            mV_CC_ROTATE_IMAGE_PARAM.nWidth=stFrameInfo.nWidth;
            mV_CC_ROTATE_IMAGE_PARAM.nHeight=stFrameInfo.nHeight;
            mV_CC_ROTATE_IMAGE_PARAM.pSrcData = pData;
            if (angle == 90)
            {
                mV_CC_ROTATE_IMAGE_PARAM.enRotationAngle = MyCamera.MV_IMG_ROTATION_ANGLE.MV_IMAGE_ROTATE_90;
            }
            else if (angle == 180)
            {
                mV_CC_ROTATE_IMAGE_PARAM.enRotationAngle = MyCamera.MV_IMG_ROTATION_ANGLE.MV_IMAGE_ROTATE_180;
            }
            else if (angle == 270)
            {
                mV_CC_ROTATE_IMAGE_PARAM.enRotationAngle = MyCamera.MV_IMG_ROTATION_ANGLE.MV_IMAGE_ROTATE_270;
            }
            else
            {
                return;
            }
            int nRet = m_pMyCamera[cameraIndex].MV_CC_RotateImage_NET(ref mV_CC_ROTATE_IMAGE_PARAM);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机" + cameraIndex.ToString() + "旋转失败!", nRet);
                return;
            }
        }
        /// <summary>
        /// 相继停止采集图像
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        public void StopGrabbing(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("相机索引超出可用相机数量", 0);
                return;
            }
            m_bGrabbing[cameraIndex] = false;
            int nRet = m_pMyCamera[cameraIndex].MV_CC_StopGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机" + cameraIndex.ToString() + "取图失败!", nRet);
                return;
            }
        }
        /// <summary>
        /// 停止所有的相机采集
        /// </summary>
        public void StopAllGrabbing()
        {
            for (int i = 0; i < m_bGrabbing.Length; i++)
            {
                m_bGrabbing[i] = false;
                int nRet = m_pMyCamera[i].MV_CC_StopGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    ShowErrorMsg("相机" + i.ToString() + "取图失败!", nRet);
                    return;
                }
            }
        }
        /// <summary>
        /// 关闭所有相机
        /// </summary>
        public void CloseCamera()
        {
            for (int i = 0; i < m_nCanOpenDeviceNum; ++i)
            {
                int nRet;
                nRet = m_pMyCamera[i].MV_CC_CloseDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    ShowErrorMsg("相机 " + i.ToString() + " 关闭失败 ", nRet);
                    return;
                }
                nRet = m_pMyCamera[i].MV_CC_DestroyDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    ShowErrorMsg("相机 " + i.ToString() + " 销毁句柄失败 ", nRet);
                    return;
                }
            }
            m_nCanOpenDeviceNum = 0;
        }
        /// <summary>
        /// 相机软触发(单张取像)
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        public void TriggerSoftware(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("相机索引超出可用相机数量", 0);
                return;
            }
            if (m_bGrabbing[cameraIndex])
            {

                int nRet = m_pMyCamera[cameraIndex].MV_CC_ClearImageBuffer_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    ShowErrorMsg("相机 " + cameraIndex.ToString() + " 清理缓冲帧失败!", nRet);
                    return;
                }
                //设置相机采集模式为触发模式(采集单张)
               nRet = m_pMyCamera[cameraIndex].MV_CC_SetCommandValue_NET("TriggerSoftware");
                if (MyCamera.MV_OK != nRet)
                {
                    ShowErrorMsg("相机 " + cameraIndex.ToString() + " 设定软触发模式失败!", nRet);
                }
            }
            else
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 采集模式为打开!", 0);
            }
        }
        /// <summary>
        /// 设定相机连续采集模式
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        public void SetContinuesMode(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("相机索引超出可用相机数量", 0);
                return;
            }
            int nRet = m_pMyCamera[cameraIndex].MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 设定连续采集模式失败!", nRet);
            }
        }
        /// <summary>
        /// 设定相机单张采集模式
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        public void SetTriggerMode(int cameraIndex)
        {
            if (cameraIndex > m_nCanOpenDeviceNum - 1)
            {
                ShowErrorMsg("相机索引超出可用相机数量", 0);
                return;
            }
            int nRet = m_pMyCamera[cameraIndex].MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("相机 " + cameraIndex.ToString() + " 设定单张采集模式失败!", nRet);
            }
        }
        /// <summary>
        /// 触发单张
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        public void TriggerSingle(int cameraIndex)
        {
            //先清空图像
            Universal.operMVCameraClass.bmpNows[cameraIndex] = null;


            //再发触发命令
            int nRet = m_pMyCamera[cameraIndex].MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Trigger Software Fail!", nRet);
            }
        }
        public void SetSoftware(int cameraIndex)
        {
            int nRet = m_pMyCamera[cameraIndex].MV_CC_SetEnumValue_NET("TriggerSource", 
                (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("SetSoftware Fail!", nRet);
            }
        }
    }
}
