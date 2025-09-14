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
        public void ApplyRecipe(string gaaraRecipeName = null, bool optWritebackToRecipe = false)
        {
            // 載入 TransformsModel 設定 (全域)
            var transformsModel = this.TransformsModel;
            transformsModel.Load(GaMvcPaths.CALIB_TRANSFORMS_FILE);
            transformsModel.BuildAll();

            // 載入 Camera Grid (保存在個別參數)
            EzBlocsGrid camGrid = ActiveCarrierID == CarrierEnum.C1 ?
                                    _xRecipe.xCamGrid1 :
                                    _xRecipe.xCamGrid2;

            // 檢查 camGrid 的數據狀態
            if (checkCameraGrid(ActiveCarrierID, camGrid, notify: true) != null)
                return;

            // Config Plc Grid
            var rows = camGrid.Rows;
            var cols = camGrid.Cols;
            //var pitchX = (double)traySettings.PitchX.Value;
            //var pitchY = (double)traySettings.PitchY.Value;
            double pitchX = _xRecipe.xRealOffsetX;
            double pitchY = _xRecipe.xRealOffsetY;
            transformsModel.ConfigPlcGrid(rows, cols, pitchX, pitchY);

            // 重新載入 EmptyTrayAoiRecipe
            using (var jx = loadEmptyTrayAoiRecipe(false))
            {
                if (jx != null)
                    EmptyTrayAoiModel.SetRecipe(jx);
            }

            // 自動建立陣列
            buildRegionCells(ActiveCarrierID, camGrid, false);
        }
        public MatchResult AutoBuildRegionCells(Bitmap fullfovBmp)
        {
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

            buildRegionCells(ActiveCarrierID, camGrid, true);

            return matchResult;
        }

        #region PRIVATE_REGION_CELL_FUNCTIONS
        private JxAoiRecipe loadEmptyTrayAoiRecipe(bool alwayCreateOne)
        {
            var gaaraRcpName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
            var fileName = LtAoiFactory.RcpGetRecipeFileName(gaaraRcpName);
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
        private void buildRegionCells(CarrierEnum carrierID, EzBlocsGrid camGrid, bool optWriteBlackToRecipe)
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
            var trfCamToWorld = this.TransformsModel.GetCameraPhysicTransform(carrierID);
            if (trfCamToWorld == null)
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
                var worldPt0 = trfCamToWorld.Trans(camPt0);
                var goldenP1 = worldPt0 - (standardChipSize / 2);
                var goldenP2 = worldPt0 + (standardChipSize / 2);
                goldenP1 = trfCamToWorld.InvTrans(goldenP1);
                goldenP2 = trfCamToWorld.InvTrans(goldenP2);
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
                var wCenter = trfCamToWorld.Trans(bloc.Center);
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
                var p00 = trfCamToWorld.Trans(camGrid[0, 0].Center);
                var p01 = trfCamToWorld.Trans(camGrid[0, cols - 1].Center);
                var p10 = trfCamToWorld.Trans(camGrid[rows - 1, 0].Center);
                var p11 = trfCamToWorld.Trans(camGrid[rows - 1, cols - 1].Center);
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
        string checkCameraGrid(CarrierEnum carrierID, EzBlocsGrid camGrid, bool notify)
        {
            var errCode = ErrCodes.OK;
            string errMsg = null;

            if (camGrid == null)
            {
                errCode = ErrCodes.NO_CAMERA_GRID;
                errMsg = JetEazy.QxNums.GetEnumDescription(carrierID)
                       + " " + JetEazy.QxNums.GetEnumDescription(errCode)
                       + " !";
            }
            else if (camGrid.Rows < 2 || camGrid.Cols < 2)
            {
                errCode = ErrCodes.LOW_GRID_ROWS_COLS;
                errMsg = JetEazy.QxNums.GetEnumDescription(carrierID)
                       + " " + JetEazy.QxNums.GetEnumDescription(errCode)
                       + $" rows={camGrid.Rows}, cols={camGrid.Rows} !";
            }
            
            if (notify)
                OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));

            return errMsg;
        }
        #endregion

        public bool GetCoordsRef(CarrierEnum carrierID, SuckerRowEnum suckerRowID, out QVector camCoord, out QVector suckerWorldCoord, out string msg)
        {
            camCoord = new QVector(0, 0);
            suckerWorldCoord = new QVector(0, 0);

            //(0) PlcIO
            var plcIO = ((MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE)?.PLCIO;
            if (plcIO == null)
            {
                //msg = "Machine.PLCIO 還沒配置!";
                msg = JetEazy.QxNums.GetEnumDescription(ErrCodes.NO_PLC_IO) + " !";
                return false;
            }

            //(1) Transform
            var transCM = this.TransformsModel.GetCameraMotorTransform(carrierID, suckerRowID);
            var transCP = this.TransformsModel.GetCameraPhysicTransform(carrierID);
            if (transCM == null || transCP == null)
            {
                //msg = "缺少完整的 \"全域校正\" 數據!";
                msg = JetEazy.QxNums.GetEnumDescription(ErrCodes.NO_CALIB_TRANSFORM) + " !";
                return false;
            }

            //(2) Camera Grid (從 _xRecipe 取得)
            var camGrid = (carrierID == CarrierEnum.C1) ? _xRecipe.xCamGrid1 : _xRecipe.xCamGrid2;

            //(3) 檢查 camGrid 數據狀態
            var errMsg = checkCameraGrid(ActiveCarrierID, camGrid, false);
            if (errMsg != null)
            {
                msg = errMsg;
                return false;
            }

            //(4) 返回座標值
            camCoord = camGrid[0, 0].Center;
            suckerWorldCoord = transCM.Trans(camCoord);

            //(5) Msg
            msg = JetEazy.QxNums.GetEnumDescription(ErrCodes.OK);
            return true;
        }
        public bool WriteCoordsToPlc(CarrierEnum carrierID, SuckerRowEnum suckerRowID, out PointF camCoord, out PointF suckerCoord, out string msg)
        {
            PointF _P(QVector v) { return v == null ? PointF.Empty : new PointF((float)v.X, (float)v.Y); }

            bool ok = GetCoordsRef(carrierID, suckerRowID, out var camPt, out var suckerPt, out msg);

            camCoord = _P(camPt);
            suckerCoord = _P(suckerPt);

            if (ok)
                mxWriteToPlc(carrierID, suckerRowID, suckerCoord);

            return ok;
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

            foreach (CarrierEnum C in carrierIDs)
            {
                foreach (SuckerRowEnum S in suckerIDs)
                {
                    bool ok = GetCoordsRef(C, S, out var camPt, out var suckerPt, out var msg);
                    if (ok)
                        mxWriteToPlc(C, S, _P(suckerPt));
                    else
                        errMsgs.Add(msg);
                    totalOK &= ok;
                }
            }

            message = totalOK ? "OK" : string.Join("\n\r", errMsgs);
            return totalOK;
        }

        #region PRIVATE_PLC_FUNCTIONS
        void mxWriteToPlc(CarrierEnum carrierID, SuckerRowEnum suckerRowID, PointF suckerCoord)
        {
            var plcIO = ((MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE)?.PLCIO;

            var NA = PointF.Empty;

            if (carrierID == CarrierEnum.C1 && suckerRowID == SuckerRowEnum.S1)
            {
                plcIO?.SetStage1((int)suckerRowID, suckerCoord, NA);
            }
            else if (carrierID == CarrierEnum.C1 && suckerRowID == SuckerRowEnum.S2)
            {
                plcIO?.SetStage1((int)suckerRowID, NA, suckerCoord);
            }
            else if (carrierID == CarrierEnum.C2 && suckerRowID == SuckerRowEnum.S1)
            {
                plcIO?.SetStage2((int)suckerRowID, suckerCoord, NA);
            }
            else if (carrierID == CarrierEnum.C2 && suckerRowID == SuckerRowEnum.S2)
            {
                plcIO?.SetStage2((int)suckerRowID, NA, suckerCoord);
            }
        }
        #endregion
    }
}
