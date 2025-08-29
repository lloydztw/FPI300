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


using AUVision;
using JetEazy.Match;
using JetEazy.QvMath;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using RecipeParams = LaserAlignDX.OPSpace.RecipeSpace.InspectX3ParaClass;

namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 使用全新 LT AoiLib 套件
    /// (注意: 只能 專門用於 格點晶粒)
    /// </summary>
    public class MvdChipMatcherRG : IMvdTemplateMatcher
    {
        #region PRIVATE_MVD_VisionDesigner_Members
        EzRigidBodyGridMatcher _ezChipMatcher = new EzRigidBodyGridMatcher();
        #endregion

        #region PRIVATE_RUNTIME_DATA
        bool _isMvdParamsChanged = false;
        EzRigidBodyGridMatcher.RigidBody _resultChipInfo;
        #endregion

        #region RECIPE_PARAMS
        RecipeParams _recipeParams;
        //public float xMvdAngle { get; set; } = 5;
        //public int xMvdMaxOcc { get; set; } = 1;
        //public float xMvdTolerance { get; set; } = 0.5f;
        //public PointF xMvdFixed { get; set; } = new PointF(-1, -1);
        //public int xMaxOverlap { get; set; } = 80;
        #endregion

        public Size TemplateSize
        {
            get;
            private set;
        } = new Size(1, 1);
        public List<xFindResult> xResults
        {
            get;
            private set;
        } = new List<xFindResult>();

        ~MvdChipMatcherRG()
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
        /// MVD 訓練 (對外統一接口 !)
        /// (1) 不要跟別的專案混雜在一起, 不要暴露一大堆 HikTrain2, HikTrain3 這些雜亂的函式!
        /// (2) 如果眾多專案有共同的部分, 請用抽出 Interface 或 Abstract Class
        /// </summary>
        /// <param name="bmpTemplate">由 caller 維護其生命週期</param>
        public bool Train(Bitmap bmpTemplate)
        {
            this.TemplateSize = bmpTemplate.Size;
            _ezChipMatcher.SetGoldenTemplate(bmpTemplate);
            return true;
        }

        /// <summary>
        /// MVD 執行比對 (對外統一接口 !)
        /// (1) 不要跟別的專案混雜在一起, 不要暴露一大堆 HikTrain2, HikTrain3 這些雜亂的函式!
        /// (2) 如果眾多專案有共同的部分, 請用抽出 Interface 或 Abstract Class
        /// </summary>
        /// <param name="bmpScene">由 caller 維護其生命週期</param>
        public bool RunMatch(Bitmap bmpScene)
        {
            xResults.Clear();

            _ezChipMatcher.PadThreshold = _recipeParams.xGridPadThreshold;
            var rigidBody = _ezChipMatcher.FindBestMatch(bmpScene);

            if (rigidBody != null)
                convert_to_gaara_result(rigidBody, xResults);
            
            _resultChipInfo = rigidBody;

            bool bOK = xResults.Count > 0;
            return bOK;
        }

        /// <summary>
        /// 取得定位後 Chip 上面 PAD 的資訊
        /// </summary>
        public EzBlocsGrid GetResultPadsGrid()
        {
            return _resultChipInfo?.Grid;
        }

        public QvBox2D GetResultBox2D()
        {
            return _resultChipInfo?.CalcBox2D(true);
        }

        #region PRIVATE_HELPER_FUNCTIONS
        void convert_to_gaara_result(EzRigidBodyGridMatcher.RigidBody rigidBody, List<xFindResult> results)
        {
            results?.Clear();

            var golden = _ezChipMatcher.GetGoldenBody();
            var goldenGrid = golden?.Grid;
            var rigidBodyGrid = rigidBody?.Grid;

            if (goldenGrid == null || rigidBodyGrid == null)
                return;

            EzBlocsGridAnalyzer.CalcRotatedBox2D(rigidBodyGrid, out var box2d, useBoundaryPoints: false);
            var center = box2d.Center;
            var angle = box2d.Theta * 180.0 / Math.PI;
            var score = rigidBody.Score;

            EzBlocsGridAnalyzer.CalcRotatedBox2D(goldenGrid, out var goldenBox2d, useBoundaryPoints: false);
            //var angleG = goldenBox2d.Theta * 180.0 / Math.PI;
            //angle -= angleG;

            // SCALE
            double scaleW = box2d.MinAreaRectSize.Width / (goldenBox2d.MinAreaRectSize.Width + 0.01);
            double scaleH = box2d.MinAreaRectSize.Height / (goldenBox2d.MinAreaRectSize.Height + 0.01);
            double scale = (scaleW + scaleH) / 2;

            // Gaara Result
            xFindResult result = new xFindResult
            {
                fCenterX = center.X,
                fCenterY = center.Y,
                fAngle = (float)angle,
                fScale = (float)scale,
                fScore = (float)score
            };

            xResults.Add(result);
        }
        #endregion
    }
}
