namespace Reactor.Localization;

/// <summary>
/// The required implementation of a localization provider class
/// </summary>
public abstract class LocalizationProvider
{
    /// <summary>
    /// The priority of this <see cref="LocalizationProvider"/>.
    /// The higher the priority is, the earlier it will be invoked in relation to other providers.
    /// <br/>
    /// You can use the <see cref="HarmonyLib.Priority"/> class for this value if you want to make this easier.
    /// </summary>
    public virtual int Priority => 0;

    /// <summary>
    /// Whether or not this <see cref="LocalizationProvider"/> can handle this <see cref="StringNames"/>
    /// <br/>
    /// Returning true here will subsequently call <see cref="GetText"/> with the same <see cref="StringNames"/>
    /// </summary>
    /// <param name="stringName"></param>
    /// <returns></returns>
    public virtual bool CanHandle(StringNames stringName) => false;

    /// <summary>
    /// Returns the localized text for the given <see cref="StringNames"/>
    /// </summary>
    /// <param name="stringName"></param>
    /// <returns></returns>
    public virtual string GetText(StringNames stringName) => "STRMISS";

    /// <summary>
    /// Whether or not this <see cref="LocalizationProvider"/> can handle this <see cref="SystemTypes"/>
    /// <br/>
    /// Returning true here will subsequently call <see cref="GetStringName(SystemTypes)"/> with the same <see cref="SystemTypes"/>
    /// </summary>
    /// <param name="systemType"></param>
    /// <returns></returns>
    public virtual bool CanHandle(SystemTypes systemType) => false;
    
    /// <summary>
    /// Returns the matching <see cref="StringNames"/> for the given <see cref="SystemTypes"/>
    /// </summary>
    /// <param name="systemType"></param>
    /// <returns></returns>
    public virtual StringNames GetStringName(SystemTypes systemType) => StringNames.ExitButton;
    
    /// <summary>
    /// Whether or not this <see cref="LocalizationProvider"/> can handle this <see cref="TaskTypes"/>
    /// <br/>
    /// Returning true here will subsequently call <see cref="GetStringName(TaskTypes)"/> with the same <see cref="TaskTypes"/>
    /// </summary>
    /// <param name="taskType"></param>
    /// <returns></returns>
    public virtual bool CanHandle(TaskTypes taskType) => false;
    
    /// <summary>
    /// Returns the matching <see cref="StringNames"/> for the given <see cref="TaskTypes"/>
    /// </summary>
    /// <param name="taskType"></param>
    /// <returns></returns>
    public virtual StringNames GetStringName(TaskTypes taskType) => StringNames.ExitButton;
}
