using MalfunctionBoard.Exceptions;
using MalfunctionBoard.Records;
using MalfunctionBoard.Records.Displays;
using MalfunctionBoard.Records.GridData;
using MalfunctionBoard.SubPages;
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
            SaveData saveData = new(layoutData, NetworkTableReader.TableName);
            string jsonData = JsonSerializer.Serialize(saveData, SerializerOptions);
            File.WriteAllText(FileName, jsonData);
        }

        public static void LoadLayout(MainPage mainPage)
        {
            if (!File.Exists(FileName)) return;

            string jsonData = File.ReadAllText(FileName);
            var saveData = JsonSerializer.Deserialize<SaveData>(jsonData);

            bool incompleteLoading = false;
            if (saveData is not null)
            {
                var layoutData = saveData.Layout;

                foreach (var display in layoutData.Displays)
                {
                    try
                    {
                        if (display.DisplayType is null) throw new DisplayLoadingException();

                        var displayType = Type.GetType(display.DisplayType) ?? throw new DisplayLoadingException();

                        var addInfo = typeof(MainPage).GetMethod("AddDisplay") ?? throw new DisplayLoadingException();

                        var genericAddInfo = addInfo.MakeGenericMethod(displayType);

                        var addDisplayMethod = (AddDisplayAction)Delegate.CreateDelegate(typeof(AddDisplayAction), mainPage, genericAddInfo)
                            ?? throw new DisplayLoadingException();

                        addDisplayMethod(display.Title, display.Binding, display.Position, display.Dimensions);
                    }
                    catch (Exception)
                    {
                        incompleteLoading = true;
                    }
                }

                NetworkTableReader.TableName = saveData.TableName;
            }
            else WarningPage.ShowWarning("Failed To Load Previous Layout", mainPage.Window);

            if (incompleteLoading) WarningPage.ShowWarning("Could Not Fully Load Previous Layout", mainPage.Window);
        }
    }
}
