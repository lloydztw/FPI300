using AwFramework;
using EzAoiChipLocQC.Ctrl;
using EzAoiChipLocQC.Model;

namespace EzAoiChipLocQC
{
    public class Global
    {
        public const string TITLE = "ATM20 ChipLoc QC";

        public static AppPath APP_PATH = new AppPath(@"D:\AUTOMATION\Eazy FPI30\Aoi.ATM20");

        public static readonly JxAppSettings AppSettings = new JxAppSettings();
        //{
        //    get => EzAppForDll.Instance.appSettings as JxAppSettings;
        //}

        public static IxChipLocator AoiModel
        {
            get => AoiEmptyTrayInspector.Instance;
        }

        public static void Dispose()
        {
            EzRcpContraintCtrl.Instance.CleanGarbages();
            var model = AoiModel;
            model?.Dispose();
        }
    }
}
