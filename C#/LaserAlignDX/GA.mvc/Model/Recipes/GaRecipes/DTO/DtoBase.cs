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

namespace LaserAlignDX.Mvc.Model.Recipe
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
    }
}
