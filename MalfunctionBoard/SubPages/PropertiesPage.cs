using MalfunctionBoard.Displays;
using MalfunctionBoard.InputFields;
using MalfunctionBoard.Records.GridData;
using MalfunctionBoard.Utilities;
using System.Reflection;

namespace MalfunctionBoard.SubPages
{
    public partial class PropertiesPage : ContentPage
    {
        const double WindowWidth = 500;
        const double WindowHeight = 400;
        internal const double VerticalSpacing = 10;
        const double ButtonSpacing = 10;
        internal const double TextSize = 20;
        internal static readonly Thickness Margin = new(10);
        internal static readonly Color WindowColor = Colors.DarkGray;
        internal static readonly Color ConfirmColor = Colors.Green;
        internal static readonly Color ConfirmHoverColor = Colors.DarkGreen;
        static readonly Color CancelColor = Colors.SlateGray;
        static readonly Color CancelHoverColor = Colors.DarkSlateGray;
        static readonly Color DeleteColor = Colors.Red;
        static readonly Color DeleteHoverColor = Colors.DarkRed;
        public string DisplayTitle
        {
            get => (string)GetValue(DisplayTitleProperty);
            set => SetValue(DisplayTitleProperty, value);
        }
        public static readonly BindableProperty DisplayTitleProperty =
            BindableProperty.Create(nameof(DisplayTitle), typeof(string), typeof(PropertiesPage), string.Empty);
        public string DisplayBinding
        {
            get => (string)GetValue(DisplayBindingProperty);
            set => SetValue(DisplayBindingProperty, value);
        }
        public static readonly BindableProperty DisplayBindingProperty =
            BindableProperty.Create(nameof(DisplayBinding), typeof(string), typeof(PropertiesPage), string.Empty);
        public GridPos DisplayPosition
        {
            get => (GridPos)GetValue(DisplayPositionProperty);
            set => SetValue(DisplayPositionProperty, value);
        }
        public static readonly BindableProperty DisplayPositionProperty =
            BindableProperty.Create(nameof(DisplayPosition), typeof(GridPos), typeof(PropertiesPage), new GridPos());
        public GridDims DisplayDimensions
        {
            get => (GridDims)GetValue(DisplayDimensionsProperty);
            set => SetValue(DisplayDimensionsProperty, value);
        }
        public static readonly BindableProperty DisplayDimensionsProperty =
            BindableProperty.Create(nameof(DisplayDimensions), typeof(GridDims), typeof(PropertiesPage), new GridDims());
        readonly MainPage MainPage;
        readonly DashboardDisplay? ShownDisplay;
        readonly MethodInfo? AddDisplayMethod;

        public static void OpenPropertiesPage(Type displayType, MainPage mainPage, DashboardDisplay? display = null)
        {
            var propertiesWindow = new Window(new PropertiesPage(displayType, mainPage, display));
            Application.Current?.OpenWindow(propertiesWindow);

            WindowUtils.MakeModalWindow(propertiesWindow, mainPage.Window);
        }

