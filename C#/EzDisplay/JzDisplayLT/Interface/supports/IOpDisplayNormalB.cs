using JetEazy;
using System.Drawing;

namespace JzDisplay.Interface
{
    /// <summary>
    /// OPDisplay 與 Bitmap 操作相關的函式
    /// </summary>
    public interface IOpDisplayNormalB
    {
        /// <summary>
        /// bmp == null 代表回到全景畫面
        /// </summary>
        void ReplaceDisplayImage(Bitmap bmp = null, bool IsResetMover = false);
        void ReplaceDisplayImage(Bitmap bmp);

        void BackupImage();
        void RestoreImage();

        /// <summary>
        /// 此函式會把 GeoFigure也畫入圖像, 生成【新的 bmp】.
        /// <br/> 調用者必須維持此新的 bmp 生命週期 !!!
        /// </summary>
        Bitmap GetScreen();
        void SaveScreen(string saveFileName = null);

        /// <summary>
        /// 此函式直接傳回 bmpOrg 的參考 (reference)
        /// 調用者不可以將其 Dispose() !!!
        /// </summary>
        Bitmap GetOrgBMP();

        /// <summary>
        /// 在Capture時使用的抓圖方式
        /// <br/> 此函式會生成 【新的 bmp】.
        /// <br/> 調用者必須維持此新的 bmp 生命週期 !!!
        /// </summary>
        Bitmap GetOrgBMP(RectangleF rectf);
        void SaveOrgBMP(string savefilename);

        void SetMatching(Bitmap bmp, MatchMethodEnum matchmethod);
        void GenSearchImage(ref Bitmap bmp);
        void GetMask(int outRangeX, int outRangeY);
    }
}