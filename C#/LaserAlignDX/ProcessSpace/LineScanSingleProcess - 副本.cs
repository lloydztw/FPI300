using FreeImageAPI;
using JetEazy.BasicSpace;
using NeedleX.ProcessSpace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Traveller106;
using TravellerMINIX6.OPSpace;

namespace TravellerMINIX6.ProcessSpace
{
    public class LineScanSingleProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES

        System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        int m_CollectDataIndex = 0;//收集数据的编号
        Bitmap m_bmpCacheOrg = new Bitmap(1, 1);
        List<AnalyzeClass> m_AutoAssignClassesTmp = new List<AnalyzeClass>();

        #endregion

        #region SINGLETON
        static LineScanSingleProcess _singleton = null;
        private LineScanSingleProcess()
        {
        }
        #endregion
        public static LineScanSingleProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new LineScanSingleProcess();
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

                        //ScanCam.EncoderReset();
                        Process.NextDuriation = 200;
                        Process.ID = 501;

                        break;
                    case 501:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.LineScanStart)
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
                            m_AutoAssignClassesTmp.Clear();
                            CommonLogClass.Instance.LogMessage("单次抓图 ", Color.Black);

                            FireLiveImaging(new Bitmap(1, 1));

                            m_CollectDataIndex = 0;
                            IScanCam.IsGrapImageComplete = false;
                            if (!Universal.IsNoUseCCD)
                                IScanCam.StartGrab();

                            m_Stopwatch.Restart();

                            Process.NextDuriation = 200;
                            Process.ID = 6;
                        }
                        break;
                    case 6:
                        if (Process.IsTimeup)
                        {
                            if (IScanCam.IsGrapImageComplete || Universal.IsNoUseCCD)
                            {
                                bool bOK = false;
                                //开始抓图
                                if (IScanCam.IsGrapImageOK)
                                {
                                    if (m_bmpCacheOrg != null)
                                        m_bmpCacheOrg.Dispose();
                                    //FreeImageBitmap freeImageBitmap = new FreeImageBitmap(IScanCam.GetPageBitmap());
                                    //m_bmpCacheOrg = freeImageBitmap.ToBitmap();


                                    m_bmpCacheOrg = IScanCam.GetFreeImageBitmap().ToBitmap();
                                    //FireLiveImaging(m_bmpCacheOrg);
                                    //freeImageBitmap.Dispose();

                                    switch (Process.RelateString)
                                    {
                                        case "Snap":
                                            FireLiveImaging(m_bmpCacheOrg);
                                            break;
                                    }

                                    bOK = true;
                                    //Process.ID = 10;
                                }
                                else
                                {
                                    if (Universal.IsNoUseCCD)
                                    {
                                        string pathfilename = Universal.DEBUGSRCPATH + "\\" + m_CollectDataIndex.ToString("00000") + ".png";
                                        //pathfilename = Universal.DEBUGSRCPATH + "\\" + m_CollectDataIndex.ToString("00000") + ".tif";
                                        //pathfilename = Universal.DEBUGSRCPATH + "\\" + m_CollectDataIndex.ToString("00000") + ".bmp";
                                        Process.Pause();
                                        string _filename = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
                                        Process.Continue();
                                        if (!string.IsNullOrEmpty(_filename))
                                        {
                                            pathfilename = _filename;
                                            m_bmpCacheOrg.Dispose();

                                            FreeImageBitmap freeImageBitmap = new FreeImageBitmap(pathfilename);
                                            m_bmpCacheOrg = freeImageBitmap.ToBitmap();

                                            //显示图片
                                            //DS.SetDisplayImage(m_bmpCacheOrg);

                                            switch (Process.RelateString)
                                            {
                                                case "Snap":
                                                    FireLiveImaging(m_bmpCacheOrg);
                                                    break;
                                            }
                                            freeImageBitmap.Dispose();

                                            CommonLogClass.Instance.LogMessage($"单次抓图成功{m_CollectDataIndex} File=" + pathfilename, Color.Black);
                                            bOK = true;
                                        }

                                    }
                                    else
                                        CommonLogClass.Instance.LogMessage("单次抓图失败 Step=" + m_CollectDataIndex.ToString(), Color.Red);

                                    //Process.ID = 10;
                                }

                                IScanCam.IsGrapImageComplete = false;

                                if (!Universal.IsNoUseCCD)
                                    IScanCam.StopGrab();

                                if (bOK)
                                {
                                    //Universal.bmpGlobalFreeImage.Dispose();
                                    //Universal.bmpGlobalFreeImage = new FreeImageBitmap(m_bmpCacheOrg);

                                    switch (Process.RelateString)
                                    {
                                        case "Snap":
                                            CommonLogClass.Instance.LogMessage("单次抓图采集结束 ", Color.Black);
                                            Process.Stop();
                                            break;
                                        case "Test":
                                            Process.ID = 10;
                                            Process.NextDuriation = 200;
                                            break;
                                    }
                                }
                                else
                                {
                                    CommonLogClass.Instance.LogMessage("单次抓图失败 Step=" + m_CollectDataIndex.ToString(), Color.Red);
                                    Process.Stop();
                                }

                                LightControl.LightONOFF(false);
                                CommonLogClass.Instance.LogMessage("关闭灯光... ", Color.Black);

                            }
                            else if (m_Stopwatch.ElapsedMilliseconds >= INI.Instance.GetImageDelayTime * 1000)
                            {
                                m_Stopwatch.Stop();

                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("线扫抓图超时 ", Color.Red);
                                GC.Collect();

                                IScanCam.IsGrapImageComplete = false;

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
                            //if (IScanCam.IsGrapImageComplete || Universal.IsNoUseCCD)
                            {
                                ////提前发送信号
                                //Task.Run(() =>
                                //{
                                //    MACHINE.PLCIO.ADR_LinePCToPlcSign = true;
                                //});

                                //模拟测试的数据
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

                                m_bmpCacheOrg.Dispose();

                                if (INI.Instance.IsSaveDebugBMP)
                                    assignClass.freeImageBitmapInput.ToBitmap().Save(
                                        Universal.DEBUGRESULTPATH + "\\" +
                                        assignClass.SaveFileName + $"_Step_{m_CollectDataIndex.ToString()}.bmp",
                                        System.Drawing.Imaging.ImageFormat.Bmp);

                                assignClass.Run();
                                m_AutoAssignClassesTmp.Add(assignClass);

                                GC.Collect();
                                GC.Collect();

                                CommonLogClass.Instance.LogMessage($"已拍摄Tray盘数 {m_CollectDataIndex} pcs ", Color.Black);

                                m_CollectDataIndex++;

                                Process.NextDuriation = 200;
                                Process.ID = 15;
                            }
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            //if (Universal.IsNoUseCCD && m_CollectDataIndex > 0)
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 20;
                                //Process.Stop();
                                CommonLogClass.Instance.LogMessage("单次抓图采集结束 ", Color.Black);
                            }
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
                                if (m_AutoAssignClassesTmp[0].IsPass)
                                    CommonLogClass.Instance.LogMessage("单次抓图计算PASS ", Color.Black);
                                else
                                    CommonLogClass.Instance.LogMessage($"单次抓图计算FAIL 原因{m_AutoAssignClassesTmp[0].ResultDesc}", Color.Red);

                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("单次抓图计算结束 ", Color.Black);
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

