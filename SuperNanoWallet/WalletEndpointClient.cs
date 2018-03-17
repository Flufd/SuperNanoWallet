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
        class OverrideNamingStrategy : SnakeCaseNamingStrategy
        {
            public string GetPropertyName(string name) => this.ResolvePropertyName(name);
        }

        private readonly string endpoint;
        private Dictionary<string, Type> models;
        private ClientWebSocket webSocket;
        private OverrideNamingStrategy overrideNamingStrategy;

        public WalletEndpointClient(string endpoint)
        {
            this.endpoint = endpoint;

            overrideNamingStrategy = new OverrideNamingStrategy();

            // Register event models
            models = new Dictionary<string, Type>();
            models.Add(GetPropertyNames(typeof(ExchangeRateEvent)), typeof(ExchangeRateEvent));

        }

        public async Task Start()
        {
            webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri($"wss://{endpoint}"), CancellationToken.None);

            //Task.Run(async () => await ReceiveData());
            try
            {
                await ReceiveData().ConfigureAwait(false);
            }
            catch (Exception e)
            {

            }

            //while (true)
            //{
            //    Console.ReadLine();

            //    var text = "{\"account\":\"xrb_3iuommeb5gpnhxbsubeswmbg5rdydfstzpih9qgmmfcnqmxs1bzeootdqryq\",\"action\":\"account_subscribe\",\"currency\":\"USD\"}";
            //    var bytes = Encoding.UTF8.GetBytes(text);
            //    await x.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            //}
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

        private string GetPropertyNames(Type type)
        {
            return String.Join("", type.GetProperties().Select(a => overrideNamingStrategy.GetPropertyName(a.Name)).OrderBy(a => a));
        }

        private void RaiseReceivedData(string json)
        {

            var jsonObject = JObject.Parse(json);

            // What kind of object do we have?
            var typeSignature = String.Join("", jsonObject.Properties().Select(a => a.Name).OrderBy(a => a));

            if (models.TryGetValue(typeSignature, out Type type))
            {
                switch (true)
                {
                    case bool _ when type == typeof(ExchangeRateEvent):
                        ReceivedExchangeRateEvent?.Invoke(this, new EventArgs<ExchangeRateEvent>(jsonObject.ToObject<ExchangeRateEvent>()));
                        break;
                    default:
                        break;
                }
            }
        }

        public event EventHandler<EventArgs<ExchangeRateEvent>> ReceivedExchangeRateEvent;
    }
}
