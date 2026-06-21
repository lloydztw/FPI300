#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-21 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiChipLocQC.Drivers.IO;
using EzComm;
using System;

namespace EzAoiChipLocQC.Machine
{
    /// <summary>
    /// 德龍 ATM20 機台
    /// </summary>
    public class Traveller_Atm20_Machine : ITravellerQcMachine
    {
        #region SINGLETON
        static Traveller_Atm20_Machine _instance;
        protected Traveller_Atm20_Machine()
        {
        }
        #endregion

        public static Traveller_Atm20_Machine Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Traveller_Atm20_Machine();
                return _instance;
            }
        }

        public void Dispose()
        {
            if(_plcIO is IDisposable d)
                d.Dispose();
            _plcIO = null;
            _instance = null;
        }

        public void Init(EzTcpIpSettings tcpSettings)
        {
            bool isSim = tcpSettings.IsSim;
            if (_plcIO == null)
            {
                var plc = new Atm20_PLC();
                plc.Init(tcpSettings);
                _plcIO = plc;
            }
        }

        #region PRIVATE_KERNEL_DATA
        private IPlcAtm20 _plcIO;
        #endregion

        public IPlcAtm20 PLC
        {
            get => _plcIO;
        }
    }
}
