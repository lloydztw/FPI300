#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-28 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using JetEazy.EzImage;
using JetEazy.Match;
using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Transform;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using NeedleX.ProcessSpace;
using OpenCvSharp.ML;
using System;
using System.Collections.Generic;
using System.Drawing;
using Traveller106;
using VsCommon.ControlSpace.MachineSpace;
using ZXing;
using EmptyTrayAoiFactory = EzAoiEmptyTrayInspector.AoiFactory;


namespace LaserAlignDX.Mvc.Model
{
    public class TravellerSysModel : ITravelerModel
    {
        public event EventHandler<ProcessEventArgs> OnError;

        #region SINGLETON
        static TravellerSysModel _instance;
        TravellerSysModel()
        {
        }
        #endregion

        #region GLOBAL_MESS
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        #endregion

        #region KERNAL_MODELS
        IProcessRunFPI _aoiModel;
        ICalibAoiModel _calibModel;
        ITravellerTransforms _transformsModel = TravellerTransforms.CommonBase;
        QMicroChipTransform[] _microTransforms = new[]
        {
            new QMicroChipTransform("C1_Micro"),
            new QMicroChipTransform("C2_Micro"),
        };
        #endregion

        internal static TravellerSysModel Instance(IProcessRunFPI aoiModel)
        {
            if (_instance == null)
            {
                _instance = new TravellerSysModel();
            }
            _instance._aoiModel = aoiModel;
            return _instance;
        }
        internal static void DisposeAll()
        {
            _instance?.Dispose();
            _instance = null;
        }
        public void Dispose()
        {
            _aoiModel?.Dispose();
            _aoiModel = null;

            _calibModel?.Dispose();
            _calibModel = null;

            TravellerTransforms.DisposeAll();
            //_transformsModel?.Dispose();
            _transformsModel = null;

            TravellerBigImagesHolder.DisposeAll();
            EmptyTrayAoiFactory.DisposeAll();

            _instance = null;
        }

        public IProcessRunFPI AoiModel
        {
            get => _aoiModel;
        }
        public ICalibAoiModel CalibAoiModel
        {
            get
            {
                if (_calibModel == null)
                {
                    _calibModel = new CalibAoiModel(EmptyTrayAoiModel);
                    //_calibModel = new CalibAoiModel();
                }
                return _calibModel;
            }
        }
        public IxEmptyTrayInspector EmptyTrayAoiModel
        {
            get
            {
                var recipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
                recipeName = LtAoiFactory.RcpStemName(recipeName, ActiveCarrierID);
                var emptyTrayAoi = EmptyTrayAoiFactory.InstanceModel(recipeName);
                return emptyTrayAoi;
            }
        }
        public ITravellerTransforms TransformsModel
        {
            get => _transformsModel;
        }
        public QMicroChipTransform GetMicroTransform(CarrierEnum C)
        {
            int index = (int)C - (int)CarrierEnum.C1;
            return _microTransforms[index];
        }
        public GaBigImageHolder LineScanImageHolder
        {
            get => TravellerBigImagesHolder.Instance.LineScanImageHolder;
        }

        public CarrierEnum ActiveCarrierID
        {
            get;
            set;
        }

        public object GetCurrentRecipe()
        {
            //return _jxRecipe;
            return _xRecipe;
        }

