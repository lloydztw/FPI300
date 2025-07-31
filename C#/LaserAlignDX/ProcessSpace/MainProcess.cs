using JetEazy.BasicSpace;
using NeedleX.ProcessSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traveller106;

namespace TravellerMINIX6.ProcessSpace
{
    public class MainProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        BaseProcess m_LinescanProcess
        {
            get { return LineScanProcess.Instance; }
        }
        System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        #endregion

        #region SINGLETON
        static MainProcess _singleton = null;
        private MainProcess()
        {
        }
        #endregion

        public static MainProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new MainProcess();
                return _singleton;
            }
        }

        public override void Tick()
        {
            //if (!IsValidPlcScanned())
            //    return;

            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:
                        //FireMessage(new ProcessEventArgs("Run"));
                        FireMessage(new ProcessEventArgs("Record.Start"));
                        SetRunningLight();

                        Process.NextDuriation = 2000;
                        Process.ID = 10;

                        INI.Instance.HistoryDataPath = Universal.HISTORY + "\\" + JzTimes.DateSerialString;
                        INI.Instance.HistoryDataBarcode = JzTimes.DateTimeSerialString;

                        CommonLogClass.Instance.LogMessage("主流程启动", Color.Black);

                        //重置码的list 并且写入读几次码
                        for (int i = 0; i < 1; i++)
                        {
                            Universal.ChannelBarcode[i].Reset();
                            Universal.ChannelBarcode[i].ReadCount = 3;// INI.Instance.ScanBarcodeCount;

                            FireMessage(new ProcessEventArgs("Reset.Data"));
                            //m_TrackItemUI[i].ResetData();
                        }

                        m_LinescanProcess.Start();

                        break;

                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (!m_LinescanProcess.IsOn)
                            {
                                m_BuzzerProcess.Start(3);

                                Process.NextDuriation = 100;
                                Process.ID = 15;
                            }
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            //if (!m_BuzzerProcess.IsOn)
                            {
                                FireMessage(new ProcessEventArgs("Record.Stop"));
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("主流程完成", Color.Black);
                                SetNormalLight();
                                FireCompleted();
                            }
                        }
                        break;
                }
            }
        }

    }
}
