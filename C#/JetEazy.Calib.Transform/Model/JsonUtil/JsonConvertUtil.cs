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
using System;
using System.Globalization;


namespace JetEazy.Transform
{
    public class MatConverter : JsonConverter<Mat>
    {
        private readonly int _decimalPlaces;

        // 建構函式，允許指定小數點位數
        public MatConverter(int decimalPlaces = 6)
        {
            _decimalPlaces = decimalPlaces;
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        // 寫入 JSON
        public override void WriteJson(JsonWriter writer, Mat value, JsonSerializer serializer)
        {
            if (value == null || value.Empty())
            {
                writer.WriteNull();
                return;
            }

            // 將 Mat 數據轉換為 double 陣列
            double[] data = new double[value.Total() * value.Channels()];
            value.GetArray(out data);

            writer.WriteStartObject();
            writer.WritePropertyName("rows");
            writer.WriteValue(value.Rows);
            writer.WritePropertyName("cols");
            writer.WriteValue(value.Cols);
            writer.WritePropertyName("type");
            writer.WriteValue(value.Type().ToString());
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

            reader.Read(); // 讀取 'type'
            string typeStr = reader.ReadAsString();
            MatType matType = MatType.Parse(typeStr);

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

    public class QVectorConverter : JsonConverter<QVector>
    {
        private readonly int _decimalPlaces;

        // 建構函式，允許指定小數點位數
        public QVectorConverter(int decimalPlaces = 6)
        {
            _decimalPlaces = decimalPlaces;
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        // 寫入 JSON
        public override void WriteJson(JsonWriter writer, QVector value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            string format = $"F{_decimalPlaces}";

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.X.ToString(format, CultureInfo.InvariantCulture));
            writer.WritePropertyName("y");
            writer.WriteValue(value.Y.ToString(format, CultureInfo.InvariantCulture));
            writer.WritePropertyName("z");
            writer.WriteValue(value.Z.ToString(format, CultureInfo.InvariantCulture));
            writer.WriteEndObject();
        }

        // 讀取 JSON
        public override QVector ReadJson(JsonReader reader, Type objectType, QVector existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException("Expected StartObject token.");
            }

            double x = 0, y = 0, z = 0;
            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = reader.Value.ToString();
                    reader.Read(); // 讀取屬性值

                    switch (propertyName)
                    {
                        case "x":
                            x = Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture);
                            break;
                        case "y":
                            y = Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture);
                            break;
                        case "z":
                            z = Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture);
                            break;
                    }
                }
            }
            return new QVector(x, y, z);
        }
    }
}
