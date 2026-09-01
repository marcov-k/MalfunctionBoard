using FRC.NetworkTables;
using MalfunctionBoard.TableDatatypes;
using System.Text.Json;

namespace MalfunctionBoard.Utilities
{
    public static class NetworkTableReader
    {
        public static string TableName
        {
            get => _tableName;
            set
            {
                _tableName = value;
                UpdateTable();
            }
        }
        static string _tableName = string.Empty;
        static NetworkTable? Table;
        static MainPage? MainPage;

        public static void InitReader(MainPage mainPage)
        {
            MainPage = mainPage;
        }

        static void UpdateTable()
        {
            Table = NetworkTableInstance.Default.GetTable(TableName);

            Table.AddEntryListener((tbl, key, in entry, in value, flags) =>
            {
                var binding = key.ToString();

                var data = ExtractData(entry);
                MainThread.BeginInvokeOnMainThread(() => MainPage?.UpdateDisplay(binding, data));
            },
            NotifyFlags.Immediate | NotifyFlags.New | NotifyFlags.Update | NotifyFlags.Local);
        }

        public static void ReadBinding(string binding)
        {
            if (Table is null) return;

            var entry = Table.GetEntry(binding);
            if (string.IsNullOrEmpty(entry.GetString(string.Empty))) return;

            var data = ExtractData(entry);
            MainPage?.UpdateDisplay(binding, data);
        }

        static object? ExtractData(NetworkTableEntry entry)
        {
            string json = entry.GetString(string.Empty);
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
