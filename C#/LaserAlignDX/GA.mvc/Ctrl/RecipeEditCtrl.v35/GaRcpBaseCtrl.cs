#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-09 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Lang;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace LaserAlignDX.Mvc.Ctrl.V35
{
    public abstract class GaRcpBaseCtrl
    {
        #region GLOBAL_MESS
        protected ITravelerModel _sysModel => GaMvcConfig.SysModel;
        protected RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        #endregion

        protected void HandleException(string funcName, Exception ex)
        {
            var errMsg = $"Error : {GetType().Name}.{funcName}";
            errMsg += "\n\r" + ex.Message;
            errMsg += "\n\r" + ex.StackTrace;
            QMessageBox.Warning(errMsg, translate: false);
        }
    }
}
