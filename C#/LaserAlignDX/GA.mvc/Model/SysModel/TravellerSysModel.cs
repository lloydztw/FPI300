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
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model.Recipe;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using NeedleX.ProcessSpace;
using System;
using System.Drawing;
using Traveller106;

namespace LaserAlignDX.Mvc.Model
{
    public interface ITravelerModel : IDisposable
    {
        event EventHandler<ProcessEventArgs> OnError;

        IProcessRunFPI AoiModel { get; }
        ICalibAoiModel CalibAoiModel { get; }
        IxEmptyTrayInspector EmptyTrayAoiModel { get; }
        TravellerTransforms TransformsModel { get; }
        GaBigImageHolder LineScanImageHolder { get; }

        void ApplyRecipe(string gaaraRecipeName = null);
        JxRecipeCombo GetCurrentRecipe();
    }


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
        public void ApplyRecipe(string gaaraRecipeName)
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

            applyRecipe(_jxRecipe);
        }

        #region PRIVATE_FUNCTIONS
        void applyRecipe(JxRecipeCombo recipe)
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

            //// 將參數餵給其他子系統 (Child Models)
            EmptyTrayAoiModel.SetRecipe(recipe.EmptyTrayParams);

            //var model = CalibAoiModel;
            //model.SetRecipe(recipe.EmptyTrayParams);

            // 建置 Cell Regions
            if (camGrid == null)
            {
                var errMsg = $"空盤參數 {recipe.Name} 建置不全, 沒有格點數據!";
                LtDebug.LOG.Error(errMsg);
                OnError?.Invoke(this, new ProcessEventArgs(errMsg));
                return;
            }
            this.BuildCellRegions(camGrid, ActiveCarrierID);
        }
        #endregion

        public void BuildCellRegions(EzBlocsGrid camGrid, CarrierEnum C)
        {
            RecipeFPIX3Class xRecipe = RecipeFPIX3Class.Instance;
            double xChipWidth = InspectX3ParaClass.Instance.mWidthStand;
            double xChipHeight = InspectX3ParaClass.Instance.mHeightStand;
            //var xChipWidth = xRecipe.xChipWidth;
            //var xChipHeight = xRecipe.xChipHeight;
            var xRegionCells = xRecipe.xRegionCells;

            //(0) TransformModel
            var transformModel = GaMvcConfig.SysModel.TransformsModel;
            var transCP = transformModel.GetCameraPhysicTransform(C);

            //(1) goldenChipRect (in camera coordinates)
            var standardChipSize = new QVector(xChipWidth, xChipHeight);
            RectangleF goldenChipRect;
            if (true)
            {
                // 從晶粒長寬規格 xChipWidth, xChipHeight (mm)
                // 反推其在 Camera 座標系上 的大小
                var camPt0 = camGrid[0, 0].Center;
                var worldPt0 = transCP.Trans(camPt0);
                var p1 = worldPt0 - (standardChipSize / 2);
                var p2 = worldPt0 + (standardChipSize / 2);
                p1 = transCP.InvTrans(p1);
                p2 = transCP.InvTrans(p2);
                var delta = p2 - p1;
                goldenChipRect = new RectangleF((float)p1.X, (float)p1.Y, (float)delta.X, (float)delta.Y);
            }
            SizeF goldenSize = goldenChipRect.Size;

            //(2) Zigzag 順序加入至 xRegionCells
            xRegionCells.Clear();

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
                cell.viewRectF = JetEazy.Qcvt.CreateCenterRect((float)bloc.Center.X, (float)bloc.Center.Y, ref goldenSize);

                //cell OrgX 與 OrgY 是 Chip 左上角 ?
                //cell.OrgX = xRealLeftX + j * xRealOffsetX;
                //cell.OrgY = xRealLeftY + i * xRealOffsetY;
                var centroid = transCP.Trans(bloc.Center);
                cell.OrgX = (float)(centroid.X);    // - standardChipSize.X / 2);
                cell.OrgY = (float)(centroid.Y);    // - standardChipSize.Y / 2);

                xRegionCells.Add(cell);
                index++;
            }
        }
    }
}
