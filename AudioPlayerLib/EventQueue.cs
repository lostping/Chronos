//////////////////////////////////////////////////////////////////////////
//
//  Copyright 2011 Sebastian Krysmanski
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
//////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Threading;

namespace AudioPlayerLib {
  /// <summary>
  /// This class basically has the same functionality as <see cref="Dispatcher"/>. It was necessary to write this class
  /// because <c>Dispatcher.Run()</c> can't be shut down (ie. canceled) from <c>AppDomain.ProcessExit</c> (for whatever
  /// reason). This class solves this problem.
  /// </summary>
  /// <remarks>This class was created just to work with <see cref="PlayerThread"/>. It may not work in other situations
  /// and therefor isn't <c>public</c>.</remarks>
  class EventQueue {
    private bool m_isShuttingDown = false;
    private readonly ManualResetEvent m_shutdownCompleteEvent = new ManualResetEvent(false);

    private readonly AutoResetEvent m_eventsWaiting = new AutoResetEvent(false);

    private readonly LinkedList<EventData> m_events = new LinkedList<EventData>();

    private readonly object m_instanceLock = new object();

    private Thread m_runningThread = null;

    public void Run() {
      lock (this.m_instanceLock) {
        EventData data = new EventData();

        this.m_runningThread = Thread.CurrentThread;

        while (!this.m_isShuttingDown) {
          lock (this.m_events) {
            LinkedListNode<EventData> firstItem = this.m_events.First;
            if (firstItem != null) {
              data = firstItem.Value;
              this.m_events.RemoveFirst();
            } else {
              data.Function = null;
              data.Parameters = null;
            }
          }

          if (data.Function == null) {
            this.m_eventsWaiting.WaitOne();
          } else {
            data.Function.DynamicInvoke(data.Parameters);
            if (data.Executed != null) {
              data.Executed.Set();
            }
          }
        }

        this.m_shutdownCompleteEvent.Set();
      }
    }

    public bool CheckAccess() {
      return Thread.CurrentThread == this.m_runningThread;
    }

    public void InvokeShutdown() {
      this.m_isShuttingDown = true;
      this.m_eventsWaiting.Set();
      this.m_shutdownCompleteEvent.WaitOne();
    }

    public void BeginInvokeShutdown() {
      this.m_isShuttingDown = true;
      this.m_eventsWaiting.Set();
    }

    public void Invoke(Delegate func, params object[] parameters) {
      ManualResetEvent executed = new ManualResetEvent(false);
      lock (this.m_events) {
        this.m_events.AddLast(new EventData() { 
          Function = func, 
          Parameters = parameters, 
          Executed = executed 
        });
        this.m_eventsWaiting.Set();
      }

      executed.WaitOne();
    }

    public void BeginInvoke(Delegate func, params object[] parameters) {
      lock (this.m_events) {
        this.m_events.AddLast(new EventData() { 
          Function = func, 
          Parameters = parameters 
        });
        this.m_eventsWaiting.Set();
      }
    }

    private struct EventData {
      public Delegate Function;
      public object[] Parameters;
      public ManualResetEvent Executed;
    }
  }
}
