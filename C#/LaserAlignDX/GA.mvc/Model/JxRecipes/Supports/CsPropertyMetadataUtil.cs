#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-12 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using MinMaxAttribute = JetEazy.MinMaxAttribute;

namespace LeTian.JxProps.PropertyMeta
{
    /// <summary>
    /// Metadata DTO (data to object)
    /// </summary>
    public class CsPropertyMetadata
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Type PropertyType { get; set; }          // 型別 Type
        public string TypeName => PropertyType?.Name;   // 型別 簡化輸出
        public string Category { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public bool ReadOnly { get; set; }
        public bool Browsable { get; set; }
        public Range MinMax { get; set; }

        public override string ToString()
        {
            string minmaxStr = MinMax != null ? MinMax.ToString() : "";
            return $"{Name} ({TypeName}) | Cat={Category}, Desc={Description}, DisplayName={DisplayName}, Browsable={Browsable}, MinMax={minmaxStr}";
        }
    }

    /// <summary>
    /// 通用枚舉工具
    /// </summary>
    public static class CsPropertyMetadataUtil
    {
        public static IEnumerable<CsPropertyMetadata> IterPropertyMetadata(object instance)
        {
            if (instance == null)
                yield break;

            Type t = instance.GetType();

            foreach (PropertyInfo prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var metadata = new CsPropertyMetadata
                {
                    Name = prop.Name,
                    Value = prop.GetValue(instance),   // 取出屬性值
                    PropertyType = prop.PropertyType,
                    Category = prop.GetCustomAttribute<CategoryAttribute>()?.Category,
                    Description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description,
                    DisplayName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName,
                    ReadOnly = prop.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly ?? false,
                    Browsable = prop.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true
                };

                var mm = prop.GetCustomAttribute<MinMaxAttribute>();
                if (mm != null)
                {
                    //((decimal)mm.Min, (decimal)mm.Max, (decimal)mm.Increment, (int)mm.DecimalPlaces);
                    metadata.MinMax = new Range(mm.Min, mm.Max, mm.Increment, mm.DecimalPlaces);
                }

                yield return metadata;
            }
        }
        public static IList<CsPropertyMetadata> GetPropertyMetadataList(object instance)
        {
            return new List<CsPropertyMetadata>(IterPropertyMetadata(instance));
        }
    }

    /// <summary>
    /// 自我測試代碼
    /// </summary>
    class UnitTest
    {
        class MyRecipeClass
        {
            const string cat1 = "00.基础设定";

            [CategoryAttribute(cat1), DescriptionAttribute("阵列角度")]
            [DisplayName("A00.阵列角度")]
            [MinMax(-180, 180)]
            [Browsable(true)]
            public float xAngle { get; set; }

            [CategoryAttribute(cat1), DescriptionAttribute("矩阵行数")]
            [DisplayName("A01.行数")]
            [MinMax(0, 99999999)]
            [Browsable(true)]
            public int xRow { get; set; }
        }

        static void Test()
        {
            var myRecipe = new MyRecipeClass();

            foreach (var meta in CsPropertyMetadataUtil.IterPropertyMetadata(myRecipe))
            {
                Console.WriteLine(meta);

                // 這裡可以直接判斷型別
                if (meta.PropertyType == typeof(float))
                    Console.WriteLine($"  → {meta.Name} 是 float (Single)");

                if (meta.PropertyType == typeof(int))
                    Console.WriteLine($"  → {meta.Name} 是 int (Int32)");
            }
        }
    }
}

