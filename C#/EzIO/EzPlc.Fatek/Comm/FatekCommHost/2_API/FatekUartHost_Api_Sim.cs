#region AUTHOR
/*
 * EzPlc.Fatek
 * Copyright (C) 2026
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2012-12-04 LeTian Chang: ReOpen UART when communication failed.
 * 2012-06-22 LeTian Chang: Revised for more robust over RS232 connection.
 * 2008-07-01 LeTian Chang: Creation.
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm.Utils;
using System;


namespace EzPlc.Fatek.Comm.Sim
{
    /// <summary>
    /// 模擬版: 支持舊有的 Fatek 通訊指令 (sync) (同步阻塞式調用方式) <br\>
    /// </summary>
    public class FatekUartHostApiSim : QxPtr<IxFatekApi>, IxFatekApi
    {
        private int _stationID;

        internal FatekUartHostApiSim(int comPort, int baudRate, int stationID)
        {
            _stationID = stationID;
        }
        protected override void OnDisposing()
        {
            FatekCommFactory.Unregister(this);
        }
        public override string ToString()
        {
            return "PLC.SIM." + _stationID;
        }

        public int ComPort
        {
            get;
            private set;
        }

        public string Echo(string strMessage)
        {
            return strMessage;
        }

        public bool IsRunning()
        {
            return true;
        }

        public void Run()
        {
        }

        public void Stop()
        {
        }

        /// <summary>
        /// Y0000 or M0000
        /// </summary>
        /// <param name="strAddress">Y0000 or M0000</param>
        /// <param name="bOnOff"></param>
        public void SetSinglePoint(string strAddress, bool bOnOff)
        {
            _simCommBlock();
        }

        /// <summary>
        /// X0000 or M0000
        /// </summary>
        /// <param name="strAddress">X0000 or M0000</param>
        /// <param name="arrData"></param>
        public void ReadSinglePoints(string strAddress, bool[] arrData)
        {
            _simCommBlock();
        }

        /// <summary>
        /// X0000 or M0000
        /// </summary>
        /// <param name="strAddress">X0000 or M0000</param>
        /// <returns></returns>
        public bool ReadSinglePoint(string strAddress)
        {
            _simCommBlock();
            return false;
        }

        /// <summary>
        /// R123456 or WY1234
        /// </summary>
        /// <param name="strAddress">R123456 or WY1234</param>
        /// <param name="uData"></param>
        public void WriteRegister(string strAddress, uint uData)
        {
            _simCommBlock();
        }

        /// <summary>
        /// R123456 or WY1234
        /// </summary>
        /// <param name="strAddress">R123456 or WY1234</param>
        /// <returns></returns>
        public uint ReadRegister(string strAddress)
        {
            _simCommBlock();
            return 0;
        }

        public short ReadRegisterI16(string strAddress)
        {
            _simCommBlock();
            return 0;
        }

        public void ReadRegisters(string address, int number, Action<int, uint> updateFunc)
        {
            //throw new NotImplementedException();
            //return -1;
        }

        public void ReadMixPoints(string[] addressGrp, Action<object, int, uint> updateFunc)
        {
        }


        private System.Random m_rnd = new Random();
        private void _simCommBlock()
        {
#if(OPT_SIM_COMM_LAG)
            lock (this)
            {
                //> System.Threading.Thread.Sleep(10);
                int n = m_rnd.Next(30) + 10;
                _Delay(n);
            }
#endif
        }
        protected void _Delay(int ms)
        {
            DateTime tm = DateTime.Now;
            _Delay(ms, ref tm);
        }
        protected void _Delay(int ms, ref DateTime tmRef)
        {
            //============================================================================
            //!!! 用 System.Threading.Thread.Sleep 最少耗費 15 ms !!!
            //============================================================================
            if (ms < 100)
            {
                TimeSpan ts;
                while (true)
                {
                    ts = DateTime.Now - tmRef;
                    if (ts.TotalMilliseconds >= ms)
                    {
                        tmRef = DateTime.Now;
                        break;
                    }
                }
            }
            else
            {
                System.Threading.Thread.Sleep(ms);
                tmRef = DateTime.Now;
            }
        }
    }
}
