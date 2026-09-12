using MalfunctionBoard.TableDatatypes;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MalfunctionBoard.Utilities
{
    public static class NetworkTableReader
    {
        static MainPage? MainPage;
        static readonly ConcurrentDictionary<int, string> IdToBinding = [];
        static readonly ConcurrentDictionary<string, object?> DataCache = [];

        public static void InitReader(MainPage mainPage)
        {
            MainPage = mainPage;
        }

        public static void AddKey(string name, int id)
        {
            name = name.Replace("/MalfunctionBoardTable/", string.Empty);
            IdToBinding[id] = name;
        }

        public static void DisplayEntry(string binding)
        {
            if (DataCache.TryGetValue(binding, out var data))
            {
                MainThread.BeginInvokeOnMainThread(() => MainPage?.UpdateDisplay(binding, data));
            }
        }

        public static void UpdateEntry(int id, string entryData)
        {
            if (IdToBinding.TryGetValue(id, out var binding))
            {
                var data = ExtractData(entryData);
                DataCache[binding] = data;
                MainThread.BeginInvokeOnMainThread(() => MainPage?.UpdateDisplay(binding, data));
            }
        }

        static object? ExtractData(string json)
        {
            using var doc = JsonDocument.Parse(json);

            object? data = null;
            if (doc.RootElement.TryGetProperty("Type", out var typeProp) && TableTypeRegistry.Registry.TryGetValue(typeProp.GetString() ?? string.Empty, out var deserializer))
            {
                data = deserializer(json);
            }
            return data;
        }
    }
}
