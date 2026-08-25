using MalfunctionBoard.InputFields;
using MalfunctionBoard.Utilities;

namespace MalfunctionBoard.SubPages
{
    public partial class SettingsPage : ContentPage
    {
        const double WindowWidth = 700;
        const double WindowHeight = 200;
        const double VerticalSpacing = 10;
        const double ButtonSpacing = 10;
        const double TextSize = 20;
        static readonly Thickness Margin = new(10);
        static readonly Color WindowColor = Colors.DarkGray;
        static readonly Color ApplyColor = Colors.Green;
        static readonly Color ApplyHoverColor = Colors.DarkGreen;
        static readonly Color CancelColor = Colors.SlateGray;
        static readonly Color CancelHoverColor = Colors.DarkSlateGray;
        public string TableName
        {
            get => (string)GetValue(TableNameProperty);
            set => SetValue(TableNameProperty, value);
        }
        public static readonly BindableProperty TableNameProperty =
            BindableProperty.Create(nameof(TableName), typeof(string), typeof(SettingsPage), string.Empty);
        readonly MainPage MainPage;

        public static void OpenSettings(Window parentWindow, MainPage mainPage)
        {
            var settingsWindow = new Window(new SettingsPage(mainPage));
            Application.Current?.OpenWindow(settingsWindow);

            WindowUtils.MakeModalWindow(settingsWindow, parentWindow);
        }

        public SettingsPage(MainPage mainPage)
        {
            Title = "Settings";
            BackgroundColor = WindowColor;
            MainPage = mainPage;

            VerticalStackLayout pageLayout = new()
            {
                Spacing = VerticalSpacing,
                Margin = Margin,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var tableNameEntry = new InputField()
            {
                Title = "Network Table",
                FirstPlaceholder = "Enter network table name..."
            };
            tableNameEntry.SetBinding(InputField.FirstInputProperty, new Binding(nameof(TableName),
                mode: BindingMode.TwoWay, source: this));
            tableNameEntry.FirstInput = NetworkTableReader.TableName;
            pageLayout.Add(tableNameEntry);

            HorizontalStackLayout buttonLayout = new()
            {
                Spacing = ButtonSpacing,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Fill
            };

            Button cancelButton = new()
            {
                Text = "Cancel",
                FontSize = TextSize,
                BackgroundColor = CancelColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            PointerGestureRecognizer cancelGesture = new();
            cancelGesture.PointerEntered += (_, _) => cancelButton.BackgroundColor = CancelHoverColor;
            cancelGesture.PointerExited += (_, _) => cancelButton.BackgroundColor = CancelColor;

            cancelButton.GestureRecognizers.Add(cancelGesture);
            cancelButton.Clicked += (_, _) => Close(false);

            buttonLayout.Add(cancelButton);

            Button applyButton = new()
            {
                Text = "Apply",
                FontSize = TextSize,
                BackgroundColor = ApplyColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            PointerGestureRecognizer applyGesture = new();
            applyGesture.PointerEntered += (_, _) => applyButton.BackgroundColor = ApplyHoverColor;
            applyGesture.PointerExited += (_, _) => applyButton.BackgroundColor = ApplyColor;

            applyButton.GestureRecognizers.Add(applyGesture);
            applyButton.Clicked += (_, _) => Close(true);

            buttonLayout.Add(applyButton);

            pageLayout.Add(buttonLayout);

            Content = pageLayout;

            Loaded += OnPageLoaded;
        }

        void OnPageLoaded(object? sender, EventArgs e)
        {
            Window.Title = Title;
            (Window.Width, Window.Height) = (WindowWidth, WindowHeight);

            #if MACCATALYST
            (Window.MinimumWidth, Window.MaximumWidth) = (WindowWidth, WindowWidth);
            (Window.MinimumHeight, Window.MaximumHeight) = (WindowHeight, WindowHeight);
            #endif

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            if (displayInfo.Density > 0)
            {
                double screenWidth = displayInfo.Width / displayInfo.Density;
                double screenHeight = displayInfo.Height / displayInfo.Density;

                Window.X = (screenWidth - WindowWidth) / 2;
                Window.Y = (screenHeight - WindowHeight) / 2;
            }
        }

        void Close(bool apply)
        {
            if (apply)
            {
                NetworkTableReader.TableName = TableName;
                Saver.SaveLayout(MainPage);
            }

            Application.Current?.CloseWindow(Window);
        }
    }
}
