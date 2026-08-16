using MalfunctionBoard.Records.Displays;
using MalfunctionBoard.Records.GridData;
using System.Text.Json;

namespace MalfunctionBoard.Utilities
{
    public static class Saver
    {
        const string FileName = "Layout.mb";
        static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

        public static void SaveLayout(MainPage mainPage)
        {
            var displays = mainPage.DisplayBindings.Select(b => b.Display).ToList();

            List<DisplayData> displayData = [];
            foreach (var display in displays)
            {
                displayData.Add(new(display.GetType().AssemblyQualifiedName, display.Title, display.Binding, display.Position, display.Dimensions));
            }

            LayoutData layoutData = new(displayData);
            string jsonData = JsonSerializer.Serialize(layoutData, SerializerOptions);
            File.WriteAllText(FileName, jsonData);
        }

        public static void LoadLayout(MainPage mainPage)
        {
            if (!File.Exists(FileName)) return;

            string jsonData = File.ReadAllText(FileName);
            var layoutData = JsonSerializer.Deserialize<LayoutData>(jsonData);

            if (layoutData is not null)
            {
                foreach (var display in layoutData.Displays)
                {
                    if (display.DisplayType is null) continue;

                    var displayType = Type.GetType(display.DisplayType);
                    if (displayType is null) continue;

                    var tryAddInfo = typeof(MainPage).GetMethod("TryAddDisplay");
                    var addDisplayMethod = tryAddInfo?.MakeGenericMethod(displayType);
                    if (addDisplayMethod is null) continue;

                    addDisplayMethod.Invoke(mainPage, [display.Title, display.Binding, display.Position, display.Dimensions]);
                }
            }
        }
    }
}
