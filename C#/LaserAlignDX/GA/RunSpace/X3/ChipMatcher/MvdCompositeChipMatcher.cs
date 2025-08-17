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

using AUVision;
using System.Collections.Generic;
using System.Drawing;

namespace LaserAlignDX.RunSpace.Supports
{
    using IMPLEMENT = MvdChipMatcherRG;

    public class MvdCompositeChipMatcher : IMvdTemplateMatcher
    {
        public static int N_CHANNLS => Traveller106.Universal.N_THREADS;

        #region PRIVATE_DATA
        IMvdTemplateMatcher[] _matchers = new IMvdTemplateMatcher[N_CHANNLS];
        #endregion

        public MvdCompositeChipMatcher()
        {
            for (int i = 0; i < N_CHANNLS; i++)
                _matchers[i] = new IMPLEMENT();
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

        public IMvdTemplateMatcher GetMatcher(int index)
        {
            return _matchers[index];
        }
        public IMvdTemplateMatcher this[int index]
        {
            get { return _matchers[index]; }
        }

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

        public Size TemplateSize
        {
            get => _matchers[0].TemplateSize;
        }
        public List<xFindResult> xResults
        {
            get => _matchers[0].xResults;
        }

        public bool Train(Bitmap bmpTemplate)
        {
            bool ok = true;
            foreach(var matcher in _matchers)
                ok &= matcher.Train(bmpTemplate);
            return ok;
        }
        public bool RunMatch(Bitmap bmpScene)
        {
            throw new System.Exception("請用個別的 _matchers[i] !");
            return _matchers[0].RunMatch(bmpScene);
        }
    }
}
