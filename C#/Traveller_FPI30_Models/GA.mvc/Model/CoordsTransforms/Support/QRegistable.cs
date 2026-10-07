#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


namespace LaserAlignDX.Model.Coords.Support
{
    public abstract class QRegistable_using_lock
    {
        #region PRIVATE_STATIC_REGISTER_TABLE
        private static readonly object _sync = new object();
        static Dictionary<string, QRegistable> _registerTable = new Dictionary<string, QRegistable>();
        #endregion

        public virtual string Name { get; protected set; } = "";

        public static void Register(QRegistable obj)
        {
            if (obj == null)
                return;

            lock (_sync)
            {
                if (!_registerTable.ContainsKey(obj.Name))
                    _registerTable.Add(obj.Name, obj);
            }
        }

        public static void Unregister(QRegistable obj)
        {
            if (obj == null) return;

            lock (_sync)
            {
                if (_registerTable.ContainsKey(obj.Name))
                    _registerTable.Remove(obj.Name);
            }
        }

        public static QRegistable Find(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            lock (_sync)
            {
                if (_registerTable.ContainsKey(name))
                    return _registerTable[name];
                return null;
            }
        }

        /// <summary>
        /// 卸載所有已註冊之物件 (exclusiveNames 除外)
        /// </summary>
        public static void DisposeAll(params string[] exclusiveNames)
        {
            lock (_sync)
            {
                var keysToRemove = _registerTable.Keys
                    .Where(k => !exclusiveNames.Contains(k))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    if (_registerTable[key] is IDisposable d)
                        d.Dispose();

                    _registerTable.Remove(key);
                }
            }
        }
    }


    public abstract class QRegistable
    {
        #region PRIVATE_STATIC_REGISTER_TABLE
        // 使用 ConcurrentDictionary 確保線程安全
        private static readonly ConcurrentDictionary<string, QRegistable> _registerTable =
            new ConcurrentDictionary<string, QRegistable>();
        #endregion

        public virtual string Name { get; protected set; }

        public static void Register(QRegistable obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.Name)) return;
            // TryAdd 確保原子性：檢查並新增
            _registerTable.TryAdd(obj.Name, obj);
        }

        public static void Unregister(QRegistable obj)
        {
            if (obj == null) return;
            _registerTable.TryRemove(obj.Name, out _);
        }

        public static QRegistable Find(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            _registerTable.TryGetValue(name, out var obj);
            return obj;
        }

        /// <summary>
        /// 卸載所有已註冊之物件 (exclusiveNames 除外)
        /// </summary>
        public static void DisposeAll(params string[] exclusiveNames)
        {
            // 找出不在排除名單中的所有 Key
            var keysToRemove = _registerTable.Keys
                .Where(key => !exclusiveNames.Contains(key))
                .ToList();

            foreach (var key in keysToRemove)
            {
                // 原子化移除，確保不會重複 Dispose
                if (_registerTable.TryRemove(key, out var obj))
                {
                    if (obj is IDisposable d)
                        d.Dispose();
                }
            }
        }
    }
}
