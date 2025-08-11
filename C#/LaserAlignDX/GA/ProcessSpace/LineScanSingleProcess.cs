using FreeImageAPI;
using JetEazy.BasicSpace;
using JetEazy.Utils;
using LaserAlignDX.FormSpace;
using LaserAlignDX.RunSpace;
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
            var process = this;

            if (!process.IsOn)
                return;

            switch (process.ID)
            {
                case 5:

                    FireMessage(new ProcessEventArgs("Record.Start"));

                    process.Pause();

                    string fileName = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("JPG Files (*.jpg)|*.JPG|" + "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
                    //>>> fileName = $"{Universal.RCPPATH}\\{xRecipe.IndexStr}\\org.bmp";

                    process.Continue();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        //using (EzMvdImageConvertor.LoadBigImage(fileName))
                        using (Bitmap bmp = EzMvdImageConvertor.LoadBigImage(fileName))
                        {
                            pRun.cMvdInput?.Dispose();
                            pRun.cMvdInput = EzMvdImageConvertor.BitmapToCMvdImage(bmp);

                            using (var dummy = new Bitmap(1, 1))
                            {
                                //LETIAN: FireLiveImaging 必須由 caller 負責 bitmap 的 life-cycle
                                FireLiveImaging(dummy);
                            }
                        }
                    }
                    else
                    {
                        process.Stop();
                    }

                    //>>> 以後最好統一改成調用
                    //>>> process.SetNextState(6, 0);
                    process.NextDuriation = 0;
                    process.ID = 6;

                    break;

                case 6:
                    if (process.IsTimeup)
                    {
                        process.Pause();

                        using (var selectionDialog = new frmSelectScanInspectMode())
                        {
                            if (DialogResult.OK == selectionDialog.ShowDialog())
                            {
                                pRun.xScanInspectMode = (ScanInspectMode)selectionDialog.SelectScanMode;

                                switch (pRun.xScanInspectMode)
                                {
                                    case ScanInspectMode.QRCODE:
                                        pRun.QrUsed = true;
                                        break;
                                }

                                //>>> 以後最好統一改成調用
                                //>>> process.SetNextState(10, 0);
                                process.NextDuriation = 0;
                                process.ID = 10;

                                process.Continue();
                            }
                            else
                            {
                                process.Stop();
                            }
                        }
                    }
                    break;

                case 10:
                    if (process.IsTimeup)
                    {
                        pRun.FileBarcodeStr = JzTimes.DateTimeSerialString;
                        pRun.Run();

                        process.NextDuriation = 0;
                        process.ID = 20;
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
                                case LaserAlignDX.RunSpace.ScanInspectMode.MEASUREAOI:
                                    break;

                                case LaserAlignDX.RunSpace.ScanInspectMode.QRCODE:

                                    int[] ints1 = pRun.GetQrResult();

                                    StringBuilder sb2 = new StringBuilder();
                                    foreach (var ix in ints1)
                                    {
                                        sb2.Append(ix.ToString() + ",");
                                    }
                                    _LOG("QrResult:" + sb2.ToString(), Color.Black);
                                    break;

                                case LaserAlignDX.RunSpace.ScanInspectMode.NOTRAY:
                                    break;
                            }

                            #endregion

                            FireMessage(new ProcessEventArgs("Show.X", $"{(pRun.ElapsedTime * 1.0 / 1000).ToString("0.0")} s"));
                        }
                    }
                    break;
            }
        }
    }
}

