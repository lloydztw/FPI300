using Common.RecipeSpace;
using Eazy_Project_III;
using FreeImageAPI;
using JetEazy.BasicSpace;
using LaserAlignDX.FormSpace;
using LaserAlignDX.RunSpace;
using NeedleX.ProcessSpace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.OPSpace;
using VisionDesigner;

namespace TravellerMINIX6.ProcessSpace
{
    public class LineScanSingleProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES

        //System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        //int m_CollectDataIndex = 0;//收集数据的编号
        //Bitmap m_bmpCacheOrg = new Bitmap(1, 1);
        //List<AnalyzeClass> m_AutoAssignClassesTmp = new List<AnalyzeClass>();

        frmSelectScanInspectMode frmSelectScan = null;

        #endregion

        #region SINGLETON
        static LineScanSingleProcess _singleton = null;
        private LineScanSingleProcess()
        {
            //Task.Run(() =>
            //{
            //    TcpRun();
            //});
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

                        FireMessage(new ProcessEventArgs("Record.Start"));

                        Process.Pause();
                        string _filename = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("JPG Files (*.jpg)|*.JPG|" + "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
                        //_filename = $"{Universal.RCPPATH}\\{xRecipe.IndexStr}\\org.bmp";
                        Process.Continue();
                        if (!string.IsNullOrEmpty(_filename))
                        {
                            using (FreeImageBitmap freeImageBitmap = new FreeImageBitmap(_filename))
                            {
                                pRun.cMvdInput = BitmapToCMvdImage(freeImageBitmap.ToBitmap());
                                FireLiveImaging(new Bitmap(1, 1));
                            }
                        }
                        else
                        {
                            Process.Stop();
                        }

                        Process.NextDuriation = 0;
                        Process.ID = 6;

                        break;
                    case 6:
                        if (Process.IsTimeup)
                        {
                            Process.Pause();

                            frmSelectScan = new frmSelectScanInspectMode();
                            if (DialogResult.OK == frmSelectScan.ShowDialog())
                            {
                                pRun.xScanInspectMode = (ScanInspectMode)frmSelectScan.SelectScanMode;
                                switch(pRun.xScanInspectMode)
                                {
                                    case ScanInspectMode.QRCODE:
                                        pRun.QrUsed = true;
                                        break;
                                }

                                Process.NextDuriation = 0;
                                Process.ID = 10;

                                Process.Continue();
                            }
                            else
                            {
                                Process.Stop();
                            }

                            frmSelectScan.Dispose();
                            frmSelectScan = null;
                        }
                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            pRun.FileBarcodeStr = JzTimes.DateTimeSerialString;
                            pRun.Run();

                            int[] ints0 = pRun.GetSingleResult();
                            float[] floats0 = pRun.GetScanOffset();

                            StringBuilder sb = new StringBuilder();
                            foreach (var ix in ints0)
                            {
                                sb.Append(ix.ToString() + ",");
                            }
                            _LOG("SingleResult:" + sb.ToString(), Color.Black);

                            StringBuilder sb1 = new StringBuilder();
                            foreach (var ix in floats0)
                            {
                                sb1.Append(ix.ToString() + ",");
                            }
                            _LOG("ScanOffset:" + sb1.ToString(), Color.Black);

                            switch (pRun.xScanInspectMode)
                            {
                                case LaserAlignDX.RunSpace.ScanInspectMode.MEASUREAOI:

                                    //MACHINEx3.PLCIO.iSingleResult(ints0);
                                    //MACHINEx3.PLCIO.rScanOffset(floats0);

                                    break;
                                case LaserAlignDX.RunSpace.ScanInspectMode.QRCODE:

                                    int[] ints1 = pRun.GetQrResult();

                                    StringBuilder sb2 = new StringBuilder();
                                    foreach (var ix in ints1)
                                    {
                                        sb2.Append(ix.ToString() + ",");
                                    }
                                    _LOG("QrResult:" + sb2.ToString(), Color.Black);

                                    //MACHINEx3.PLCIO.iQRResult(ints1);

                                    //MACHINEx3.PLCIO.iSingleResult(ints0);
                                    //MACHINEx3.PLCIO.rScanOffset(floats0);

                                    break;
                                case LaserAlignDX.RunSpace.ScanInspectMode.NOTRAY:

                                    //MACHINEx3.PLCIO.iSingleResult(ints0);

                                    break;
                            }

                            Process.NextDuriation = 0;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            bool ret = !pRun.Running;
                            if (ret)
                            {
                                Process.Stop();

                                //if (Traveller106.Universal.IsNoUseCCD)
                                //{
                                //    IsPass = pRun.IsPass;
                                //    //m_DLResultOK.Start();
                                //    ResultStart();
                                //    _LOG($"{ToChangeLanguage("发送结果为")}{(IsPass ? "PASS" : "FAIL")}", Color.Red);
                                //}

                                FireMessage(new ProcessEventArgs("Show.X", $"{(pRun.ElapsedTime * 1.0 / 1000).ToString("0.0")} s"));
                            }
                        }
                        break;
                }
            }
        }

    }
}

