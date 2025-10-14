#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-11 開始適用於 2.6.x.x 以後的版本
 *      2025-08-29 開始準備重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace LaserAlignDX.Mvc.Ctrl
{
    /// <summary>
    /// 長時間無限循環自測
    /// </summary>
    public partial class GaContinousSelfTestCtrl
    {
        const string IMG_PATH = @"D:\Lloydz\JetEazy\Projects\FPI300\Images\2025_0930_#4";
        
        #region MACHINE
        MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE; }
        }
        #endregion

        #region GLOBAL_MESS_RECIPES
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        IProcessRunFPI _aoiModel => _sysModel?.AoiModel;
        #endregion

        #region PRIVATE_DATA
        int _runCount = 0;
        #endregion

        public void Init()
        {
            //_lsProcess.OnCompleted += (s, e) => ((Action)restartTest).BeginInvoke(null, null);
            _aoiModel.OnAoiEnd += (s, e) => ((Action)restartTest).BeginInvoke(null, null);
        }

        private void restartTest()
        {
            if(getLocatedChipsCount() > 0 &&_aoiModel.xScanInspectMode != ScanInspectMode.NOTRAY)
                return;

            string[] files = System.IO.Directory.GetFiles(IMG_PATH, "*.jpg");
            Array.Sort(files);
            _runCount = (_runCount + 1) % files.Length;
            string file = files[_runCount];
            System.Threading.Thread.Sleep(1500);

            var bmp = GaImageUtil.LoadBigImage(file);
            var srcName = System.IO.Path.GetFileNameWithoutExtension(file);
            _sysModel.LineScanImageHolder.TakeOver(bmp, srcName);

            var plcIO = MACHINE.PLCIO;
            if (plcIO != null)
            {
                plcIO.bSoftwareReady = false;
                System.Threading.Thread.Sleep(500);
                plcIO.bSoftwareReady = true;
                plcIO.bFlyReady = true;
            }
        }

        private int getLocatedChipsCount()
        {
            int count = 0;
            foreach (var cell in _xRecipe.xRegionCells)
            {
                if (cell?.ChipData?.ChipBox2D!=null)
                    count++;
                //if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                //    passCount++;
                //else if (cell.inspectReason != InspectReason.INS_ALIGNERR)
                //    ngCount++;
            }
            return count;
        }
    }
}
