using MessagePack;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MalfunctionBoard.Utilities
{
    public static class WebSocketsWrapper
    {
        const int ConnectionTimeout = 3000;
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
                    ClientSocket.Options.AddSubProtocol("networktables.first.wpi.edu");

                    try
                    {
                        using CancellationTokenSource connectTimeout = new(ConnectionTimeout);
                        await ClientSocket.ConnectAsync(Uri, connectTimeout.Token);

                        using CancellationTokenSource handshakeTimeout = new(ConnectionTimeout);
                        await ClientSocket.SendAsync(payloadByteBuffer, WebSocketMessageType.Text, true, handshakeTimeout.Token);

                        _ = StartReadingLoop();

                        UpdateConnectionStatus(true);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                        ClientSocket?.Dispose();
                        ClientSocket = null;
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
                    var frameType = WebSocketMessageType.Text;

                    bool endOfMessage = false;
                    while (!endOfMessage)
                    {
                        var result = await ClientSocket.ReceiveAsync(chunkBuffer, ReconnectToken.Token);

                        if (result.MessageType == WebSocketMessageType.Close) return;

                        frameType = result.MessageType;
                        memoryStream.Write(chunkBuffer, 0, result.Count);
                        endOfMessage = result.EndOfMessage;
                    }

                    try
                    {
                        if (frameType == WebSocketMessageType.Text)
                        {
                            string json = Encoding.UTF8.GetString(memoryStream.ToArray());
                            var messages = JsonSerializer.Deserialize<NT4Message[]>(json, JsonConfig);
                            if (messages is null) continue;

                            foreach (var message in messages)
                            {
                                switch (message.Method)
                                {
                                    case "announce":
                                        HandleAnnounce(message.Params);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            HandleBinaryUpdate(memoryStream.ToArray());
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }

                    await Task.Delay(ReaderRefresh);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
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

        static void HandleBinaryUpdate(byte[] data)
        {
            var reader = new MessagePackReader(data);
            while (!reader.End)
            {
                _ = reader.ReadArrayHeader(); // should be 4: id, timestamp, type, value
                int id = reader.ReadInt32();
                reader.Skip(); // skip timestamp
                reader.Skip(); // skip type - should always be "string"

                string? value = reader.ReadString();
                if (value is not null)
                {
                    System.Diagnostics.Debug.WriteLine(value);
                    NetworkTableReader.UpdateEntry(id, value);
                }
            }
        }

        public static void CloseConnection() => ReconnectToken.Cancel();

        [Serializable]
        struct NT4HandshakePayload()
        {
            public string Method { get; set; } = "subscribe";
            public string[] Topics { get; set; } = ["/MalfunctionBoardTable"];
            public ParamsPayload Params { get; set; } = new();

            [Serializable]
            public struct ParamsPayload()
            {
                public string[] Topics { get; set; } = ["/MalfunctionBoardTable"];
                public int Subuid { get; set; } = 1;
                public OptionsPayload Options { get; set; } = new();

                [Serializable]
                public struct OptionsPayload()
                {
                    public bool Prefix { get; set; } = true;
                    public bool All { get; set; } = true;
                }
            }
        }

        [Serializable]
        struct NT4Message
        {
            public string Method { get; set; }
            public JsonElement Params { get; set; }
        }

        [Serializable]
        struct NT4AnnounceParams
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public string Type { get; set; }
        }
    }
}
