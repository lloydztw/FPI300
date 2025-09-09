#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-06 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public interface IvRecipeEditorUI
    {
        Control Window { get; }
        JezTransImageViewPanel ImgViewer { get; }
        //PropertyGrid pgParamsView { get; }
        Control wndVisionSettingsPanel { get; }

        Button btnLoadImage { get; }
        Button btnGrabImage { get; }
        Button btnSaveImage { get; }

        Button btnPickGoldenEmptyRegion { get; }        // 選取標準 空位 樣本
        Button btnRunEmptyTrayInspect { get; }          // 空盤檢查

        Button btnPickGoldenChipRegion { get; }         // 選取標準 晶粒 樣本
        Button btnCreateCellRegions { get; }            // 自動抓取 Cell Regions

        Button btnOpenTemplateMatchWindow { get; }
        Button btnOpenEmptyTrayWindow { get; }
        Button btnOpenFlyCamRcpWindow { get; }
        Button btnOpenLightCtrlWindow { get; }

        Button btnCancel { get; }
        Button btnOK { get; }
    }
}
