using JetEazy.BasicSpace;
using System.Drawing;
using Traveller106;

namespace NeedleX.ProcessSpace
{
    /// <summary>
    /// INIT流程 即初始化流程 所有轴在手动模式下归位 归位完成后 运动至各轴初始化位置 <br/>
    /// @LETIAN: 20220619 重新包裝
    /// </summary>
    public class ResetProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        #endregion

        #region SINGLETON
        static ResetProcess _singleton = null;
        private ResetProcess()
        {
        }
        #endregion

        public static ResetProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new ResetProcess();
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

                        SetRunningLight();

                        //MACHINE.PLCIO.ADR_RESET = true;
                        
                        Process.NextDuriation = 2000;
                        Process.ID = 10;

                        CommonLogClass.Instance.LogMessage("所有轴复位中", Color.Black);

                        m_Stopwatch.Restart();

                        break;

                    case 10:
                        if (Process.IsTimeup)
                        {
                            //if (!MACHINE.PLCIO.ADR_RESETING && MACHINE.PLCIO.ADR_RESETCOMPLETE || Universal.IsNoUseIO)
                            if(Universal.IsNoUseIO)
                            {
                                m_Stopwatch.Stop();

                                switch (Process.RelateString)
                                {
                                    case "CloseWindows":
                                        break;
                                    default:
                                        m_BuzzerProcess.Start(1);
                                        break;
                                }
                                
                                Process.NextDuriation = 100;
                                Process.ID = 15;
                            }
                            else if (m_Stopwatch.ElapsedMilliseconds >= 5 * 60 * 1000)
                            {
                                m_Stopwatch.Stop();
                                //Time out
                                Process.Stop();
                                switch (Process.RelateString)
                                {
                                    case "CloseWindows":
                                        break;
                                    default:
                                        CommonLogClass.Instance.LogMessage("复位超時", Color.Red);
                                        m_BuzzerProcess.Start(3);
                                        SetAbnormalLight();
                                        break;
                                }
                            }
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            //if (!m_BuzzerProcess.IsOn)
                            {
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("所有轴复位完成", Color.Black);
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
