#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-19 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;
using DispUI = JzDisplay.UISpace.DispUI;

namespace LaserAlignDX.Mvc.Gui
{
    public interface IvTemplateEditorUI 
                    : IvTemplLinebordersEditorUI 
                    , IvTemplDefectsEditorUI 
                    , IvTemplBadConnsEditorUI
                    , IvTemplQrCodeEditorUI
    {
        Control Window { get; }
        
        //DispUI[] DispViewers { get; }   // 準備廢除, 全面改用 ImvViewers
        Control[] ImgViewers { get; }
        
        Control wndVisionSettingsPanel { get; }

        Control lblActiveCarrierID { get; }

        RadioButton[] rdoBoxSelectors { get; }

        /// <summary>
        /// 轉正模板
        /// </summary>
        Button btnRotateGolden { get; }

        /// <summary>
        /// 擷取模板
        /// </summary>
        Button btnPickGolden { get; }

        Button btnTrainTemplate { get; }

        Button btnSaveAllParams { get; }

        Button btnCancel { get; }
    }


    public interface IvTemplLinebordersEditorUI
    {
        /// <summary>
        /// 尺寸X 量測數量
        /// </summary>
        NumericUpDown numMeasureDistXs { get; }
        /// <summary>
        /// 尺寸Y 量測數量
        /// </summary>
        NumericUpDown numMeasureDistYs { get; }
        /// <summary>
        /// 遮罩數量
        /// </summary>
        NumericUpDown numMeasureMasks { get; }
        /// <summary>
        /// 內緣
        /// </summary>
        NumericUpDown numBorderIndent { get; }
        /// <summary>
        /// 外緣
        /// </summary>
        NumericUpDown numBorderExtend { get; }
        /// <summary>
        /// 跨距
        /// </summary>
        NumericUpDown numLineSpanPercentage { get; }

        /// <summary>
        /// 顯示 邊線前置濾波 效果
        /// </summary>
        CheckBox chkShowFilterResult { get; }
        /// <summary>
        /// 邊線前置濾波: 灰階上限
        /// </summary>
        NumericUpDown numGrayLimitHi { get; }
        /// <summary>
        /// 邊線前置濾波: 灰階下限
        /// </summary>
        NumericUpDown numGrayLimitLo { get; }

        /// <summary>
        /// 一鍵自動框
        /// </summary>
        Button btnAutoLineBorders { get; }
        /// <summary>
        /// 精算尺寸
        /// </summary>
        Button btnBuildMircoTransform { get; }
    }


    public interface IvTemplDefectsEditorUI
    {
        Button btnDefectRegionAdd { get; }
        Button btnDefectRegionDelete { get; }
        Button btnDefectRegionClearAll { get; }
    }


    public interface IvTemplBadConnsEditorUI
    {
        Button btnAddRegion { get; }
        Button btnDeleteRegion { get; }
        Button btnClearAllRegions { get; }
    }


    public interface IvTemplQrCodeEditorUI
    {
        Button btnTryScanQrCode { get; }
        Control wndQrCodeResult { get; }
    }
}
