using Reactor.Localization;

namespace Reactor.Example;

public class ExampleLocalizationProvider : LocalizationProvider
{
    public override bool CanHandle(StringNames stringName)
    {
        return stringName == (StringNames) 1337;
    }

    public override string GetText(StringNames stringName, SupportedLangs language)
    {
        switch (language)
        {
            case SupportedLangs.English:
                return "Cringe English";

            default:
                return "Based " + language;
        }
    }
}
