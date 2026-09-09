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


using JetEazy.Lang;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Windows.Forms;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace.IOSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace LaserAlignDX.Mvc.Ctrl
{
    /// <summary>
    /// 長時間無限循環自測
    /// </summary>
    public partial class GaContinousSelfTestCtrl
    {
        public static bool OPT_INCLUDE_FLY_AOI = false;
        string IMG_PATH = "";

        #region SINGLETON
        static GaContinousSelfTestCtrl _instance = null;
        GaContinousSelfTestCtrl()
        {
        }
        #endregion

        public static GaContinousSelfTestCtrl Instance
        {
            get
            {
                if(_instance == null )
                {
                    _instance = new GaContinousSelfTestCtrl();
                    _instance.init();
                }
                return _instance;
            }
        }

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

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(IMG_PATH);
        }

        public string BrowseImageFileThenStart()
        {
            if (!Traveller106.Universal.IsNoUseCCD)
                return null;

            //var ret = MessageBox.Show("即將進行 循環自測\n\r\n\r是否也要包含 飛拍 模擬?", "離線循環自測", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            var ret = QMessageBox.Show(Prompts.Question_To_Simulate_With_FlyCam, null, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (DialogResult.Cancel == ret)
                return null;

            string imgFile = GaUtil.BrowseImageFile();
            if (string.IsNullOrEmpty(imgFile))
                return null;

            IMG_PATH = System.IO.Path.GetDirectoryName(imgFile);
            OPT_INCLUDE_FLY_AOI = (DialogResult.Yes == ret);

            turn_off_ini_image_savings();
            return imgFile;
        }

        #region PRIVATE_FUNCTIONS

        private void init()
        {
            if (!Traveller106.Universal.IsNoUseCCD)
                return;

            _aoiModel.OnAoiEnd += (s, e) =>
            {
                var plcIO = MACHINE?.PLCIO;
                if (plcIO == null || !plcIO.bSoftwareReady)
                {
                    // 結束循環自測
                    end();
                    return;
                }

                if (OPT_INCLUDE_FLY_AOI)
                    ((Action)restartTest_full_processes).BeginInvoke(null, null);
                else
                    ((Action)restartTest_only_chip_loc).BeginInvoke(null, null);
            };
        }

        private void end()
        {
            IMG_PATH = null;
            reload_ini_settings();
        }

        /// <summary>
        /// 再次 啟動 模擬所有流程
        /// </summary>
        private void restartTest_full_processes()
        {
            if (IsEmpty()) return;
            if (getLocatedChipsCount() > 0 &&_aoiModel.xScanInspectMode != ScanInspectMode.NOTRAY)
                return;

            System.Threading.Thread.Sleep(1500);

            // 無限循環
            bool go = loadNextImageToModel(out int id, out string lotID, circulate: true);
            if (!go)
                return;

            var plcIO = MACHINE.PLCIO as MainFPIX3IOSim;
            if (plcIO != null)
            {
                plcIO.bSoftwareReady = false;
                System.Threading.Thread.Sleep(500);
                plcIO.bSoftwareReady = true;
                plcIO.bFlyReady = true;
                //plcIO.simLotID($"Lot_SIM_{id:000}");
                //plcIO.simStripID($"Strip_SIM_{id:000}");
                plcIO.simLotID(lotID);
            }
        }

        /// <summary>
        /// 再次 啟動  模擬定位
        /// </summary>
        private void restartTest_only_chip_loc()
        {
            if (IsEmpty()) return;
            System.Threading.Thread.Sleep(500);

            var plcIO = MACHINE?.PLCIO as MainFPIX3IOSim;
            if (plcIO != null)
            {
                plcIO.bScanStart = false;
                plcIO.bFlyReady = false;
            }

            var process = LineScanProcess.Instance;
            process.Stop();

            // 跑完最後檔案後, 會自動停止
            bool go = loadNextImageToModel(out int id, out string lotID, circulate: false);
            if (!go)
            {
                plcIO.bSoftwareReady = false;
                end();
                return;
            }

            if (plcIO != null)
            {
                plcIO.iScanStatus = 0;
                plcIO.bScanStart = false;
                plcIO.bFlyReady = false;
                //plcIO.simLotID($"Lot_SIM_{id:000}");
                //plcIO.simStripID($"Strip_SIM_{id:000}");
                plcIO.simLotID(lotID);
            }

            process.Start();
        }

        private bool loadNextImageToModel(out int index, out string lotID, bool circulate)
        {
            bool go = getNextImgFileName(out string fileName, out index, out int totalNumber, circulate);
            lotID = $"Lot_SIM_{index:000}";

            if (go)
            {
                var bmp = GaImageUtil.LoadBigImage(fileName);
                var srcName = System.IO.Path.GetFileNameWithoutExtension(fileName);
                if (srcName.Contains("-"))
                    lotID = srcName.Split('-')[0].Trim();
                _sysModel.LineScanImageHolder.TakeOver(bmp, srcName + $" ({index+1}/{totalNumber})");
            }
            return go;
        }

        private bool getNextImgFileName(out string imgFileName, out int index, out int totalNumber, bool circulate)
        {
            string[] files = System.IO.Directory.GetFiles(IMG_PATH, "*.jpg");
            Array.Sort(files);
            totalNumber = files.Length;

            if (totalNumber == 0)
            {
                imgFileName = null;
                index = 0;
                return false;
            }

            index = ++_runCount;

            if (index >= totalNumber)
            {
                if (!circulate)
                {
                    // 自動終止.
                    index = _runCount = 0;
                    imgFileName = null;
                    return false;
                }
                else
                {
                    index = _runCount = 0;
                }
            }

            imgFileName = files[index];
            return true;
        }
        
        private int getLocatedChipsCount()
        {
            int count = 0;
            foreach (var cell in PlcDataPacker.IterFinalResultCells())
            {
                //if (cell?.ChipData?.ChipQuad2D!=null)
                //    count++;
                if (cell != null && cell.IsLocated())
                    count++;
            }
            return count;
        }

        private void turn_off_ini_image_savings()
        {
            var ini = Traveller106.INI.Instance;
            ini.IsSaveDebugBmp = false;
            ini.IsSaveDebugOrgBmp = false;
            //ini.IsSaveStripImage = false;
            ini.IsSaveTestImage = false;
        }

        private void reload_ini_settings()
        {
            var ini = Traveller106.INI.Instance;
            ini.Load();
        }

        #endregion
    }
}
