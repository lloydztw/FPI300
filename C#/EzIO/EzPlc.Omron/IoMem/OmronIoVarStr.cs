#region AUTHOR
/*
 * EzPlc.Omron
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-24 LeTian Chang : Integration with EzIO
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

namespace EzPlc.Omron
{
    public class OmronIoVarStr : OmronIoVar
    {
        public OmronIoVarStr(OmronAddress address) : base(address)
        {
        }

        public string Value
        {
            get
            {
                var v = base.ReadVar(_address);
                return v?.ToString();
            }
            set
            {
                base.WriteVar(_address, value);
            }
        }

        public void Set(string value)
        {
            Value = value;
        }
    }
}