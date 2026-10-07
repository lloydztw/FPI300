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

using System.Collections.Generic;
using System.Linq;

namespace LeTian.JxProps.PropertyMeta
{
    public class JxMeta
    {
        public CsPropertyMetadata MetaData
        {
            get;
            internal set;
        }
        public IProp Prop
        {
            get;
            internal set;
        }
    }
    
    public static class JxMetaUtil
    {
        public static Dictionary<IProp, JxMeta> CreateJxPropsDict(object instance, string category)
        {
            var dict = new Dictionary<IProp, JxMeta>();
            foreach (var meta in CsPropertyMetadataUtil.IterPropertyMetadata(instance))
            {
                if (meta.Category != category) continue;
                var jx = CreateJxProp(meta);
                if (jx != null)
                {
                    var item = new JxMeta() { MetaData = meta, Prop = jx };
                    dict.Add(jx, item);
                }
            }
            return dict;
        }
        public static List<JxMeta> CreateJxPropsList(object instance, string category)
        {
            var lst = new List<JxMeta>();
            foreach (var meta in CsPropertyMetadataUtil.IterPropertyMetadata(instance))
            {
                if (meta.Category != category) continue;
                var jx = CreateJxProp(meta);
                if (jx != null)
                {
                    var item = new JxMeta() { MetaData = meta, Prop = jx };
                    lst.Add(item);
                }
            }
            return lst;
        }
        public static List<JxMeta> CreateJxPropsList(params object[] instances)
        {
            var metaList = new List<CsPropertyMetadata>();
            foreach (var instance in instances)
                metaList.AddRange(CsPropertyMetadataUtil.GetPropertyMetadataList(instance));
            var jxMetaList = new List<JxMeta>();
            foreach (var meta in metaList)
            {
                var jx = CreateJxProp(meta);
                if (jx == null) continue;
                var jxMeta = new JxMeta()
                {
                    MetaData = meta,
                    Prop = jx
                };
                jxMetaList.Add(jxMeta);
            }
            return jxMetaList;
        }
        public static IProp CreateJxProp(CsPropertyMetadata meta)
        {
            if (meta == null) return null;

            string description = meta.DisplayName;
            if (meta.ReadOnly)
                description += " (ReadOnly)";

            if (meta.PropertyType == typeof(int))
            {
                var jx = new JxInt(meta.Name, description, (int)meta.Value, range: meta.MinMax);
                return jx;
            }
            else if (meta.PropertyType == typeof(System.Single))
            {
                var value = (float)(System.Single)meta.Value;
                var jx = new JxNumber(meta.Name, description, (decimal)value, range: meta.MinMax);
                return jx;
            }
            else if (meta.PropertyType == typeof(float) || meta.PropertyType == typeof(double))
            {
                var jx = new JxNumber(meta.Name, description, (decimal)meta.Value, range: meta.MinMax);
                return jx;
            }
            else if (meta.PropertyType == typeof(string))
            {
                var jx = new JxText(meta.Name, (string)meta.Value, description);
                return jx;
            }
            return null;
        }
    }

    public class JxMetaContainer
    {
        #region PRIVATE_DATA
        Dictionary<IProp, JxMeta> _propsDict;
        #endregion

        public JxMetaContainer(object instance, string category)
        {
            _propsDict = JxMetaUtil.CreateJxPropsDict(instance, category);
            foreach (var jxProp in _propsDict.Keys)
            {
                jxProp.OnModified += (s, e) =>
                {
                    if (s is IProp jx && _propsDict.TryGetValue(jx, out JxMeta meta))
                    {
                        meta.MetaData.Value = jx.Data;
                    }
                };
            }
        }
        public IProp[] GetBrowsableProps()
        {
            var props = new List<IProp>();
            foreach(var jxProp in _propsDict.Keys)
            {
                if (_propsDict.TryGetValue(jxProp, out JxMeta meta))
                {
                    if (meta != null && meta.MetaData.Browsable)
                        props.Add(jxProp);
                }
            }
            return props.ToArray();
        }
    }
}

