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
using JetEazy.Lang;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Model;
using System;
using System.Drawing;
using System.Linq;
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
            btnSaveToRcp.Click += BtnOK_Click;
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
            QMSG.Translate(this);
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
            sb.Append(QMSG.T("格點")).Append(": [").Append(row).Append(",").Append(col).Append("]");

            #region 尺寸量測詳細點位
            bool isConditionOK = false;
            var chipDim = cell?.ChipData.ChipDimension;
            if (chipDim != null)
            {
                if (_xRecipe.LineBorderParams.IsSimpleQuad)
                {
                    var measureX = chipDim?["X"];
                    var measureY = chipDim?["Y"];
                    var meansurePts = new[]
                    {
                        measureX?.CamMeasurePts[0],     //  LEFT
                        measureY?.CamMeasurePts[0],     //  TOP
                        measureX?.CamMeasurePts[1],     //  RIGHT
                        measureY?.CamMeasurePts[1],     //  BOTTOM
                    };

                    if (meansurePts[0] != null && meansurePts[1] != null &&
                        meansurePts[2] != null && meansurePts[3] != null)
                    {
                        var dpX = (meansurePts[0] - meansurePts[2]).NormLength;
                        var dpY = (meansurePts[1] - meansurePts[3]).NormLength;
                        sb.AppendLine().Append(QMSG.T("晶粒.尺寸X")).Append($" = {dpX:0.0} pix");
                        sb.AppendLine().Append(QMSG.T("晶粒.尺寸Y")).Append($" = {dpY:0.0} pix");
                        isConditionOK = true;
                    }
                }
                else
                {
                    string tagName = QMSG.T("晶粒.尺寸X").Replace("X", "");
                    foreach (var key in chipDim.Keys)
                    {
                        var meansure = chipDim[key];
                        if (meansure == null) continue;
                        var pts = meansure.CamMeasurePts;
                        if (pts[0] != null && pts[1] != null)
                        {
                            var dist = (pts[0] - pts[1]).NormLength;
                            sb.AppendLine().Append(tagName).Append(key).Append($" = {dist:0.0} pix");
                            isConditionOK = true;
                        }
                    }
                }
            }
            if (!isConditionOK)
            {
                sb.AppendLine().AppendLine(QMSG.T("沒有 完整邊線, 無法建構 有效量測點位!"));
                numChipWidth.Enabled = false;
                numChipHeight.Enabled = false;
                btnSaveToRcp.Enabled = false;
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
            // Model
            var aoiModel = _sysModel?.AoiModel;
            if (aoiModel == null)
                return ErrorCodes.NO_AOI_MODEL;

            // Fullfov Bitmap
            var fullfovBmp = _sysModel?.LineScanImageHolder?.PeekBitmap();
            if (fullfovBmp == null)
                return ErrorCodes.NO_LINE_SCAN_IMAGE;

            // chipData
            var chipData = ActiveCell?.ChipData;
            if (chipData == null)
                return ErrorCodes.ERR_NO_CHIP_LOCATION;

            // Region Roi
            var regionRoi = Rectangle.Round(chipData.CellRoi);
            GaUtil.Clip(ref regionRoi, fullfovBmp.Size);
            if (regionRoi.Width < 2 || regionRoi.Height < 2)
                return ErrorCodes.ERR_NO_CHIP_LOCATION;

            // Region Bmp
            using (var regionBmp = fullfovBmp.Clone(regionRoi, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                var targetW = (float)numChipWidth.Value;
                var targetH = (float)numChipHeight.Value;
                var targetDim = new SizeF(targetW, targetH);

                // 實時 邊線框數據 (Fullfov Camera Coordinates) 
                var lineBorderPairs = chipData.LineBorderPairs;

                // 設定目標尺寸 
                lineBorderPairs.SetTargetDists(targetDim);

                // 是否 使用 簡單四邊線
                if (_xRecipe.LineBorderParams.IsSimpleQuad)
                {
                    // 使用 原有 微距轉換系統 的建構方式
                    var edgeLines4 = lineBorderPairs.GetQuadLineSegments();
                    var err = aoiModel.BuildMicroChipTransform(targetDim, edgeLines4, regionBmp, regionRoi, lineBorderPairs.IsLocal);
                    return err;
                }
                else
                {
                    // 使用 新的 微距轉換系統 的建構方式
                    var err = aoiModel.BuildMicroChipTransform(lineBorderPairs, regionBmp, regionRoi);
                    return err;
                }
            }
        }
    }
}
