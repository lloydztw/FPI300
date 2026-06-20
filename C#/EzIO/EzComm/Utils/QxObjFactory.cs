#region AUTHOR
/****************************************************************************
 *                                                                          
 * Copyright (c) 2012 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$
 *          20121204 LeTian Chang: ReOpen UART when communication failed.
 *          20120622 LeTian Chang: Revised for more robust over RS232 connection.
 *	        20080701 LeTian Chang: Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EzComm.Utils
{
    public class QxObjFactory<TKey>
    {
        #region PRIVATE_DATA
        // 1. 使用 ConcurrentDictionary 並儲存 Lazy 物件，確保初始化邏輯的原子性
        private readonly ConcurrentDictionary<TKey, Lazy<object>> m_table = new ConcurrentDictionary<TKey, Lazy<object>>();
        // 2. 移除公開的 Sync 屬性，改為私有的 lock 物件用於 Dispose 等全局操作
        private readonly object m_globalLock = new object();
        #endregion

        /// <summary>
        /// 嘗試找出已經註冊的物件; 如果沒有找到, 則自動調用生成函式, 生成新物件, 並自動註冊.
        /// </summary>
        public TObj Instance<TObj>(TKey key, Func<object, TObj> createFunc, object createArg = null) where TObj : class
        {
            // GetOrAdd 本身是線程安全的
            // LazyThreadSafetyMode.ExecutionAndPublication 確保 createFunc 只會被呼叫一次

            var lazyObj = m_table.GetOrAdd(key, k => new Lazy<object>(() =>
            {
                return createFunc(createArg);
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication));

            return lazyObj.Value as TObj;
        }

        public object Find(TKey key)
        {
            if (m_table.TryGetValue(key, out var lazyObj))
                return lazyObj.Value;
            return null;
        }

        public void Register(TKey key, object obj)
        {
            // 如果物件已存在則不動作，確保唯一性
            m_table.TryAdd(key, new Lazy<object>(() => obj));
        }

        public void Unregister(TKey key)
        {
            m_table.TryRemove(key, out _);
        }

        public void DisposeAll()
        {
            // DisposeAll 屬於破壞性操作，建議加上全域鎖避免與 Instance 衝突
            lock (m_globalLock)
            {
                foreach (var lazyObj in m_table.Values)
                {
                    var obj = lazyObj.Value;
                    if (obj is IDisposable disposable)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch
                        {
                            // 根據需求記錄日誌或吞掉異常
                        }
                    }
                }
                m_table.Clear();
            }
        }

        public IEnumerable<KeyValuePair<TKey, object>> EnumKeyAndObjs()
        {
            // 轉為 IEnumerable 時，使用 ConcurrentDictionary 的快照功能
            return m_table.Select(kv => new KeyValuePair<TKey, object>(kv.Key, kv.Value.Value));
        }
    }
}
