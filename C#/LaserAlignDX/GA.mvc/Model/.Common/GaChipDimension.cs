#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-03 重新設計 (by LeTian Chang)
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
using VM.PlatformSDKCS;

namespace LaserAlignDX.Model
{
    public class GaChipDimension
    {
        public class Measurement
        {
            /// <summary>
            /// 量測名稱
            /// </summary>
            public string Name;
            /// <summary>
            /// 單位: mm
            /// </summary>
            public float Value = 0;
            /// <summary>
            /// 判定
            /// </summary>
            public bool IsPass = false;
            /// <summary>
            /// 量測點位 (camera coordinates) 單位: pixels
            /// </summary>
            public readonly QVector[] CamMeasurePts = new QVector[2];
            /// <summary>
            /// Constructor
            /// </summary>
            public Measurement(string name)
            {
                Name = name;
            }
        }

        #region PRIVATE_DATA
        readonly Dictionary<string, Measurement> _dict = new Dictionary<string, Measurement>();
        #endregion

        #region PUBLIC_OPERATIONS
        public IEnumerable<string> Keys => _dict.Keys;
        public Measurement this[string key]
        {
            get
            {
                if (_dict.TryGetValue(key, out var measurment))
                    return measurment;
                return null;
            }
        }
        #endregion

        internal bool IsSimpleQuad
        { 
            get; private set; 
        } = true;

        public void Reset(IEnumerable<string> measureKeyNames)
        {
            bool hasX = false;
            bool hasY = false;
            
            _dict.Clear();

            foreach (string key in measureKeyNames)
            {
                _dict[key] = new Measurement(key);
                hasX |= key.StartsWith("X");
                hasY |= key.StartsWith("Y");
            }

            //IsSimpleQuad = _dict.Count == 2 && _dict.ContainsKey("X") && _dict.ContainsKey("Y");

            IsSimpleQuad = hasX && hasY;
        }
        public void UpdateMeasurement(string key, float value)
        {
            if (!_dict.TryGetValue(key, out var measurment))
                _dict[key] = measurment = new Measurement(key);
            measurment.Value = value;
        }
        public void UpdateMeasurement(string key, bool isPass)
        {
            if (!_dict.TryGetValue(key, out var measurment))
                _dict[key] = measurment = new Measurement(key);
            measurment.IsPass= isPass;
        }
        public void UpdateMeasurement(string key, params QVector[] camPts)
        {
            if (camPts == null || camPts.Length < 2)
                return;

            if (!_dict.TryGetValue(key, out var measurment))
                _dict[key] = measurment = new Measurement(key);

            // 直接複製前兩點 (允許元素為 null)
            measurment.CamMeasurePts[0] = camPts[0];
            measurment.CamMeasurePts[1] = camPts[1];
        }

        public bool IsAllPass()
        {
            //int passCount = 0;
            //if (PassNgResults != null)
            //{
            //    foreach (var pass in PassNgResults)
            //    {
            //        if (pass)
            //            passCount++;
            //    }
            //}
            //return passCount >= 2;

            foreach(var kvp in  _dict)
            {
                var measure = kvp.Value;
                if (measure == null) 
                    return false;
                if (!measure.IsPass)
                    return false;
            }

            return true;
        }

        #region 量測點相關函式
        public IEnumerable<QVector> IterMeasureCamPoints()
        {
            foreach(var kvp in _dict)
            {
                var measurement = kvp.Value;
                var camPts = measurement?.CamMeasurePts;
                if (camPts != null)
                {
                    foreach (var pt in camPts)
                        if (pt != null)
                            yield return pt;
                }
            }
        }
        public QVector[] GetMeasureCamPointsQuad()
        {
            if (_dict.TryGetValue("X", out Measurement measureX) && 
                _dict.TryGetValue("Y", out Measurement measureY))
            {
                var ptsH = measureX?.CamMeasurePts;
                var ptsV = measureY?.CamMeasurePts;
                if (ptsH != null && ptsV != null && ptsH.Length >= 2 && ptsV.Length >= 2)
                {
                    var pts = new QVector[]
                    {
                        ptsH?[0],
                        ptsV?[0],
                        ptsH?[1],
                        ptsV?[1],
                    };

                    //NOTE: 目前允許 pts[i] == null

                    //foreach (var pt in pts)
                    //{
                    //    if (pt == null)
                    //        return null;
                    //}

                    return pts;
                }
            }
            return null;
        }
        public void SetMeasureCamPointsQuad(QVector[] pts)
        {
            if (pts != null && pts.Length >= 4)
            {
                //NOTE: 目前允許 pts[i] == null

                if (!_dict.TryGetValue("X", out var measureX))
                    _dict["X"] = measureX = new Measurement("X");

                if (!_dict.TryGetValue("Y", out var measureY))
                    _dict["Y"] = measureY = new Measurement("Y");

                measureX.CamMeasurePts[0] = pts[0];     //LEFT
                measureY.CamMeasurePts[0] = pts[1];     //TOP
                measureX.CamMeasurePts[1] = pts[2];     //RIGHT
                measureY.CamMeasurePts[1] = pts[3];     //BOTTOM
            }
        }
        #endregion

        #region SIMPLE_QUAD_舊接口
        /// <summary>
        /// 量測结果: 晶粒尺寸X (單位 mm)
        /// </summary>
        public float ChipWidth
        {
            get
            {
                return _dict.TryGetValue("X", out var measure) ? measure.Value : 0f;
            }
            set
            {
                UpdateMeasurement("X", value);
            }
        }
        /// <summary>
        /// 量測结果: 晶粒尺寸Y (單位 mm)
        /// </summary>
        public float ChipHeight
        {
            get
            {
                return _dict.TryGetValue("Y", out var measure) ? measure.Value : 0f;
            }
            set
            {
                UpdateMeasurement("Y", value);
            }
        }
        /// <summary>
        /// 尺寸量測點 (左上右下) (單位 pixels) 
        /// (FullFov Cammera Coordinates)
        /// (顯示繪圖用)
        /// </summary>
        public QVector[] DimMeasurePoints
        {
            get => GetMeasureCamPointsQuad();
            //set => SetMeasureCamPointsQuad(value);
        }
        /// <summary>
        /// 取得晶粒 像素 長寬 (單位 pixels) (GUI 顯示用)
        /// </summary>
        public bool GetPixelSize(out double pixWidth, out double pixHeight)
        {
            pixWidth = 0;
            pixHeight = 0;

            //// 0:左, 1:上, 2:右, 3:下
            //var pts = DimMeasurePoints;
            //if (pts == null)
            //    return false;
            //if (pts[0] != null && pts[2] != null)
            //    pixWidth = Math.Round((pts[0] - pts[2]).NormLength, 1);
            //if (pts[1] != null && pts[3] != null)
            //    pixHeight = Math.Round((pts[1] - pts[3]).NormLength, 1);

            if (_dict.TryGetValue("X", out var measureX))
            {
                var pts = measureX.CamMeasurePts;
                if (pts?[0] != null && pts?[1] != null)
                    pixWidth = Math.Round((pts[0] - pts[1]).NormLength, 1);
            }

            if (_dict.TryGetValue("Y", out var measureY))
            {
                var pts = measureY.CamMeasurePts;
                if (pts?[0] != null && pts?[1] != null)
                    pixHeight = Math.Round((pts[0] - pts[1]).NormLength, 1);
            }

            return pixWidth > 0 && pixHeight > 0;
        }
        #endregion
    }
}
