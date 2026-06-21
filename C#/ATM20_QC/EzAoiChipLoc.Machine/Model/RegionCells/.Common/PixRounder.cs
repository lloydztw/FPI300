#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-22 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using System;
using System.Collections.Generic;
using EzBloc = JetEazy.Match.EzBloc;

namespace LaserAlignDX.Model
{
    public static class PixRounder
    {
        public static double RoundPix(double pix)
        {
            return Math.Round(pix, 2);
        }
        public static void RoundPix(this QVector v)
        {
            if (v != null)
            {
                for (int i = 0, N = v.Dimensions; i < N; i++)
                    v[i] = RoundPix(v[i]);
            }
        }
        public static void RoundPix(this EzBloc bloc)
        {
            bloc?.Center?.RoundPix();
        }
        public static void RoundPix(IEnumerable<QVector> pts)
        {
            if (pts != null)
            {
                foreach (var p in pts)
                    p?.RoundPix();
            }
        }
        public static void RoundPix(IEnumerable<EzBloc> pts)
        {
            if (pts != null)
            {
                foreach (var p in pts)
                    p?.RoundPix();
            }
        }
    }
}
