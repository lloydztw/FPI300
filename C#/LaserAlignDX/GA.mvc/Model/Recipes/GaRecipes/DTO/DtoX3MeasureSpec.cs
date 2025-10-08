#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-20 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using System;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    /// <summary>
    /// 規格 (兩載台共用一份)
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    public class DtoX3MeasureSpec : DtoBase
    {
        #region SINGLETON
        static DtoX3MeasureSpec _instance;
        private DtoX3MeasureSpec()
        {
        }
        #endregion

        internal static DtoX3MeasureSpec Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DtoX3MeasureSpec();
                return _instance;
            }
        }

        // 啟用尺寸量測
        public bool optChipMeasurement = false;
        // 啟用 邊緣寬度 比對 (限有格點Pad的晶粒)
        public bool optChipEdgesCompare = false;
        // 啟用 瑕疵 與 QRCode 檢測
        public bool optChipDefectsInspect = false;
        // 尺寸與瑕疵檢測進階選項量: 使用整盤 NG 百分比 
        public bool optUseTotalNgPercentage = false;
        // 尺寸與瑕疵檢測進階選項量: 顯示個別 NG
        public bool optShowIndividualNG = false;
        // 尺寸與瑕疵檢測進階選項量: 整盤 NG 百分比
        public float TotalNgPercentage = 5.0f;

        // 尺寸宽度 spec (mm)
        public readonly DtoSpecValue StandardWidth = new DtoSpecValue(9.0f, 0.050f, 0.050f);
        public readonly DtoSpecValue StandardHeight = new DtoSpecValue(9.9f, 0.050f, 0.050f);

        // 尺寸偏移 spec (mm)
        public float PadEdgeDiffMaxX = 0.050f;
        public float PadEdgeDiffMaxY = 0.050f;


        public override void Load(string iniFileName)
        {
            normalizeFileName(ref iniFileName);

            string sectName = "Basic";

            Read(iniFileName, sectName, "optChipMeasurement", false, out optChipMeasurement);
            Read(iniFileName, sectName, "optChipEdgesCompare", false, out optChipEdgesCompare);
            Read(iniFileName, sectName, "optChipDefectsInspect", false, out optChipDefectsInspect);
            Read(iniFileName, sectName, "optUseTotalNgPercentage", false, out optUseTotalNgPercentage);
            Read(iniFileName, sectName, "optShowIndividualNG", true, out optShowIndividualNG);

            Read(iniFileName, sectName, "TotalNgPercentage", 5.0f, out TotalNgPercentage);

            StandardWidth.Load(iniFileName, sectName, "StandardWidth");
            StandardHeight.Load(iniFileName, sectName, "StandardHeight");
            Read(iniFileName, sectName, "PadEdgeDiffMaxX", 0.050f, out PadEdgeDiffMaxX);
            Read(iniFileName, sectName, "PadEdgeDiffMaxY", 0.050f, out PadEdgeDiffMaxY);
        }
        public override void Save(string iniFileName)
        {
            normalizeFileName(ref iniFileName);

            string sectName = "Basic";

            Write(iniFileName, sectName, "optChipMeasurement", optChipMeasurement);
            Write(iniFileName, sectName, "optChipEdgesCompare", optChipEdgesCompare);
            Write(iniFileName, sectName, "optChipDefectsInspect", optChipDefectsInspect);
            Write(iniFileName, sectName, "optUseTotalNgPercentage", optUseTotalNgPercentage);
            Write(iniFileName, sectName, "optShowIndividualNG", optShowIndividualNG);

            Write(iniFileName, sectName, "TotalNgPercentage", TotalNgPercentage);

            StandardWidth.Save(iniFileName, sectName, "StandardWidth");
            StandardHeight.Save(iniFileName, sectName, "StandardHeight");
            Write(iniFileName, sectName, "PadEdgeDiffMaxX", PadEdgeDiffMaxX);
            Write(iniFileName, sectName, "PadEdgeDiffMaxY", PadEdgeDiffMaxY);
        }

        void normalizeFileName(ref string iniFileName)
        {
            var path = System.IO.Path.GetDirectoryName(iniFileName);
            iniFileName = System.IO.Path.Combine(path, "Inspect_spec.ini");
        }
    }


    public class DtoSpecValue
    {
        public float Standard;
        public float DeltaUpper;
        public float DeltaLower;
        public float Max => Standard + DeltaUpper;
        public float Min => Standard - DeltaLower;

        public DtoSpecValue(float value, float deltaLower, float deltaUpper)
        {
            Standard = value;
            DeltaUpper = Math.Abs(deltaUpper);
            DeltaLower = Math.Abs(deltaLower);
        }
        public void Load(string iniFileName, string sectName, string keyName)
        {
            DtoBase.Read(iniFileName, sectName, keyName, Standard, out Standard);
            DtoBase.Read(iniFileName, sectName, keyName + "_delta_upper", DeltaUpper, out DeltaUpper);
            DtoBase.Read(iniFileName, sectName, keyName + "_delta_lower", DeltaLower, out DeltaLower);
        }
        public void Save(string iniFileName, string sectName, string keyName)
        {
            DtoBase.Write(iniFileName, sectName, keyName, Standard);
            DtoBase.Write(iniFileName, sectName, keyName + "_delta_upper", DeltaUpper);
            DtoBase.Write(iniFileName, sectName, keyName + "_delta_lower", DeltaLower);
        }
    }
}
