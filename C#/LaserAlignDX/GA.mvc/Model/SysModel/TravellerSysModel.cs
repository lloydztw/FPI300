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
using JetEazy.Match;
using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using NeedleX.ProcessSpace;
using System;
using System.Drawing;
using Traveller106;
using VsCommon.ControlSpace.MachineSpace;
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
        TravellerTransforms _transformsModel = TravellerTransforms.Instance;
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

            _transformsModel?.Dispose();
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
                }
                return _calibModel;
            }
        }
        public IxEmptyTrayInspector EmptyTrayAoiModel
        {
            get
            {
                var recipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
                var emptyTrayAoi = EmptyTrayAoiFactory.InstanceModel(recipeName);
                return emptyTrayAoi;
            }
        }
        public TravellerTransforms TransformsModel
        {
            get => _transformsModel;
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

        public void ApplyRecipe(params object[] args)
        {
            //(0) 載入 TransformsModel 設定 (全域)
            var transformsModel = this.TransformsModel;
            transformsModel.Load(GaMvcPaths.CALIB_TRANSFORMS_FILE);
            transformsModel.BuildAll();

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

            //(3) 重新載入 EmptyTrayAoiRecipe
            using (var jx = loadEmptyTrayAoiRecipe(false))
            {
                if (jx != null)
                {
                    EmptyTrayAoiModel.SetRecipe(jx);
                }
            }

            ////(3) PitchX and PitchY 
            //double pitchX = _xRecipe.xRealOffsetX;  // default from xRecipe
            //double pitchY = _xRecipe.xRealOffsetY;  // default from xRecipe
            ////(5) 設定 Runtime 跑線時期 P座標 (PLC) 格點
            //var rows = camGrid.Rows;
            //var cols = camGrid.Cols;
            //var plcGrid = new PlcGridPoints(rows, cols, pitchX, pitchY);

            //(4) build Runtime Plc Grid
            var plcGrid = buildRuntimePlcGrid(runtimeCamGrid);

            //(5) set Runtime Plc Grid
            transformsModel.UpdateRuntimePlcGrid(plcGrid, camGridC1, camGridC2);

            //(6) 自動建立陣列
            buildRegionCells(ActiveCarrierID, runtimeCamGrid, false);
        }
        public MatchResult AutoBuildRegionCells(Bitmap fullfovBmp)
        {
            //(1) 自動抓取 格點
            var matchResult = fetchCameraGrid(fullfovBmp);
            var camGrid = matchResult?.Grid;
            if (camGrid == null)
            {
                //"無法抓到格點!"
                var errCode = ErrCodes.CAN_NOT_FETCH_CAMERA_GRID;
                var errMsg = JetEazy.QxNums.GetEnumDescription(errCode);
                OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return null;
            }
            else if (checkCameraGrid(ActiveCarrierID, camGrid, notify: true) != null)
            {
                return null;
            }

            //(2) 建立陣列
            buildRegionCells(ActiveCarrierID, camGrid, true);

            //(3) 更新 camGrid
            var camGridC1 = ActiveCarrierID == CarrierEnum.C1 ? camGrid : null;
            var camGridC2 = ActiveCarrierID == CarrierEnum.C2 ? camGrid : null;
            TransformsModel.UpdateRuntimePlcGrid(null, camGridC1, camGridC2);

            return matchResult;
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
        private MatchResult fetchCameraGrid(Bitmap fullfovBmp)
        {
            using (var jx = loadEmptyTrayAoiRecipe(true))
            {
                var aoiModel = CalibAoiModel;

                aoiModel.SetRecipe(jx);

                aoiModel.RunAll(fullfovBmp, wait: true);

                var matchResult = aoiModel.GetMatchResult(SideID.A);

                if (matchResult != null)
                {
                    using (var bridge = new QxImageBridge(fullfovBmp))
                    {
                        aoiModel.RefineCentroidLocations(matchResult, bridge.Image);
                    }
                }

                return matchResult;
            }
        }
        private PlcGridPoints buildRuntimePlcGrid(EzBlocsGrid camGrid)
        {
            //(1) PitchX and PitchY 
            double pitchX = _xRecipe.xRealOffsetX;  // default from xRecipe
            double pitchY = _xRecipe.xRealOffsetY;  // default from xRecipe

            //(2) 重新載入 EmptyTrayAoiRecipe
            using (var jx = loadEmptyTrayAoiRecipe(false))
            {
                if (jx != null)
                {
                    //EmptyTrayAoiModel.SetRecipe(jx);
                    pitchX = (double)jx.TrayMiscSettings.PitchX.Value;
                    pitchY = (double)jx.TrayMiscSettings.PitchY.Value;
                }
            }

            //(3) 設定 Runtime 跑線時期 P座標 (PLC) 格點
            var rows = camGrid.Rows;
            var cols = camGrid.Cols;
            var plcGrid = new PlcGridPoints(rows, cols, pitchX, pitchY);
            return plcGrid;
        }

        private void buildRegionCells(CarrierEnum carrierID, EzBlocsGrid camGrid, bool optWriteBlackToRecipe)
        {
            if (TravellerTransforms.OPT_USING_XRECIPE_CAM_GRID)
                buildRegionCells_motor(carrierID, camGrid, optWriteBlackToRecipe);
            else
                buildRegionCells_world(carrierID, camGrid, optWriteBlackToRecipe);
        }
        private void buildRegionCells_world(CarrierEnum carrierID, EzBlocsGrid camGrid, bool optWriteBlackToRecipe)
        {
            if (camGrid == null)
            {
                //>>> camGrid = _jxRecipe.EmptyTrayParams.TrayMiscSettings.GetGoldenGrid(true);
                return;
            }

            //(0) Global MESS
            var xParaGrid = RecipeParaGridClass.Instance;
            var xInpectParam = InspectX3ParaClass.Instance;
            var xRegionCells = _xRecipe.xRegionCells;
            xRegionCells.Clear();

            double standardChipWidth = xInpectParam.mWidthStand;
            double standardChipHeight = xInpectParam.mHeightStand;

            //(1) TransformModel
            var transCP = this.TransformsModel.GetCameraPhysicTransform(carrierID);
            if (transCP == null)
            {
                var errCode = ErrCodes.NO_CALIB_TRANSFORM;
                var errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + " !";
                OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return;
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
                    return;
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
                using (var jx = loadEmptyTrayAoiRecipe(false))
                {
                    var traySettings = jx?.TrayMiscSettings;
                    if (traySettings != null)
                    {
                        xParaGrid.xRealOffsetX = (float)traySettings.PitchX.Value;
                        xParaGrid.xRealOffsetY = (float)traySettings.PitchY.Value;
                    }
                }
            }
        }
        private void buildRegionCells_motor(CarrierEnum carrierID, EzBlocsGrid runtimeCamGrid, bool optWriteBlackToRecipe)
        {
            if (runtimeCamGrid == null)
            {
                //>>> camGrid = _jxRecipe.EmptyTrayParams.TrayMiscSettings.GetGoldenGrid(true);
                return;
            }

            ErrCodes errCode;
            string errMsg;

            //(0) Global MESS
            var xParaGrid = RecipeParaGridClass.Instance;
            var xInpectParam = InspectX3ParaClass.Instance;
            var xRegionCells = _xRecipe.xRegionCells;
            xRegionCells.Clear();

            //(1) TransformModel
            var transCM1 = this.TransformsModel.GetCameraMotorTransform(carrierID, SuckerRowEnum.S1);
            var transCP = this.TransformsModel.GetCameraPhysicTransform(carrierID);
            if (transCP == null || transCM1 == null)
            {
                errCode = ErrCodes.NO_CALIB_TRANSFORM;
                errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + " !";
                OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return;
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
            using (var jx = loadEmptyTrayAoiRecipe(false))
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
                    return;
                }

                // 更新 CameraGrid
                if (carrierID == CarrierEnum.C1)
                {
                    _xRecipe.xCamGrid1?.Dispose();
                    _xRecipe.xCamGrid1 = runtimeCamGrid;
                }
                else
                {
                    _xRecipe.xCamGrid2?.Dispose();
                    _xRecipe.xCamGrid2 = runtimeCamGrid;
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
        }
        string checkCameraGrid(CarrierEnum carrierID, EzBlocsGrid camGrid, bool notify)
        {
            //var errCode = ErrCodes.OK;
            //string errMsg = null;

            //if (camGrid == null)
            //{
            //    errCode = ErrCodes.NO_CAMERA_GRID;
            //    errMsg = JetEazy.QxNums.GetEnumDescription(carrierID)
            //           + " " + JetEazy.QxNums.GetEnumDescription(errCode)
            //           + " !";
            //}
            //else if (camGrid.Rows < 2 || camGrid.Cols < 2)
            //{
            //    errCode = ErrCodes.LOW_GRID_ROWS_COLS;
            //    errMsg = JetEazy.QxNums.GetEnumDescription(carrierID)
            //           + " " + JetEazy.QxNums.GetEnumDescription(errCode)
            //           + $" rows={camGrid.Rows}, cols={camGrid.Rows} !";
            //}

            //if (notify)
            //    OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
            //return errMsg;

            (var errCode, var errMsg) = TransformsModel.checkCameraGrid(carrierID, camGrid);

            if (errCode != ErrCodes.OK)
            {
                if (notify)
                    OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                return errMsg;
            }

            return null;
        }
        #endregion

        bool ITravelerModel.WriteCoordsToPlc(CarrierEnum carrierID, SuckerRowEnum suckerRowID, out PointF camPt, out PointF suckerMotorPt, out string errMsg)
        {
            PointF _P(QVector v) { return v == null ? PointF.Empty : new PointF((float)v.X, (float)v.Y); }

            ErrCodes errCode;

            (errCode, errMsg) = TransformsModel.GetCoordsRef(carrierID, out var camCoord, out var s1MotorCoord, out var s2MotorCoord);

            camPt = _P(camCoord);
            suckerMotorPt = suckerRowID == SuckerRowEnum.S1 ? _P(s1MotorCoord) : _P(s2MotorCoord);

            if (errCode == ErrCodes.OK)
                mxWriteToPlc(carrierID, suckerRowID, suckerMotorPt);

            return (errCode == ErrCodes.OK);
        }
        public bool WriteAllCoordsToPlc(out string message)
        {
            PointF _P(QVector v) { return v == null ? PointF.Empty : new PointF((float)v.X, (float)v.Y); }

            //(0) PlcIO
            var plcIO = ((MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE)?.PLCIO;
            if (plcIO == null)
            {
                //msg = "Machine.PLCIO 還沒配置!";
                message = JetEazy.QxNums.GetEnumDescription(ErrCodes.NO_PLC_IO) + " !";
                return false;
            }

            var carrierIDs = Enum.GetValues(typeof(CarrierEnum));
            var suckerIDs = Enum.GetValues(typeof(SuckerRowEnum));

            var errMsgs = new System.Collections.Generic.List<string>();
            bool totalOK = true;

            ErrCodes err;
            string errMsg;

            foreach (CarrierEnum C in carrierIDs)
            {
                (err, errMsg) = TransformsModel.GetCoordsRef(C, out var camCoord, out var s1MotorCoord, out var s2MotorCoord);
                if (err == ErrCodes.OK)
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