        void __ApplyRecipe_000(params object[] args)
        {
#if (OPT_REMARK_2026_0308)
            //(0) 載入 TransformsModel 設定 (全域)
            var transformsModel = this.TransformsModel;
            transformsModel.Load(GaMvcPaths.CALIB_TRANSFORMS_FILE);
            transformsModel.BuildAll();

            GetMicroTransform(ActiveCarrierID)?.Load(null);
            GetMicroTransform(ActiveCarrierID)?.Build();

            //(1) 重新載入 _xRecipe
            _xRecipe.ChangeActiveCarrier(ActiveCarrierID, forceToReload: true);

            //(2) 載入 Camera Grid (保存在個別參數 _xRecipe 中)
            var camGridC1 = _xRecipe.xCamGrid1;
            var camGridC2 = _xRecipe.xCamGrid2;
            var runtimeCamGrid = (CarrierEnum.C1 == ActiveCarrierID) ? camGridC1 : camGridC2;

            //(3) 檢查 camGrid 的數據狀態
            var err = checkCameraGrid(ActiveCarrierID, runtimeCamGrid, notify: true);
            if (err != null)
                return;

            #region LOG
            string gaaraRecipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
            GaUtil.LOG($"[SysModel] 重新載入參數 {gaaraRecipeName}", Color.Blue);
            #endregion

            //(4) 重新載入 EmptyTrayAoiRecipe
            using (var jx = loadEmptyTrayAoiRecipe(false))      //將參數載入 AoiModel @ ApplyRecipe
            {
                if (jx != null)
                {
                    EmptyTrayAoiModel.SetRecipe(jx);
                }
            }

            #region NOT_USED_CODE
            ////(3) PitchX and PitchY 
            //double pitchX = _xRecipe.xRealOffsetX;  // default from xRecipe
            //double pitchY = _xRecipe.xRealOffsetY;  // default from xRecipe
            ////(5) 設定 Runtime 跑線時期 P座標 (PLC) 格點
            //var rows = camGrid.Rows;
            //var cols = camGrid.Cols;
            //var plcGrid = new PlcGridPoints(rows, cols, pitchX, pitchY);
            #endregion

            //(5) build Runtime Plc Grid
            var plcGrid = buildRuntimePlcGrid(runtimeCamGrid.Rows, runtimeCamGrid.Cols);

            //(6) set Runtime Plc Grid
            transformsModel.UpdateRuntimePlcGrid(plcGrid, camGridC1, camGridC2);

            //(7) 自動建立陣列
            buildRegionCellsArray(transformsModel, ActiveCarrierID, runtimeCamGrid, false);          //@ ApplyRecipe
#endif
        }

        public void ApplyRecipe(params object[] args)
        {
            //(0) 載入個別的 TransformsModel
            string gaaraRecipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
            TravellerTransforms.DisposeAll();
            var trfModel = TravellerTransforms.Instance(gaaraRecipeName);
            var trfFile = GaMvcPaths.TRANSFORMS_INI_FILE(_xRecipe.INIFILE);
            if (!System.IO.File.Exists(trfFile))
                trfFile = GaMvcPaths.COMMON_BASE_TRANSFORMS_INI_FILE;
            trfModel.Load(trfFile);
            trfModel.BuildAll();

            //(1) MicroTransform
            GetMicroTransform(ActiveCarrierID)?.Load(null);
            GetMicroTransform(ActiveCarrierID)?.Build();

            //(2) 重新載入 _xRecipe
            _xRecipe.ChangeActiveCarrier(ActiveCarrierID, forceToReload: true);

            //(3) 載入 Camera Grid (保存在個別參數 _xRecipe 中, 與 trfModel 內)
            var trfCamGridC1 = trfModel.GetCalibCameraGrid(CarrierEnum.C1);
            var trfCamGridC2 = trfModel.GetCalibCameraGrid(CarrierEnum.C2);
            if (trfCamGridC1 != null) _xRecipe.xCamGrid1 = trfCamGridC1;
            if (trfCamGridC2 != null) _xRecipe.xCamGrid2 = trfCamGridC2;
            var camGridC1 = _xRecipe.xCamGrid1;
            var camGridC2 = _xRecipe.xCamGrid2;
            var runtimeCamGrid = (CarrierEnum.C1 == ActiveCarrierID) ? camGridC1 : camGridC2;

            //(4) 檢查 camGrid 的數據狀態
            var err = checkCameraGrid(trfModel, ActiveCarrierID, runtimeCamGrid, notify: true);
            if (err != null)
                return;

            GaUtil.LOG($"[SysModel] 重新載入參數 {gaaraRecipeName}", Color.Blue);

            //(5) 重新載入 EmptyTrayAoiRecipe
            using (var jx = loadEmptyTrayAoiRecipe(false))      //將參數載入 AoiModel @ ApplyRecipe
            {
                if (jx != null)
                {
                    EmptyTrayAoiModel.SetRecipe(jx);
                }
            }

            //(6) 根據當下 _xRecipe 數據, 來建立 Runtime Plc Grid
            var plcGrid = buildRuntimePlcGrid(runtimeCamGrid.Rows, runtimeCamGrid.Cols);

            //(7) set Runtime Plc Grid
            trfModel.UpdateRuntimePlcGrid(plcGrid, camGridC1, camGridC2);

            //(8) 自動建立陣列
            buildRegionCellsArray(trfModel, ActiveCarrierID, runtimeCamGrid, false);          //@ ApplyRecipe

            //(9) 設定當下的 Transforms
            _transformsModel = trfModel;
        }

