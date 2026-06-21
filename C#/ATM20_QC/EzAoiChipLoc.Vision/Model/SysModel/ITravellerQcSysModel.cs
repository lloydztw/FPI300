#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-28 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;

namespace EzAoiChipLocQC.Model
{
    public interface ITravellerQcModel : IDisposable
    {
        event EventHandler<ProgressEventArgs> OnError;
        
        /// <summary>
        /// 保留
        /// </summary>
        object GetCurrentRecipe();

        /// <summary>
        /// 套用參數
        /// </summary>
        void ApplyRecipe(params object[] args);

        
    }
}
