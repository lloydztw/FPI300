using AwFramework;
using EzAoiEmptyTrayInspector.Model;

namespace EzAoiEmptyTrayInspector
{
    internal class Global
    {
        public const string TITLE = "FPI30 AOI 空盤檢測";

        public static AppPath APP_PATH = new AppPath(@"D:\AUTOMATION\Eazy FPI30\Aoi");

        public static JxAppSettings AppSettings
        {
            get => EzAppForDll.Instance.appSettings as JxAppSettings;
        }

        public static IxEmptyTrayInspector AoiModel
        {
            get => AoiEmptyTrayInspector.Instance;
        }

        public static void Dispose()
        {
            var model = AoiModel;
            model?.Dispose();
        }
    }
}
