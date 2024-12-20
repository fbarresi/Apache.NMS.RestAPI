﻿using System;
using System.Reactive.Disposables;

namespace Apache.NMS.RestAPI.Interfaces.Extensions
{
    public static class DisposableExtensions
    {
        public static T AddDisposableTo<T>(this T source, CompositeDisposable disposables) where T : IDisposable
        {
            disposables.Add((IDisposable) source);
            return source;
        }
    }
}