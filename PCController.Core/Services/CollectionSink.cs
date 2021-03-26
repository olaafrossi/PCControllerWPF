// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 12
// by Olaaf Rossi

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Serilog.Core;
using Serilog.Events;
// ReSharper disable CheckNamespace

namespace PCController.Core.Services
{

    public class CollectionSink : ILogEventSink
    {
        public CollectionSink()
        {
            
        }

        public ConcurrentBag<LogEvent> LogMessage { get; } = new ConcurrentBag<LogEvent>();

        public event EventHandler<string> LogString;


        public void Emit(LogEvent logMsg)
        {
            LogMessage.Add(logMsg);
            LogString?.Invoke(this, "hello");
        }

        protected virtual void CallEvent()
        {
            
        }
    }
}