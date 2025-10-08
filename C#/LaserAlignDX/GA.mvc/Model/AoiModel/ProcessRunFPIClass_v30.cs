#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-01 開始重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Threading;
using ErrCodes = LaserAlignDX.Mvc.Model.ErrCodes;
using ProcessEventArgs = NeedleX.ProcessSpace.ProcessEventArgs;


namespace LaserAlignDX.AoiModel.V3
{
    public class ProcessRunFPIClass : AoiModelBase, IProcessRunFPI
    {
        #region SINGLETON
        protected ProcessRunFPIClass()
        {

        }
        private static ProcessRunFPIClass _instance = null;
        #endregion

        public static ProcessRunFPIClass Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProcessRunFPIClass();
                    _instance.initSubModels();
                }
                return _instance;
            }
        }
        public static void DisposeAll()
        {
            _instance?.Dispose();
            _instance = null;
        }
        public void Dispose()
        {
            // To DO: 請把自己清乾淨
            //_DisposeTools();
        }

        #region GLOBAL_MESS
        //RecipeFPIX3Class xRecipe
        //{
        //    get { return RecipeFPIX3Class.Instance; }
        //}
        //InspectX3ParaClass xInspect
        //{
        //    get { return InspectX3ParaClass.Instance; }
        //}
        #endregion

        #region KENERL_MEMBERS
        ///// <summary>
        ///// 2025-08-28 LETIAN: 巨圖 統一由 TravellerBigImagesHolder 保管其生命週期
        ///// </summary>
        //public GaBigImageHolder LineScanCamImageHolder => _sysModel.LineScanImageHolder;
        ///// <summary>
        ///// 2025-09-10 新座標轉換
        ///// </summary>
        //TravellerTransforms _transformModel => _sysModel.TransformsModel;
        ///// <summary>
        ///// SystemModel
        ///// </summary>
        //ITravelerModel _sysModel => GaMvcConfig.SysModel;
        ///// <summary>
        ///// 當下的載台
        ///// </summary>
        //CarrierEnum getActiveCarrierID()
        //{
        //    return _sysModel.ActiveCarrierID;
        //}
        #endregion

        #region SUB_MODELS
        AoiModel_ChipLoc _aoiChipLoc = new AoiModel_ChipLoc();
        AoiModel_ChipMeasure _aoiChipMeasure = new AoiModel_ChipMeasure();
        AoiModel_EmptyTray _aoiEmptyTray = new AoiModel_EmptyTray();
        #endregion

        void initSubModels()
        {
            var subModels = new AoiModelBase[] { _aoiChipLoc, _aoiChipMeasure, _aoiEmptyTray };
            foreach(var subModel in subModels)
            {
                //model.OnAoiBegin+=
                subModel.OnAoiProgressing += SubModel_OnAoiProgressing;
                subModel.OnAoiBegin += SubModel_OnAoiBegin;
                subModel.OnAoiEnd += SubModel_OnAoiEnd;
                subModel.OnError += SubModel_OnError;
            }
        }

        #region EVENT_HANLDERS
        private void SubModel_OnError(object sender, ProcessEventArgs e)
        {
            fire_AoiError(sender, e);
        }
        private void SubModel_OnAoiProgressing(object sender, GaProgressEventArgs e)
        {
            fire_AoiProgressing(sender, e);
        }
        private void SubModel_OnAoiBegin(object sender, GaProgressEventArgs e)
        {
            //>>> fire_AoiBegin();
            if (e != null && e.Message != null)
                fire_AoiBegin(e.Message);
        }
        private void SubModel_OnAoiEnd(object sender, GaProgressEventArgs e)
        {
            // 不要轉發個別 AoiEnd
            // fire_AoiEnd();
        }
        #endregion

        public ScanInspectMode xScanInspectMode
        {
            get;
            set;
        }
        public bool QrUsed
        {
            get { return _aoiChipMeasure.QrUsed; }
            set { _aoiChipMeasure.QrUsed = value; }
        }
        public bool QrJudged
        {
            get { return _aoiChipMeasure.QrJudged; }
            set { _aoiChipMeasure.QrJudged = value; }
        }

        #region PUBLIC_RESULT_PACKERS_FOR_PLC
        /// <summary>
        /// 单颗的线扫结果(预留300个) PLC用此信号来将每颗产品放到对应的Tray盘
        /// </summary>
        /// <returns>ARRAY[0..299] OF INT PC->PLC 单颗结果, 1:OK 2:外观NG, 3:空, 4:读码NG, 9:切割NG </returns>
        public int[] GetSingleResult()
        {
            //PC->PLC 单颗结果,1-Ok,2-外观Ng,3-空,4-读码NG,9-切割NG

            var xRecipe = _xRecipe;

            int[] states = new int[xRecipe.xRegionCells.Count];
            int i = 0;
            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                    states[i] = 1;
                else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                    states[i] = 3;
                else if (cell.inspectReason == InspectReason.INS_2DERR || cell.inspectReason == InspectReason.INS_2DMAPNG)
                    states[i] = 4;
                else if (cell.inspectReason == InspectReason.INS_CUTTINGERR)
                    states[i] = 9;
                else
                    states[i] = 2;
                i++;
            }
            return states;
        }
        /// <summary>
        /// 单颗产品的读码比对结果(预留300个) 视觉软件需要将读码结果保存在本地或服务器
        /// </summary>
        /// <returns>ARRAY[0..299] OF INT PC->PLC 读码结果, 1:OK, 2:比对NG, 3:空, 4:有码未读到</returns>
        public int[] GetQrResult()
        {
            //PC->PLC 读码结果,1-Ok,2-比对Ng,3-空,4-有码未读到

            var xRecipe = _xRecipe;
            int[] states = new int[xRecipe.xRegionCells.Count];
            int i = 0;
            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                    states[i] = 1;
                else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                    states[i] = 3;
                else if (cell.inspectReason == InspectReason.INS_2DERR)
                    states[i] = 4;
                else if (cell.inspectReason == InspectReason.INS_2DMAPNG)
                    states[i] = 2;
                else
                    states[i] = 1;
                i++;
            }
            return states;
        }
        /// <summary>
        /// 单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个) 线扫引导功能启用时PLC需要用到这些值
        /// </summary>
        /// <returns>ARRAY[0..899] OF REAL PC->PLC 线扫偏移值XYR</returns>
        public float[] GetScanOffset()
        {
            //PC->PLC 线扫偏移值XYR
            //单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个)

            var xRecipe = _xRecipe;

            float[] states = new float[xRecipe.xRegionCells.Count * 3];
            int i = 0;
            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                {
                    states[i] = cell.RunX;
                    states[i + 1] = cell.RunY;
                    states[i + 2] = cell.RunAngle;
                }
                else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                {
                    states[i] = 0;
                    states[i + 1] = 0;
                    states[i + 2] = 0;

                }
                else
                {
                    states[i] = cell.RunX;
                    states[i + 1] = cell.RunY;
                    states[i + 2] = cell.RunAngle;

                }
                i += 3;
            }
            return states;
        }
        #endregion

        public ErrCodes BuildMicroChipTransform(SizeF targetSize, EzLSD.LineSegment[] lines, Bitmap regionBmp, RectangleF regionRoi)    
        {
            //(1) 晶粒定位
            //      chipData.Roi = cellRoi;
            //      chipData.ChipBox2D = chipBox2D;
            //      chipData.PadsGrid = chipMatcher.GetResultPadsGrid();
            //      chipData.PadsGrid.Offset(cellRoi.X, cellRoi.Y);
            bool ok = _aoiChipLoc.LocateOneChip(regionBmp, regionRoi, out var chipData);
            if (!ok || chipData == null)
                return ErrCodes.ERR_NO_CHIP_LOCATION;

            //(2) 檢查 PadsGrid
            if (_xRecipe.InspectParams.xAlgorithm == MatchAlgorithmEnum.GridMatch && chipData.PadsGrid == null)
            {
                return ErrCodes.ERR_NO_CHIP_PADS;
            }
            
            //(3) 建立 Micro Transform
            var carrierID = getActiveCarrierID();
            var microTrf = _sysModel.GetMicroTransform(carrierID);
            var err = microTrf.BuildMicroTransform(targetSize, lines, chipData);

            //(4) 保存參數
            if (err == ErrCodes.OK)
                microTrf.Save(null);

            return err;
        }

        public override void Run()
        {
            #region DIRECTORIES_可以搬到後面處理_才不會有遲滯感覺
            //if (INI.Instance.IsSaveDebugBMP)
            //{
            //    m_PicResultPath = $"{INI.Instance.ResultImagePath}\\linescanImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";
            //    if (!System.IO.Directory.Exists(m_PicResultPath))
            //    {
            //        System.IO.Directory.CreateDirectory(m_PicResultPath);
            //    }
            //}
            //if (INI.Instance.IsSaveDebugOrgBmp)
            //{
            //    m_PicResultOrgPath = $"{INI.Instance.ResultImagePath}\\linescanImageOrg\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";
            //    if (!System.IO.Directory.Exists(m_PicResultOrgPath))
            //    {
            //        System.IO.Directory.CreateDirectory(m_PicResultOrgPath);
            //    }
            //}
            #endregion

            GaUtil.LOG($"{GetType().Name} [V3.0] Run", Color.Purple);

            ThreadStart runFunc;
            if (xScanInspectMode == ScanInspectMode.NOTRAY)
                runFunc = _RunEmptyTray;
            else
                runFunc = _RunChipLocAndMeasurement;

            Thread thread = new Thread(runFunc);
            thread.Priority = ThreadPriority.Highest;
            thread.IsBackground = false;
            thread.Start();
        }

        private void _RunChipLocAndMeasurement()
        {
            try
            {
                fire_AoiBegin();
                markRunStart();

                // 定位
                _aoiChipLoc.Run();
                var cellGroups = _aoiChipLoc.CellGroups;

                // 量測
                _aoiChipMeasure.SetCellGroups(cellGroups);
                _aoiChipMeasure.Run();

                // 釋放 多執行續的 CellGroups
                _aoiChipLoc.DisposeCellGroups();

                bool pass = _CheckChipsTotalPass();
                markRunEnd(pass);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                markRunEnd(false);
                //fire_AoiEnd();
                var errCode = Mvc.Model.ErrCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                fire_AoiError(errCode, errMsg);
                _LOG_ERROR(ex, "_RunChipLocAndMeasurement");
            }
            finally
            {
                // 以後如果 其他內部 IDisposable 物件生命週期管理 優化完成
                // 可以 移除 GC 
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
        private void _RunEmptyTray()
        {
            try
            {
                fire_AoiBegin();
                markRunStart();

                _aoiEmptyTray.Run();

                markRunEnd(_aoiEmptyTray.IsPass);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                markRunEnd(false);
                //fire_AoiEnd();
                var errCode = Mvc.Model.ErrCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                fire_AoiError(errCode, errMsg);
                _LOG_ERROR(ex, "_RunEmptyTray");
            }
        }
        private bool _CheckChipsTotalPass()
        {
            bool optUsePercentage = _xRecipe.InspectParams.optUseTotalNgPercentage && _xRecipe.InspectParams.optChipMeasurement;

            int passCount = 0;
            int ngCount = 0;
            foreach (var cell in _xRecipe.xRegionCells)
            {
                if (cell == null)
                    continue;
                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                    passCount++;
                else if (cell.inspectReason != InspectReason.INS_ALIGNERR)
                    ngCount++;
            }

            bool isPass;
            if (optUsePercentage)
            {
                var total = passCount + ngCount;
                if (total > 0)
                    isPass = 100.0 * ngCount / total < _xRecipe.InspectParams.xTotalNgPercentage;
                else
                    isPass = true;
            }
            else
            {
                isPass = ngCount == 0;
            }
            return isPass;
        }
    }
}
