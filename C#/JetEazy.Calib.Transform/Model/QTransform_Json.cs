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

using JetEazy.JsonConverters;
using JetEazy.QMath;
using Newtonsoft.Json;
using OpenCvSharp;


namespace JetEazy.Transform
{
    partial class QTransform
    {
        const int DECIMAL_PLACES = 12;

        #region JSON_DATA
        class JsData
        {
            public Mat mat;
            public Mat matInv;
            public QVector[,] srcPoints;
            public QVector[,] dstPoints;
        }
        #endregion

        public void SaveJson(string fileName)
        {
            #region 檢查檔案
            fileName = System.IO.Path.ChangeExtension(fileName, ".json");
            #endregion

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new Mat_JsonConverter(DECIMAL_PLACES), new QVector_JsonConverter(DECIMAL_PLACES) }
            };

            // 將整個數據物件序列化
            var data = new JsData
            {
                mat = _mat,
                matInv = _matInv,
                srcPoints = _srcPoints,
                dstPoints = _dstPoints,
            };

            string json = JsonConvert.SerializeObject(data, settings);
            System.IO.File.WriteAllText(fileName, json);
        }
        public void LoadJson(string fileName)
        {
            #region 檢查檔案
            fileName = System.IO.Path.ChangeExtension(fileName, ".json");
            if (!System.IO.File.Exists(fileName))
            {
                //throw new System.IO.FileNotFoundException("File not found.", fileName);
                return;
            }
            #endregion

            string json = System.IO.File.ReadAllText(fileName);

            int decimalPlaces = 6;
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new Mat_JsonConverter(decimalPlaces), new QVector_JsonConverter(decimalPlaces) }
            };

            bool ok = false;
            var oldMat = _mat;
            var oldMatInv = _matInv;
            var oldSrcPoints = _srcPoints;
            var oldDstPoints = _dstPoints;

            // 將整個 JSON 字串反序列化為數據物件
            var jsData = JsonConvert.DeserializeObject<JsData>(json, settings);

            try
            {
                _mat = jsData?.mat;
                _matInv = jsData?.matInv;
                _srcPoints = jsData?.srcPoints;
                _dstPoints = jsData?.dstPoints;

                if (_srcPoints == null || _dstPoints == null)
                    throw new System.Exception("null points");

                if (_mat == null || _matInv == null)
                    throw new System.Exception("null mats");

                checkPointsCondition(_srcPoints, _dstPoints);
                ok = CheckBuildCondition(out _, out _);
            }
            catch
            {
                ok = false;
            }

            if (ok)
            {
                oldMat?.Dispose();
                oldMatInv?.Dispose();
            }
            else
            {
                _mat?.Dispose();
                _mat = oldMat;
                _matInv?.Dispose();
                _matInv = oldMatInv;

                _srcPoints = oldSrcPoints;
                _dstPoints = oldDstPoints;
            }
        }
    }
}
