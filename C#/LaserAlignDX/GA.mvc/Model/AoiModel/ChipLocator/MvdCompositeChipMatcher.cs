#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 開始重整優化 Gaara 原來的 MvdFindClass (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Match;
using JetEazy.QvMath;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Drawing;
using System.Threading.Tasks;
using RecipeParams = LaserAlignDX.OPSpace.RecipeSpace.InspectX3ParaClass;

namespace LaserAlignDX.AoiModel
{
    public class MvdCompositeChipMatcher : IMvdTemplateMatcher
    {
        public static int N_CHANNLS => Traveller106.Universal.N_THREADS;

        #region GLOBAL_RECIPE_MESS
        InspectX3ParaClass _recipeParams => InspectX3ParaClass.Instance;
        #endregion

        #region PRIVATE_DATA
        MatchAlgorithmEnum _algorithm = MatchAlgorithmEnum.GridMatch;
        IMvdTemplateMatcher[] _matchers;
        #endregion

        public MvdCompositeChipMatcher()
        {
            ConstructMatchers(_algorithm);
        }
        public void Dispose()
        {
            if (_matchers == null)
                return;

            var old = _matchers;
            _matchers = null;

            foreach (var matcher in old)
                matcher?.Dispose();
        }

        #region PRIVATE_CUSTRUCTOR
        /// <summary>
        /// 建構 matchers 實體
        /// </summary>
        void ConstructMatchers(MatchAlgorithmEnum algorithm)
        {
            if (_matchers == null)
                _matchers = new IMvdTemplateMatcher[N_CHANNLS];

            if (algorithm == MatchAlgorithmEnum.GridMatch)
            {
                for (int i = 0; i < N_CHANNLS; i++)
                {
                    _matchers[i]?.Dispose();
                    _matchers[i] = new JezGridPadsChipMatcher();
                }
            }
            else
            {
                for (int i = 0; i < N_CHANNLS; i++)
                {
                    _matchers[i]?.Dispose();
                    _matchers[i] = new MvdChipMatcher();
                }
            }
        }
        #endregion

        /// <summary>
        /// 演算法
        /// </summary>
        public MatchAlgorithmEnum Algorithm
        {
            get => _algorithm;
        }
        public void ChangeAlgorithm(MatchAlgorithmEnum algorithm)
        {
            if(_algorithm != algorithm)
                ConstructMatchers(_algorithm = algorithm);
        }

        public IMvdTemplateMatcher GetMatcher(int index)
        {
            return _matchers[index];
        }
        public IMvdTemplateMatcher this[int index]
        {
            get { return _matchers[index]; }
        }

#if (false)
        public int xMaxOverlap
        {
            get => _matchers[0].xMaxOverlap;
            set
            {
                foreach(var matcher in _matchers)
                    matcher.xMaxOverlap = value;
            }
        }
        public float xMvdAngle
        {
            get => _matchers[0].xMvdAngle;
            set
            {
                foreach (var matcher in _matchers)
                    matcher.xMvdAngle = value;
            }
        }
        public PointF xMvdFixed
        {
            get => _matchers[0].xMvdFixed;
            set
            {
                foreach (var matcher in _matchers)
                    matcher.xMvdFixed = value;
            }
        }
        public int xMvdMaxOcc
        {
            get => _matchers[0].xMvdMaxOcc;
            set
            {
                foreach (var matcher in _matchers)
                    matcher.xMvdMaxOcc = value;
            }
        }
        public float xMvdTolerance
        {
            get => _matchers[0].xMvdTolerance;
            set
            {
                foreach (var matcher in _matchers)
                    matcher.xMvdTolerance = value;
            }
        }
#endif

        public void SetRecipeParams(RecipeParams recipeParams)
        {
            ChangeAlgorithm(_recipeParams.xAlgorithm);
            foreach (var matcher in _matchers) 
                matcher.SetRecipeParams(recipeParams);
        }
        public bool Train(Bitmap bmpTemplate)
        {
            //bool ok = true;
            //foreach(var matcher in _matchers)
            //    ok &= matcher.Train(bmpTemplate);
            //return ok;

            //---------------------------------------------
            // 暫時使用 Parallel.For 來加速 Train 的時間.
            //---------------------------------------------
            int N = _matchers.Length;
            bool[] oks = new bool[N];
            Bitmap[] bmps = new Bitmap[N];
            for (int i = 0; i < N; i++)
                bmps[i] = (Bitmap)bmpTemplate.Clone();

#if (OPT_DEBUG_ONE_BY_ONE)
            for (int i = 0; i < N; i++)
            {
                oks[i] = _matchers[i].Train(bmps[i]);
                System.Diagnostics.Trace.WriteLine($"[循序慢速] 訓練模板 Train({i}) = {oks[i]}");
            }
#else
            Parallel.For(0, N, i =>
            {
                oks[i] = _matchers[i].Train(bmps[i]);
            });
#endif

            foreach (var bmp in bmps)
                bmp.Dispose();

            return Array.TrueForAll(oks, ok => ok);
        }
        public Size TemplateSize
        {
            get => _matchers[0].TemplateSize;
        }
        public QvQuad2D GoldenQuad2D
        {
            get => _matchers[0].GoldenQuad2D;
        }

        public bool RunMatch(Bitmap bmpScene)
        {
            throw new System.Exception("請用個別的 _matchers[i] !");
            return _matchers[0].RunMatch(bmpScene);
        }

        #region OLD_CODE
        //public List<xFindResult> xResults
        //{
        //    get => _matchers[0].xResults;
        //}
        #endregion

        public EzBlocsGrid GetResultPadsGrid()
        {
            return _matchers[0]?.GetResultPadsGrid();
        }
        public QvQuad2D GetResultQuad2D()
        {
            return _matchers[0]?.GetResultQuad2D();
        }
        public object GetResultDetails()
        {
            return _matchers[0]?.GetResultDetails();
        }

        public void ShowGoldenTemplateVisualizer(bool show)
        {
            if (_matchers.Length > 0)
                _matchers[0]?.ShowGoldenTemplateVisualizer(show);
        }
    }
}
