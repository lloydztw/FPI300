using System;
using System.ComponentModel;

namespace LaserAlignDX
{
    public class PGTranslator : CustomTypeDescriptor
    {
        const string _langPack = "rcp";

        private readonly object _target;

        public PGTranslator(object target)
        {
            _target = target;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var originalProps = TypeDescriptor.GetProperties(_target, attributes, true);
            var localizedProps = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor prop in originalProps)
            {
                if (QMSG.Lang().LanguageID != 1)
                {
                    try
                    {
                        string newCategory = QMSG.Text(prop.Category, _langPack);
                        string newDisplayName = QMSG.Text(prop.DisplayName, _langPack);
                        string newDescription = QMSG.Text(prop.Description, _langPack);
                        localizedProps.Add(new LocalizedPropertyDescriptor(prop, _target, newDisplayName, newCategory, newDescription));
                    }
                    catch
                    {
                        localizedProps.Add(prop);
                    }
                }
                else
                {
                    localizedProps.Add(prop);
                }
            }

            return localizedProps;
        }

        public override PropertyDescriptorCollection GetProperties() => GetProperties(null);
    }

    public class LocalizedPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _baseProp;
        private readonly object _target; // 修正：保存原始目標物件
        private readonly string _displayName;
        private readonly string _category;
        private readonly string _description;

        public LocalizedPropertyDescriptor(PropertyDescriptor baseProp, object target, string displayName, string category, string description)
            : base(baseProp)
        {
            _baseProp = baseProp;
            _target = target; // 修正：將原始目標指派進來
            _displayName = displayName;
            _category = category;
            _description = description;
        }

        public override string DisplayName => _displayName;
        public override string Category => _category;
        public override string Description => _description;

        // 修正：針對這五個需要與底層物件交互的方法，將外部傳入的 component 改為實際的 _target
        public override bool CanResetValue(object component) => _baseProp.CanResetValue(_target);
        public override object GetValue(object component) => _baseProp.GetValue(_target);
        public override void ResetValue(object component) => _baseProp.ResetValue(_target);
        public override void SetValue(object component, object value) => _baseProp.SetValue(_target, value);
        public override bool ShouldSerializeValue(object component) => _baseProp.ShouldSerializeValue(_target);

        public override Type ComponentType => _baseProp.ComponentType;
        public override bool IsReadOnly => _baseProp.IsReadOnly;
        public override Type PropertyType => _baseProp.PropertyType;
    }
}