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

using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Globalization;


namespace JetEazy.JsonConverters
{
    public class Mat_JsonConverter : JsonConverter<Mat>
    {
        private readonly int _decimalPlaces;

        // 建構函式，允許指定小數點位數
        public Mat_JsonConverter(int decimalPlaces = 6)
        {
            _decimalPlaces = decimalPlaces;
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        // 寫入 JSON
        public override void WriteJson(JsonWriter writer, Mat mat, JsonSerializer serializer)
        {
            if (mat == null || mat.Empty())
            {
                writer.WriteNull();
                return;
            }

            // 將 Mat 數據轉換為 double 陣列
            double[] data = new double[mat.Total() * mat.Channels()];
            mat.GetArray(out data);

            writer.WriteStartObject();
            writer.WritePropertyName("rows");
            writer.WriteValue(mat.Rows);
            writer.WritePropertyName("cols");
            writer.WriteValue(mat.Cols);
            writer.WritePropertyName("MatTypeValue");
            writer.WriteValue(mat.Type().ToInt32());
            writer.WritePropertyName("data");
            writer.WriteStartArray();

            // 格式化數據並寫入，指定小數點位數
            string format = $"F{_decimalPlaces}";
            foreach (var val in data)
            {
                writer.WriteValue(val.ToString(format, CultureInfo.InvariantCulture));
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        // 讀取 JSON
        public override Mat ReadJson(JsonReader reader, Type objectType, Mat existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            // 讀取 JSON 物件
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException("Expected StartObject token.");
            }

            reader.Read(); // 讀取 'rows'
            int rows = (int)reader.ReadAsInt32();

            reader.Read(); // 讀取 'cols'
            int cols = (int)reader.ReadAsInt32();

            reader.Read(); // 讀取 'MatTypeValue'
            int typeValue = (int)reader.ReadAsInt32();
            MatType matType = MatType.FromInt32(typeValue);

            reader.Read(); // 讀取 'data'
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonSerializationException("Expected StartArray token for data.");
            }

            // 讀取數據陣列
            double[] data = serializer.Deserialize<double[]>(reader);

            // 將數據轉換回 Mat 物件
            Mat mat = new Mat(rows, cols, matType);
            mat.SetArray(data);
            return mat;
        }
    }
}