        public bool BuildCommonBaseTransforms(CarrierEnum carrierID, EzBlocsGrid camGrid)
        {
            // 使用多點校正版 建立 所有參數檔 共用的 座標轉換 與 Region Cells 格點
            // 用於 校正 參數編輯模式
            // 2025-12-11 針對 校正塊 改版
            
            bool ok = false;
            var commonBaseTrf = TravellerTransforms.CommonBase;

            //(0) 檢查 camGrid 狀態
            if (camGrid == null)
            {
                //"無法抓到格點!"
                var errCode = ErrorCodes.CAN_NOT_FETCH_CAMERA_GRID;
                var errMsg = JetEazy.QxNums.GetEnumDescription(errCode);
                OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return false;
            }
            else if (checkCameraGrid(commonBaseTrf, ActiveCarrierID, camGrid, notify: true) != null)
            {
                return false;
            }

            //(1) 建立 Transform
            commonBaseTrf.BuildAll();

            if (camGrid != null)
            {
                //(2) 去除異常 格位 再重建一次 Transform
#if (OPT_RESERVED)
                var calibModel = CalibAoiModel;
                if (calibModel.AdjustBadNodes(carrierID, camGrid))
                    transformsModel.BuildAll();
#endif

                //(3) 建立 格位陣列 (RegionCellsArrary)
                ok = buildRegionCellsArray(commonBaseTrf, carrierID, camGrid, optWriteBlackToRecipe: true);         //@ BuildTransformAndRegionCells (Global)

                //(4) 更新 格位點 (camGrid)
                var camGridC1 = carrierID == CarrierEnum.C1 ? camGrid : null;
                var camGridC2 = carrierID == CarrierEnum.C2 ? camGrid : null;
                commonBaseTrf.UpdateRuntimePlcGrid(null, camGridC1, camGridC2);

                //(5) 更新當下 xRecipe
                //>>> 注意: 必須由 個別參數 編輯模式 重新執行 【抓取陣列】
                //>>> AutoBuildRegionCellsArray(carrierID, null, out var _);

                //(6) 查核 參數設定 正確性
                var err = checkTrayParams(carrierID, camGrid, notify: true);

                ok &= string.IsNullOrEmpty(err);
            }

            return ok;
        }

        public bool AutoBuildRegionCellsArray(CarrierEnum carrierID, Bitmap fullfovBmp, out MatchResult matchResult)
        {
            // 個別參數 【自動抓取陣列】 並 進行 【座標系統 線性遷移】
            // <br/> 用於 參數編輯模式
            // <br/> 必須曾經執行過 BuildTransformAndRegionCells

            //(1) 抓取 空盤 格位點
            matchResult = fetchEmptyTrayCameraGrid(fullfovBmp);
            var camGrid = matchResult?.Grid;
            if (camGrid == null)
            {
                //"無法抓到格位點!"
                var errCode = ErrorCodes.CAN_NOT_FETCH_CAMERA_GRID;
                var errMsg = JetEazy.QxNums.GetEnumDescription(errCode);
                OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return false;
            }

            //(2) 線性轉移 建立 座標轉換系統
            var commonBaseTrf = TravellerTransforms.CommonBase;
            commonBaseTrf.Load(GaMvcPaths.COMMON_BASE_TRANSFORMS_INI_FILE);
            commonBaseTrf.BuildAll();
            string gaaraRecipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
            var trfModel = commonBaseTrf.BuildLinearMigration(gaaraRecipeName, carrierID, camGrid);

            //(3) 核查 結果
            if (checkCameraGrid(trfModel, carrierID, camGrid, notify: true) != null)
            {
                // checkCameraGrid 內部, 會自動發出 異常事件.
                return false;
            }

            //(4) 指定 線性轉移後 座標轉換系統
            _transformsModel = (TravellerTransforms)trfModel;

            //(5) 建立格點陣列
            bool ok = buildRegionCellsArray(trfModel, carrierID, camGrid, optWriteBlackToRecipe: true);   //@ AutoBuildRegionCellsArray

            //(6) 更新 個別 座標轉換系統 的 camGrid (格點陣列)
            var camGridC1 = carrierID == CarrierEnum.C1 ? camGrid : null;
            var camGridC2 = carrierID == CarrierEnum.C2 ? camGrid : null;

            return ok && camGrid != null;
        }

