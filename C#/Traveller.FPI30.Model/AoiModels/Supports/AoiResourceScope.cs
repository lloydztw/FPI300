#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.Collections.Generic;

namespace Traveller.FPI30.Model
{
    /// <summary>Owns cleanup actions in reverse registration order, including partial initialization.</summary>
    internal sealed class AoiResourceScope : IDisposable
    {
        #region PRIVATE_DATA

        readonly Stack<Action> _cleanup = new Stack<Action>();
        bool _disposed;

        #endregion

        public void Add(Action cleanup)
        {
            if (cleanup == null) throw new ArgumentNullException(nameof(cleanup));
            if (_disposed) throw new ObjectDisposedException(nameof(AoiResourceScope));
            _cleanup.Push(cleanup);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            var errors = new List<Exception>();
            while (_cleanup.Count != 0)
            {
                try { _cleanup.Pop()(); }
                catch (Exception ex) { errors.Add(ex); }
            }
            if (errors.Count != 0)
                throw new AggregateException("AOI resource cleanup failed.", errors);
        }
    }
}
