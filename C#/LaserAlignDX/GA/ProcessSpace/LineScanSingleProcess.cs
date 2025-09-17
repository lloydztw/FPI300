using JetEazy.BasicSpace;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.FormSpace;
using NeedleX.ProcessSpace;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace TravellerMINIX6.ProcessSpace
{
    /// <summary>
    /// Process for 單次離線測試
    /// </summary>
    public class LineScanSingleProcess : BaseProcess
    {
        #region PRIVATE_DATA
        ScanInspectMode? _modeArg = null;
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

        public override void Start(params object[] args)
        {
            _modeArg = null;
            if (args.Length > 0 && args[0] is ScanInspectMode mode)
                _modeArg = mode;
            base.Start();
        }

        public override void Tick()
        {
            var process = this;

            if (!process.IsOn)
                return;

            switch (process.ID)
            {
                case 5:

                    //FireMessage(new ProcessEventArgs("Record.Start"));
                    FireStarted();

                    if (_modeArg != null)
                    {
                        SetNextState(10, 0);
                    }
                    else
                    {
                        #region 加載圖檔
                        process.Pause();

                        string fileName = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("JPG Files (*.jpg)|*.JPG|" + "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
                        //>>> fileName = $"{Universal.RCPPATH}\\{xRecipe.IndexStr}\\org.bmp";

                        process.Continue();

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            // 2025-08-28 LETIAN:
                            //  巨圖 統一由 LineScanCamImageHolder 保管其生命週期
                            //  不再使用不安全的 cMvdInput !!!
                            Bitmap bitmap = GaImageUtil.LoadBigImage(fileName);
                            pRun.LineScanCamImageHolder.TakeOver(bitmap, System.IO.Path.GetFileName(fileName));
                            //FireLiveImaging(bitmap);
                        }
                        else
                        {
                            process.Stop();
                        }
                        #endregion

                        SetNextState(6, 0);
                    }
                    break;

                case 6:
                    if (process.IsTimeup)
                    {
                        #region 選擇檢測模式
                        process.Pause();
                        using (var selectionDialog = new frmSelectScanInspectMode())
                        {
                            if (DialogResult.OK == selectionDialog.ShowDialog())
                            {
                                //pRun.xScanInspectMode = (ScanInspectMode)selectionDialog.SelectScanMode;
                                //switch (pRun.xScanInspectMode)
                                //{
                                //    case ScanInspectMode.QRCODE:
                                //        pRun.QrUsed = true;
                                //        break;
                                //}

                                _modeArg = (ScanInspectMode)selectionDialog.SelectScanMode;

                                //process.NextDuriation = 0;
                                //process.ID = 10;
                                SetNextState(10, 0);

                                process.Continue();
                            }
                            else
                            {
                                process.Stop();
                            }
                        }
                        #endregion
                    }
                    break;

                case 10:
                    if (process.IsTimeup)
                    {
                        pRun.FileBarcodeStr = JzTimes.DateTimeSerialString;

                        pRun.xScanInspectMode = _modeArg.Value;
                        pRun.QrUsed = (ScanInspectMode.QRCODE == pRun.xScanInspectMode);
                        pRun.Run();

                        //process.NextDuriation = 100;
                        //process.ID = 20;
                        SetNextState(20, 100);
                    }
                    break;

                case 20:
                    if (process.IsTimeup)
                    {
                        bool ret = !pRun.Running;

                        if (ret)
                        {
                            process.Stop();

                            #region 发送数据到plc

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
                                case ScanInspectMode.MEASUREAOI:
                                    break;

                                case ScanInspectMode.QRCODE:

                                    int[] ints1 = pRun.GetQrResult();

                                    StringBuilder sb2 = new StringBuilder();
                                    foreach (var ix in ints1)
                                    {
                                        sb2.Append(ix.ToString() + ",");
                                    }
                                    _LOG("QrResult:" + sb2.ToString(), Color.Black);
                                    break;

                                case ScanInspectMode.NOTRAY:
                                    break;
                            }

                            #endregion

                            FireCompleted(new ProcessEventArgs("Show.X", $"{(pRun.ElapsedTime * 1.0 / 1000).ToString("0.0")} s"));
                        }
                    }
                    break;
            }
        }
    }
}

