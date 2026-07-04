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

using System;
using System.ComponentModel;

namespace JetEazy.Lang
{
    public class PGTranslator : CustomTypeDescriptor
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

        private readonly object _target;

        internal PGTranslator(object target)
        {
            _target = target;
        }

        /// <summary>
        /// 靜態方法：為指定物件註冊/更新多語系翻譯器
        /// </summary>
        /// <param name="target">要放入 PropertyGrid 的原始物件 (如 _xParamGrid)</param>
        public static void Register(object target)
        {
            if (target == null) return;

            // 1. 取得目前物件身上所有的 Providers
            // 因為 AddProvider 允許重複堆疊，我們必須強制迴圈檢查，直到把所有 PGTranslationProvider 都拔除乾淨
            while (true)
            {
                var provider = TypeDescriptor.GetProvider(target);

                // 如果最頂層是我們的 Provider，直接拔除
                if (provider is PGTranslationProvider)
                {
                    TypeDescriptor.RemoveProvider(provider, target);
                    continue; // 繼續檢查下一層
                }

                // 如果最頂層不是，但它是被封裝的（例如某些擴充套件的 Provider），
                // 透過反射去檢查它底層有沒有包含我們的 Provider，有的話將其拔除。
                // 最安全、最簡單的做法是直接強制 Remove 一次我們新 new 出來的類型複本（.NET 會比對類型）
                break;
            }

            // 2. 為了徹底防止有些隱藏的 Provider 拔不掉，最頂級的安全牌做法：
            // 在 Add 之前，建立一個明確的實例，並用它來做一次移除動作
            var dummy = new PGTranslationProvider();
            TypeDescriptor.RemoveProvider(dummy, target);

            // 3. 註冊全新的多語系 Provider
            TypeDescriptor.AddProvider(dummy, target);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            // 修正：傳入 _target.GetType() 而不是 _target 實例，打破無窮迴圈
            var originalProps = TypeDescriptor.GetProperties(_target.GetType(), attributes);

            if (_lang == null || _lang.LanguageID == 1)
                return originalProps;

            var localizedProps = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor prop in originalProps)
            {
                try
                {
                    string newCategory = _T(prop.Category);
                    string newDisplayName = _T(prop.DisplayName);
                    string newDescription = _T(prop.Description);
                    localizedProps.Add(new LocalizedPropertyDescriptor(prop, _target, newDisplayName, newCategory, newDescription));
                }
                catch
                {
                    localizedProps.Add(prop);
                }
            }

            return localizedProps;
        }

        public override PropertyDescriptorCollection GetProperties() => GetProperties(null);
    }

    /// <summary>
    /// 自訂的 TypeDescriptionProvider
    /// </summary>
    public class PGTranslationProvider : TypeDescriptionProvider
    {
        public PGTranslationProvider() : base(TypeDescriptor.GetProvider(typeof(object)))
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if (instance != null)
            {
                return new PGTranslator(instance);
            }
            return base.GetTypeDescriptor(objectType, instance);
        }
    }

    public class LocalizedPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _baseProp;
        private readonly object _target;
        private readonly string _displayName;
        private readonly string _category;
        private readonly string _description;

        public LocalizedPropertyDescriptor(PropertyDescriptor baseProp, object target, string displayName, string category, string description)
            : base(baseProp)
        {
            _baseProp = baseProp;
            _target = target;
            _displayName = displayName;
            _category = category;
            _description = description;
        }

        public override string DisplayName => _displayName;
        public override string Category => _category;
        public override string Description => _description;

        // 優化：優先使用元件自己傳進來的 component 物件（這通常就是 _target）
        // 如果 PropertyGrid 丟進來的是包裝過的特殊物件，我們再用 _target 做安全兜底，確保絕對能讀寫成功。
        public override bool CanResetValue(object component) => _baseProp.CanResetValue(component ?? _target);
        public override object GetValue(object component) => _baseProp.GetValue(component ?? _target);
        public override void ResetValue(object component) => _baseProp.ResetValue(component ?? _target);
        public override void SetValue(object component, object value) => _baseProp.SetValue(component ?? _target, value);
        public override bool ShouldSerializeValue(object component) => _baseProp.ShouldSerializeValue(component ?? _target);

        public override Type ComponentType => _baseProp.ComponentType;
        public override bool IsReadOnly => _baseProp.IsReadOnly;
        public override Type PropertyType => _baseProp.PropertyType;

        // 修正：必須複寫此屬性，否則內建的列舉型別轉換器（如 EnumConverter）在某些進階反射操作中可能會找不到對應的 Converter
        public override TypeConverter Converter => _baseProp.Converter;
    }
}