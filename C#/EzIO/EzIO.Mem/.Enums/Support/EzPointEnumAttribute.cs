#region AUTHOR
/*
 * EzIO.Mem
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.ComponentModel;
using System.Reflection;

namespace EzIO.Mem
{
    public class EzPointEnumAttribute : Attribute
    {
        /// <summary>
        /// ezIoName 格式為: 
        /// <br/> $"{StationID}:{Category}{Address}.{BitOffset}"
        /// </summary>
        public EzPointEnumAttribute(string description, 
                                    string ezIoName = "", 
                                    bool normalOpen = true, 
                                    bool autoScan = false
                                )
        {
            Description = description;
            EzIoName = ezIoName;
            NormalOpen = normalOpen;
            AutoScan = autoScan;
        }

        public string Description
        {
            get;
            private set;
        }
        public string EzIoName
        {
            get;
            private set;
        }
        public bool NormalOpen
        {
            get;
            private set;
        }
        public bool AutoScan
        {
            get;
            private set;
        }

        public static EzPointEnumAttribute GetAttribute(Enum e)
        {
            System.Reflection.FieldInfo info = e.GetType().GetField(e.ToString());
            if (info.GetCustomAttribute(typeof(EzPointEnumAttribute), false) is EzPointEnumAttribute attr)
            {
                if (string.IsNullOrEmpty(attr.EzIoName))
                    attr.EzIoName = e.ToString();
                return attr;
            }
            else
            {
                return null;
            }
        }
        public static string GetDescription(Enum e)
        {
            var attr = GetAttribute(e);
            if (attr != null)
            {
                return attr.Description;
            }
            else
            {
                DescriptionAttribute[] array = (DescriptionAttribute[])e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
                if (array != null && array.Length != 0)
                {
                    return array[0].Description;
                }
                return e.ToString();
            }
        }
    }
}
