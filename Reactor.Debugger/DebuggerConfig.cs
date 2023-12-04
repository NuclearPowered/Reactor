using BepInEx.Configuration;

namespace Reactor.Debugger;

internal static class DebuggerConfig
{
    public const string FeaturesSection = "Features";
    public const string AutoJoinSection = "AutoJoin";

    public static ConfigEntry<bool> DisableGameEnd { get; private set; } = null!;
    public static ConfigEntry<bool> RedirectLogger { get; private set; } = null!;
    public static ConfigEntry<bool> AutoPlayAgain { get; private set; } = null!;
    public static ConfigEntry<bool> DisableTimeout { get; private set; } = null!;

    public static ConfigEntry<bool> JoinGameOnStart { get; private set; } = null!;

    public static void Bind(ConfigFile config)
    {
        DisableGameEnd = config.Bind(FeaturesSection, nameof(DisableGameEnd), false, "Stops the game from ending regardless of conditions");
        RedirectLogger = config.Bind(FeaturesSection, nameof(RedirectLogger), false, "Redirect base game Logger calls into BepInEx logging");
        AutoPlayAgain = config.Bind(FeaturesSection, nameof(AutoPlayAgain), true, "Automatically calls Play Again after game ends");
        DisableTimeout = config.Bind(FeaturesSection, nameof(DisableTimeout), false, "Disable the network disconnection timeout");

        JoinGameOnStart = config.Bind(AutoJoinSection, nameof(JoinGameOnStart), false, "Automatically hosts/joins a game on start");
    }
}
