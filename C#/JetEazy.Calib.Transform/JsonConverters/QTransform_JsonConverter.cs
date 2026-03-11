using JetEazy.QMath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using QTransform = JetEazy.Transform.QTransform;


namespace JetEazy.JsonConverters
{
    public class QTransform_JsonConverter : JsonConverter<QTransform>
    {
        private readonly int _decimalPlaces;

        public QTransform_JsonConverter(int decimalPlaces = 6)
        {
            _decimalPlaces = decimalPlaces;
        }

        public override void WriteJson(JsonWriter writer, QTransform value, JsonSerializer serializer)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteStartObject();
            writer.WritePropertyName("Name"); writer.WriteValue(value.Name);

            // 序列化矩陣與點位
            writer.WritePropertyName("mat"); serializer.Serialize(writer, value._mat);
            writer.WritePropertyName("matInv"); serializer.Serialize(writer, value._matInv);
            writer.WritePropertyName("srcPoints"); serializer.Serialize(writer, value._srcPoints);
            writer.WritePropertyName("dstPoints"); serializer.Serialize(writer, value._dstPoints);
            writer.WriteEndObject();
        }

        public override QTransform ReadJson(JsonReader reader, Type objectType, QTransform existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var jsonObject = JObject.Load(reader);

            // 1. 先解析出所有數據
            string name = jsonObject["Name"]?.Value<string>();
            // 1.1 使用 Mat_JsonConverter 進行反序列化
            Mat mat = jsonObject["mat"]?.ToObject<Mat>(serializer);
            Mat matInv = jsonObject["matInv"]?.ToObject<Mat>(serializer);
            // 1.2 使用 QVector_JsonConverter 進行反序列化
            QVector[,] srcPoints = jsonObject["srcPoints"]?.ToObject<QVector[,]>(serializer);
            QVector[,] dstPoints = jsonObject["dstPoints"]?.ToObject<QVector[,]>(serializer);

            // 2. 驗證數據品質 (事務性檢查)
            try
            {
                if (srcPoints == null || dstPoints == null)
                    throw new Exception("null points");

                if (mat == null || matInv == null)
                    throw new Exception("null mat"); 

                // 這裡可以呼叫原本 QTransform 的靜態驗證邏輯
                // 確保載入的資料是可用的
            }
            catch (Exception)
            {
                // 釋放剛產生的非託管資源防止洩漏
                mat?.Dispose();
                matInv?.Dispose();
                return existingValue; // 載入失敗時保留原狀
            }

            // 3. 建立或更新物件
            var target = existingValue ?? new QTransform(name);

            // 釋放舊資源
            target._mat?.Dispose();
            target._matInv?.Dispose();

            target.Name = name;
            target._mat = mat;
            target._matInv = matInv;
            target._srcPoints = srcPoints;
            target._dstPoints = dstPoints;

            return target;
        }
    }
}
