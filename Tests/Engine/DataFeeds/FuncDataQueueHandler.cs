﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using System.Collections.Generic;
using System.Linq;
using com.sun.org.apache.bcel.@internal.generic;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;
using QuantConnect.Packets;

namespace QuantConnect.Tests.Engine.DataFeeds
{
    /// <summary>
    /// Provides an implementation of <see cref="IDataQueueHandler"/> that can be specified
    /// via a function
    /// </summary>
    public class FuncDataQueueHandler : IDataQueueHandler
    {
        private readonly object _lock = new object();
        private readonly HashSet<Symbol> _subscriptions = new HashSet<Symbol>();
        private readonly Func<FuncDataQueueHandler, IEnumerable<BaseData>> _getNextTicksFunction;

        /// <summary>
        /// Gets the subscriptions currently being managed by the queue handler
        /// </summary>
        public List<Symbol> Subscriptions
        {
            get { lock (_lock) return _subscriptions.ToList(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncDataQueueHandler"/> class
        /// </summary>
        /// <param name="getNextTicksFunction">The functional implementation for the <see cref="GetNextTicks"/> function</param>
        public FuncDataQueueHandler(Func<FuncDataQueueHandler, IEnumerable<BaseData>> getNextTicksFunction)
        {
            _getNextTicksFunction = getNextTicksFunction;
        }

        /// <summary>
        /// Get the next ticks from the live trading data queue
        /// </summary>
        /// <returns>IEnumerable list of ticks since the last update.</returns>
        public IEnumerable<BaseData> GetNextTicks()
        {
            return _getNextTicksFunction(this);
        }

        /// <summary>
        /// Adds the specified symbols to the subscription
        /// </summary>
        /// <param name="request">defines the parameters to subscribe to a data feed</param>
        /// <returns></returns>
        public IEnumerator<BaseData> Subscribe(SubscriptionRequest request, EventHandler newDataAvailableHandler)
        {
            Subscribe(new[] { request.Security.Symbol });

            return null;
        }

        /// <summary>
        /// Adds the specified symbols to the subscription
        /// </summary>
        /// <param name="job">Job we're subscribing for:</param>
        /// <param name="symbols">The symbols to be added keyed by SecurityType</param>
        public void Subscribe(IEnumerable<Symbol> symbols)
        {
            foreach (var symbol in symbols)
            {
                lock (_lock) _subscriptions.Add(symbol);
            }
        }

        /// <summary>
        /// Removes the specified symbols to the subscription
        /// </summary>
        /// <param name="dataConfig">Job we're processing.</param>
        public void Unsubscribe(SubscriptionDataConfig dataConfig)
        {
            Unsubscribe(new Symbol[] { dataConfig.Symbol });
        }

        /// <summary>
        /// Removes the specified symbols to the subscription
        /// </summary>
        /// <param name="symbols">The symbols to be removed keyed by SecurityType</param>
        public void Unsubscribe(IEnumerable<Symbol> symbols)
        {
            foreach (var symbol in symbols)
            {
                lock (_lock) _subscriptions.Remove(symbol);
            }
        }

        /// <summary>
        /// Returns whether the data provider is connected
        /// </summary>
        /// <returns>true if the data provider is connected</returns>
        public bool IsConnected => true;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}