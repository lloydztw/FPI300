using AwFramework;
using EzAoiChipLocQC.Ctrl;
using EzAoiChipLocQC.Machine;
using EzAoiChipLocQC.Model;
using EzCamera.Manager;

namespace EzAoiChipLocQC
{
    public class Global
    {
        public const string TITLE = "ATM20 ChipLoc QC";

        public const bool IsSim = true;

        public static readonly AppPath APP_PATH = new AppPath(@"D:\AUTOMATION\Eazy FPI30\Aoi.ATM20");

        public static JxAppSettings AppSettings
        {
            get => EzAppForDll.Instance.appSettings as JxAppSettings;
        }

        public static IxChipLocator AoiModel
        {
            get => AoiEmptyTrayInspector.Instance;
        }

        public static ITravellerQcMachine Machine
        {
            get => EzAoiChipLocQC.Drivers.Factory.InstanceMachine();
        }

        public static void Dispose()
        {
            EzRcpContraintCtrl.Instance.CleanGarbages();
            var model = AoiModel;
            model?.Dispose();

            // 強制關掉所有相機
            AppCamerasManager.DisposeAll();
        }
    }
}
