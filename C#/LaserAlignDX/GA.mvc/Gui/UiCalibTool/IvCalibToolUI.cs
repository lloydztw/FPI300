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

using System;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public interface IvCalibToolUI
    {
        event EventHandler OnActiveViewChanged;

        Control Window { get; }
        RadioButton[] rdoCarriers { get; }
        RadioButton[] rdoSuckerRows { get; }

        int ActiveViewID { get; }
        JezTransImageViewPanel[] ImgViewers { get; }

        GvCalibPointsDataGridView dgvCalibPointsListView { get; }
        Control wndVisionSettingsPanel { get; }

        Button btnGrabImage { get; }
        Button btnLoadImage { get; }
        Button btnPickupGolden { get; }
        Button btnRunAutoFetch { get; }
        Button btnBuildCalib { get; }

        Button btnAutoFetchInkPts { get; }
        Button btnBuildCalibInkAdj { get; }

        Button btnCancel { get; }
        Button btnOK { get; }
    }
}
