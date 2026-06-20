using AwFramework;
using EzAoiChipLocQC.Ctrl;
using EzAoiChipLocQC.Machine;
using EzAoiChipLocQC.Model;

namespace EzAoiChipLocQC
{
    public class Global
    {
        public const string TITLE = "ATM20 ChipLoc QC";

        public const bool IsSim = true;

        public static AppPath APP_PATH = new AppPath(@"D:\AUTOMATION\Eazy FPI30\Aoi.ATM20");

        public static readonly JxAppSettings AppSettings = new JxAppSettings();

        public static IxChipLocator AoiModel
        {
            get => AoiEmptyTrayInspector.Instance;
        }

        public static ITravellerQcMachine Machine
        {
            get => EzAoiChipLocQC.Drivers.DevFactory.InstanceMachine();
        }

        public static void Dispose()
        {
            if (AppSettings.Modified)
                AppSettings.Save(null);

            EzRcpContraintCtrl.Instance.CleanGarbages();
            var model = AoiModel;
            model?.Dispose();
        }
    }
}
