using BSA.ControlSpace;
using Eazy_Project_III;
using FreeImageAPI;
using JetEazy.BasicSpace;
using NeedleX.ProcessSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.OPSpace;

namespace TravellerMINIX6.ProcessSpace
{
    public class LineScanProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES

        System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();

        List<AnalyzeClass> m_AutoAssignClassesTmp = new List<AnalyzeClass>();
        int m_CollectDataIndex = 0;//收集数据的编号
        Bitmap m_bmpCacheOrg = new Bitmap(1, 1);

        #endregion

        #region SINGLETON
        static LineScanProcess _singleton = null;
        private LineScanProcess()
        {
        }
        #endregion

        public static LineScanProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new LineScanProcess();
                return _singleton;
            }
        }

        public override void Tick()
        {
            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        MACHINECollection.WriteToPlcRecipe();

                        //ScanCam.EncoderReset();
                        CommonLogClass.Instance.LogMessage("等待plc通知pc信号... ", Color.Black);
                        Process.NextDuriation = 200;
                        Process.ID = 501;

                        break;
                    case 501:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.LineScanStart)
                            {
                                LightControl.LightONOFF(true);
                                CommonLogClass.Instance.LogMessage("打开灯光... ", Color.Black);

                                Process.NextDuriation = INI.Instance.LightDelayTime;
                                Process.ID = 502;
                            }
                        }
                        break;
                    case 502:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.LineScanStart)
                            {
                                FireLiveImaging(new Bitmap(1, 1));

                                m_AutoAssignClassesTmp.Clear();
                                CommonLogClass.Instance.LogMessage("打开线扫采集... ", Color.Black);

                                m_CollectDataIndex = 0;
                                IScanCam.IsGrapImageComplete = false;
                                if (!Universal.IsNoUseCCD)
                                    IScanCam.StartGrab();

                                m_Stopwatch.Restart();

                                Process.NextDuriation = 200;
                                Process.ID = 503;
                            }
                        }
                        break;
                    case 503:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.LineScanStart)
                            {
                                MACHINE.PLCIO.LineScanReady = true;
                                CommonLogClass.Instance.LogMessage("通知plc采集开始... ", Color.Black);

                                Process.NextDuriation = 200;
                                Process.ID = 6;
                            }
                        }
                        break;
                    case 6:
                        if (Process.IsTimeup)
                        {
                            if (IScanCam.IsGrapImageComplete)
                            {
                                bool bOK = false;
                                //开始抓图
                                if (IScanCam.IsGrapImageOK)
                                {
                                    if (m_bmpCacheOrg != null)
                                        m_bmpCacheOrg.Dispose();
                                    //m_bmpCacheOrg = (Bitmap)IScanCam.GetPageBitmap().Clone();
                                    //FreeImageBitmap freeImageBitmap = new FreeImageBitmap(IScanCam.GetPageBitmap());
                                    //m_bmpCacheOrg = freeImageBitmap.ToBitmap();

                                    m_bmpCacheOrg = IScanCam.GetFreeImageBitmap().ToBitmap();

                                    //显示图片
                                    //DS.SetDisplayImage(m_bmpCacheOrg);
                                    //FireLiveImaging(m_bmpCacheOrg);
                                    //freeImageBitmap.Dispose();

                                    ////存图
                                    //if (INI.Instance.IsSaveDebugBMP)
                                    //    m_bmpCacheOrg.Save(Universal.DEBUGRESULTPATH + "\\" + JzTimes.DateTimeSerialString + ".png", ImageFormat.Png);

                                    Process.NextDuriation = 200;
                                    Process.ID = 10;

                                    bOK = true;
                                }
                                else
                                {
                                    MACHINE.PLCIO.LineScanDone = false;
                                    MACHINE.PLCIO.LineScanResult = "2";

                                    Process.Stop();
                                    CommonLogClass.Instance.LogMessage("线扫抓图失败 ", Color.Red);
                                    GC.Collect();
                                }

                                IScanCam.IsGrapImageComplete = false;
                                MACHINE.PLCIO.LineScanReady = false;

                                if (!Universal.IsNoUseCCD)
                                    IScanCam.StopGrab();

                                //if (bOK)
                                //{
                                //    Universal.bmpGlobalFreeImage.Dispose();
                                //    Universal.bmpGlobalFreeImage = new FreeImageBitmap(m_bmpCacheOrg);
                                //}

                                LightControl.LightONOFF(false);
                                CommonLogClass.Instance.LogMessage("关闭灯光... ", Color.Black);

                            }
                            else if (m_Stopwatch.ElapsedMilliseconds >= INI.Instance.GetImageDelayTime * 1000)
                            {
                                m_Stopwatch.Stop();

                                MACHINE.PLCIO.LineScanDone = false;
                                MACHINE.PLCIO.LineScanResult = "2";

                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("线扫抓图超时 ", Color.Red);
                                GC.Collect();

                                IScanCam.IsGrapImageComplete = false;
                                MACHINE.PLCIO.LineScanReady = false;

                                if (!Universal.IsNoUseCCD)
                                    IScanCam.StopGrab();

                                LightControl.LightONOFF(false);
                                CommonLogClass.Instance.LogMessage("关闭灯光... ", Color.Black);
                            }
                        }
                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            AnalyzeClass assignClass = new AnalyzeClass();
                            assignClass.Index = m_CollectDataIndex;
                            assignClass.SaveFileName = JzTimes.DateTimeSerialStringFFF;
                            assignClass.myPath = Traveller106.Universal.COLLECT + "\\tmp_" + m_CollectDataIndex.ToString("0000") + ".cfg";
                            assignClass.RectFStart = myRecipe.rect_start;
                            assignClass.RectFEnd = myRecipe.rect_end;
                            assignClass.CreateRowCol(myRecipe.TrayRowCount,
                                                                         myRecipe.TrayColCount,
                                                                         myRecipe.IsOpenAuFind);
                            assignClass.ChipClassDataFilePath = INI.Instance.HistoryDataPath + "\\" + INI.Instance.HistoryDataBarcode + "\\Channel0";
                            assignClass.ChipClassBarcode = (string.IsNullOrEmpty(BarcodeStr) ? JzTimes.DateTimeSerialStringFFF : BarcodeClass.Barcode);
                            BarcodeStr = string.Empty;
                            assignClass.IsDrawRect = myRecipe.IsDrawRect;
                            assignClass.freeImageBitmapInput = new FreeImageBitmap(m_bmpCacheOrg);

                            if (INI.Instance.IsSaveDebugBMP)
                                assignClass.freeImageBitmapInput.ToBitmap().Save(
                                    Universal.DEBUGRESULTPATH + "\\" +
                                    assignClass.SaveFileName + $"_Step_{m_CollectDataIndex.ToString()}.bmp",
                                    System.Drawing.Imaging.ImageFormat.Bmp);

                            assignClass.Run();

                            GC.Collect();
                            GC.Collect();
                            m_AutoAssignClassesTmp.Add(assignClass);

                            CommonLogClass.Instance.LogMessage($"已拍摄Tray盘数 {m_CollectDataIndex} pcs ", Color.Black);

                            m_CollectDataIndex++;

                            Process.NextDuriation = 200;
                            Process.ID = 20;

                            MACHINE.PLCIO.LineScanDone = true;

                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            bool ret = true;
                            foreach (AnalyzeClass autoAssign in m_AutoAssignClassesTmp)
                            {
                                ret &= !autoAssign.Running;
                            }
                            if (ret)
                            {
                                MACHINE.PLCIO.LineScanResult = (m_AutoAssignClassesTmp[0].IsPass ? "1" : "2");

                                if (m_AutoAssignClassesTmp[0].IsPass)
                                    CommonLogClass.Instance.LogMessage("线扫计算PASS ", Color.Black);
                                else
                                    CommonLogClass.Instance.LogMessage($"线扫计算FAIL 原因{m_AutoAssignClassesTmp[0].ResultDesc} ", Color.Red);


                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("线扫计算结束 ", Color.Black);
                                GC.Collect();
                                FireLiveImaging(m_AutoAssignClassesTmp[0].freeImageBitmapOutputDraw);

                                m_AutoAssignClassesTmp[0].Dispose();
                                m_AutoAssignClassesTmp.Clear();

                                GC.Collect();
                                GC.Collect();

                            }
                        }
                        break;
                }
            }
        }


    }
}
