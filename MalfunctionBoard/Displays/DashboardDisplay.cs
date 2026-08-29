using MalfunctionBoard.Interfaces;
using MalfunctionBoard.Records.GridData;
using MalfunctionBoard.SubPages;
using Microsoft.Maui.Layouts;

namespace MalfunctionBoard.Displays
{
    public abstract partial class DashboardDisplay : ContentView, IHasGridDims, IHasGridPos
    {
        public required MainPage? MyPage { get; set; }
        public GridPos Position { get; set; } = new() { Row = 0, Col = 0 };
        public virtual GridDims Dimensions { get; set; } = new() { Width = 1, Height = 1 };
        public string Binding { get; set; } = string.Empty;
        public string Title
        {
            get => ((string)GetValue(TitleProperty))[..^1];
            set => SetValue(TitleProperty, value + ":");
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(DashboardDisplay), string.Empty);
        public double TitleSize
        {
            get => (double)GetValue(TitleSizeProperty);
            set => SetValue(TitleSizeProperty, value);
        }
        public static readonly BindableProperty TitleSizeProperty =
            BindableProperty.Create(nameof(TitleSize), typeof(double), typeof(DashboardDisplay), 30.0);
        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(DashboardDisplay), Colors.Black);
        protected AbsoluteLayout MyLayout;

        public DashboardDisplay()
        {
            BackgroundColor = MainPage.CellColor;
            Title = "New Display";

            PointerGestureRecognizer clickGesture = new();
            clickGesture.PointerPressed += (_, _) => ChangeDisplay();
            GestureRecognizers.Add(clickGesture);

            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;

            var title = new Label();
            title.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));
            title.SetBinding(Label.FontSizeProperty, new Binding(nameof(TitleSize), source: this));
            title.SetBinding(Label.TextColorProperty, new Binding(nameof(TextColor), source: this));

            MyLayout = [];

            MyLayout.SetLayoutBounds(title, new(0.5, 0, -1, -1));
            MyLayout.SetLayoutFlags(title, AbsoluteLayoutFlags.PositionProportional);

            MyLayout.Children.Add(title);

            Content = MyLayout;
        }

        void ChangeDisplay()
        {
            if (MyPage is not null) PropertiesPage.OpenPropertiesPage(GetType(), MyPage, this);
        }
    }
}
