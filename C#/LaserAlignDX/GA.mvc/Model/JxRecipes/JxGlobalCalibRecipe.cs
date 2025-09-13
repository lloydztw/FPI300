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

using LeTian.JxProps;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    public class JxGlobalCalibRecipe: JxContainer
    {
        public override void Load(string fileName)
        {
            //string emptyTrayRecipeFile = LtAoiFactory.RcpGetRecipeFileName(null);
            //EmptyTrayParams.Load(emptyTrayRecipeFile);
            //GaGridParams.Load(fileName);
            //Modified = false;
        }
        public override void Save(string fileName)
        {
            //string emptyTrayRecipeFile = LtAoiFactory.RcpGetRecipeFileName(null);
            //EmptyTrayParams.Save(emptyTrayRecipeFile);
            //GaGridParams.Save(fileName);
            //Modified = false;
        }
    }
}
