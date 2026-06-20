namespace EzCamera.Driver.Hikvision.Huang
{
    internal class Universal
    {
        /// <summary>
        /// Universal.operMVCameraClass 依賴於 OperMVCameraClass
        /// OperMVCameraClass 卻又反身 依賴於 Univeral.CameraLock
        /// 此為不良設計 !!!
        /// </summary>
        public static OperMVCameraClass operMVCameraClass;

        public static object[] CameraLock = {
            new object(),
            new object(),
            new object(),
            new object(),
            new object(),
            new object()
        };
    }
}
