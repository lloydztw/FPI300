#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-07-04 改版 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace JetEazy.Lang
{
    public class JzEnumConverter : EnumConverter
    {
        #region LANGUAGE
        private QxLang _lang => QxLang.Instance("rcp");
        private string _T(string text)
        {
            var t = _lang?.Translate(text);
            if (!string.IsNullOrEmpty(t))
                return t;
            return text;
        }
        #endregion

        public string FilterName = string.Empty;
        private Type _enumType;

        /// <summary>Initializing instance</summary>
        /// <param name="type">type Enum</param>
        public JzEnumConverter(Type type)
            : base(type)
        {
            _enumType = type;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (value == null)
                return value;

            FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
            if (fi == null) return value.ToString();

            DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

            if (dna != null)
            {
                // 使用 _T(...) 代替 QMSG.Text
                return _T(dna.Description);
            }
            else
            {
                return value.ToString();
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string inputStr = value as string;
            if (string.IsNullOrEmpty(inputStr))
                return base.ConvertFrom(context, culture, value);

            foreach (FieldInfo fi in _enumType.GetFields())
            {
                DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

                if (dna != null)
                {
                    // 反查時同樣使用 _T(...) 轉成當前語系進行比對
                    if (inputStr == _T(dna.Description))
                        return Enum.Parse(_enumType, fi.Name);
                }
            }
            return Enum.Parse(_enumType, inputStr);
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            StandardValuesCollection _v = base.GetStandardValues(context);
            var filteredValues = new System.Collections.ArrayList();
            foreach (var v in _v)
            {
                if (FilterName.Contains(v.ToString()))
                    continue;

                filteredValues.Add(v);
            }

            return new StandardValuesCollection(filteredValues);
        }
    }


    public class EnumTypeConverter : EnumConverter
    {
        #region LANGUAGE
        private QxLang _lang => QxLang.Instance("rcp");
        private string _T(string text)
        {
            var t = _lang?.Translate(text);
            if (!string.IsNullOrEmpty(t))
                return t;
            return text;
        }
        #endregion

        private Type _enumType;

        public EnumTypeConverter(Type type) : base(type)
        {
            _enumType = type;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (value == null)
                return value;

            FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
            if (fi == null) return value.ToString();

            DescriptionAttribute da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

            if (da != null)
            {
                // 使用 _T(...) 代替 QMSG.Text
                return _T(da.Description);
            }
            else
            {
                return value.ToString();
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string inputStr = value as string;
            if (string.IsNullOrEmpty(inputStr))
                return base.ConvertFrom(context, culture, value);

            foreach (FieldInfo fi in _enumType.GetFields())
            {
                DescriptionAttribute da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

                if (da != null)
                {
                    // 反查時使用 _T(...) 進行多語系匹配
                    if (inputStr == _T(da.Description))
                        return Enum.Parse(_enumType, fi.Name);
                }
            }
            return Enum.Parse(_enumType, inputStr);
        }
    }


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
