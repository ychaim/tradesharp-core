/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.Tradier.Provider;

namespace TradeHub.MarketDataProvider.Tradier.Tests.Integration
{
    [TestFixture]
    public class MarketDataTestCase
    {
        private TradierMarketDataProvider _marketDataProvider;

        [SetUp]
        public void SetUp()
        {
            _marketDataProvider = new TradierMarketDataProvider();
        }

        [Test]
        [Category("Integration")]
        public void Logon_SendRequestToServer_ReceiveLogonArrived()
        {
            bool logonReceived = false;

            var logonManualResetEvent = new ManualResetEvent(false);

            _marketDataProvider.LogonArrived += delegate(string providerName)
            {
                logonReceived = true;
                logonManualResetEvent.Set();
            };

            _marketDataProvider.Start();

            logonManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, logonReceived, "Logon Received");
        }

        [Test]
        [Category("Integration")]
        public void NewSubscription_SendRequestToServer_ReceiveQuoteStreamByServer()
        {
            bool logonReceived = false;
            bool tickReceived = false;

            var logonManualResetEvent = new ManualResetEvent(false);
            var tickManualResetEvent = new ManualResetEvent(false);

            _marketDataProvider.LogonArrived += delegate(string providerName)
            {
                logonReceived = true;
                logonManualResetEvent.Set();

                _marketDataProvider.SubscribeTickData(new Subscribe() { Security = new Security() { Symbol = "AAPL" } });
            };

            _marketDataProvider.TickArrived += delegate(Tick tick)
            {
                tickReceived = true;
                //tickManualResetEvent.Set();
                Console.WriteLine(tick);
            };

            _marketDataProvider.Start();

            logonManualResetEvent.WaitOne(10000, false);
            tickManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, logonReceived, "Logon Received");
            Assert.AreEqual(true, tickReceived, "Tick Received");
        }

        [Test]
        [Category("Integration")]
        public void HistoricalData_SendRequestToServer_ReceiveHistoricalDataFromServer()
        {
            bool logonReceived = false;
            bool dataReceived = false;

            var logonManualResetEvent = new ManualResetEvent(false);
            var dataManualResetEvent = new ManualResetEvent(false);

            var dataRequestMessage = new HistoricDataRequest() {Security = new Security() {Symbol = "AAPL"}};
            dataRequestMessage.BarType = BarType.MONTHLY;
            dataRequestMessage.StartTime = new DateTime(2015, 2, 1);
            dataRequestMessage.EndTime = DateTime.Now;

            _marketDataProvider.LogonArrived += delegate(string providerName)
            {
                logonReceived = true;
                logonManualResetEvent.Set();

                _marketDataProvider.HistoricBarDataRequest(dataRequestMessage);
                
            };

            _marketDataProvider.HistoricBarDataArrived += delegate(HistoricBarData data)
            {
                dataReceived = true;
                dataManualResetEvent.Set();
                Console.WriteLine(data.Security.Symbol);
            };

            _marketDataProvider.Start();

            logonManualResetEvent.WaitOne(10000, false);
            dataManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, logonReceived, "Logon Received");
            Assert.AreEqual(true, dataReceived, "Data Received");
        }
    }
}
