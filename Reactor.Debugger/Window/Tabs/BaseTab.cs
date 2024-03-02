namespace Reactor.Debugger.Window.Tabs;

internal abstract class BaseTab
{
    public abstract string Name { get; }

    public abstract void OnGUI();
}
