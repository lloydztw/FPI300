using EzCamera.Interface;
using MvCamCtrl.NET;

namespace EzCamera.Driver.Hikvision
{
    public class HikvError : EzCameraError
    {
        public HikvError(int errCode, string message = null, string description = null)
            : base(errCode, message, description)
        {
            //ErrorCode = errCode;
            if (message == null)
                Message = GetErrorText(errCode);
            else if (description == null)
                Description = GetErrorText(errCode);
        }

        /// <summary>
        /// 海康專屬的ErrCode對應字串
        /// </summary>
        public static string GetErrorText(int errCode)
        {
            string errorMsg = null;

#if(false)
            switch (errCode)
            {
                case MyCamera.MV_E_HANDLE: errorMsg = "Error or invalid handle"; break;
                case MyCamera.MV_E_SUPPORT: errorMsg = "Not supported function"; break;
                case MyCamera.MV_E_BUFOVER: errorMsg = "Cache is full"; break;
                case MyCamera.MV_E_CALLORDER: errorMsg = "Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg = "Incorrect parameter"; break;
                case MyCamera.MV_E_RESOURCE: errorMsg = "Applying resource failed"; break;
                case MyCamera.MV_E_NODATA: errorMsg = "No data"; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg = "Precondition error, or running environment changed"; break;
                case MyCamera.MV_E_VERSION: errorMsg = "Version mismatches"; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg = "Insufficient memory"; break;
                case MyCamera.MV_E_UNKNOW: errorMsg = "Unknown error"; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg = "General error"; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg = "Node accessing condition error"; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg = "No permission"; break;
                case MyCamera.MV_E_BUSY: errorMsg = "Device is busy, or network disconnected"; break;
                case MyCamera.MV_E_NETER: errorMsg = "Network error"; break;
            }
#endif

            switch (errCode)
            {
                case MyCamera.MV_E_HANDLE: errorMsg = "错误或无效的句柄"; break;
                case MyCamera.MV_E_SUPPORT: errorMsg = "不支持的功能"; break;
                case MyCamera.MV_E_BUFOVER: errorMsg = "缓存已满"; break;
                case MyCamera.MV_E_CALLORDER: errorMsg = "函数调用顺序有误"; break;
                case MyCamera.MV_E_PARAMETER: errorMsg = "错误的参数"; break;
                case MyCamera.MV_E_RESOURCE: errorMsg = "资源申请失败"; break;
                case MyCamera.MV_E_NODATA: errorMsg = "无数据"; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg = "前置条件有误，或运行环境已发生变化"; break;
                case MyCamera.MV_E_VERSION: errorMsg = "版本不匹配"; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg = "传入的内存空间不足"; break;
                case MyCamera.MV_E_UNKNOW: errorMsg = "未知的错误"; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg = "通用错误"; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg = "节点访问条件有误"; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg = "设备无访问权限"; break;
                case MyCamera.MV_E_BUSY: errorMsg = "设备忙，或网络断开"; break;
                case MyCamera.MV_E_NETER: errorMsg = "网络相关错误"; break;
                default:
                    break;
            }

            return errorMsg;
        }
    }
}