        #region PRIVATE_REGION_CELL_FUNCTIONS
        private JxAoiRecipe loadEmptyTrayAoiRecipe(bool alwayCreateOne)
        {
            var gaaraRcpName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
            var fileName = LtAoiFactory.RcpGetRecipeFileName(gaaraRcpName, ActiveCarrierID);

            if (System.IO.File.Exists(fileName))
            {
                var jx = new JxAoiRecipe();
                jx.Load(fileName);
                return jx;
            }
            else if (alwayCreateOne)
            {
                return new JxAoiRecipe();
            }
            else
            {
                return null;
            }
        }
        private MatchResult fetchEmptyTrayCameraGrid(Bitmap fullfovBmp)
        {
            //------------------------
            // 進階 抓取 空載台 格位點
            //------------------------

            using (var bridge = new QxImageBridge(fullfovBmp))
            {
                var fullfovImg = bridge.Image;
                var result = CalibAoiModel.FetchGridNodes(EmptyTrayAoiModel, ActiveCarrierID, fullfovImg, refine: false);
                return result;
            }
        }
        
        private PlcGridPoints buildRuntimePlcGrid(int rows, int cols)
        {
            //-------------------------------------------------
            // 根據當下 _xRecipe 數據, 來建立 Runtime Plc Grid
            //-------------------------------------------------

            //(1) PitchX and PitchY 
            double pitchX = Math.Round(_xRecipe.xRealOffsetX, 5);   // default from xRecipe
            double pitchY = Math.Round(_xRecipe.xRealOffsetY, 5);   // default from xRecipe

            //(2) 重新載入 EmptyTrayAoiRecipe
            using (var jx = loadEmptyTrayAoiRecipe(false))          //只讀取當下參數設定值 @ buildRuntimePlcGrid
            {
                if (jx != null)
                {
                    //EmptyTrayAoiModel.SetRecipe(jx);
                    pitchX = (double)jx.TrayMiscSettings.PitchX.Value;
                    pitchY = (double)jx.TrayMiscSettings.PitchY.Value;
                }
            }

            //(3) 設定 Runtime 跑線時期 P座標 (PLC) 格點
            //var rows = camGrid.Rows;
            //var cols = camGrid.Cols;
            var plcGrid = new PlcGridPoints(rows, cols, pitchX, pitchY);
            return plcGrid;
        }
        private bool buildRegionCellsArray(ITravellerTransforms trfModel, CarrierEnum carrierID, EzBlocsGrid camGrid, bool optWriteBlackToRecipe)
        {
            bool ok;
            if (TravellerTransforms.OPT_CALIB_GRID_USING_MOTOR_COORD)
                ok = buildRegionCells_motor(trfModel, carrierID, camGrid, optWriteBlackToRecipe);
            else
                ok = buildRegionCells_world(trfModel, carrierID, camGrid, optWriteBlackToRecipe);
            return ok;
        }
        private bool buildRegionCells_world(ITravellerTransforms trfModel, CarrierEnum carrierID, EzBlocsGrid camGrid, bool optWriteBlackToRecipe)
        {
            if (camGrid == null)
            {
                //>>> camGrid = _jxRecipe.EmptyTrayParams.TrayMiscSettings.GetGoldenGrid(true);
                return false;
            }

            //(0) Global MESS
            var xParaGrid = RecipeParaGridClass.Instance;
            var xInpectParam = _xRecipe.InspectParams;
            var xRegionCells = _xRecipe.xRegionCells;
            clearRegionCells(xRegionCells);

            double standardChipWidth = xInpectParam.mWidthStand;
            double standardChipHeight = xInpectParam.mHeightStand;

            //(1) TransformModel
            var transCP = trfModel.GetCameraPhysicTransform(carrierID);
            if (transCP == null)
            {
                var errCode = ErrorCodes.NO_CALIB_TRANSFORM;
                var errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + " !";
                OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return false;
            }

            //(2) goldenChipRect (in camera coordinates)
            SizeF cellViewSizeF;
            if (true)
            {
                // 從晶粒長寬規格 mWidthStand, mHeightStand (mm)
                // 反推其在 Camera 座標系上 的大小
                var standardChipSize = new QVector(standardChipWidth, standardChipHeight);

                var camPt0 = camGrid[0, 0].Center;
                var worldPt0 = transCP.Trans(camPt0);
                var goldenP1 = worldPt0 - (standardChipSize / 2);
                var goldenP2 = worldPt0 + (standardChipSize / 2);
                goldenP1 = transCP.InvTrans(goldenP1);
                goldenP2 = transCP.InvTrans(goldenP2);
                var delta = goldenP2 - goldenP1;

                var goldenChipRect = new RectangleF((float)goldenP1.X, (float)goldenP1.Y, (float)delta.X, (float)delta.Y);
                cellViewSizeF = goldenChipRect.Size;
            }

            //(3) 用 Zigzag 順序加入至 xRegionCells
            int index = 0;
            foreach ((int r, int c, EzBloc bloc) in camGrid.IterZigzag())
            {
                var cell = new RegionCellX3Class();
                cell.Index = index;
                cell.CellRow = r;
                cell.CellCol = c;

                //cell.lblName = "ROW" + r.ToString("000") + "-COL" + c.ToString("000");
                //cell.viewRectF = new RectangleF(_baserect.X + j * _coloffset, _baserect.Y + i * _rowoffset, _baserect.Width, _baserect.Height);
                cell.lblName = $"ROW{r:000}-COL{c:000}";
                cell.viewRectF = JetEazy.Qcvt.CreateCenterRect((float)bloc.Center.X, (float)bloc.Center.Y, ref cellViewSizeF);

                //cell (OrgX, OrgY) 是 Chip 的中心點?
                //cell.OrgX = xRealLeftX + j * xRealOffsetX;
                //cell.OrgY = xRealLeftY + i * xRealOffsetY;
                var wCenter = transCP.Trans(bloc.Center);
                cell.OrgX = (float)(wCenter.X);    // - standardChipSize.X / 2);
                cell.OrgY = (float)(wCenter.Y);    // - standardChipSize.Y / 2);

                xRegionCells.Add(cell);
                index++;
            }

            //(4) 更新 數據到 xParaGrid 參數
            if (optWriteBlackToRecipe)
            {
                var rows = camGrid.Rows;
                var cols = camGrid.Cols;
                if (rows < 2 || cols < 2)
                {
                    // 上層的 AutoBuildRegionCells 已經警示過了. 
                    return false;
                }

                // 更新 CameraGrid
                if (carrierID == CarrierEnum.C1)
                {
                    _xRecipe.xCamGrid1?.Dispose();
                    _xRecipe.xCamGrid1 = camGrid;
                }
                else
                {
                    _xRecipe.xCamGrid2?.Dispose();
                    _xRecipe.xCamGrid2 = camGrid;
                }

                // Camera Grid Points
                var camPt00 = camGrid[0, 0];
                var p00 = transCP.Trans(camGrid[0, 0].Center);
                var p01 = transCP.Trans(camGrid[0, cols - 1].Center);
                var p10 = transCP.Trans(camGrid[rows - 1, 0].Center);
                var p11 = transCP.Trans(camGrid[rows - 1, cols - 1].Center);
                var corners = new[] { p00, p01, p11, p10 }; // 左上, 右上, 右下, 左下

                // xAngle
                var box2D = new QvBox2D();
                box2D.Corners = Array.ConvertAll(corners, c => new PointF((float)c.X, (float)c.Y));
                double angle = box2D.Theta * 180.0 / Math.PI;
                xParaGrid.xAngle = (float)Math.Round(angle, 3);

                // xRows, xColumn
                xParaGrid.xRow = rows;
                xParaGrid.xColumn = cols;

                // xLeftTopX, xLeftTopY
                // 矩阵左上角Chip的左上角图像位置 X, Y
                xParaGrid.xLeftTopX = (int)(camPt00.Center.X - cellViewSizeF.Width);
                xParaGrid.xLeftTopY = (int)(camPt00.Center.Y - cellViewSizeF.Height);

                // xRowOffset, xColumnOffset
                // Row Pitch and Col Pitch (runtime)
                xParaGrid.xRowOffset = (float)Math.Round((p10 - p00).Y / (rows - 1), 3);
                xParaGrid.xColumnOffset = (float)Math.Round((p01 - p00).X / (cols - 1), 3);

                // xChipWidth, xChipHeight
                // Chip Size (runtime)
                xParaGrid.xChipWidth = (float)Math.Round(standardChipWidth, 3);
                xParaGrid.xChipHeight = (float)Math.Round(standardChipHeight, 3);

                //xParaGrid.xRealLeftX = (float)p00.X;
                //xParaGrid.xRealLeftY = (float)p00.Y;
                //xParaGrid.xRealOffsetX = (float)(p01 - p00).X / (cols - 1);
                //xParaGrid.xRealOffsetY = (float)(p10 - p00).Y / (rows - 1);

                //從 EmptyTrayAoi 取得 PitchX, PitchY 的設定
                using (var jx = loadEmptyTrayAoiRecipe(false))      //只讀取當下參數設定值 @ buildRegionCells_world
                {
                    var traySettings = jx?.TrayMiscSettings;
                    if (traySettings != null)
                    {
                        xParaGrid.xRealOffsetX = (float)traySettings.PitchX.Value;
                        xParaGrid.xRealOffsetY = (float)traySettings.PitchY.Value;
                    }
                }
            }

            return true;
        }
        private bool buildRegionCells_motor(ITravellerTransforms trfModel, CarrierEnum carrierID, EzBlocsGrid runtimeCamGrid, bool optWriteBlackToRecipe)
        {
            if (runtimeCamGrid == null)
            {
                //>>> camGrid = _jxRecipe.EmptyTrayParams.TrayMiscSettings.GetGoldenGrid(true);
                return false;
            }

            ErrorCodes errCode;
            string errMsg;

            //(0) Global MESS
            var xParaGrid = RecipeParaGridClass.Instance;
            var xInpectParam = InspectX3ParaClass.Instance;
            var xRegionCells = _xRecipe.xRegionCells;
            clearRegionCells(xRegionCells);

            //(1) TransformModel
            var transCM1 = trfModel.GetCameraMotorTransform(carrierID, SuckerRowEnum.S1);
            var transCP = trfModel.GetCameraPhysicTransform(carrierID);
            if (transCP == null || transCM1 == null)
            {
                errCode = ErrorCodes.NO_CALIB_TRANSFORM;
                errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + " !";
                OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return false;
            }

            //(2) goldenChipRect (in camera coordinates)
            SizeF cellViewSizeF = _xRecipe.xRegionTrain.Size;
            double standardChipWidth = xInpectParam.mWidthStand;
            double standardChipHeight = xInpectParam.mHeightStand;

            //(3) CamBasePt and Sucker1_BasePt
            var camBasePt = runtimeCamGrid[0, 0].Center;
            var motorBasePt = transCM1.Trans(camBasePt);

            //(4) 從 EmptyTrayAoi 取得 PitchX, PitchY 的設定
            double pitchX = xParaGrid.xRealOffsetX;
            double pitchY = xParaGrid.xRealOffsetY;
            using (var jx = loadEmptyTrayAoiRecipe(false))      //只讀取當下參數設定值 @ buildRegionCells_motor
            {
                var traySettings = jx?.TrayMiscSettings;
                if (traySettings != null)
                {
                    pitchX = (double)traySettings.PitchX.Value;
                    pitchY = (double)traySettings.PitchY.Value;
                }
            }

            //(5) 用 Zigzag 順序加入至 xRegionCells
            int index = 0;
            QVector nodeBase = motorBasePt;
            foreach ((int r, int c, EzBloc bloc) in runtimeCamGrid.IterZigzag())
            {
                var cell = new RegionCellX3Class();
                cell.Index = index;
                cell.CellRow = r;
                cell.CellCol = c;

                //cell.lblName = "ROW" + r.ToString("000") + "-COL" + c.ToString("000");
                //cell.viewRectF = new RectangleF(_baserect.X + j * _coloffset, _baserect.Y + i * _rowoffset, _baserect.Width, _baserect.Height);
                cell.lblName = $"ROW{r:000}-COL{c:000}";
                cell.viewRectF = JetEazy.Qcvt.CreateCenterRect((float)bloc.Center.X, (float)bloc.Center.Y, ref cellViewSizeF);

                //cell (OrgX, OrgY) 是 Chip 的中心點?
                //cell.OrgX = xRealLeftX + j * xRealOffsetX;
                //cell.OrgY = xRealLeftY + i * xRealOffsetY;
                cell.OrgX = (float)(nodeBase.X + c * pitchX);
                cell.OrgY = (float)(nodeBase.Y + r * pitchY);

                xRegionCells.Add(cell);
                index++;
            }

            //(6) 更新 數據到 xParaGrid 參數
            if (optWriteBlackToRecipe)
            {
                var rows = runtimeCamGrid.Rows;
                var cols = runtimeCamGrid.Cols;
                if (rows < 2 || cols < 2)
                {
                    // 上層的 AutoBuildRegionCells 已經警示過了. 
                    return false;
                }

                // 更新 CameraGrid
                if (carrierID == CarrierEnum.C1)
                {
                    var old = _xRecipe.xCamGrid1;
                    _xRecipe.xCamGrid1 = runtimeCamGrid;
                    if (old != runtimeCamGrid)
                        old?.Dispose();
                }
                else
                {
                    var old = _xRecipe.xCamGrid2;
                    _xRecipe.xCamGrid2 = runtimeCamGrid;
                    if (old != runtimeCamGrid)
                        old?.Dispose();
                }

                // Camera Grid Points
                var p00 = transCM1.Trans(runtimeCamGrid[0, 0].Center);
                var p01 = transCM1.Trans(runtimeCamGrid[0, cols - 1].Center);
                var p10 = transCM1.Trans(runtimeCamGrid[rows - 1, 0].Center);
                var p11 = transCM1.Trans(runtimeCamGrid[rows - 1, cols - 1].Center);
                var corners = new[] { p00, p01, p11, p10 }; // 左上, 右上, 右下, 左下

                // xAngle
                var box2D = new QvBox2D();
                box2D.Corners = Array.ConvertAll(corners, c => new PointF((float)c.X, (float)c.Y));
                double angle = box2D.Theta * 180.0 / Math.PI;
                xParaGrid.xAngle = (float)Math.Round(angle, 3);

                // xRows, xColumn
                xParaGrid.xRow = rows;
                xParaGrid.xColumn = cols;

                // xLeftTopX, xLeftTopY
                // 矩阵左上角Chip的左上角图像位置 X, Y
                xParaGrid.xLeftTopX = (int)(camBasePt.X - cellViewSizeF.Width / 2f);
                xParaGrid.xLeftTopY = (int)(camBasePt.Y - cellViewSizeF.Height / 2f);

                // xRowOffset, xColumnOffset
                // Average Row Pitch and Col Pitch (runtime)
                xParaGrid.xRowOffset = (float)Math.Round((p10 - p00).Y / (rows - 1), 3);
                xParaGrid.xColumnOffset = (float)Math.Round((p01 - p00).X / (cols - 1), 3);

                // xChipWidth, xChipHeight
                // Chip Size (runtime)
                xParaGrid.xChipWidth = (float)Math.Round(standardChipWidth, 3);
                xParaGrid.xChipHeight = (float)Math.Round(standardChipHeight, 3);

                xParaGrid.xRealLeftX = (float)Math.Round(p00.X, 3);
                xParaGrid.xRealLeftY = (float)Math.Round(p00.Y, 3);

                // PitchX and PitchY
                xParaGrid.xRealOffsetX = (float)Math.Round(pitchX, 3);
                xParaGrid.xRealOffsetY = (float)Math.Round(pitchY, 3);
            }

            return true;
        }
        private void clearRegionCells(List<RegionCellX3Class> xRegionCells)
        {
            try
            {
                if (xRegionCells != null)
                {
                    foreach (var xCell in xRegionCells)
                        xCell?.Dispose();
                    xRegionCells.Clear();
                }
            }
            catch(Exception ex)
            {
                GaUtil.LOG_ERROR(ex, "clearRegionCells");
            }
        }
        #endregion

