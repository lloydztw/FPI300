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

using JetEazy.QMath;
using Newtonsoft.Json;
using OpenCvSharp;


namespace JetEazy.Transform
{
    partial class QTransform
    {
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
            fileName = System.IO.Path.ChangeExtension(fileName, ".json");

            int decimalPlaces = 6;
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new MatConverter(decimalPlaces), new QVectorConverter(decimalPlaces) }
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
            fileName = System.IO.Path.ChangeExtension(fileName, ".json");
            if (!System.IO.File.Exists(fileName))
            {
                //throw new System.IO.FileNotFoundException("File not found.", fileName);
                return;
            }

            string json = System.IO.File.ReadAllText(fileName);

            int decimalPlaces = 6;
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new MatConverter(decimalPlaces), new QVectorConverter(decimalPlaces) }
            };

            // 將整個 JSON 字串反序列化為數據物件
            var jsData = JsonConvert.DeserializeObject<JsData>(json, settings);
            _mat = jsData.mat;
            _matInv = jsData.matInv;
            _srcPoints = jsData.srcPoints;
            _dstPoints = jsData.dstPoints;
        }
    }
}
