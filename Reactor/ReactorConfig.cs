using BepInEx.Configuration;

namespace Reactor;

internal static class ReactorConfig
{
    public const string FeaturesSection = "Features";

    public static ConfigEntry<bool> ForceDisableServerAuthority { get; private set; } = null!;
    public static ConfigEntry<bool> IgnoreHandshakePopup { get; private set; } = null!;

    public static void Bind(ConfigFile config)
    {
        ForceDisableServerAuthority = config.Bind(FeaturesSection, nameof(ForceDisableServerAuthority), false, "Enables the DisableServerAuthority flag even when no mods declare it");
        IgnoreHandshakePopup = config.Bind(FeaturesSection, nameof(IgnoreHandshakePopup), false, "Disables the \"This server doesn't support modded handshake\" popup");
    }
}
