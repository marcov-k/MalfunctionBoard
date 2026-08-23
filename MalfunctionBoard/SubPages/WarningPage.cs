using MalfunctionBoard.Utilities;

namespace MalfunctionBoard.SubPages
{
    public partial class WarningPage : ContentPage
    {
        const double WindowWidth = 600;
        const double WindowHeight = 250;
        const double WarningTextSize = 25;
        const double ButtonWidth = 100;
        static readonly Color WarningTextColor = Colors.DarkRed;

        public static void ShowWarning(string warning, Window propertiesWindow)
        {
            var warningWindow = new Window(new WarningPage(warning));
            Application.Current?.OpenWindow(warningWindow);

            WindowUtils.MakeModalWindow(warningWindow, propertiesWindow);
        }

        public WarningPage(string warning)
        {
            Title = "Warning";
            BackgroundColor = PropertiesPage.WindowColor;

            VerticalStackLayout pageLayout = new()
            {
                Spacing = PropertiesPage.VerticalSpacing,
                Margin = PropertiesPage.Margin,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };

            var warningLabel = new Label()
            {
                Text = warning,
                FontSize = WarningTextSize,
                TextColor = WarningTextColor,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Fill
            };
            pageLayout.Add(warningLabel);

            Button closeButton = new()
            {
                Text = "Ok",
                FontSize = PropertiesPage.TextSize,
                BackgroundColor = PropertiesPage.ConfirmColor,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = ButtonWidth,
                VerticalOptions = LayoutOptions.Fill
            };

            PointerGestureRecognizer closeGesture = new();
            closeGesture.PointerEntered += (_, _) => closeButton.BackgroundColor = PropertiesPage.ConfirmHoverColor;
            closeGesture.PointerExited += (_, _) => closeButton.BackgroundColor = PropertiesPage.ConfirmColor;

            closeButton.GestureRecognizers.Add(closeGesture);
            closeButton.Clicked += (_, _) => Close();

            pageLayout.Add(closeButton);

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

                Window.X = (screenWidth / 2) - (WindowWidth / 2);
                Window.Y = (screenHeight / 2) - (WindowHeight / 2);
            }
        }

        void Close() => Application.Current?.CloseWindow(Window);
    }
}
