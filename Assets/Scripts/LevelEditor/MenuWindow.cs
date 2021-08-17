namespace Graphene.LevelEditor
{
    public class MenuWindowOpen
    {
        public readonly MenuWindow Menu;

        public MenuWindowOpen(MenuWindow menu)
        {
            Menu = menu;
        }
    }
    public enum MenuWindow
    {
        None = 0,
        Enemies = 1,
        Walls = 2,
        Systems = 3,
        Settings = 4,
    }
}