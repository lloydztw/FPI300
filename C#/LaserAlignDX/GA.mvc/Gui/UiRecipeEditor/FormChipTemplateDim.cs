#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-10-08 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.FormSpace;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Model;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;
using XCell = LaserAlignDX.OPSpace.RegionCellX3Class;
using XRecipe = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;


namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormChipTemplateDim : Form
    {
        #region GLOBAL_MESS
        XRecipe _xRecipe => XRecipe.Instance;
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        #endregion

        public FormChipTemplateDim()
        {
            InitializeComponent();

            DialogResult = DialogResult.Cancel;
            btnOK.Click += BtnOK_Click;
            Load += Form_Load;
        }

        public XCell ActiveCell
        {
            get; set;
        }

        #region EVENT_HANDLERS
        private void Form_Load(object sender, EventArgs e)
        {
            updateInfo();
            updateSettings(false);
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            var err = BuildMicroTransform();
            if (err != ErrorCodes.OK)
            {
                VsMessageBox.Warning(GaUtil.GetEnumDescription(err));
                return;
            }
            updateSettings(true);

            this.DialogResult = DialogResult.OK;
            Close();
        }
        #endregion

        void updateInfo()
        {
            var cell = ActiveCell;
            if (cell == null)
            {
                lblInfo.Text = "";
                return;
            }
            var sb = new StringBuilder();
            int row = cell.CellRow;
            int col = cell.CellCol;
            sb.Append("格點: [").Append(row).Append(",").Append(col).Append("]");

            #region 尺寸量測詳細點位
            var meansurePts = cell?.ChipData.ChipDimension.DimMeasurePoints;
            if (meansurePts != null && meansurePts.Length >= 4 &&
                meansurePts[0] != null && meansurePts[1] != null &&
                meansurePts[2] != null && meansurePts[3] != null)
            {
                var dpX = (meansurePts[0] - meansurePts[2]).NormLength;
                var dpY = (meansurePts[1] - meansurePts[3]).NormLength;
                sb.AppendLine().Append($"晶粒.尺寸X = {dpX:0.0} pix");
                sb.AppendLine().Append($"晶粒.尺寸Y = {dpY:0.0} pix");
            }
            else
            {
                sb.AppendLine().AppendLine("沒有 完整邊線, 無法建構 有效量測點位!");
                numChipWidth.Enabled = false;
                numChipHeight.Enabled = false;
                btnOK.Enabled = false;
            }
            #endregion

            #region 晶粒_PADS
            var padsGrid = cell?.ChipData?.PadsGrid;
            if (padsGrid != null)
            {
                var p0 = padsGrid.Get(0, 0)?.Center;
                var p1 = padsGrid.Get(0, padsGrid.Cols - 1)?.Center;
                var p2 = padsGrid.Get(padsGrid.Rows - 1, 0)?.Center;
                if (p0 != null && p1 != null && p2 != null)
                {
                    var pW = (p0 - p1).NormLength;
                    var pH = (p0 - p2).NormLength;
                    sb.AppendLine().Append($"PAD.尺寸X = {pW:0.0} pix");
                    sb.AppendLine().Append($"PAD.尺寸Y = {pH:0.0} pix");
                }
            }
            #endregion

            lblInfo.Text = sb.ToString();
        }

        void updateSettings(bool toModel)
        {
            var settings = _xRecipe.InspectParams;
            if (settings == null)
                return;

            if (toModel)
            {
                var cW = (float)numChipWidth.Value;
                var cH = (float)numChipHeight.Value;
                if (cW != settings.xTemplateChipWidth || cH != settings.xTemplateChipHeight)
                {
                    settings.xTemplateChipWidth = cW;
                    settings.xTemplateChipHeight = cH;
                    _xRecipe.Save();
                }
            }
            else
            {
                GaUtil.SetNum(numChipWidth, (decimal)settings.xTemplateChipWidth);
                GaUtil.SetNum(numChipHeight, (decimal)settings.xTemplateChipHeight);
            }
        }

        ErrorCodes BuildMicroTransform()
        {
            var aoiModel = _sysModel?.AoiModel;
            if (aoiModel == null)
                return ErrorCodes.NO_AOI_MODEL;

            var fullfovBmp = _sysModel?.LineScanImageHolder?.PeekBitmap();
            if (fullfovBmp == null)
                return ErrorCodes.NO_LINE_SCAN_IMAGE;

            var chipData = ActiveCell?.ChipData;
            if (chipData == null)
                return ErrorCodes.ERR_NO_CHIP_LOCATION;

            var goldenW = (float)numChipWidth.Value;
            var goldenH = (float)numChipHeight.Value;
            var goldenDim = new SizeF(goldenW, goldenH);

            // Region Bitmap
            var regionRoi = Rectangle.Round(chipData.CellRoi);
            GaUtil.Clip(ref regionRoi, fullfovBmp.Size);
            if (regionRoi.Width < 2 || regionRoi.Height < 2)
                return ErrorCodes.ERR_NO_CHIP_LOCATION;

            using (var regionBmp = fullfovBmp.Clone(Rectangle.Round(regionRoi), System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                var err = aoiModel.BuildMicroChipTransform(goldenDim, chipData.LineSegments, regionBmp, regionRoi);
                return err;
            }
        }
    }
}
