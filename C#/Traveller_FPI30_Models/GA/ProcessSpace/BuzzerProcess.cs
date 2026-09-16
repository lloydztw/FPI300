using JetEazy.BasicSpace;

namespace NeedleX.ProcessSpace
{
    /// <summary>
    /// 蜂鳴器 <br/>
    /// @LETIAN: 20220619 (Refactor)
    /// </summary>
    public class BuzzerProcess : BaseProcess
    {
        #region PRIVATE_DATA
        /// <summary>
        /// 叫的第几次
        /// </summary>
        int m_BuzzerIndex = 0;
        /// <summary>
        /// 叫几声
        /// </summary>
        int m_BuzzerCount = 3;
        #endregion

        #region SINGLETON
        static BuzzerProcess _singleton = null;
        private BuzzerProcess()
        {
        }
        #endregion

        public static BuzzerProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new BuzzerProcess();
                return _singleton;
            }
        }

        /// <summary>
        /// 第一個參數可以指定 m_BuzzerCount
        /// </summary>
        /// <param name="args">args[0] 可以指定 m_BuzzerCount</param>
        public override void Start(params object[] args)
        {
            m_BuzzerIndex = 0;
            m_BuzzerCount = 3;
            try
            {
                if (args.Length > 0)
                    m_BuzzerCount = (int)args[0];
            }
            catch
            {

            }
            ((ProcessClass)this).Start();
        }

        public override void Tick()
        {
            //if (!IsValidPlcScanned())
            //    return;

            var Process = this;

            //iNextDurtime[3] = 1000;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = NextDurtimeTmp;
                        Process.ID = 10;

                        switch (Process.RelateString)
                        {
                            default:
                                m_BuzzerIndex = 0;
                                //m_BuzzerCount = 3;
                                break;
                        }

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (m_BuzzerIndex < m_BuzzerCount)
                            {
                                _set_buzzer(true);
                                Process.NextDuriation = 500;
                                Process.ID = 15;
                                m_BuzzerIndex++;
                            }
                            else
                            {
                                _set_buzzer(false);
                                Process.Stop();
                                FireCompleted();
                            }
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            _set_buzzer(false);

                            Process.NextDuriation = 500;
                            Process.ID = 10;
                        }
                        break;
                }
            }
        }

        void _set_buzzer(bool on)
        {
#if (OPT_SIM)
            if (on)
            {
                //>> System.Media.SystemSounds.Beep.Play();
                System.Console.Beep();
            }
#else
            //if (on)
            //{
            //    if (!Traveller106.Universal.IsSilentMode)
            //        MACHINE.PLCIO.ADR_BUZZER = true;
            //}
            //else
            //{
            //    MACHINE.PLCIO.ADR_BUZZER = false;
            //}
#endif
        }

    }
}
