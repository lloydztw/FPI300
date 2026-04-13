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

using AX.Gui;
using JetEazy.QMath;
using System;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public interface IvRecipeEditorUI
    {
        event EventHandler OnSelectedChanged;

        Control Window { get; }
        Control wndVisionSettingsPanel { get; }

        int SelectedIndex { get; set; }
        JezTransImageViewPanel ImgViewer { get; }

        RadioButton[] rdoCarriers { get; }
        Button btnLoadImage { get; }
        Button btnGrabImage { get; }
        Button btnSaveImage { get; }

        Button btnPickGoldenChipRegion { get; }             // 選取標準 晶粒區域 樣本
        Button btnAutoCreateCellRegions { get; }            // 自動抓取 Cell Regions (生成陣列)

        Button btnOpenTemplateMatchWindow { get; }
        Button btnOpenEmptyTrayWindow { get; }

        Button btnOpenFlyCamRcpWindow { get; }
        Button btnOpenLightCtrlWindow { get; }
        Button btnWriteCoordsToPlc { get; }

        GwMotorSimpleGoPanel wndFocusMotorGoPanel { get; }

        Button btnCancel { get; }
        Button btnOK { get; }

        void UpdateCoordsRef(QVector camPt, QVector motorPtSucker1, QVector motorPtSucker2);
    }
}
