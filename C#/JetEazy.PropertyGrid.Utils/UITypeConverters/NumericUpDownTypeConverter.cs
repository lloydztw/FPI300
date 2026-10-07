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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace JetEazy.PropertyGrid
{
    /// <summary>
    /// LETIAN: 修改於 2025-10-03
    /// </summary>
    public class NumericUpDownTypeConverter : TypeConverter
    {
        private int _digits = 3;

        public NumericUpDownTypeConverter()
        {
        }
        public NumericUpDownTypeConverter(int digits)
        {
            _digits = digits;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            // Attempt to do them all
            return true;
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            try
            {
                string valueStr;
                if( value is string str)
                    valueStr = str;
                else
                    valueStr = Convert.ChangeType(value, context.PropertyDescriptor.PropertyType).ToString();

                decimal decVal;
                if (!decimal.TryParse(valueStr, out decVal))
                    decVal = decimal.One;

                MinMaxAttribute attr = (MinMaxAttribute)context.PropertyDescriptor.Attributes[typeof(MinMaxAttribute)];
                if (attr != null)
                {
                    decVal = attr.PutInRange(decVal);
                }

                return Convert.ChangeType(decVal, context.PropertyDescriptor.PropertyType);

#if(OPT_REVISING)
                MinMaxAttribute attr = (MinMaxAttribute)context.PropertyDescriptor.Attributes[typeof(MinMaxAttribute)];
                int digits = attr != null ? attr.DecimalPlaces : _digits;

                if (value is float valueF)
                {
                    // 使用 "F3" 格式字串來固定顯示三位小數
                    return valueF.ToString($"F{digits}", culture);
                }
                else if (value is double valueD)
                {
                    // 使用 "F3" 格式字串來固定顯示三位小數
                    return valueD.ToString($"F{digits}", culture);
                }
                else if (value is decimal valueM)
                {
                    return valueM.ToString($"F{digits}", culture);
                }
                else
                {
                    string valueStr;
                    if (value is string str)
                        valueStr = str;
                    else
                        valueStr = Convert.ChangeType(value, context.PropertyDescriptor.PropertyType).ToString();

                    decimal decVal;
                    if (!decimal.TryParse(valueStr, out decVal))
                        decVal = decimal.One;

                    if (attr != null)
                        decVal = attr.PutInRange(decVal);

                    var ret = Convert.ChangeType(decVal, context.PropertyDescriptor.PropertyType);
                    return ret;
                }
#endif
            }
            catch
            {
                return base.ConvertFrom(context, culture, value);
            }
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            try
            {
                MinMaxAttribute attr = (MinMaxAttribute)context.PropertyDescriptor.Attributes[typeof(MinMaxAttribute)];
                int digits = attr != null ? attr.DecimalPlaces : _digits;

                if (destinationType == typeof(string))
                {
                    if (value is float valueF)
                    {
                        // 使用 "F3" 格式字串來固定顯示三位小數
                        return valueF.ToString($"F{digits}", culture);
                    }
                    else if (value is double valueD)
                    {
                        // 使用 "F3" 格式字串來固定顯示三位小數
                        return valueD.ToString($"F{digits}", culture);
                    }
                    else if (value is decimal valueM)
                    {
                        return valueM.ToString($"F{digits}", culture);
                    }
                    else
                    {
                        Convert.ChangeType(value, context.PropertyDescriptor.PropertyType).ToString();
                    }
                }
                else
                {
                    Convert.ChangeType(value, destinationType);
                }
            }
            catch
            {
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    // ReSharper disable MemberCanBePrivate.Global
    /// <summary>
    /// Attribute to allow ranges to be added to the numeric updowner
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MinMaxAttribute : Attribute
    {
        public decimal Min { get; private set; }
        public decimal Max { get; private set; }
        public decimal Increment { get; private set; }
        public int DecimalPlaces { get; private set; }

        /// <summary>
        /// Use to make a simple UInt16 max. Starts at 0, increment = 1
        /// </summary>
        /// <param name="max"></param>
        public MinMaxAttribute(UInt16 max)
           : this((decimal)UInt16.MinValue, max)
        {
        }

        /// <summary>
        /// Use to make a simple integer (or default conversion) based range.
        /// default inclrement is 1
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="increment"></param>
        public MinMaxAttribute(int min, int max, int increment = 1)
           : this((decimal)min, max, increment)
        {
        }

        /// <summary>
        /// Set the Min, Max, increment, and decimal places to be used.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="increment"></param>
        /// <param name="decimalPlaces"></param>
        public MinMaxAttribute(decimal min, decimal max, decimal increment = decimal.One, int decimalPlaces = 0)
        {
            Min = min;
            Max = max;
            Increment = increment;
            DecimalPlaces = decimalPlaces;
        }
        /// <summary>
        /// Set the Min, Max, increment, and decimal places to be used.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="increment"></param>
        /// <param name="decimalPlaces"></param>
        public MinMaxAttribute(float min, float max, float increment, int decimalPlaces)
        {
            Min = (decimal)min;
            Max = (decimal)max;
            Increment = (decimal)increment;
            DecimalPlaces = decimalPlaces;
        }

        /// <summary>
        /// Validation function to check if the value is withtin the range (inclusive)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsInRange(object value)
        {
            decimal checkedValue = (decimal)Convert.ChangeType(value, typeof(decimal));
            return ((checkedValue <= Max)
               && (checkedValue >= Min)
               );
        }

        /// <summary>
        /// Takes the value and adjusts if it is out of bounds.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public decimal PutInRange(object value)
        {
            decimal checkedValue = (decimal)Convert.ChangeType(value, typeof(decimal));
            if (checkedValue > Max)
                checkedValue = Max;
            else if (checkedValue < Min)
                checkedValue = Min;
            return checkedValue;
        }
    }

    // ReSharper restore MemberCanBePrivate.Global
    public class NumericUpDownTypeEditor : UITypeEditor
    {
        //public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        //{
        //    return true;
        //}



        //public override void PaintValue(PaintValueEventArgs e)
        //{

        //    //ControlPaint.DrawCheckBox(e.Graphics, e.Bounds, ((NumericUpDown)e.Context.Instance).Isalarm ? ButtonState.Checked : ButtonState.Normal);
        //}

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context == null || context.Instance == null)
                return base.GetEditStyle(context);
            return context.PropertyDescriptor.IsReadOnly ? UITypeEditorEditStyle.None : UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            try
            {
                if (context == null || context.Instance == null || provider == null)
                    return value;

                //use IWindowsFormsEditorService object to display a control in the dropdown area  
                IWindowsFormsEditorService frmsvr = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (frmsvr == null)
                    return value;
                
                MinMaxAttribute attr = (MinMaxAttribute)context.PropertyDescriptor.Attributes[typeof(MinMaxAttribute)];
                if (attr != null)
                {
                    NumericUpDown nmr = new NumericUpDown
                    {
                        Size = new Size(60, 120),
                        Minimum = attr.Min,
                        Maximum = attr.Max,
                        Increment = attr.Increment,
                        DecimalPlaces = attr.DecimalPlaces,
                        Value = attr.PutInRange(value)
                    };
                    frmsvr.DropDownControl(nmr);
                    context.OnComponentChanged();
                    var ret = Convert.ChangeType(nmr.Value, context.PropertyDescriptor.PropertyType);
                    return ret;
                }
            }
            catch { }
            return value;
        }
    }
}
