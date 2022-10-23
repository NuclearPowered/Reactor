using Reactor.Localization;

namespace Reactor.Example;

public class ExampleLocalizationProvider : LocalizationProvider
{
    public override bool TryGetText(StringNames stringName, out string? result)
    {
        if (stringName == (StringNames) 1337)
        {
            switch (CurrentLanguage)
            {
                case SupportedLangs.English:
                    result = "Cringe English";
                    return true;

                default:
                    result = "Based " + CurrentLanguage;
                    return true;
            }
        }

        result = null;
        return false;
    }
}
