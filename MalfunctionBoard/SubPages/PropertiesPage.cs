using MalfunctionBoard.Displays;
using MalfunctionBoard.Exceptions;
using MalfunctionBoard.InputFields;
using MalfunctionBoard.Records.GridData;
using MalfunctionBoard.Utilities;

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
        readonly AddDisplayAction? AddDisplayMethod;

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

            if (ShownDisplay is null)
            {
                var addInfo = typeof(MainPage).GetMethod("AddDisplay");
                var genericAddInfo = addInfo?.MakeGenericMethod(displayType);
                AddDisplayMethod = (AddDisplayAction)Delegate.CreateDelegate(typeof(AddDisplayAction), MainPage, genericAddInfo!);
            }

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

        void Confirm()
        {
            try
            {
                if (string.IsNullOrEmpty(DisplayTitle)) throw new MissingPropertyException("Title");
                if (string.IsNullOrEmpty(DisplayBinding)) throw new MissingPropertyException("Binding");
                if (DisplayPosition is null) throw new MissingPropertyException("Position");
                if (DisplayDimensions is null) throw new MissingPropertyException("Dimensions");

                if (ShownDisplay is null) AddDisplayMethod!(DisplayTitle, DisplayBinding, DisplayPosition, DisplayDimensions);
                else MainPage.ChangeDisplay(ShownDisplay, DisplayTitle, DisplayBinding, DisplayPosition, DisplayDimensions);

                Saver.SaveLayout(MainPage);
                Close();
            }
            catch (Exception e)
            {
                WarningPage.ShowWarning(FormatWarning(e), Window);
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

        string FormatWarning(Exception e) => (ShownDisplay is null ? "Could Not Add Display" : "Could Not Update Display") + $"\n({e.Message})";
    }
}
