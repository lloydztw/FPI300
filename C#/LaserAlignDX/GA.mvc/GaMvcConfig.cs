#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System.Drawing;
using System.Windows.Forms;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Model;
using GaMainCtrl = LaserAlignDX.Mvc.Ctrl.Abs.GaMainCtrl;

namespace LaserAlignDX
{
    /// <summary>
    /// Compatibility facade. AppBootstrapper owns shared services;
    /// AppDialogService owns modal editor creation.
    /// </summary>
    public static class GaMvcConfig
    {
        public static int TOTAL_FLY_FRAMES_COUNT => 4;
        public static ITravelerModel SysModel => AppBootstrapper.Instance.SystemModel;
        public static IProcessRunFPI InstanceAoiModel() => AppBootstrapper.Instance.Aoi;
        public static ICalibAoiModel CreateCalibAoiModel() => AppBootstrapper.Instance.CreateCalibAoiModel();
        public static IxReportBuilder CreateReportBuilder() => AppBootstrapper.Instance.CreateReportBuilder();
        public static GaMainCtrl InstanceMainCtrl() => AppBootstrapper.Instance.MainController;

        public static void OpenRecipeEditor() => AppBootstrapper.Instance.Dialogs.OpenRecipeEditor();
        public static void OpenTamplateEditor(CarrierEnum carrier) => AppBootstrapper.Instance.Dialogs.OpenTamplateEditor(carrier);
        public static void OpenCalibrationTool() => AppBootstrapper.Instance.Dialogs.OpenCalibrationTool();
        public static void OpenEmptyTrayInspectTool(Form owner, string recipeName = null, Bitmap bmpToShow = null)
            => AppBootstrapper.Instance.Dialogs.OpenEmptyTrayInspectTool(owner, recipeName, bmpToShow);
        public static void PushBitmapToEmptyTrayTool(Bitmap bmp, string name)
            => AppBootstrapper.Instance.Dialogs.PushBitmapToEmptyTrayTool(bmp, name);

        public static void DisposeAll() => AppBootstrapper.Shutdown();
    }
}
