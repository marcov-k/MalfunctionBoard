using FRC.NetworkTables;

namespace MalfunctionBoard.Utilities
{
    public static class NetworkTableReader
    {
        public static void StartReader(string tableName, MainPage mainPage)
        {
            var table = NetworkTableInstance.Default.GetTable(tableName);

            table.AddEntryListener((tbl, key, in entry, in value, flags) =>
            {
                var binding = key.ToString();
                var data = entry.GetObjectValue();

                MainThread.BeginInvokeOnMainThread(() =>
                    mainPage.UpdateDisplay(binding, data));
            },
            NotifyFlags.Immediate | NotifyFlags.New | NotifyFlags.Update | NotifyFlags.Local);
        }
    }
}
