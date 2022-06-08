
public class DevPanelCreator
{
    public void create(DevPanelController dev)
    {
        var root = dev.root;
        root.button("lunar console", () =>
        {
            LunarConsolePlugin.LunarConsole.Show();    
        });
    }
}