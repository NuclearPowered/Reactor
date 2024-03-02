using BepInEx.Configuration;

namespace Reactor;

internal static class ReactorConfig
{
    public const string FeaturesSection = "Features";
    public const string MessagesSection = "Messages";

    public static ConfigEntry<bool> ForceDisableServerAuthority { get; private set; } = null!;

    public static ConfigEntry<string> HandshakePopupMessage { get; private set; } = null!;
    public static ConfigEntry<string> MakePublicDisallowedPopupMessage { get; private set; } = null!;

    public static void Bind(ConfigFile config)
    {
        ForceDisableServerAuthority = config.Bind(FeaturesSection, nameof(ForceDisableServerAuthority), false, "Enables the DisableServerAuthority flag even when no mods declare it");

        HandshakePopupMessage = config.Bind(MessagesSection, nameof(HandshakePopupMessage), string.Empty);
        MakePublicDisallowedPopupMessage = config.Bind(MessagesSection, nameof(MakePublicDisallowedPopupMessage), string.Empty);
    }
}
