using Reactor.Localization;

namespace Reactor.Example;

public class ExampleLocalizationProvider : LocalizationProvider
{
    public override bool TryGetText(StringNames stringName, SupportedLangs language, out string? result)
    {
        if (stringName == (StringNames) 1337)
        {
            switch (language)
            {
                case SupportedLangs.English:
                    result = "Cringe English";
                    return true;

                default:
                    result = "Based " + language;
                    return true;
            }
        }

        result = null;
        return false;
    }
}
