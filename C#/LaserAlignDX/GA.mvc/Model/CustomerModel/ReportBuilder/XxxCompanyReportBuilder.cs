#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Text;
using Traveller106;

using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;
using RECIPE = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;


namespace LaserAlignDX.Model
{
    /// <summary>
    /// 其他 廠家 報表範本
    /// </summary>
    public class XxxCompanyReportBuilder : IxReportBuilder
    {
        #region CONFIG
        const string _digitFormat = "0.000";
        #endregion

        #region PRIVATE_DATA
        /// <summary>
        /// RegionCellX3Class 太雜亂了 !!!
        /// </summary>
        RECIPE _xRecipe => RECIPE.Instance;
        string _resultImagePath => INI.Instance.ResultImagePath;
        string _stripId;
        string _fileName;
        #endregion

        public string GenerateReport(string stripId, string fileName, bool optOutFinalText)
        {
            var reportSB = new StringBuilder();

            appendHeader(reportSB);

            //>>> foreach (CELL cell in _xRecipe.xRegionCells)
            foreach (var cell in PlcDataPacker.IterFinalResultCells())
            {
                appendOneCellData(reportSB, cell);
            }

            saveReportData(reportSB);

            if (optOutFinalText)
                reportSB.ToString();

            return null;
        }

        #region PRIVATE_FUNCTIONS
        void appendHeader(StringBuilder reportSB)
        {
            throw new NotImplementedException();
        }
        void appendOneCellData(StringBuilder reportSB, CELL cell)
        {
            throw new NotImplementedException();
        }
        void saveReportData(StringBuilder reportSB)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}