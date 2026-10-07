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

using JetEazy.Lang;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using System;
using System.Drawing;
using System.Threading;

using AoiModel_ChipLoc = LaserAlignDX.AoiModel.v31.AoiModel_ChipLoc;
using AoiModel_ChipMeasure = LaserAlignDX.AoiModel.V38.Combo.AoiModel_ChipMeasure;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;
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
        public override void Dispose()
        {
            // 請把自己清乾淨
            _aoiChipLoc?.Dispose();
            _aoiChipLoc = null;            
            _aoiChipMeasure?.Dispose();
            _aoiChipMeasure = null;
            _aoiChipDefects?.Dispose();
            _aoiChipDefects = null;
            _aoiBadConnInspector?.Dispose();
            _aoiBadConnInspector = null;
            _aoiFlyCam?.Dispose();
            _aoiFlyCam = null;
            _mvdQrCodeDecorder?.Dispose();
            _mvdQrCodeDecorder = null;
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
        AoiModel_EmptyTray _aoiEmptyTray = new AoiModel_EmptyTray();
        AoiModel_ChipLoc _aoiChipLoc = new AoiModel_ChipLoc();
        AoiModel_ChipMeasure _aoiChipMeasure = new AoiModel_ChipMeasure();
        AoiModel_Defects _aoiChipDefects = new AoiModel_Defects();
        AoiModel_BadConnInpector _aoiBadConnInspector = new AoiModel_BadConnInpector();
        AoiModel_QrCode _aoiQrDecoder = new AoiModel_QrCode();
        AoiModel_FlyCam _aoiFlyCam = new AoiModel_FlyCam();
        Mvd2DReaderClass _mvdQrCodeDecorder = new Mvd2DReaderClass();
        #endregion

        void initSubModels()
        {
            var subModels = new AoiModelBase[] { 
                _aoiChipLoc,
                _aoiChipMeasure,
                //_aoiChipMeasure.BaseQ,
                //_aoiChipMeasure.BaseN,
                _aoiChipDefects,
                _aoiBadConnInspector,
                _aoiQrDecoder, 
                _aoiEmptyTray 
            };
            foreach (var subModel in subModels)
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
            get { return _aoiQrDecoder.QrUsed; }
            set { _aoiQrDecoder.QrUsed = value; }
        }
        public bool QrJudged
        {
            get { return _aoiQrDecoder.QrJudged; }
            set { _aoiQrDecoder.QrJudged = value; }
        }

        #region PUBLIC_RESULT_PACKERS_FOR_PLC
        /// <summary>
        /// 单颗的线扫结果(预留300个) PLC用此信号来将每颗产品放到对应的Tray盘
        /// </summary>
        /// <returns>ARRAY[0..299] OF INT PC->PLC 单颗结果, 1:OK 2:外观NG, 3:空, 4:读码NG, 9:切割NG </returns>
        public int[] GetSingleResult()
        {
            #region OLD_CODE
            ////PC->PLC 单颗结果, 1-Ok, 2-外观Ng, 3-空, 4-读码NG, 8-切割偏移NG, 9-切割NG

            //bool optUsePercentage = _xRecipe.InspectParams.optUseTotalNgPercentage && _xRecipe.InspectParams.optChipMeasurement;
            //bool forceAllPass = (optUsePercentage && xScanInspectMode != ScanInspectMode.NOTRAY && IsPass);

            //int[] states = new int[_xRecipe.xRegionCells.Count];
            //int i = 0;
            //foreach (RegionCellX3Class cell in IterResultCells(_xRecipe.xRegionCells))
            //{
            //    if (forceAllPass)
            //    {
            //        bool isEmpty = cell == null || !cell.IsLocated();
            //        states[i] = isEmpty ? 3 : 1;
            //    }
            //    else
            //    {
            //        if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
            //            states[i] = 1;
            //        else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
            //            states[i] = 3;
            //        else if (cell.inspectReason == InspectReason.INS_2DERR || cell.inspectReason == InspectReason.INS_2DMAPNG)
            //            states[i] = 4;
            //        else if (cell.inspectReason == InspectReason.INS_CUTTINGERR)
            //            states[i] = 9;
            //        else if (cell.inspectReason == InspectReason.INS_PADEDGEGAPERR)
            //            states[i] = 8;
            //        else
            //            states[i] = 2;
            //    }
            //    i++;
            //}

            //return states;
            #endregion

            bool optUsePercentage = _xRecipe.InspectParams.optUseTotalNgPercentage && _xRecipe.InspectParams.optChipMeasurement;
            bool forceAllPass = (optUsePercentage && xScanInspectMode != ScanInspectMode.NOTRAY && this.IsPass);

            return PlcDataPacker.GetSingleResult(forceAllPass);
        }
        /// <summary>
        /// 单颗产品的读码比对结果(预留300个) 视觉软件需要将读码结果保存在本地或服务器
        /// </summary>
        /// <returns>ARRAY[0..299] OF INT PC->PLC 读码结果, 1:OK, 2:比对NG, 3:空, 4:有码未读到</returns>
        public int[] GetQrResult()
        {
            #region OLD_CODE
            ////PC->PLC 读码结果,1-Ok,2-比对Ng,3-空,4-有码未读到

            //int[] states = new int[_xRecipe.xRegionCells.Count];
            //int i = 0;
            //foreach (RegionCellX3Class cell in IterResultCells(_xRecipe.xRegionCells))
            //{
            //    if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
            //        states[i] = 1;
            //    else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
            //        states[i] = 3;
            //    else if (cell.inspectReason == InspectReason.INS_2DERR)
            //        states[i] = 4;
            //    else if (cell.inspectReason == InspectReason.INS_2DMAPNG)
            //        states[i] = 2;
            //    else
            //        states[i] = 1;
            //    i++;
            //}
            //return states;
            #endregion

            return PlcDataPacker.GetQrResult();
        }
        /// <summary>
        /// 单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个) 线扫引导功能启用时PLC需要用到这些值
        /// </summary>
        /// <returns>ARRAY[0..899] OF REAL PC->PLC 线扫偏移值XYR</returns>
        public float[] GetScanOffset()
        {
            #region OLD_CODE
            ////PC->PLC 线扫偏移值XYR
            ////单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个)

            //float[] states = new float[_xRecipe.xRegionCells.Count * 3];
            //int i = 0;
            //foreach (RegionCellX3Class cell in IterResultCells(_xRecipe.xRegionCells))
            //{
            //    if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
            //    {
            //        states[i] = cell.RunX;
            //        states[i + 1] = cell.RunY;
            //        states[i + 2] = cell.RunAngle;
            //    }
            //    else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
            //    {
            //        states[i] = 0;
            //        states[i + 1] = 0;
            //        states[i + 2] = 0;

            //    }
            //    else
            //    {
            //        states[i] = cell.RunX;
            //        states[i + 1] = cell.RunY;
            //        states[i + 2] = cell.RunAngle;

            //    }
            //    i += 3;
            //}
            //return states;
            #endregion

            return PlcDataPacker.GetScanOffset();
        }
        #endregion

        public ErrorCodes BuildMicroChipTransform(SizeF targetSize, EzLSD.LineSegment[] lines, Bitmap regionBmp, RectangleF regionRoi, bool isLocalLineCoord = false)
        {
            ErrorCodes err = ErrorCodes.OK;

            //(0) 如果 lines 為 Local Camera Coordinates, 則轉換成 Fullfov Camera Coordinates.
            if (isLocalLineCoord)
            {
                lines = Array.ConvertAll(lines, ln => ln?.Clone());
                foreach (var ln in lines)
                    ln?.Offset(regionRoi.X, regionRoi.Y);
            }

            using (var workBmp = (Bitmap)regionBmp.Clone())
            {
                //(1) 晶粒定位
                bool ok = _aoiChipLoc.LocateOneChip(workBmp, ref regionRoi, out var chipData);
                if (!ok || chipData == null)
                    return ErrorCodes.ERR_NO_CHIP_LOCATION;

                //(2) 檢查 PadsGrid
                if (_xRecipe.InspectParams.xAlgorithm == MatchAlgorithmEnum.GridMatch && chipData.PadsGrid == null)
                {
                    return ErrorCodes.ERR_NO_CHIP_PADS;
                }

                //(3) 建立 Micro Transform
                var carrierID = getActiveCarrierID();
                var microTrf = _sysModel.GetMicroTransform(carrierID);
                err = microTrf.BuildMicroTransform(targetSize, lines, chipData);

                //(4) 保存參數
                if (err == ErrorCodes.OK)
                    microTrf.Save(null);
            }

            if (err == ErrorCodes.OK)
                _aoiChipMeasure.AnalyzeGoldenData();

            return err;
        }
        public ErrorCodes BuildMicroChipTransform(LineBorderPairsCollection lineBorderPairs, Bitmap regionBmp, RectangleF regionRoi)
        {
            //(0) 是否 使用 簡單四邊線 (相容原有的計算)
            if (_xRecipe.LineBorderParams.IsSimpleQuad)
            {
                //(1.0) 取得目標尺寸 targetSize
                var targetDim = new SizeF();
                if (lineBorderPairs.TryGetValue("X", out var pairX))
                    targetDim.Width = pairX.TargetDist;
                if (lineBorderPairs.TryGetValue("Y", out var pairY))
                    targetDim.Height = pairX.TargetDist;

                //(1.1) 取得 4邊線
                var edgeLines4 = lineBorderPairs.GetQuadLineSegments();

                //(1.2) 調用 原來 微距座標轉換系統 的建構方法
                return BuildMicroChipTransform(targetDim, edgeLines4, regionBmp, regionRoi, lineBorderPairs.IsLocal);
            }
            else
            {
                //(2) 新方法
                ErrorCodes err = ErrorCodes.OK;

                using (var workBmp = (Bitmap)regionBmp.Clone())
                {
                    //(2.*) 晶粒定位 
                    bool ok = _aoiChipLoc.LocateOneChip(workBmp, ref regionRoi, out var chipData);
                    if (!ok || chipData == null)
                        return ErrorCodes.ERR_NO_CHIP_LOCATION;

                    //(3) 準備建立 Micro Transform
                    var carrierID = getActiveCarrierID();
                    var microTrf = _sysModel.GetMicroTransform(carrierID);

                    //(3.0) Global Transform (保留未來優化使用)
                    var trfGlobal = _sysModel.TransformsModel.GetCameraPhysicTransform(carrierID);

                    //(3.1) chipQuad (FullFov Coordinates)
                    var chipQuad = chipData.ChipQuad2D;
                    if (chipQuad == null)
                        return ErrorCodes.ERR_NO_CHIP_LOCATION;

                    //(3.2) 目標值
                    var targetDim = new SizeF(
                        _xRecipe.InspectParams.xTemplateChipWidth,
                        _xRecipe.InspectParams.xTemplateChipHeight
                    );

                    //(3.3) 建立轉換公式
                    err = microTrf.BuildMicroTransform(targetDim, lineBorderPairs, chipData, regionRoi);

                    //(3.4) 保存參數
                    if (err == ErrorCodes.OK)
                        microTrf.Save(null);
                }

                if (err == ErrorCodes.OK)
                    _aoiChipMeasure.AnalyzeGoldenData();

                return err;
            }
        }

        public bool TryRunOneChip(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi)
        {
            if (cell == null) return false;
            cell.Reset();

            bool ok = _aoiChipLoc.TryLocateOneChip(cell, cellBmp, ref cellRoi);

            if (ok && _xRecipe.InspectParams.optChipMeasurement)
                _aoiChipMeasure.TryMeasureOneChip(cell, cellBmp, ref cellRoi);

            return ok;
        }
        
        public IAoiChipLocator GetChipLocAoi()
        {
            return _aoiChipLoc;
        }
        public IAoiChipMeasurer GetChipMeasureAoi()
        {
            return _aoiChipMeasure;
        }
        public IAoiDefectsDetector GetDefectsAoi()
        {
            return _aoiChipDefects;
        }
        public IAoiBadConnInspector GetBadConnInspector()
        {
            return _aoiBadConnInspector;
        }
        public IAoiQrDecoder GetAoiQrDecoder()
        {
            return _aoiQrDecoder;
        }
        public IAoiFlyCamMatcher GetFlyCameraAoi()
        {
            return _aoiFlyCam;
        }

        public void Train()
        {
            var err1 = Mvc.Model.ErrorCodes.AoiErr_Template_Train_Failed;
            var err2 = Mvc.Model.ErrorCodes.AoiErr_FlyCam_Train_Failed;

            try
            {
                bool ok = GetChipLocAoi().Train(_xRecipe.GoldenChipBmp);
                if (ok) err1 = ErrorCodes.OK;

                ok = GetFlyCameraAoi().Train(_xRecipe.FlyTemplateBmp);
                if (ok) err2 = ErrorCodes.OK;
            }
            catch
            {
            }

            if (err1 != ErrorCodes.OK)
            {
                throw new Exception(QMSG.Text(err1));
            }
            if (err1 != ErrorCodes.OK)
            {
                throw new Exception(QMSG.Text(err2));
            }
        }

        public override void Run(Bitmap sceneBmp = null)
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

            #region 把 LotID 與 StripID 設定給 SubAoiModel
            _aoiChipLoc.LotData = this.LotData;
            ((AoiModelBase)_aoiChipMeasure).LotData = this.LotData;
            _aoiEmptyTray.LotData = this.LotData;
            #endregion

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

        #region PRIVATE_IMPLEMENT_FUNCTIONS
        private void _RunChipLocAndMeasurement()
        {
            try
            {
                fire_AoiBegin();
                markRunStart();

                //(0) 取得線掃巨圖: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                Bitmap bmpOrgBig = LineScanCamImageHolder.PeekBitmap();

                // 定位
                _aoiChipLoc.Run();
                var cellGroups = _aoiChipLoc.CellGroups;

                // 尺寸量測
                if (_xRecipe.InspectParams.optChipMeasurement)
                {
                    _aoiChipMeasure.SetCellGroups(cellGroups);
                    _aoiChipMeasure.Run();
                }

                // 瑕疵檢測
                if (_xRecipe.InspectParams.optChipDefectsInspect)
                {
                    _aoiChipDefects.SetCellGroups(cellGroups);
                    _aoiChipDefects.Run();
                }

                // QRCODE
                if (QrUsed || QrJudged)
                {
                    _aoiQrDecoder.SetCellGroups(cellGroups);
                    _aoiQrDecoder.Run();
                }

                // 連筋檢測
                if (_xRecipe.InspectParams.BadConnsCount > 0)
                {
                    _aoiBadConnInspector.SetCellGroups(cellGroups);
                    _aoiBadConnInspector.Run();
                }

                // 釋放 多執行續的 CellGroups
                _aoiChipLoc.DisposeCellGroups();

                // 後處理: 標記不明區塊
                _aoiChipLoc.PostMarkAmbiguousBlocs();

                bool pass = _CheckChipsTotalPass(out int badConnsCount);

                // 2026-07-09 LETIAN: 泰國要求按照 PASS/NG 分流存檔原圖
                AsyncSaveOrgImage(bmpOrgBig, pass);

                // 2026-06-06 LETIAN: 檢查定位結果是否為全空盤!
                if (xScanInspectMode == ScanInspectMode.MEASUREAOI)
                {
                    if (_CheckIfAllEmpty())
                    {
                        var errCode = ErrorCodes.ERR_CHIP_LOC_ALL_EMPTY;
                        string errMsg = GaUtil.GetEnumDescription(errCode);
                        bool cancel = fire_AoiError(errCode, errMsg, true);
                        if (cancel)
                        {
                            markRunEnd(false);
                            return;
                        }
                    }
                }

                // 2026-09-08 LETIAN: 檢查是否有連筋
                if (_xRecipe.InspectParams.BadConnsCount > 0)
                {
                    if (badConnsCount > 0)
                    {
                        var errCode = ErrorCodes.WARN_EXISTING_BAD_CONNS_BLOBS;
                        string errMsg = GaUtil.GetEnumDescription(errCode);
                        bool cancel = fire_AoiError(errCode, errMsg, true);
                        if (cancel)
                        {
                            markRunEnd(false);
                            return;
                        }
                    }
                }

                markRunEnd(pass);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
#if (OPT_OLD_CODE)
                markRunEnd(false);
                //fire_AoiEnd();
                //var errCode = Mvc.Model.ErrorCodes.EXCEPTION_AT_AOI_RUN;
                //string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                var errCode = ErrorCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode)
                                + "\n\r\n\r" + GetType().Name
                                + "\n\r\n\r" + GetDeepExceptionMessage(ex);
                fire_AoiError(errCode, errMsg);
                _LOG_ERROR(ex, "_RunChipLocAndMeasurement");
#endif
                base.HandleAoiException(ex, "_RunChipLocAndMeasurement");
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

                //(0) 取得線掃巨圖: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                Bitmap bmpOrgBig = LineScanCamImageHolder.PeekBitmap();

                _aoiEmptyTray.Run();

                // 2026-07-09 LETIAN: 泰國要求按照 PASS/NG 分流存檔原圖
                bool pass = _aoiEmptyTray.IsPass;
                AsyncSaveOrgImage(bmpOrgBig, pass);

                markRunEnd(_aoiEmptyTray.IsPass);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
#if (OPT_OLD_CODE)
                markRunEnd(false);
                //fire_AoiEnd();
                //var errCode = Mvc.Model.ErrorCodes.EXCEPTION_AT_AOI_RUN;
                //string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                var errCode = ErrorCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode)
                                + "\n\r" + GetType().Name
                                + "\n\r\n\r" + GetDeepExceptionMessage(ex);
                fire_AoiError(errCode, errMsg);
                _LOG_ERROR(ex, "_RunEmptyTray");
#endif
                base.HandleAoiException(ex, "_RunEmptyTray");
            }
        }
        private bool _CheckChipsTotalPass(out int badConnsCount)
        {
            bool optUsePercentage = _xRecipe.InspectParams.optUseTotalNgPercentage && _xRecipe.InspectParams.optChipMeasurement;

            int passCount = 0;
            int ngCount = 0;
            int total = 0;

            // 連筋數量統計
            badConnsCount = 0;

            //---------------------------------------------------------------------------------------------
            // 2026-04-04
            // 使用 RegionCellsDataCollection 來管理 _xRecipe.xRegionCells
            // 以維持 PASS/NG 統計數量的一致性 !
            //---------------------------------------------------------------------------------------------

            using (var cellsCollection = new RegionCellsDataCollection(_xRecipe.xRegionCells))
            {
                total = cellsCollection.GetStatistics(out passCount, out ngCount, out int emptyCount, out int unknowns);

                // 連筋數量統計
                foreach (var cell in cellsCollection.IterFinalCells())
                {
                    var badConnBlocs = cell?.ChipData?.BadConnBlocs;
                    if (badConnBlocs != null && badConnBlocs.Length > 0)
                    {
                        badConnsCount += badConnBlocs.Length;
                    }
                }
            }

            bool isPass;
            if (optUsePercentage)
            {
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
        private bool _CheckIfAllEmpty()
        {
            // 2026-06-06 LETIAN: 檢查定位結果是否為全空盤!
            bool allEmpty = true;
            var plcCodes = GetSingleResult();
            foreach (var code in plcCodes)
            {
                if (code != (int)PlcResultCode.NG_EMPTY)
                {
                    allEmpty = false;
                    break;
                }
            }
            return allEmpty;
        }
        #endregion

        #region OLD_CODE
#if (OPT_REMOVED_TO_GaPlcDataPacker)
        internal IEnumerable<RegionCellX3Class> IterResultCells(IEnumerable<RegionCellX3Class> cells = null)
        {
            if (cells == null)
                cells = _xRecipe.xRegionCells;

            foreach (var cell in cells)
            {
                var outGridCell = cell?.OutGridLink;
                if (outGridCell != null)
                    yield return outGridCell;
                else
                    yield return cell;
            }
        }
#endif
        #endregion

        /// <summary>
        /// 非同步保存 原圖 (caller 負責 bmpFullfov 生命)
        /// </summary>
        void AsyncSaveOrgImage(Bitmap bmpFullfov, bool pass)
        {
#if (false)
            if (bmpFullfov == null)
                return;

            if (!INI.Instance.IsSaveDebugBmp && !INI.Instance.IsSaveDebugOrgBmp)
                return;

            var args = new object[]
            {
                bmpFullfov.Clone(),
                pass
            };

            ThreadPool.QueueUserWorkItem(argv =>
            {
                try
                {
                    var argvs = (object[])argv;
                    var cPass = (bool)argvs[1];

                    using (Bitmap bmpBig = (Bitmap)argvs[0])
                    {
                        //(1) 保存壓縮圖檔 (IsSaveDebugBMP)
                        if (INI.Instance.IsSaveDebugBmp)
                        {
                            string fileName = GetDebugBmpFileName(cPass);
                            GaImageUtil.SaveImageWithQuality(bmpBig, fileName, INI.Instance.ImageQuality);
                        }

                        //(2) 保存原始圖檔 (IsSaveDebugOrgBmp)
                        if (INI.Instance.IsSaveDebugOrgBmp)
                        {
                            string fileName = GetDebugOrgBmpFileName(cPass);
                            GaImageUtil.SaveBigImage(fileName, bmpBig);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //_LOG($"异常捕获:{ex.Message}", Color.Red);
                    _LOG_ERROR(ex, $"{GetType().Name}.saveDumpImageAsync");
                }
            },
                args
            );
#endif
            _lotDataHolder.AsyncSaveOrgImage(bmpFullfov, pass);
        }
    }
}
