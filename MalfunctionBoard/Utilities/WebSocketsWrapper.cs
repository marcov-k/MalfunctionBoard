using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MalfunctionBoard.Utilities
{
    public static class WebSocketsWrapper
    {
        const int ConnectionTimeout = 1000;
        const int ReconnectCooldown = 200;
        const int ReaderRefresh = 20;
        static readonly Uri Uri = new("ws://10.96.68.2:5810/nt/MalfunctionBoardClient");
        static ClientWebSocket? ClientSocket;
        static readonly CancellationTokenSource ReconnectToken = new();
        static readonly JsonSerializerOptions JsonConfig = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        static MainPage? MainPage;
        
        public static async Task ConnectWebSocket(MainPage mainPage)
        {
            MainPage = mainPage;

            NT4HandshakePayload[] payload = [new()];
            string payloadJson = JsonSerializer.Serialize(payload, JsonConfig);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
            ArraySegment<byte> payloadByteBuffer = new(payloadBytes);

            while (!ReconnectToken.IsCancellationRequested)
            {
                if (ClientSocket is null || ClientSocket.State != WebSocketState.Open)
                {
                    UpdateConnectionStatus(false);

                    ClientSocket?.Dispose();
                    ClientSocket = new();
                    ClientSocket.Options.AddSubProtocol("v4.0.networktables.first.wpi.edu");

                    using (CancellationTokenSource timeoutTokenSource = new(ConnectionTimeout))
                    {
                        try
                        {
                            await ClientSocket.ConnectAsync(Uri, timeoutTokenSource.Token);
                            await ClientSocket.SendAsync(payloadByteBuffer, WebSocketMessageType.Text, true, timeoutTokenSource.Token);

                            _ = StartReadingLoop();

                            UpdateConnectionStatus(true);
                        }
                        catch (Exception) { }
                    }
                }

                await Task.Delay(ReconnectCooldown);
            }
        }

        static async Task StartReadingLoop()
        {
            var chunkBuffer = new byte[4096];

            try
            {
                while (ClientSocket?.State == WebSocketState.Open && !ReconnectToken.IsCancellationRequested)
                {
                    using var memoryStream = new MemoryStream();

                    bool endOfMessage = false;
                    while (!endOfMessage)
                    {
                        var result = await ClientSocket.ReceiveAsync(chunkBuffer, ReconnectToken.Token);

                        if (result.MessageType == WebSocketMessageType.Close) return;

                        memoryStream.Write(chunkBuffer, 0, result.Count);
                        endOfMessage = result.EndOfMessage;
                    }

                    string fullJsonMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var messages = JsonSerializer.Deserialize<NT4Message[]>(fullJsonMessage, JsonConfig);
                    if (messages is null) continue;

                    foreach (var message in messages)
                    {
                        switch (message.Method)
                        {
                            case "announce":
                                HandleAnnounce(message.Params);
                                break;
                            case "update":
                                HandleUpdate(message.Params);
                                break;
                        }
                    }

                    await Task.Delay(ReaderRefresh);
                }
            }
            catch (Exception) { }
        }

        static void UpdateConnectionStatus(bool status)
        {
            MainThread.BeginInvokeOnMainThread(() => MainPage?.Connected = status);
        }

        static void HandleAnnounce(JsonElement data)
        {
            var announce = JsonSerializer.Deserialize<NT4AnnounceParams>(data, JsonConfig);
            NetworkTableReader.AddKey(announce.Name, announce.Id);
        }

        static void HandleUpdate(JsonElement data)
        {
            var update = JsonSerializer.Deserialize<NT4UpdateParams>(data, JsonConfig);
            NetworkTableReader.UpdateEntry(update.Id, update.Val);
        }

        public static void CloseConnection() => ReconnectToken.Cancel();

        [Serializable]
        struct NT4HandshakePayload()
        {
            public string Action { get; set; } = "subscribe";
            public string[] Topics { get; set; } = ["/MalfunctionBoardTable"];
            public OptionsPayload Options { get; set; } = new();

            public struct OptionsPayload()
            {
                public bool Prefix { get; set; } = true;
                public bool All { get; set; } = true;
            }
        }

        struct NT4Message
        {
            public string Method { get; set; }
            public JsonElement Params { get; set; }
        }

        struct NT4AnnounceParams
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public string Type { get; set; }
        }

        struct NT4UpdateParams
        {
            public int Id { get; set; }
            public int Pubuid { get; set; }
            public JsonElement Val { get; set; }
        }
    }
}
