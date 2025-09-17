#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;


namespace JetEazy.Transform
{
    public class DynamicRange
    {
        public double Min
        {
            get;
            protected set;
        }
        public double Max
        {
            get;
            protected set;
        }

        public DynamicRange(DynamicRange src = null)
        {
            if (src != null)
            {
                Min = src.Min;
                Max = src.Max;
            }
            else
            {
                Min = 0;
                Max = 10000;
            }
        }
        public DynamicRange(double min, double max)
        {
            Min = Math.Min(min, max);
            Max = Math.Max(Math.Max(max, min), min + 1);
        }
        public double Normalize(double v)
        {
            return (v - Min) / (Max - Min);
        }
        public double DeNormalize(double v)
        {
            return v * (Max - Min) + Min;
        }

        public override string ToString()
        {
            return $"DynamicRange({Min:0.000}, {Max:0.000}), Span={Max - Min:0.000}";
        }
    }
}
