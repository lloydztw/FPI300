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
using Newtonsoft.Json;


namespace JetEazy.Transform
{
    partial class QTransform
    {
        const int DECIMAL_PLACES = 12;

        public void SaveJson(string fileName)
        {
            #region 檢查檔案
            //>>> fileName = System.IO.Path.ChangeExtension(fileName, ".json");
            System.Diagnostics.Debug.Assert(System.IO.Path.GetExtension(fileName).ToLower() == ".json");
            #endregion

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new QTransform_JsonConverter(DECIMAL_PLACES), 
                               new Mat_JsonConverter(DECIMAL_PLACES), 
                               new QVector_JsonConverter(DECIMAL_PLACES) }
            };

            string json = JsonConvert.SerializeObject(this, settings); // 直接序列化 this
            System.IO.File.WriteAllText(fileName, json);
        }

        public void LoadJson(string fileName)
        {
            #region 檢查檔案
            //>>> fileName = System.IO.Path.ChangeExtension(fileName, ".json");
            if (!System.IO.File.Exists(fileName))
            {
                //throw new System.IO.FileNotFoundException("File not found.", fileName);
                return;
            }
            System.Diagnostics.Debug.Assert(System.IO.Path.GetExtension(fileName).ToLower() == ".json");
            #endregion

            string json = System.IO.File.ReadAllText(fileName);

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new QTransform_JsonConverter(DECIMAL_PLACES),
                               new Mat_JsonConverter(DECIMAL_PLACES),
                               new QVector_JsonConverter(DECIMAL_PLACES) }
            };

            // 直接將資料填入當前物件 (Populate)
            JsonConvert.PopulateObject(json, this, settings);
        }
    }
}