        #region PRIVATE_CHECK_FUNCTIONS
        string checkCameraGrid(ITravellerTransforms trModel, CarrierEnum carrierID, EzBlocsGrid camGrid, bool notify)
        {
            (var errCode, var errMsg) = trModel.checkCameraGrid(carrierID, camGrid);

            if (errCode != ErrorCodes.OK)
            {
                if (notify)
                    OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return errMsg;
            }

            return null;
        }
        string checkTrayParams(CarrierEnum carrierID, EzBlocsGrid camGrid, bool notify)
        {
            var errCode = ErrorCodes.OK;
            var errs = new List<string>();

            // 比對 EmptyTrayAoi 的設定
            using (var jx = loadEmptyTrayAoiRecipe(false))      //只讀取當下參數設定值 @ checkTrayParams
            {
                var traySettings = jx?.TrayMiscSettings;
                if (traySettings != null)
                {
                    double pitchX = Math.Round((double)traySettings.PitchX.Value, 3);
                    double pitchY = Math.Round((double)traySettings.PitchY.Value, 3);

                    var plcGrid = _transformsModel.getCalibPlcGrid();
                    if (plcGrid != null)
                    {
                        double plcPitchX = Math.Round(plcGrid.PitchX, 3);
                        double plcPitchY = Math.Round(plcGrid.PitchY, 3);
                        if (pitchX != plcPitchX)
                            errs.Add($"空盤檢測 PitchX= {pitchX}  vs  校正參數 PitchX={plcPitchX}");
                        if (pitchY != plcPitchY)
                            errs.Add($"空盤檢測 PitchY= {pitchY}  vs  校正參數 PitchY={plcPitchY}");
                    }

                    if (camGrid != null)
                    {
                        int rows = traySettings.FullRows.Value;
                        int cols = traySettings.FullCols.Value;
                        if (rows != camGrid.Rows)
                            errs.Add($"空盤檢測 Rows= {rows}  vs  校正參數 Rows={camGrid.Rows}");
                        if (cols != camGrid.Cols)
                            errs.Add($"空盤檢測 Cols= {cols}  vs  校正參數 Cols={camGrid.Cols}");
                    }
                }
            }

            if (errs.Count > 0)
            {
                errCode = ErrorCodes.ERR_TRAY_CONFIG_CONFLICTS;
                string errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + "\n\r" + string.Join("\n\r", errs);
                if (notify)
                    OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return errMsg;
            }

            return null;
        }
        #endregion

