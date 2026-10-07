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
using System.Collections.Generic;
using System.ComponentModel;

namespace JetEazy.PropertyGrid
{
    public class CustomReadOnlyTypeDescriptionProvider : TypeDescriptionProvider
    {
        #region PRIVATE_DATA
        private readonly TypeDescriptionProvider _baseProvider;
        private readonly List<string> _readOnlyProperties;
        #endregion

        public CustomReadOnlyTypeDescriptionProvider(Type objectType, List<string> readOnlyProperties)
            : base(TypeDescriptor.GetProvider(objectType))
        {
            _baseProvider = TypeDescriptor.GetProvider(objectType);
            _readOnlyProperties = readOnlyProperties;
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            ICustomTypeDescriptor baseDescriptor = _baseProvider.GetTypeDescriptor(objectType, instance);
            return new CustomReadOnlyTypeDescriptor(baseDescriptor, _readOnlyProperties);
        }
    }


    public class CustomReadOnlyTypeDescriptor : CustomTypeDescriptor
    {
        #region PRIVATE_DATA
        private readonly List<string> _readOnlyProperties;
        #endregion

        public CustomReadOnlyTypeDescriptor(ICustomTypeDescriptor parent, List<string> readOnlyProperties)
            : base(parent)
        {
            _readOnlyProperties = readOnlyProperties;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection originalProps = base.GetProperties(attributes);
            var newProps = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor prop in originalProps)
            {
                // 檢查屬性名稱是否在唯讀清單中
                if (_readOnlyProperties.Contains(prop.Name))
                {
                    // 如果是，建立一個新的 PropertyDescriptor 並添加唯讀屬性
                    var newProp = TypeDescriptor.CreateProperty(
                        prop.ComponentType,
                        prop,
                        new ReadOnlyAttribute(true)
                    );
                    newProps.Add(newProp);
                }
                else
                {
                    newProps.Add(prop);
                }
            }
            return newProps;
        }
    }
}

