#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-10 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.ComponentModel;

namespace JetEazy.PropertyGrid.Legacy
{
    public class ListStringConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "A", "B" });
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}
