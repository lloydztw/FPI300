using JetEazy.QMath;
using Newtonsoft.Json; // 改用 Newtonsoft.Json
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace JetEazy.Match
{
    /// <summary>
    /// 用於將 EzBloc 序列化為 JSON 時的資料傳輸物件 (DTO)
    /// 目的在於排除不需要儲存的執行期物件 (如 Owner, Tag)，並將 Rectangle 簡化
    /// </summary>
    class EzBlocSaveModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Score { get; set; }
        public double SQRatio { get; set; }
        public uint Bin { get; set; }
        public int Pixels { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
    }

    public static class EzBlocsStorage
    {
        // 設定 Newtonsoft.Json 的序列化參數
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented, // 讓 JSON 檔案格式化，方便縮排閱讀
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// 將 List<EzBloc> 保存到指定路徑的檔案 (使用 Newtonsoft.Json)
        /// </summary>
        public static void SaveToFile(string filePath, IList<EzBloc> blocs)
        {
            if (blocs == null)
                throw new ArgumentNullException(nameof(blocs));

            // 確保目標資料夾存在
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 轉換為輕量化的儲存模型，過濾掉無法/不需序列化的 Owner 與 Tag 拓撲關係
            var saveList = new List<EzBlocSaveModel>(blocs.Count);
            foreach (var bloc in blocs)
            {
                if (bloc == null) continue;

                saveList.Add(new EzBlocSaveModel
                {
                    X = bloc.Rect.X,
                    Y = bloc.Rect.Y,
                    Width = bloc.Rect.Width,
                    Height = bloc.Rect.Height,
                    Score = bloc.Score,
                    SQRatio = bloc.SQRatio,
                    Bin = ((IxBlob)bloc).Bin,           // 擷取實作介面的屬性 (由 EzBloc2.cs 提供)
                    Pixels = ((IxBlob)bloc).Pixels,     // 擷取實作介面的屬性 (由 EzBloc2.cs 提供)
                    CenterX = bloc.Center.X,
                    CenterY = bloc.Center.Y
                });
            }

            // 使用 Newtonsoft 序列化並寫入檔案
            string jsonString = JsonConvert.SerializeObject(saveList, _jsonSettings);
            File.WriteAllText(filePath, jsonString, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// 從指定路徑的檔案載入 List<EzBloc> (使用 Newtonsoft.Json)
        /// </summary>
        public static List<EzBloc> LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("找不到指定的 EzBlocs 檔案。", filePath);

            string jsonString = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

            // 使用 Newtonsoft 反序列化
            var saveList = JsonConvert.DeserializeObject<List<EzBlocSaveModel>>(jsonString, _jsonSettings);
            if (saveList == null)
                return new List<EzBloc>();

            var blocs = new List<EzBloc>(saveList.Count);
            foreach (var model in saveList)
            {
                var bloc = new EzBloc(new Rectangle(model.X, model.Y, model.Width, model.Height), model.Score);
                bloc.Center = new QVector2(model.CenterX, model.CenterY);
                bloc.SQRatio = model.SQRatio;
                bloc.Pixels = model.Pixels;
                ((IxBlob)bloc).Bin = model.Bin;

                // 重新初始化 Center 向量 (EzBloc 內部的 getter 會根據新的 Rect 自動重新計算，不需手動代入)
                // Owner 與 Tag 預設為 null，後續交由 Builder 重新建立網格拓撲
                bloc.Owner = null;
                bloc.Tag = null;
                blocs.Add(bloc);
            }

            return blocs;
        }
    }
}