        public PropertiesPage(Type displayType, MainPage mainPage, DashboardDisplay? display = null)
        {
            Title = "Display Properties";
            BackgroundColor = WindowColor;
            MainPage = mainPage;
            ShownDisplay = display;

            VerticalStackLayout pageLayout = new()
            {
                Spacing = VerticalSpacing,
                Margin = Margin,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var typeLabel = new Label()
            {
                Text = $"Type: {displayType.Name}",
                FontSize = TextSize,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            pageLayout.Add(typeLabel);

            var titleEntry = new InputField()
            {
                Title = "Name",
                FirstPlaceholder = "Enter name..."
            };
            titleEntry.SetBinding(InputField.FirstInputProperty, new Binding(nameof(DisplayTitle),
                mode: BindingMode.TwoWay, source: this));
            if (ShownDisplay is not null) titleEntry.FirstInput = ShownDisplay.Title;
            pageLayout.Add(titleEntry);

            var bindingEntry = new InputField()
            {
                Title = "Binding",
                FirstPlaceholder = "Enter binding..."
            };
            bindingEntry.SetBinding(InputField.FirstInputProperty, new Binding(nameof(DisplayBinding),
                mode: BindingMode.TwoWay, source: this));
            if (ShownDisplay is not null) bindingEntry.FirstInput = ShownDisplay.Binding;
            pageLayout.Add(bindingEntry);

            var posEntry = new Vector2InputField<GridPos>()
            {
                Title = "Position",
                FirstPlaceholder = "Enter row...",
                SecondPlaceholder = "Enter column..."
            };
            posEntry.SetBinding(Vector2InputField<GridPos>.VectorProperty, new Binding(nameof(DisplayPosition),
                mode: BindingMode.TwoWay, source: this));
            (posEntry.FirstInput, posEntry.SecondInput) = ShownDisplay is null ? ("0", "0") : (ShownDisplay.Position.X.ToString(), ShownDisplay.Position.Y.ToString());
            pageLayout.Add(posEntry);

            var dimsEntry = new Vector2InputField<GridDims>()
            {
                Title = "Size",
                FirstPlaceholder = "Enter width...",
                SecondPlaceholder = "Enter height..."
            };
            dimsEntry.SetBinding(Vector2InputField<GridDims>.VectorProperty, new Binding(nameof(DisplayDimensions),
                mode: BindingMode.TwoWay, source: this));
            (dimsEntry.FirstInput, dimsEntry.SecondInput) = ShownDisplay is null ? ("1", "1") : (ShownDisplay.Dimensions.Width.ToString(), ShownDisplay.Dimensions.Height.ToString());
            pageLayout.Add(dimsEntry);

            HorizontalStackLayout buttonLayout = new()
            {
                Spacing = ButtonSpacing,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Fill
            };

            if (ShownDisplay is not null)
            {
                Button deleteButton = new()
                {
                    Text = "Delete",
                    FontSize = TextSize,
                    BackgroundColor = DeleteColor,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };

                PointerGestureRecognizer deleteGesture = new();
                deleteGesture.PointerEntered += (_, _) => deleteButton.BackgroundColor = DeleteHoverColor;
                deleteGesture.PointerExited += (_, _) => deleteButton.BackgroundColor = DeleteColor;

                deleteButton.GestureRecognizers.Add(deleteGesture);
                deleteButton.Clicked += (_, _) => Delete();

                buttonLayout.Add(deleteButton);
            }

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
            cancelButton.Clicked += (_, _) => Close();

            buttonLayout.Add(cancelButton);

            Button confirmButton = new()
            {
                Text = "Confirm",
                FontSize = TextSize,
                BackgroundColor = ConfirmColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            PointerGestureRecognizer confirmGesture = new();
            confirmGesture.PointerEntered += (_, _) => confirmButton.BackgroundColor = ConfirmHoverColor;
            confirmGesture.PointerExited += (_, _) => confirmButton.BackgroundColor = ConfirmColor;

            confirmButton.GestureRecognizers.Add(confirmGesture);
            confirmButton.Clicked += (_, _) => Confirm();

            buttonLayout.Add(confirmButton);

            pageLayout.Add(buttonLayout);

            Content = pageLayout;

            var tryAddInfo = typeof(MainPage).GetMethod("TryAddDisplay");
            AddDisplayMethod = tryAddInfo?.MakeGenericMethod(displayType);

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

        void Confirm()
        {
            dynamic? success = false;
            if (!string.IsNullOrEmpty(DisplayTitle) && !string.IsNullOrEmpty(DisplayBinding) && DisplayPosition is not null && DisplayDimensions is not null)
            {
                if (ShownDisplay is null) success = AddDisplayMethod?.Invoke(MainPage, [DisplayTitle, DisplayBinding, DisplayPosition, DisplayDimensions]);
                else success = MainPage.TryChangeDisplay(ShownDisplay, DisplayTitle, DisplayBinding, DisplayPosition, DisplayDimensions);
            }

            if (success is not null && success)
            {
                Saver.SaveLayout(MainPage);
                Close();
            }
            else
            {
                string warning = string.Empty;

                if (string.IsNullOrEmpty(DisplayTitle)) warning = "Missing Display Title";
                else if (string.IsNullOrEmpty(DisplayBinding)) warning = "Missing Display Binding";
                else if (!MainPage.ValidBinding(DisplayBinding, ShownDisplay)) warning = "Invalid Binding - binding already exists";
                else if (DisplayPosition is null) warning = "Missing Display Position";
                else if (DisplayDimensions is null) warning = "Missing Display Dimensions";
                else if (!MainPage.ValidPosition(DisplayDimensions, DisplayPosition, out _, ShownDisplay)) warning = "Invalid Position - position already occupied";

                WarningPage.ShowWarning(warning, Window);
            }
        }

        void Delete()
        {
            if (ShownDisplay is not null)
            {
                MainPage.RemoveDisplay(ShownDisplay);
                Saver.SaveLayout(MainPage);
            }
            Close();
        }

        void Close() => Application.Current?.CloseWindow(Window);
    }
}
