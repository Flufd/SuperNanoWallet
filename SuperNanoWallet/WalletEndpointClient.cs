using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SuperNanoWallet.Models.LightWallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNanoWallet
{
    public class WalletEndpointClient
    {
        private readonly string endpoint;
        private readonly JsonParser jsonParser;
        private ClientWebSocket webSocket;
       
        private JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };


        public WalletEndpointClient(string endpoint)
        {
            this.endpoint = endpoint;
            this.jsonParser = new JsonParser();          
        }

        public async Task Start()
        {
            while (true)
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri($"wss://{endpoint}"), CancellationToken.None);

                WalletStartComplete?.Invoke(this, new EventArgs());

                try
                {
                    await ReceiveData().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    break;                   
                }
            }          
        }

        public async Task RegisterAccount(string account)
        {            
            var json = JsonConvert.SerializeObject(new { Account = account, Action = "account_subscribe", Currency = "USD" }, Formatting.None, jsonSerializerSettings);
            var bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task GetAccountHistory(string account, int count)
        {
            var json = JsonConvert.SerializeObject(new { Account = account, Action = "account_history", Count = count}, Formatting.None, jsonSerializerSettings);
            var bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task ReceiveData()
        {
            while (true)
            {
                ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);

                WebSocketReceiveResult result = null;

                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            var json = await reader.ReadToEndAsync();
                            RaiseReceivedData(json);
                        }
                    }
                }
            }
        }
        
        private void RaiseReceivedData(string json)
        {
            var walletEvent = jsonParser.ParseEvent(json);

            if (walletEvent != null)
            {
                switch (walletEvent)
                {
                    case ExchangeRateEvent typedEvent:
                        ReceivedExchangeRateEvent?.Invoke(this, new EventArgs<ExchangeRateEvent>(typedEvent));
                        break;
                    case AccountSummaryEvent typedEvent:
                        ReceivedAccountSummaryEvent?.Invoke(this, new EventArgs<AccountSummaryEvent>(typedEvent));
                        break;
                    case AccountHistoryEvent typedEvent:
                        ReceivedAccountHistoryEvent?.Invoke(this, new EventArgs<AccountHistoryEvent>(typedEvent));
                        break;
                    default:
                        break;
                }
            }
        }

        public event EventHandler<EventArgs<ExchangeRateEvent>> ReceivedExchangeRateEvent;
        public event EventHandler<EventArgs<AccountSummaryEvent>> ReceivedAccountSummaryEvent;
        public event EventHandler<EventArgs<AccountHistoryEvent>> ReceivedAccountHistoryEvent;

        public event EventHandler<EventArgs> WalletStartComplete;

    }
}
