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
        PropertyGrid pgParamsView { get; }

        Button btnLoadImage { get; }
        Button btnGrabImage { get; }
        Button btnSaveImage { get; }

        Button btnPickGoldenRegion { get; }
        Button btnCreateCellRegions { get; }

        Button btnOpenTemplateMatchWindow { get; }
        Button btnOpenEmptyTrayWindow { get; }
        Button btnOpenFlyCamRcpWindow { get; }
        Button btnOpenLightCtrlWindow { get; }

        Button btnCancel { get; }
        Button btnOK { get; }
    }
}
