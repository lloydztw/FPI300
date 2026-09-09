
#region REVISION
// 
//  2025-08-01 初創 LeTian Chang
//
#endregion

using JetEazy.CCDSpace.CamLinkDriver;
using JetEazy.Interface;


namespace JetEazy.CCDSpace
{
    public class GaCameraFactory
    {
        public static IxLineScanCam LoadLineScanCamera(CameraPara param)
        {
            if (param == null)
                return null;
            return LoadLineScanCamera(param.CameraType, param.IsDebug, param.ToCameraString());
        }

        public static IxLineScanCam LoadLineScanCamera(string cameraType, bool isDebug, string paramStr, bool autoOpen = false)
        {
            IxLineScanCam camera = null;

            if (string.IsNullOrEmpty(cameraType))
                cameraType = cameraType.ToUpper();

            switch (cameraType)
            {
                case "HUARUI":
                    camera = new LINESCAN_HUARUI();
                    break;
                case "DVP2":
                    if (isDebug)
                        camera = new Linescan_Sim();
                    else
                        camera = new Linescan_Dvp2();
                    break;
                case "ITK":
                    camera = new Linescan_iTK();
                    break;
                case "MIND":
                    camera = new Linescan_Mind();
                    break;
                default:
                    camera = new Linescan_Sim();
                    break;
            }

            camera?.Init(isDebug, paramStr);

            if (autoOpen)
                camera?.Open();

            return camera;
        }
    }
}
