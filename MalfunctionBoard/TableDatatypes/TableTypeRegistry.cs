using System.Text.Json;

namespace MalfunctionBoard.TableDatatypes
{
    static class TableTypeRegistry
    {
        public static readonly Dictionary<string, Func<string, object?>> Registry = new()
        {
            ["Int"] = json => JsonSerializer.Deserialize<MBInt>(json),
            ["Double"] = json => JsonSerializer.Deserialize<MBDouble>(json),
            ["String"] = json => JsonSerializer.Deserialize<MBString>(json),
            ["Bool"] = json => JsonSerializer.Deserialize<MBBool>(json)
        };
    }
}
