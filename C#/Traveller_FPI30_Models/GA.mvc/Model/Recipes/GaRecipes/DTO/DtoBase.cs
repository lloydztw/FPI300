#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-20 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Drawing;

namespace JetEazy.DTO
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    public abstract class DtoBase : JetEazy.Utils.WinIni
    {
        public virtual void Load(string iniFileName)
        {
        }
        public virtual void Save(string iniFileName)
        {
        }

        #region INI_HELPER_FUNCTIONS
        /// <summary>
        /// 寫入 PointF[] 至 INI 檔案 (扁平化為 float[]: X1, Y1, X2, Y2...)
        /// </summary>
        protected void Write(string iniFile, string section, string key, PointF[] values)
        {
            if (values == null || values.Length == 0)
            {
                WriteINIValue(section, key, "", iniFile);
                return;
            }

            float[] flatValues = new float[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                flatValues[i * 2] = values[i].X;
                flatValues[i * 2 + 1] = values[i].Y;
            }

            // 呼叫 WinIni 內建的 Write(string, string, string, params float[])
            Write(iniFile, section, key, flatValues);
        }

        /// <summary>
        /// 從 INI 檔案讀取 PointF[]
        /// </summary>
        protected void Read(string iniFile, string section, string key, PointF[] defaultValue, out PointF[] value)
        {
            // 呼叫 WinIni 內建的 Read(string, string, string, out float[])
            bool ok = Read(iniFile, section, key, out float[] flatValues);
            if (ok && flatValues != null && flatValues.Length >= 2)
            {
                int count = flatValues.Length / 2;
                value = new PointF[count];
                for (int i = 0; i < count; i++)
                {
                    value[i] = new PointF(flatValues[i * 2], flatValues[i * 2 + 1]);
                }
            }
            else
            {
                value = defaultValue ?? new PointF[0];
            }
        }
        #endregion
    }
}
