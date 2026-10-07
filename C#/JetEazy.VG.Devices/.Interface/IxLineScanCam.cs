using System;
using static JetEazy.CCDSpace.CameraPara;

namespace JetEazy.Interface
{
    public interface IxLineScanCam : IDisposable
    {
        void Init(bool debug, string inipara);
        bool IsSim();
        bool Open();
        bool Open(string configFile);
        bool Close();

        void SetExposure(float val);
        float GetExposure();
        void SetGain(float val);
        float GetGain();

        /// <summary>
        /// 正确取得图像标志
        /// </summary>
        bool IsGrapImageOK { get; set; }

        /// <summary>
        /// 取像完成的标志
        /// </summary>
        bool IsGrapImageComplete { get; set; }

        void SoftTrigger();
        /// <summary>
        /// 获取图像
        /// 大于0 放大倍数 等于0 不变尺寸 小于0缩小倍数
        /// </summary>
        /// <param name="size">大于0 放大倍数 等于0 不变尺寸 小于0缩小倍数</param>
        /// <returns>返回图像</returns>
        System.Drawing.Bitmap GetPageBitmap(int size = 0);
        //System.Drawing.Bitmap GetPageBitmap(int size);
        void EncoderReset();
        void ShowSetup();
        void StartGrab();
        void StopGrab();

        FreeImageAPI.FreeImageBitmap GetFreeImageBitmap(int size = 0);
        string OperateShowMessage { get; set; }

        //void LineTriggerHandler(/*dvpHandle*/uint handle, dvpStreamEvent _event, /*void **/IntPtr pContext, ref dvpFrame refFrame, /*void **/IntPtr pBuffer);
        event LineTriggerHandler LineTriggerAction;

    }
}
