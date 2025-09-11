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
using JetEazy.BasicSpace;
using JetEazy.Machine;
using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.QvMath;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model.Recipe;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using NeedleX.ProcessSpace;
using System;
using System.Drawing;
using Traveller106;
using VsCommon.ControlSpace.MachineSpace;

namespace LaserAlignDX.Mvc.Model
{
    public class TravellerSysModel : ITravelerModel
    {
        public event EventHandler<ProcessEventArgs> OnError;
        public CarrierEnum ActiveCarrierID
        {
            get => CarrierEnum.C1;
        }

        #region SINGLETON
        static TravellerSysModel _instance;
        TravellerSysModel()
        {
        }
        #endregion

        #region AOI_MODELS
        IProcessRunFPI _aoiModel;
        IxEmptyTrayInspector _aoiEmptyTrayModel;
        ICalibAoiModel _calibModel;
        JxRecipeCombo _jxRecipe;
        #endregion

        #region TRANSFORMS
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

            _aoiEmptyTrayModel?.Dispose();
            _aoiEmptyTrayModel = null;

            _calibModel?.Dispose();
            _calibModel = null;

            _transformsModel?.Dispose();
            _transformsModel = null;

            _jxRecipe?.Release();
            _jxRecipe = null;

            TravellerBigImagesHolder.DisposeAll();

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
                _aoiEmptyTrayModel = AoiEmptyTrayInspector.Instance;
                return _aoiEmptyTrayModel;
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

        public JxRecipeCombo GetCurrentRecipe()
        {
            return _jxRecipe;
        }

        public void ApplyRecipe(string gaaraRecipeName, bool optWritebackToRecipe = false)
        {
            if (gaaraRecipeName == null)
                gaaraRecipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();

            if (_jxRecipe == null || _jxRecipe.Name != gaaraRecipeName)
            {
                var old = _jxRecipe;
                _jxRecipe = new JxRecipeCombo() { Name = gaaraRecipeName };
                _jxRecipe.Load(null);
                old?.Dispose();
            }

            applyRecipe(_jxRecipe, optWritebackToRecipe);
        }

        #region PRIVATE_FUNCTIONS
        void applyRecipe(JxRecipeCombo recipe, bool optWritebackToRecipe)
        {
            // 載入 TransformsModel 設定 (全域)
            var transformsModel = TransformsModel;
            transformsModel.Load(GaMvcPaths.CALIB_TRANSFORMS_FILE);
            transformsModel.BuildAll();

            // 強制重新載入 Camera Grid (個別參數)
            var traySettings = recipe.EmptyTrayParams.TrayMiscSettings;
            var camGrid = traySettings.GetGoldenGrid(reload: true);
            if (camGrid != null)
            {
                // Config Plc Grid
                var rows = camGrid.Rows;
                var cols = camGrid.Cols;
                var pitchX = (double)traySettings.PitchX.Value;
                var pitchY = (double)traySettings.PitchY.Value;
                transformsModel.ConfigPlcGrid(rows, cols, pitchX, pitchY);
            }

            // 將參數餵給其他子系統 (Child Models)
            EmptyTrayAoiModel.SetRecipe(recipe.EmptyTrayParams);

            // 建置 Cell Regions
            if (camGrid == null)
            {
                var errMsg = $"空盤參數 {recipe.Name} 建置不全, 沒有格點數據!";
                LtDebug.LOG.Error(errMsg);
                OnError?.Invoke(this, new ProcessEventArgs(errMsg));
                return;
            }

            this.BuildCellRegions(camGrid, ActiveCarrierID, optWritebackToRecipe);
        }
        #endregion


