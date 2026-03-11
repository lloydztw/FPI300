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
using System;
using System.Globalization;


namespace JetEazy.JsonConverters
{
    public class QVector_JsonConverter : JsonConverter<QVector>
    {
        private readonly int _decimalPlaces;

        // 建構函式，允許指定小數點位數
        public QVector_JsonConverter(int decimalPlaces = 6)
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
