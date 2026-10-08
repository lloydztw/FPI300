#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *      2025-09-22 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


namespace LaserAlignDX.Model
{
    public class LotData
    {
        public string StripID;
        public string LotID;
        public LotData(string stripID, string lotID)
        {
            StripID = stripID;
            LotID = lotID;
        }
        public LotData() : this("Strip_NONE", "Lot_NONE")
        {
        }
    }
}