        internal void BuildCellRegions(EzBlocsGrid camGrid, CarrierEnum C, bool optWriteBlackToRecipe = false)
        {
            if (camGrid == null)
                camGrid = _jxRecipe.EmptyTrayParams.TrayMiscSettings.GetGoldenGrid(true);

            //(0) Global MESS
            var xRecipe = RecipeFPIX3Class.Instance;
            var xParaGrid = RecipeParaGridClass.Instance;
            var xInpectParam = InspectX3ParaClass.Instance;
            var xRegionCells = xRecipe.xRegionCells;

            double standardChipWidth = xInpectParam.mWidthStand;
            double standardChipHeight = xInpectParam.mHeightStand;
            xRegionCells.Clear();

            //(1) TransformModel
            var transformModel = GaMvcConfig.SysModel.TransformsModel;
            var trfCamToWorld = transformModel.GetCameraPhysicTransform(C);

            //(2) goldenChipRect (in camera coordinates)
            SizeF cellViewSizeF;
            if (true)
            {
                // 從晶粒長寬規格 xChipWidth, xChipHeight (mm)
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

            //(4) 更新到 xParaGrid 舊參數
            if (optWriteBlackToRecipe)
            {
                var rows = camGrid.Rows;
                var cols = camGrid.Cols;
                if (rows < 2 || cols < 2)
                    return;

                var camPt00 = camGrid[0, 0];
                var p00 = trfCamToWorld.Trans(camGrid[0, 0].Center);
                var p01 = trfCamToWorld.Trans(camGrid[0, cols - 1].Center);
                var p10 = trfCamToWorld.Trans(camGrid[rows - 1, 0].Center);
                var p11 = trfCamToWorld.Trans(camGrid[rows - 1, cols - 1].Center);
                var corners = new[] { p00, p01, p11, p10 };

                // xAngle
                var box2D = new QvBox2D();
                box2D.Corners = Array.ConvertAll(corners, c => new PointF((float)c.X, (float)c.Y));
                double angle = box2D.Theta * 180.0 / Math.PI;
                xParaGrid.xAngle = (float)Math.Round(angle, 3);

                // xRows, xColumn
                xParaGrid.xRow = rows;
                xParaGrid.xColumn = cols;

                // xLeftTopX, xLeftTopY
                xParaGrid.xLeftTopX = (int)(camPt00.Center.X - cellViewSizeF.Width);
                xParaGrid.xLeftTopY = (int)(camPt00.Center.Y - cellViewSizeF.Height);

                // xRowOffset, xColumnOffset
                xParaGrid.xRowOffset = (float)Math.Round((p10 - p00).Y / (rows - 1), 3);
                xParaGrid.xColumnOffset = (float)Math.Round((p01 - p00).X / (cols - 1), 3);

                // xChipWidth, xChipHeight
                xParaGrid.xChipWidth = (float)Math.Round(standardChipWidth, 3);
                xParaGrid.xChipHeight = (float)Math.Round(standardChipHeight, 3);

                //xRecipe.xRealLeftX = (float)p00.X;
                //xRecipe.xRealLeftY = (float)p00.Y;
                //xRecipe.xRealOffsetX = (float)(p01 - p00).X / (cols - 1);
                //xRecipe.xRealOffsetY = (float)(p10 - p00).Y / (rows - 1);
            }
        }

        public bool WriteCoordsToPlc(CarrierEnum carrierID, SuckerRowEnum suckerRowID, out PointF camCoord, out PointF suckerCoord, out string msg)
        {
            //(0) PlcIO
            var plcIO = ((MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE)?.PLCIO;
            if (plcIO == null)
            {
                msg = "找不到【PLCIO】!";
                camCoord = PointF.Empty;
                suckerCoord = PointF.Empty;
                return false;
            }

            //(1) Transform
            var transCM = GaMvcConfig.SysModel.TransformsModel.GetCameraMotorTransform(carrierID, suckerRowID);
            var traySettings = GaMvcConfig.SysModel.GetCurrentRecipe()?.EmptyTrayParams.TrayMiscSettings;
            if (traySettings == null)
            {
                msg = "缺少【空盤檢測】之參數!";
                camCoord = PointF.Empty;
                suckerCoord = PointF.Empty;
                return false;
            }

            //(2) Camera Grid
            var camGrid = traySettings.GetGoldenGrid(true);
            if (camGrid == null || camGrid.Rows<2 || camGrid.Cols<2)
            {
                msg = "缺少【全域校正】之數據!";
                camCoord = PointF.Empty;
                suckerCoord = PointF.Empty;
                return false;
            }

            //(3) Coords
            var camPt0 = camGrid[0, 0].Center;
            var suckerWorldPt = transCM.Trans(camPt0);

            PointF _P(QVector v) { return new PointF((float)v.X, (float)v.Y); }
            camCoord = _P(camPt0);
            suckerCoord = _P(suckerWorldPt);

            //(4) Write to plc (according to the combination of carriers and suckerRows)
            if (carrierID == CarrierEnum.C1 && suckerRowID == SuckerRowEnum.S1)
            {
                int suckerIndex = (int)suckerRowID;
                plcIO.SetStage1(suckerIndex, suckerCoord, PointF.Empty);
            }
            else if (carrierID == CarrierEnum.C1 && suckerRowID == SuckerRowEnum.S2)
            {
                int suckerIndex = (int)suckerRowID;
                plcIO.SetStage1(suckerIndex, PointF.Empty, suckerCoord);
            }
            else if (carrierID == CarrierEnum.C2 && suckerRowID == SuckerRowEnum.S1)
            {
                int suckerIndex = (int)suckerRowID;
                plcIO.SetStage2(suckerIndex, suckerCoord, PointF.Empty);
            }
            else if (carrierID == CarrierEnum.C2 && suckerRowID == SuckerRowEnum.S2)
            {
                int suckerIndex = (int)suckerRowID;
                plcIO.SetStage2(suckerIndex, PointF.Empty, suckerCoord);
            }
            else
            {
                msg = $"錯誤的組合 載台 {carrierID} 吸嘴 {suckerRowID}!";
                return false;
            }

            msg = "OK";
            return true;
        }
    }
}
