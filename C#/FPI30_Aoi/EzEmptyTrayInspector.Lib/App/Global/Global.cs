using AwFramework;
using EzAoiEmptyTrayInspector.Model;
using EzAoiEmptyTrayInspector;

namespace EzAoiEmptyTrayInspector
{
    public class Global
    {
        public const string TITLE = "FPI30 AOI 空盤檢測";

        public static AppPath APP_PATH = new AppPath(@"D:\AUTOMATION\Eazy FPI30");

        public static JxAppSettings AppSettings
        {
            get => EzApp.Instance.appSettings as JxAppSettings;
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
