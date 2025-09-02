using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace JetEazy
{
    public class JzEnumConverter : EnumConverter
    {
        public string FilterName = string.Empty;

        private Type _enumType;
        /// <summary>Initializing instance</summary>
        /// <param name="type">type Enum</param>
        ///this is only one function, that you must 
        ///to change. All another functions for enums 
        ///you can use by Ctrl+C/Ctrl+V
        public JzEnumConverter(Type type)
            : base(type)
        {
            _enumType = type;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context,
                Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture,
            object value, Type destType)
        {
            FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
            DescriptionAttribute dna =
              (DescriptionAttribute)Attribute.GetCustomAttribute(
                fi, typeof(DescriptionAttribute));

            if (dna != null)
                return dna.Description;
            else
                return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type srcType)
        {
            return srcType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            foreach (FieldInfo fi in _enumType.GetFields())
            {
                DescriptionAttribute dna =
                  (DescriptionAttribute)Attribute.GetCustomAttribute(
                    fi, typeof(DescriptionAttribute));

                if ((dna != null) && ((string)value == dna.Description))
                    return Enum.Parse(_enumType, fi.Name);
            }
            return Enum.Parse(_enumType, (string)value);
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
            //if (_enumType == null)
            //{
            //    return base.GetStandardValues(context);
            //}

            //// 获取所有枚举值
            //Array values = Enum.GetValues(_enumType);

            //// 过滤掉不需要的枚举值
            //var filteredValues = new System.Collections.ArrayList();
            //foreach (var value in values)
            //{
            //    if (value.ToString() != "Value3") // 隐藏 Value3
            //    {
            //        filteredValues.Add(value);
            //    }
            //}

            return new StandardValuesCollection(filteredValues);
        }
    }


    public class EnumTypeConverter : EnumConverter
    {
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
            FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
            DescriptionAttribute da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

            if (da != null)
                return da.Description;
            else
                return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            foreach (FieldInfo fi in _enumType.GetFields())
            {
                DescriptionAttribute da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

                if ((da != null) && ((string)value == da.Description))
                    return Enum.Parse(_enumType, fi.Name);
            }
            return Enum.Parse(_enumType, (string)value);
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
