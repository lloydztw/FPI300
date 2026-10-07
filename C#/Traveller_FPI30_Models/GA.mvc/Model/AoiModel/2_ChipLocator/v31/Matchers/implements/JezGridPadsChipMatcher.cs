#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-17 開始重整優化 Gaara 原來的 MvdFindClass (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using JetEazy.Match;
using JetEazy.QvMath;
using LeTian.AoiLib;
using System.Drawing;
using RecipeParams = LaserAlignDX.OPSpace.RecipeSpace.InspectX3ParaClass;


namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 使用全新 LT AoiLib 套件
    /// (注意: 只能 專門用於 格點晶粒)
    /// </summary>
    public class JezGridPadsChipMatcher : IMvdTemplateMatcher
    {
        #region LETIAN_RIGID_BODY_GRID_MATCHER
        EzRigidBodyGridMatcher _ezChipMatcher = new EzRigidBodyGridMatcher();
        #endregion

        #region PRIVATE_RUNTIME_DATA
        EzRigidBodyGridMatcher.RigidBody _resultChipInfo;
        #endregion

        #region RECIPE_PARAMS
        RecipeParams _recipeParams;
        #endregion

        ~JezGridPadsChipMatcher()
        {
            // for Garbage Collection
            Dispose();
        }
        public void Dispose()
        {
            _ezChipMatcher?.Dispose();
            _ezChipMatcher = null;
        }

        public void SetRecipeParams(RecipeParams recipeParams)
        {
            _recipeParams = recipeParams;
        }

        /// <summary>
        /// 分析模板圖片 (Golden Template)
        /// 為了相容 Gaara 使用 MVD 訓練之接口函式
        /// </summary>
        /// <param name="bmpTemplate">由 caller 維護其生命週期</param>
        public bool Train(Bitmap bmpTemplate)
        {
            this.TemplateSize = bmpTemplate.Size;

            _ezChipMatcher.SetGoldenTemplate(bmpTemplate);

            var rig = _ezChipMatcher.GetGoldenBody();
            var grid = rig?.Grid;
            if (grid != null)
                this.GoldenQuad2D = _GetContourQuad2D(grid);
            else
                this.GoldenQuad2D = QvQuad2D.From(new Rectangle(Point.Empty, TemplateSize));

            return true;
        }

        public Size TemplateSize
        {
            get;
            private set;
        } = new Size(1, 1);

        /// <summary>
        /// 樣板特徵外廓 (必須於調用 Train 之後, 才有有效值!)
        /// </summary>
        public QvQuad2D GoldenQuad2D
        {
            get;
            private set;
        }

        /// <summary>
        /// MVD 執行比對 (對外統一接口 !)
        /// (1) 不要跟別的專案混雜在一起, 不要暴露一大堆 HikTrain2, HikTrain3 這些雜亂的函式!
        /// (2) 如果眾多專案有共同的部分, 請用抽出 Interface 或 Abstract Class
        /// </summary>
        /// <param name="bmpScene">由 caller 維護其生命週期</param>
        public bool RunMatch(Bitmap bmpScene)
        {
            //xResults.Clear();

            _ezChipMatcher.PadThreshold = _recipeParams.xGridPadThreshold;
            _ezChipMatcher.DistTransThreshold = _recipeParams.xDistTransThreshold;
            _ezChipMatcher.LargeAngleEnabled = _recipeParams.xUseLargePadGridAngle;

            var rigidBody = _ezChipMatcher.FindBestMatch(bmpScene);

            //if (rigidBody != null)
            //    convert_to_gaara_result(rigidBody, xResults);

            _resultChipInfo = rigidBody;

            bool bOK = rigidBody?.Grid != null;
            return bOK;
        }

        /// <summary>
        /// 取得定位後 Chip 上面 PAD 的資訊
        /// </summary>
        public EzBlocsGrid GetResultPadsGrid()
        {
            return _resultChipInfo?.Grid;
        }

        /// <summary>
        /// 取得 廣義的四角多邊形
        /// </summary>
        public QvQuad2D GetResultQuad2D()
        {
            return _GetContourQuad2D(_resultChipInfo?.Grid);
        }

        /// <summary>
        /// 調試用
        /// </summary>
        public object GetResultDetails()
        {
            return _resultChipInfo;
        }

        /// <summary>
        /// 顯示 Golden Template 特徵圖 (調試用)
        /// </summary>
        public void ShowGoldenTemplateVisualizer(bool show)
        {
            _ezChipMatcher?.ShowGoldenGridVisualizer(show);
        }

        #region PRIVATE_HELPER_FUNCTIONS
        QvQuad2D _GetContourQuad2D(EzBlocsGrid grid)
        {
            if (grid == null) return null;
            EzBlocsGridAnalyzer.CalcQuad2D(grid, out var quad2D, useBoundaryPoints: true);
            return quad2D;
        }
        #endregion

        #region OLD_CODE
#if (OPT_OLD_CODE)
        /// <summary>
        /// 即將廢除
        /// </summary>
        public List<xFindResult> xResults
        {
            get;
            private set;
        } = new List<xFindResult>();

        void convert_to_gaara_result(EzRigidBodyGridMatcher.RigidBody rigidBody, List<xFindResult> results)
        {
            results?.Clear();

            var golden = _ezChipMatcher.GetGoldenBody();
            var goldenGrid = golden?.Grid;
            var rigidBodyGrid = rigidBody?.Grid;

            if (goldenGrid == null || rigidBodyGrid == null)
                return;

            //EzBlocsGridAnalyzer.CalcRotatedBox2D(rigidBodyGrid, out var box2d, useBoundaryPoints: false);
            EzBlocsGridAnalyzer.CalcQuad2D(rigidBodyGrid, out var chipQuad, useBoundaryPoints: false);
            var center = chipQuad.Center;
            var angle = chipQuad.Theta * 180.0 / Math.PI;
            var score = rigidBody.Score;

            //EzBlocsGridAnalyzer.CalcRotatedBox2D(goldenGrid, out var goldenBox2d, useBoundaryPoints: false);
            EzBlocsGridAnalyzer.CalcQuad2D(goldenGrid, out var goldenQuad, useBoundaryPoints: false);
            //var angleG = goldenBox2d.Theta * 180.0 / Math.PI;
            //angle -= angleG;

            // SCALE
            //double scaleW = box2d.MinAreaRectSize.Width / (goldenBox2d.MinAreaRectSize.Width + 0.0001);
            //double scaleH = box2d.MinAreaRectSize.Height / (goldenBox2d.MinAreaRectSize.Height + 0.0001);
            chipQuad.GetMidSize(out var sizeB);
            goldenQuad.GetMidSize(out var sizeG);
            double scaleW = sizeB.Width / (sizeG.Width + 0.00001);
            double scaleH = sizeB.Height / (sizeG.Height + 0.00001);
            double scale = (scaleW + scaleH) / 2;

            // Gaara Result
            xFindResult result = new xFindResult
            {
                fCenterX = (float)center.X,
                fCenterY = (float)center.Y,
                fAngle = (float)angle,
                fScale = (float)scale,
                fScore = (float)score
            };

            results.Add(result);
        }
#endif
        #endregion
    }
}
