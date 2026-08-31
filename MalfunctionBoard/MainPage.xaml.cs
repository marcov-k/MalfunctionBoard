using FRC.NetworkTables;
using MalfunctionBoard.Buttons;
using MalfunctionBoard.Displays;
using MalfunctionBoard.Exceptions;
using MalfunctionBoard.Interfaces;
using MalfunctionBoard.Records.Displays;
using MalfunctionBoard.Records.GridData;
using MalfunctionBoard.SubPages;
using MalfunctionBoard.Utilities;
using Microsoft.Maui.Devices;

namespace MalfunctionBoard
{
    public partial class MainPage : ContentPage
    {
        readonly Grid MainGrid;
        readonly List<GridDisplay> GridDisplays = [];
        public readonly List<DisplayBinding> DisplayBindings = [];
        readonly List<Type> DisplayTypes = [];
        internal const int RowCount = 4;
        internal const int ColumnCount = 6;
        const double RowSpacing = 10;
        const double ColumnSpacing = 10;
        static readonly Thickness GridMargin = new(10);
        const double AddButtonSpacing = 0;
        static readonly Thickness AddButtonMargin = new(10);
        const double SettingsTextSize = 27.5;
        static readonly Thickness SettingsButtonMargin = new(10, 0, 0, 10);
        internal static readonly Color CellColor = Colors.Gray;
        static readonly Color GridColor = Colors.LightGray;
        static readonly Color PageColor = Colors.DarkGray;
        static readonly Color SettingsColor = Colors.DarkBlue;
        static readonly Color SettingsHoverColor = Colors.Blue;
        const double SettingsButtonWidth = 150;

