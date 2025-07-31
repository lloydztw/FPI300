

#if MUILTSTEP
using FreeImageAPI;
using JetEazy.BasicSpace;
using NeedleX.ProcessSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        ProcessClass m_AutoLineScanProcess = new ProcessClass();
        int m_LineScanStep = 0;
        int m_LineScanStepCount = 3;
        Bitmap[] m_bmpLineScan = null;
        Bitmap[] m_bmpLineScanTemp = null;
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

                        if (myRecipe.clone_count > 4 || myRecipe.clone_count <= 0)
                            myRecipe.clone_count = 4;

                        m_LineScanStepCount = myRecipe.chip_captureCount * myRecipe.clone_count;

                        if (Universal.IsOfflineDataVerifty)
                            m_LineScanStepCount = 1;

                        m_bmpLineScan = new Bitmap[m_LineScanStepCount];
                        m_bmpLineScanTemp = new Bitmap[m_LineScanStepCount];
                        int ibmpindex = 0;
                        while (ibmpindex < m_LineScanStepCount)
                        {
                            m_bmpLineScan[ibmpindex] = new Bitmap(1, 1);
                            m_bmpLineScanTemp[ibmpindex] = new Bitmap(1, 1);
                            ibmpindex++;
                        }

                        //ScanCam.EncoderReset();

                        Process.NextDuriation = 1500;
                        Process.ID = 501;

                        break;
                    case 501:
                        if (Process.IsTimeup)
                        {
                            m_AutoAssignClassesTmp.Clear();
                            MACHINE.PLCIO.ADR_LineScanStart = true;
                            //m_LineScanModule.Start = true;
                            CommonLogClass.Instance.LogMessage("AUTO线扫抓图启动 ", Color.Black);

                            m_CollectDataIndex = 0;
                            IScanCam.IsGrapImageComplete = false;
                            //ScanCam.EncoderReset();
                            m_LineScanStep = 0;

                            Process.NextDuriation = 200;
                            Process.ID = 6;
                        }
                        break;
                    case 6:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ADR_LinePCToPlcSign && (IScanCam.IsGrapImageComplete || Universal.IsNoUseCCD) || Universal.IsNoUseIO)
                            {

                                //ScanCam.IsGrapImageComplete = false;
                                //m_LineScanStep = MACHINE.PLCIO.GetLineScanStep;
                                //开始抓图
                                if (IScanCam.IsGrapImageOK)
                                {
                                    m_bmpCacheOrg.Dispose();
                                    //m_bmpCacheOrg = (Bitmap)IScanCam.GetPageBitmap().Clone();
                                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(IScanCam.GetPageBitmap());
                                    m_bmpCacheOrg = freeImageBitmap.ToBitmap();

                                    //显示图片
                                    //DS.ReplaceDisplayImage(m_bmpCacheOrg);
                                    FireLiveImaging(m_bmpCacheOrg);

                                    //存图
                                    if (INI.Instance.IsSaveDebugBMP)
                                        m_bmpCacheOrg.Save(
                                            Universal.DEBUGRESULTPATH + "\\" + JzTimes.DateTimeSerialString + ".png",
                                            System.Drawing.Imaging.ImageFormat.Png);

                                    m_LineScanStep = 0;
                                    while (m_LineScanStep < m_LineScanStepCount)
                                    {
                                        Rectangle cloneRect = new Rectangle(
                                            myRecipe.clone1,
                                            0,
                                            myRecipe.clone_width,
                                            m_bmpCacheOrg.Height
                                            );

                                        m_bmpLineScan[m_LineScanStep].Dispose();
                                        m_bmpLineScan[m_LineScanStep] =
                                            (Bitmap)m_bmpCacheOrg.Clone(cloneRect,
                                            PixelFormat.Format8bppIndexed);

                                        m_LineScanStep++;
                                    }

                                    Process.ID = 10;

                                    //if (m_LineScanStep < m_LineScanStepCount)
                                    //{
                                    //    Rectangle cloneRect = new Rectangle(
                                    //        myRecipe.clone1,
                                    //        0,
                                    //        myRecipe.clone_width,
                                    //        m_bmpCacheOrg.Height
                                    //        );

                                    //    m_bmpLineScan[m_LineScanStep].Dispose();
                                    //    m_bmpLineScan[m_LineScanStep] =
                                    //        (Bitmap)m_bmpCacheOrg.Clone(cloneRect,
                                    //        PixelFormat.Format8bppIndexed);


                                    //    m_LineScanStep++;

                                    //    //if (myRecipe.clone_count > 1)
                                    //    //{
                                    //    //    cloneRect = new Rectangle(
                                    //    //    myRecipe.clone2,
                                    //    //    0,
                                    //    //    myRecipe.clone_width,
                                    //    //    m_bmpCacheOrg.Height
                                    //    //    );

                                    //    //    m_bmpLineScan[m_LineScanStep].Dispose();
                                    //    //    m_bmpLineScan[m_LineScanStep] =
                                    //    //        (Bitmap)m_bmpCacheOrg.Clone(
                                    //    //            cloneRect,
                                    //    //        PixelFormat.Format8bppIndexed);
                                           
                                    //    //    m_LineScanStep++;
                                    //    //}

                                    //    ////ScanCam.EncoderReset();

                                    //    //if (m_LineScanStep == m_LineScanStepCount)
                                    //    //{
                                    //    //    Process.ID = 10;
                                    //    //}
                                    //    //else
                                    //    //{
                                    //    //    Process.ID = 1501;
                                    //    //}
                                    //}
                                    //else
                                    //{
                                    //    Process.ID = 10;
                                    //}
                                }
                                else
                                {
                                    if (Universal.IsNoUseCCD)
                                    {
                                        string pathfilename = Universal.DEBUGSRCPATH + "\\" + m_CollectDataIndex.ToString("00000") + ".png";
                                        pathfilename = Universal.DEBUGSRCPATH + "\\" + m_CollectDataIndex.ToString("00000") + ".tif";

                                        m_LineScanStep = 0;
                                        while (m_LineScanStep < m_LineScanStepCount)
                                        {
                                            //pathfilename = fileInfos[m_CollectDataIndex].FullName;

                                            m_bmpLineScan[m_LineScanStep].Dispose();
                                            m_bmpLineScan[m_LineScanStep] = new Bitmap(pathfilename);

                                            CommonLogClass.Instance.LogMessage("AUTO线扫抓图成功 Step=" + m_LineScanStep.ToString(), Color.Black);
                                            CommonLogClass.Instance.LogMessage($"AUTO线扫抓图成功{m_CollectDataIndex} File=" + pathfilename, Color.Black);

                                            m_LineScanStep++;
                                        }
                                    }
                                    else
                                        CommonLogClass.Instance.LogMessage("AUTO线扫抓图失败 Step=" + m_LineScanStep.ToString(), Color.Red);

                                    Process.ID = 10;
                                }

                                Process.NextDuriation = 200;
                                IScanCam.IsGrapImageComplete = false;

                            }
                            //else if (!MACHINE.PLCIO.t2sr_ishavenoproduct)
                            //{
                            //    if ((m_LineScanModule.Complete && !m_LineScanModule.Running))
                            //    {
                            //        Process.NextDuriation = 500;
                            //        Process.ID = 15;
                            //    }
                            //}
                        }
                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.m2_GetImageOK && (ScanCam.IsGrapImageComplete || Universal.IsNoUseCCD) || Universal.IsNoUseIO)
                            if (MACHINE.PLCIO.ADR_LinePCToPlcSign || Universal.IsNoUseIO)
                            {
                                //提前发送信号
                                Task.Run(() =>
                                {
                                    MACHINE.PLCIO.ADR_LinePCToPlcSign = true;
                                });

                                //模拟测试的数据
                                AnalyzeClass assignClass = new AnalyzeClass();
                                assignClass.Index = m_CollectDataIndex;
                                assignClass.SaveFileName = JzTimes.DateTimeSerialStringFFF;
                                //if (fileInfos == null)
                                //{
                                //    assignClass.SaveFileName = JzTimes.DateTimeSerialStringFFF;
                                //}
                                //else
                                //    assignClass.SaveFileName = fileInfos[m_CollectDataIndex].Name.Replace(fileInfos[m_CollectDataIndex].Extension, "");
                                //assignClass.SetCellResult(myRecipe.TrayRowCount, myRecipe.TrayColCount);
                                assignClass.myPath = Traveller106.Universal.COLLECT + "\\tmp_" + m_CollectDataIndex.ToString("0000") + ".cfg";
                                //assignClass.SaveCurrent();
                                //if ((ScanCam.IsGrapImageOK || Universal.IsNoUseCCD) && !INI.Instance.traydata_sim)
                                //if (!INI.Instance.traydata_sim)
                                {
                                    assignClass.InputImages = new Bitmap[m_LineScanStepCount];

                                    assignClass.RectFStart = myRecipe.rect_start;
                                    assignClass.RectFEnd = myRecipe.rect_end;
                                    assignClass.CreateRowCol(myRecipe.TrayRowCount,
                                                                                 myRecipe.TrayColCount);

                                    assignClass.IsDrawRect = myRecipe.IsDrawRect;
                                    //assignClass.CutHeight = myRecipe.CutHeight;
                                    //assignClass.InputImage = bmpLine;

                                    //int captureIndex = 0;
                                    //while (captureIndex < RecipeTrayClass.Instance.chip_captureCount)
                                    //{
                                    //    //assignClass.InputImages[captureIndex] = new Bitmap(m_bmpLineScan[captureIndex]);
                                    //    assignClass.InputImages[captureIndex] = new Bitmap(m_bmpLineScan[captureIndex]);

                                    //    m_bmpLineScan[captureIndex].Dispose();
                                    //    m_bmpLineScan[captureIndex] = new Bitmap(1, 1);

                                    //    captureIndex++;
                                    //}

                                    Parallel.For(0, m_bmpLineScan.Length, itemIndex =>
                                    {
                                        //assignClass.InputImages[itemIndex] = new Bitmap(m_bmpLineScan[itemIndex]);
                                        //assignClass.InputImages[itemIndex] = new Bitmap(m_bmpLineScan[itemIndex]);

                                        assignClass.InputImages[itemIndex] = (Bitmap)m_bmpLineScan[itemIndex].Clone(new Rectangle(0, 0,
                                            m_bmpLineScan[itemIndex].Width, m_bmpLineScan[itemIndex].Height), PixelFormat.Format8bppIndexed);

                                        m_bmpLineScan[itemIndex].Dispose();
                                        m_bmpLineScan[itemIndex] = new Bitmap(1, 1);

                                        if (INI.Instance.IsSaveDebugBMP)
                                            assignClass.InputImages[itemIndex].Save(
                                                Universal.DEBUGRESULTPATH + "\\" +
                                                assignClass.SaveFileName + $"_Step_{itemIndex.ToString()}.bmp",
                                                System.Drawing.Imaging.ImageFormat.Bmp);
                                    });

                                    //assignClass.InspectModex = myRecipe.InspectModex;
                                    assignClass.Run();
                                }

                                GC.Collect();
                                GC.Collect();
                                m_AutoAssignClassesTmp.Add(assignClass);

                                m_CollectDataIndex++;

                                //MACHINE.PLCIO.m2_GetImageOK = true;
                                CommonLogClass.Instance.LogMessage("AUTO线扫步数抓图完成 ", Color.Black);
                                m_LineScanStep = 0;
                                Process.NextDuriation = 200;
                                Process.ID = 15;
                            }
                            //else if (!MACHINE.PLCIO.t2sr_ishavenoproduct)
                            //{
                            //    if ((m_LineScanModule.Complete && !m_LineScanModule.Running))
                            //    {
                            //        Process.NextDuriation = 500;
                            //        Process.ID = 15;
                            //    }
                            //}
                        }
                        break;
                    case 1501:
                        if (Process.IsTimeup)
                        {
                            MACHINE.PLCIO.ADR_LinePCToPlcSign = true;

                            CommonLogClass.Instance.LogMessage("AUTO线扫抓图成功 StepComplete", Color.Black);
                            CommonLogClass.Instance.LogMessage("m2_GetImageOK ==> True", Color.Black);


                            Process.NextDuriation = 200;
                            Process.ID = 15;
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            if ((MACHINE.PLCIO.ADR_LineScanComplete && !MACHINE.PLCIO.ADR_LineScaning))
                                //|| (Universal.IsNoUseIO && m_CollectDataIndex > fileInfos.Length - 1))
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 20;
                                //Process.Stop();
                                CommonLogClass.Instance.LogMessage("AUTO线扫抓图采集结束 ", Color.Black);
                            }
                            else
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 6;
                                CommonLogClass.Instance.LogMessage("AUTO线扫抓图采集继续 ", Color.Black);
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
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("AUTO线扫抓图计算结束 ", Color.Black);
                                GC.Collect();
                                //if (!m_AutoMainProcess.IsOn)
                                //    m_TrackItemUI[(int)TrackArea.TrackINSPECT].SetList(m_AutoAssignClassesTmp);
                            }
                        }
                        break;
                }
            }
        }


    }
}

#endif