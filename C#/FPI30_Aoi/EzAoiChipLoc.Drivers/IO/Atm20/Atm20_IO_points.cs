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

using EzIO.Mem;
using EzPlc.Omron;
using System;
using System.Collections.Generic;

namespace EzAoiChipLocQC.Drivers.IO
{
    partial class Atm20_IO
    {
        #region PRIVATE_IO_POINTS_DATA
        BaseIO _baseIO;
        MiscIO _miscIO;
        Dictionary<int, AxisIO> _axisIOs;
        #endregion

        #region PRIVATE_FUNCTIONS
        void BuildIoPoints(IoMemory ioMem)
        {
            // 基本點位 (需要高速頻繁讀寫)
            _baseIO = new BaseIO(ioMem);

            // 字串型點位 (比較耗時)
            _miscIO = new MiscIO(ioMem);

            // 軸控點位
            _axisIOs = new Dictionary<int, AxisIO>();
            for (int id = 0; id < 3; id++)
            {
                _axisIOs.Add(id, new AxisIO(ioMem, id));
            }

            // 編排 虛擬位址編號 (Optional)
            GenerateViraulAddresses(ioMem);
        }
        void GenerateViraulAddresses(IoMemory ioMem)
        {
            int virtualAddress = 0;
            foreach (BasePointEnum e in Enum.GetValues(typeof(BasePointEnum)))
            {
                var p = ioMem.OmronPoint(e);
                if (p?.Address is OmronAddress omronAddress)
                    omronAddress.Address = (ushort)virtualAddress++;

                //// Description
                //var desc = JetEazy.QxNums.GetEnumDescription(e);
                //if (string.IsNullOrEmpty(desc))
                //    desc = e.ToString();
                //p.SetDescription(desc);
            }

            virtualAddress = 1000;
            foreach (MiscPointEnum e in Enum.GetValues(typeof(MiscPointEnum)))
            {
                var p = ioMem.OmronPoint(e);
                if (p?.Address is OmronAddress omronAddress)
                    omronAddress.Address = (ushort)virtualAddress++;

                //// Description
                //var desc = JetEazy.QxNums.GetEnumDescription(e);
                //if (string.IsNullOrEmpty(desc))
                //    desc = e.ToString();
                //p.SetDescription(desc);
            }

            virtualAddress = 2000;
            for(int id = 0, N = _axisIOs.Count; id < N; id++)
            {
                int address = virtualAddress + 1000 * id;
                foreach (AxisPointEnum e in Enum.GetValues(typeof(AxisPointEnum)))
                {
                    var p = ioMem.OmronPoint(id, e);
                    if (p?.Address is OmronAddress omronAddress)
                        omronAddress.Address = (ushort)address++;
                }
            }
        }
        List<IoPoint> GetAutoScanPoints()
        {
            var points = new List<IoPoint>();

            foreach (BasePointEnum e in Enum.GetValues(typeof(BasePointEnum)))
            {
                var attrib = EzPointEnumAttribute.GetAttribute(e);
                bool needsAutoScan = (attrib != null && attrib.AutoScan);
                if (!needsAutoScan)
                    continue;

                var point = IoMem.OmronPoint(e);
                if (point != null)
                    points.Add(point);
            }

            for (int id = 0, N = _axisIOs.Count; id < N; id++)
            {
                foreach (AxisPointEnum e in Enum.GetValues(typeof(AxisPointEnum)))
                {
                    var attrib = EzPointEnumAttribute.GetAttribute(e);
                    bool needsAutoScan = (attrib != null && attrib.AutoScan);
                    if (!needsAutoScan)
                        continue;

                    var point = IoMem.OmronPoint(id, e);
                    if (point != null)
                        points.Add(point);
                }
            }

            return points;
        }
        #endregion

        public BaseIO GetBaseIO()
        {
            return _baseIO;
        }
        public MiscIO GetMiscIO()
        {
            return _miscIO;
        }
        public AxisIO GetAxisIO(int id)
        {
            if (_axisIOs.TryGetValue(id, out var axisIO))
                return axisIO;
            return null;
        }

