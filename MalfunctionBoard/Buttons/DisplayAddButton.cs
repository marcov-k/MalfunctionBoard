using MalfunctionBoard.SubPages;

namespace MalfunctionBoard.Buttons
{
    public partial class DisplayAddButton : Button
    {
        readonly Type DisplayType;
        readonly MainPage MyPage;

        public DisplayAddButton(Type displayType, MainPage page)
        {
            Text = $"Add {displayType.Name}";
            DisplayType = displayType;
            MyPage = page;
            Clicked += (_, _) => OpenProperties();
        }

        void OpenProperties() => PropertiesPage.OpenPropertiesPage(DisplayType, MyPage);
    }
}
