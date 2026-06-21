#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

namespace EzAoiChipLocQC.Machine
{
    public static class Factory
    {
        public static ITravellerQcMachine InstanceMachine()
        {
            return Traveller_Atm20_Machine.Instance;
        }
    }
}
