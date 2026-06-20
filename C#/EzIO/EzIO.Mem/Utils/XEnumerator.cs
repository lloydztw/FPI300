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

using System.Collections;
using System.Collections.Generic;


namespace EzIO.Mem.Utils
{
    public class XEnumerator<T> : IEnumerator<T>
    {
        #region PRIVATE_DATA
        IEnumerator _src;
        #endregion

        public XEnumerator(IEnumerator src)
        {
            _src = src;
        }
        object IEnumerator.Current => _src.Current;
        public T Current => (T)_src.Current;
        public bool MoveNext() => _src.MoveNext();
        public void Reset() => _src.Reset();
        public void Dispose() { }
    }
}