        public MainPage()
        {
            InitDisplayTypes();

            BackgroundColor = PageColor;

            Grid pageLayout = new()
            {
                RowDefinitions =
                {
                    new() { Height = new(1, GridUnitType.Star) },
                    new() { Height = new(10, GridUnitType.Star) },
                    new() { Height = new(0.75, GridUnitType.Star) }
                },
                ColumnDefinitions =
                {
                    new() { Width = new(1, GridUnitType.Star) }
                },
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            HorizontalStackLayout topBar = new()
            {
                BackgroundColor = CellColor,
                Spacing = AddButtonSpacing,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            foreach (var displayType in DisplayTypes)
            {
                var button = new DisplayAddButton(displayType, this)
                {
                    Margin = AddButtonMargin
                };

                topBar.Add(button);
            }

            MainGrid = new()
            {
                RowDefinitions = {},
                RowSpacing = RowSpacing,
                ColumnDefinitions = {},
                ColumnSpacing = ColumnSpacing,
                Margin = GridMargin,
                BackgroundColor = GridColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            for (int i = 0; i < RowCount; i++)
            {
                MainGrid.RowDefinitions.Add(new()
                {
                    Height = new(1, GridUnitType.Star)
                });
            }
            for (int i = 0; i < ColumnCount; i++)
            {
                MainGrid.ColumnDefinitions.Add(new()
                {
                    Width = new(1, GridUnitType.Star)
                });
            }

            Button settingsButton = new()
            {
                Text = "Settings",
                FontSize = SettingsTextSize,
                BackgroundColor = SettingsColor,
                Margin = SettingsButtonMargin,
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = SettingsButtonWidth,
                VerticalOptions = LayoutOptions.Fill
            };

            PointerGestureRecognizer settingsGesture = new();
            settingsGesture.PointerEntered += (_, _) => settingsButton.BackgroundColor = SettingsHoverColor;
            settingsGesture.PointerExited += (_, _) => settingsButton.BackgroundColor = SettingsColor;

            settingsButton.GestureRecognizers.Add(settingsGesture);
            settingsButton.Clicked += (_, _) => SettingsPage.OpenSettings(Window, this);

            pageLayout.Add(topBar, 0, 0);
            pageLayout.Add(MainGrid, 0, 1);
            pageLayout.Add(settingsButton, 0, 2);
            Content = pageLayout;

            Loaded += OnPageLoaded;
        }

        void OnPageLoaded(object? sender, EventArgs e)
        {
            if (Window != null)
            {
                var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

                if (displayInfo.Density > 0)
                {
                    double screenWidth = displayInfo.Width / displayInfo.Density;
                    double screenHeight = displayInfo.Height / displayInfo.Density - 50;

                    (Window.X, Window.Y) = (0, 0);
                    (Window.Width, Window.Height) = (screenWidth, screenHeight);

                    #if MACCATALYST
                    (Window.MinimumWidth, Window.MaximumWidth) = (screenWidth, screenWidth);
                    (Window.MinimumHeight, Window.MaximumHeight) = (screenHeight, screenHeight);
                    #endif
                }
            }

            Saver.LoadLayout(this);

            NetworkTableReader.InitReader(this);
        }

        public void AddDisplay<T>(string title, string binding, GridPos position, GridDims dimensions)
            where T : DashboardDisplay, IHasGridDims, IHasGridPos, new()
        {
            if (!ValidBinding(binding)) throw new InvalidBindingException(binding);

            if (ValidPosition(dimensions, position, out var positions))
            {
                T display = new() { MyPage = this, Title = title, Binding = binding };
                GridDisplays.AddRange(positions.Select(p => new GridDisplay(p, display)));

                display.Position = positions[0];
                display.Dimensions = dimensions;
                MainGrid.SetRow(display, positions[0].Row);
                MainGrid.SetRowSpan(display, dimensions.Height);
                MainGrid.SetColumn(display, positions[0].Col);
                MainGrid.SetColumnSpan(display, dimensions.Width);

                DisplayBindings.Add(new(binding, display));

                MainGrid.Add(display);
            }
            else throw new InvalidPositionException(position, dimensions);
        }

        public void ChangeDisplay(DashboardDisplay display, string newTitle, string newBinding, GridPos newPos, GridDims newDims)
        {
            if (!ValidBinding(newBinding, display)) throw new InvalidBindingException(newBinding);

            if (ValidPosition(newDims, newPos, out var positions, display))
            {
                DisplayBindings.RemoveAll(b => b.Display == display);
                DisplayBindings.Add(new(newBinding, display));

                GridDisplays.RemoveAll(d => d.Display == display);
                GridDisplays.AddRange(positions.Select(p => new GridDisplay(p, display)));

                MainGrid.Remove(display);

                display.Title = newTitle;
                display.Binding = newBinding;
                display.Position = positions[0];
                display.Dimensions = newDims;
                MainGrid.SetRow(display, positions[0].Row);
                MainGrid.SetRowSpan(display, newDims.Height);
                MainGrid.SetColumn(display, positions[0].Col);
                MainGrid.SetColumnSpan(display, newDims.Width);

                MainGrid.Add(display);
            }
            else throw new InvalidPositionException(newPos, newDims);
        }

        public void UpdateDisplay(string binding, object? data)
        {
            if (data is null) return;

            var display = DisplayBindings.Find(b => b.Binding == binding)?.Display;
            if (display is ITableValue valueDisplay) valueDisplay.TableValue = data;
        }

        public void RemoveDisplay(DashboardDisplay display)
        {
            DisplayBindings.RemoveAll(b => b.Display == display);
            GridDisplays.RemoveAll(d => d.Display == display);
            MainGrid.Remove(display);
        }

        public bool ValidPosition(GridDims dimensions, GridPos start, out List<GridPos> positions, DashboardDisplay? display = null)
        {
            var gridDisplays = GridDisplays.Where(d => d.Display != display);
            positions = [];

            for (int row = 0; row < dimensions.Height; row++)
            {
                for (int col = 0; col < dimensions.Width; col++)
                {
                    positions.Add(new() { Row = start.Row + row, Col = start.Col + col });
                }
            }

            foreach (var pos in positions)
            {
                if (gridDisplays.Any(d => d.Position == pos)) return false;
            }

            return true;
        }

        public bool ValidBinding(string binding, DashboardDisplay? display = null) => !DisplayBindings.Any(b => b.Binding == binding && b.Display != display);

        void InitDisplayTypes()
        {
            var displayTypes = typeof(DashboardDisplay).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DashboardDisplay)) && t.IsAssignableTo(typeof(ICreatable)));

            DisplayTypes.AddRange(displayTypes);
        }
    }
}
