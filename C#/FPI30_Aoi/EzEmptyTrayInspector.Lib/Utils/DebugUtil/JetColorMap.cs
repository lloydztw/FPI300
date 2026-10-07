#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-18 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace LeTian.AoiLib
{
    public class JetColorMap
    {
        #region PRIVATE_DATA
        private List<Scalar> _colors;
        private readonly Random _random = new Random();
        #endregion

        /// <summary>
        /// Initializes a new instance of the ColorMap class.
        /// </summary>
        /// <param name="N">The number of discrete colors to generate.</param>
        public JetColorMap(int N = 10)
        {
            _colors = InitColors(N);
        }

        /// <summary>
        /// Generates a list of colors based on the Jet colormap algorithm.
        /// </summary>
        /// <param name="N">The number of discrete colors to generate.</param>
        /// <returns>A list of System.Drawing.Color objects.</returns>
        private List<Scalar> InitColors(int N)
        {
            var colors = new List<Scalar>(N);
            for (int i = 0; i < N; i++)
            {
                // 將 i 映射到 [0, 1] 區間
                double value = (double)i / (N - 1);

                // Jet colormap 演算法
                double r, g, b;

                if (value < 0.125)
                {
                    r = 0;
                    g = 0;
                    b = 0.5 + 4 * value;
                }
                else if (value < 0.375)
                {
                    r = 0;
                    g = 4 * (value - 0.125);
                    b = 1;
                }
                else if (value < 0.625)
                {
                    r = 4 * (value - 0.375);
                    g = 1;
                    b = 1 - 4 * (value - 0.375);
                }
                else if (value < 0.875)
                {
                    r = 1;
                    g = 1 - 4 * (value - 0.625);
                    b = 0;
                }
                else
                {
                    r = 1 - 4 * (value - 0.875);
                    g = 0;
                    b = 0;
                }

                int red = (int)(r * 255);
                int green = (int)(g * 255);
                int blue = (int)(b * 255);

                //colors.Add(Color.FromArgb(255, red, green, blue));
                colors.Add(new Scalar(red, green, blue, 255));
            }

            return colors;
        }

        /// <summary>
        /// Gets a color from the list at a specific index, with wrapping.
        /// </summary>
        /// <param name="i">The index of the color to retrieve.</param>
        /// <returns>A System.Drawing.Color object.</returns>
        public Scalar GetColor(int i)
        {
            int n = _colors.Count;
            int index = i % n;
            return _colors[index];
        }

        /// <summary>
        /// Gets a random color from the list.
        /// </summary>
        /// <returns>A random System.Drawing.Color object.</returns>
        public Scalar GetRndColor()
        {
            int n = _colors.Count;
            int index = _random.Next(0, n);
            return _colors[index];
        }

        public Scalar this[int i]
        {
            get => GetColor(i);
        }
    }
}