        #region INTERNAL_CLASSES
        public class BaseIO
        {
            public readonly OmronIoVarBool bSyncClock;
            public readonly OmronIoVarBool bSoftwareReady;
            public readonly OmronIoVarInt32 iRecipeNum;
            public readonly OmronIoVarInt32 iScanStage;
            public readonly OmronIoVarBool bScanStart;
            public readonly OmronIoVarBool bScanReady;
            public readonly OmronIoVarBool bScanDone;
            public readonly OmronIoVarInt32 iScanStatus;
            public readonly OmronIoVarInt32 iScanResult;
            public readonly OmronIoVarBool bQRUsed;
            public readonly OmronIoVarBool bQRJudgeUsed;
            public readonly OmronIoVarInt32 iFlyStart;
            public readonly OmronIoVarBool bFlyReady;
            public readonly OmronIoVarBool bFlyDone;
            public BaseIO(IoMemory ioMem)
            {
                bSyncClock =        ioMem.OmronPoint(BasePointEnum.bSyncClock) as OmronIoVarBool;
                bSoftwareReady =    ioMem.OmronPoint(BasePointEnum.bSoftwareReady) as OmronIoVarBool;
                iRecipeNum =        ioMem.OmronPoint(BasePointEnum.iRecipeNum) as OmronIoVarInt32;
                iScanStage =        ioMem.OmronPoint(BasePointEnum.iScanStage) as OmronIoVarInt32;
                bScanStart =        ioMem.OmronPoint(BasePointEnum.bScanStart) as OmronIoVarBool;
                bScanReady =        ioMem.OmronPoint(BasePointEnum.bScanReady) as OmronIoVarBool;
                bScanDone =         ioMem.OmronPoint(BasePointEnum.bScanDone) as OmronIoVarBool;
                iScanStatus =       ioMem.OmronPoint(BasePointEnum.iScanStatus) as OmronIoVarInt32;
                iScanResult =       ioMem.OmronPoint(BasePointEnum.iScanResult) as OmronIoVarInt32;
                bQRUsed =           ioMem.OmronPoint(BasePointEnum.bQRUsed) as OmronIoVarBool;
                bQRJudgeUsed =      ioMem.OmronPoint(BasePointEnum.bQRJudgeUsed) as OmronIoVarBool;
                iFlyStart =         ioMem.OmronPoint(BasePointEnum.iFlyStart) as OmronIoVarInt32;
                bFlyReady =         ioMem.OmronPoint(BasePointEnum.bFlyReady) as OmronIoVarBool;
                bFlyDone =          ioMem.OmronPoint(BasePointEnum.bFlyDone) as OmronIoVarBool;
            }
        }

        public class AxisIO
        {
            public readonly OmronIoVarBool bHomed;
            public readonly OmronIoVarBool bAbsMoveDone;
            public readonly OmronIoVarBool bLimitP;
            public readonly OmronIoVarBool bLimitN;
            public readonly OmronIoVarBool bOrg;
            public readonly OmronIoVarBool bEnable;
            public readonly OmronIoVarBool bError;
            public readonly OmronIoVarBool bStart;
            public readonly OmronIoVarBool bHomeStart;
            public readonly OmronIoVarBool bJogFW;
            public readonly OmronIoVarBool bJogNW;
            public readonly OmronIoVarFloat32 rSped;
            public readonly OmronIoVarFloat32 rFPOS;
            public readonly OmronIoVarFloat32 rRPOS;
            public AxisIO(IoMemory ioMem, int id)
            {
                bHomed =        ioMem.OmronPoint(id, AxisPointEnum.bHomed) as OmronIoVarBool;
                bAbsMoveDone =  ioMem.OmronPoint(id, AxisPointEnum.bAbsMoveDone) as OmronIoVarBool;
                bLimitP =       ioMem.OmronPoint(id, AxisPointEnum.bLimitP) as OmronIoVarBool;
                bLimitN =       ioMem.OmronPoint(id, AxisPointEnum.bLimitN) as OmronIoVarBool;
                bOrg =          ioMem.OmronPoint(id, AxisPointEnum.bCalibrationCam) as OmronIoVarBool;
                bEnable =       ioMem.OmronPoint(id, AxisPointEnum.bEnable) as OmronIoVarBool;
                bError =        ioMem.OmronPoint(id, AxisPointEnum.bError) as OmronIoVarBool;
                bStart =        ioMem.OmronPoint(id, AxisPointEnum.bStart) as OmronIoVarBool;
                bHomeStart =    ioMem.OmronPoint(id, AxisPointEnum.bHomeStart) as OmronIoVarBool;
                bJogFW =        ioMem.OmronPoint(id, AxisPointEnum.bJogFW) as OmronIoVarBool;
                bJogNW =        ioMem.OmronPoint(id, AxisPointEnum.bJogNW) as OmronIoVarBool;
                rSped =         ioMem.OmronPoint(id, AxisPointEnum.rSped) as OmronIoVarFloat32;
                rFPOS =         ioMem.OmronPoint(id, AxisPointEnum.rFPOS) as OmronIoVarFloat32;
                rRPOS =         ioMem.OmronPoint(id, AxisPointEnum.rRPOS) as OmronIoVarFloat32;
            }
        }

        public class MiscIO
        {
            public readonly OmronIoVarStr sLotID;
            public readonly OmronIoVarStr sStripID;
            public readonly OmronIoVarStr sRecipeName;
            public MiscIO(IoMemory ioMem)
            {
                sLotID =      ioMem.OmronPoint(MiscPointEnum.sLotID) as OmronIoVarStr;
                sStripID =    ioMem.OmronPoint(MiscPointEnum.sStripID) as OmronIoVarStr;
                sRecipeName = ioMem.OmronPoint(MiscPointEnum.sRecipeName) as OmronIoVarStr;
            }
        }
        #endregion
    }
}
