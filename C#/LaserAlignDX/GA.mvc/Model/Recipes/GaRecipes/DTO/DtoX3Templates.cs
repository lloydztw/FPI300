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
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    public class DtoX3Templates : DtoBase, IDisposable
    {
        #region PRIVATE_DATA
        private CarrierEnum _carrierID;
        #endregion

        public DtoBmpTemplate GoldenRegionTemplate;
        public DtoBmpTemplate GoldenChipTemplate;
        public DtoBmpTemplate DefectMaskTemplate;
        public DtoBmpTemplate QrCodeTemplate;

        public DtoX3Templates(CarrierEnum carrierID)
        {
            _carrierID = carrierID;

            string tag = carrierID != CarrierEnum.C1 ? $"@{carrierID}" : "";
            string sectName = "Recipe Basic";

            GoldenRegionTemplate = new DtoBmpTemplate("bmpPrintTemplate" + tag, sectName, "xRectRegionPrint");
            GoldenChipTemplate = new DtoBmpTemplate("bmpDefectTemplate" + tag, sectName, "xRegionTrain");
            DefectMaskTemplate = new DtoBmpTemplate("bmpPrintMask" + tag, sectName, "xRectMask");
            QrCodeTemplate = new DtoBmpTemplate("bmpCode" + tag, sectName, "xRectCodeRegion");
        }
        public void Dispose()
        {
            GoldenRegionTemplate?.Dispose();
            GoldenRegionTemplate = null;
            GoldenChipTemplate?.Dispose();
            GoldenChipTemplate = null;
            DefectMaskTemplate?.Dispose();
            DefectMaskTemplate = null;
            QrCodeTemplate?.Dispose();
            QrCodeTemplate = null;
                
        }

        public override void Load(string iniFile, string sectName = null, string keyName = null)
        {
            if (_carrierID != CarrierEnum.C1)
                System.IO.Path.ChangeExtension(iniFile, $".{_carrierID}.ini");

            GoldenRegionTemplate.Load(iniFile); //, sectName, keyName);
            GoldenChipTemplate.Load(iniFile); //, sectName, keyName);
            DefectMaskTemplate.Load(iniFile); //, sectName, keyName);
            QrCodeTemplate.Load(iniFile); //, sectName, keyName);
        }
        public override void Save(string iniFile, string sectName = null, string keyName = null)
        {
            if (_carrierID != CarrierEnum.C1)
                System.IO.Path.ChangeExtension(iniFile, $".{_carrierID}.ini");

            GoldenRegionTemplate.Save(iniFile); //, sectName, keyName);
            GoldenChipTemplate.Save(iniFile); //, sectName, keyName);
            DefectMaskTemplate.Save(iniFile); //, sectName, keyName);
            QrCodeTemplate.Save(iniFile); //, sectName, keyName);
        }
    }
}
