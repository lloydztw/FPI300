#region AUTHOR
/*
 * EzIO.Mem
 * Copyright (C) 2019
 * 2019-11-30 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using System;
using System.Collections;
using System.Collections.Generic;


namespace EzIO.Mem.Utils
{
    public class XSortedKeyEnumerator<K,T> : IEnumerator<T>
    {
        #region PRIVATE_DATA
        Dictionary<K,T> _src;
        K[] _keys;
        int _idx;
        #endregion

        public XSortedKeyEnumerator(Dictionary<K, T> src)
        {
            _src = src;
            int N = _src.Count;
            _keys = new K[N];
            _src.Keys.CopyTo(_keys, 0);
            Array.Sort(_keys);
            _idx = -1;
        }
        public T Current
        {
            get
            {
                return (0 <= _idx && _idx < _keys.Length) ? 
                    _src[_keys[_idx]] : default;
            }
        }
        object IEnumerator.Current => Current;
        public bool MoveNext() => ++_idx < _keys.Length;
        public void Reset() => _idx = -1;
        public void Dispose() { }
    }
}
