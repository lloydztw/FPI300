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
    {
        Control Window { get; }
        DispUI[] DispViewers { get; }

        Control lblActiveCarrierID { get; }
        RadioButton[] rdoBoxSelectors { get; }

        Button btnPickGolden {  get; }
        Button btnAutoLineBorders { get; }
        NumericUpDown numBorderIndent { get; }
        NumericUpDown numBorderSize { get; }

        Button btnTryScanQrCode { get; }
        RichTextBox rtbQrCodeResult { get; }

        Control wndVisionSettingsPanel { get; }
        Button btnDefectRegionAdd { get; }  
        Button btnDefectRegionDelete {  get; }  
        Button btnDefectRegionClearAll { get; }

        Button btnCreateTemplate { get; }
        Button btnSaveTemplateAndParams { get; }
    }
}
