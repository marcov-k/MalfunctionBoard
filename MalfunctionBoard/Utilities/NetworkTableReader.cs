using FRC.NetworkTables;

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

            Table?.AddEntryListener((tbl, key, in entry, in value, flags) =>
            {
                var binding = key.ToString();
                var data = entry.GetObjectValue();

                MainThread.BeginInvokeOnMainThread(() =>
                    MainPage?.UpdateDisplay(binding, data));
            },
            NotifyFlags.Immediate | NotifyFlags.New | NotifyFlags.Update | NotifyFlags.Local);
        }
    }
}
