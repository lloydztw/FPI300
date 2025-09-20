using Eazy_Project_III;
using JetEazy;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    internal class DtoX3Templates : DtoBase, IDisposable
    {
        public DtoBmpTemplate _goldenRegionTemplate =  new DtoBmpTemplate("bmpPrintTemplate");
        public DtoBmpTemplate _goldenChipTemplate = new DtoBmpTemplate("bmpDefectTemplate");
        public DtoBmpTemplate _qrCodeTemplate = new DtoBmpTemplate("bmpCode");

        public DtoX3Templates(CarrierEnum carrierID)
        {
            string tag = carrierID == CarrierEnum.C1 ? "" : $"@{carrierID}";
            _goldenRegionTemplate = new DtoBmpTemplate("bmpPrintTemplate" + tag);
            _goldenChipTemplate = new DtoBmpTemplate("bmpDefectTemplate" + tag);
            _qrCodeTemplate = new DtoBmpTemplate("bmpCode" + tag);
        }

        public void Dispose()
        {
            _goldenRegionTemplate?.Dispose();
            _goldenRegionTemplate = null;
            _goldenChipTemplate?.Dispose();
            _goldenChipTemplate = null;
            _qrCodeTemplate?.Dispose();
            _qrCodeTemplate = null;
                
        }
        public override void Load(string iniFile, string sectNam = null, string keyName = null)
        {
        }
        public override void Save(string iniFile, string sectNam = null, string keyName = null)
        {
            
        }
    }
}
