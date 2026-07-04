using System;
using System.Runtime.InteropServices;

namespace JetEazy.Lang
{
    public class JzLangKernel32
    {
        ///
        /// 使用系統 kernel32.dll 進行轉換
        ///
        private const int LocaleSystemDefault = 0x0800;
        private const int LcmapSimplifiedChinese = 0x02000000;
        private const int LcmapTraditionalChinese = 0x04000000;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int LCMapString(int locale, int dwMapFlags, string lpSrcStr, int cchSrc,
                                              [Out] string lpDestStr, int cchDest);

        public static string ToSimplified(string argSource)
        {
            var t = new string(' ', argSource.Length);
            LCMapString(LocaleSystemDefault, LcmapSimplifiedChinese, argSource, argSource.Length, t, argSource.Length);
            return t;
        }

        public static string ToTraditional(string argSource)
        {
            var t = new string(' ', argSource.Length);
            LCMapString(LocaleSystemDefault, LcmapTraditionalChinese, argSource, argSource.Length, t, argSource.Length);
            return t;
        }

        public static string ToVietnamese(string chineseStr)
        {
            // 會產生亂碼
            // 沒有用
            int bufLen = chineseStr.Length * 2;
            string vietnameseStr = new string(' ', bufLen);

            int len = LCMapString(
                0x0400,                    // 系統預設地區設置
                0x800,                     // 將中文轉換為越南文
                chineseStr,                // 要轉換的中文字符串
                chineseStr.Length,         // 計算字符串的長度
                vietnameseStr,             // 存放轉換後的越南文字符串
                vietnameseStr.Length       // 存放轉換後的越南文字符串的緩衝區大小
            );

            if (len > 0)
            {
                return vietnameseStr.Substring(0, len);
            }
            else
            {
                return null;
            }
        }
    }
}