        bool ITravelerModel.WriteCoordsToPlc(CarrierEnum carrierID, SuckerRowEnum suckerRowID, out PointF camPt, out PointF suckerMotorPt, out string errMsg)
        {
            var trfModel = _transformsModel;

            PointF _P(QVector v) { return v == null ? PointF.Empty : new PointF((float)v.X, (float)v.Y); }

            ErrorCodes errCode;

            (errCode, errMsg) = trfModel.GetCoordsRef(carrierID, out var camCoord, out var s1MotorCoord, out var s2MotorCoord);

            camPt = _P(camCoord);
            suckerMotorPt = suckerRowID == SuckerRowEnum.S1 ? _P(s1MotorCoord) : _P(s2MotorCoord);

            if (errCode == ErrorCodes.OK)
                mxWriteToPlc(carrierID, suckerRowID, suckerMotorPt);

            return (errCode == ErrorCodes.OK);
        }
        
        public bool WriteAllCoordsToPlc(out string message)
        {
            var trfModel = _transformsModel;

            PointF _P(QVector v) { return v == null ? PointF.Empty : new PointF((float)v.X, (float)v.Y); }

            //(0) PlcIO
            var plcIO = ((MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE)?.PLCIO;
            if (plcIO == null)
            {
                //msg = "Machine.PLCIO 還沒配置!";
                message = JetEazy.QxNums.GetEnumDescription(ErrorCodes.NO_PLC_IO) + " !";
                return false;
            }

            var carrierIDs = Enum.GetValues(typeof(CarrierEnum));
            var suckerIDs = Enum.GetValues(typeof(SuckerRowEnum));

            var errMsgs = new System.Collections.Generic.List<string>();
            bool totalOK = true;

            ErrorCodes err;
            string errMsg;

            foreach (CarrierEnum C in carrierIDs)
            {
                (err, errMsg) = trfModel.GetCoordsRef(C, out var camCoord, out var s1MotorCoord, out var s2MotorCoord);
                if (err == ErrorCodes.OK)
                {
                    mxWriteToPlc(C, SuckerRowEnum.S1, _P(s1MotorCoord));
                    mxWriteToPlc(C, SuckerRowEnum.S2, _P(s2MotorCoord));
                }
                else
                {
                    errMsgs.Add(errMsg);
                    totalOK = false;
                }
            }

            message = totalOK ? "OK" : string.Join("\n\r", errMsgs);
            return totalOK;
        }

        #region PRIVATE_PLC_FUNCTIONS
        void mxWriteToPlc(CarrierEnum C, SuckerRowEnum S, PointF suckerCoord)
        {
            var plcIO = ((MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE)?.PLCIO;

            var _na = PointF.Empty;

            if (C == CarrierEnum.C1 && S == SuckerRowEnum.S1)
            {
                // 載台1 吸嘴排1 
                plcIO?.SetStage1((int)S, suckerCoord, _na);
            }
            else if (C == CarrierEnum.C1 && S == SuckerRowEnum.S2)
            {
                // 載台1 吸嘴排2
                plcIO?.SetStage1((int)S, _na, suckerCoord);
            }
            else if (C == CarrierEnum.C2 && S == SuckerRowEnum.S1)
            {
                // 載台2 吸嘴排1
                plcIO?.SetStage2((int)S, suckerCoord, _na);
            }
            else if (C == CarrierEnum.C2 && S == SuckerRowEnum.S2)
            {
                // 載台2 吸嘴排2
                plcIO?.SetStage2((int)S, _na, suckerCoord);
            }
        }
        #endregion
    }
}
