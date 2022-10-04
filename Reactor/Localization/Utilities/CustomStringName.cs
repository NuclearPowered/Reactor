using Reactor.Localization.Providers;

namespace Reactor.Localization.Utilities;

public class CustomStringName
{
    private static int _lastId = int.MinValue + 1;

    public int Id { get; }

    public string Value { get; }
    
    private CustomStringName(int id, string value)
    {
        Id = id;
        Value = value;
    }
    
    public static CustomStringName Register(string value)
    {
        var id = _lastId++;
        var customStringName = new CustomStringName(id, value);
        
        HardCodedLocalizationProvider.Strings[(StringNames) id] = customStringName;

        return customStringName;
    }

    public static implicit operator StringNames(CustomStringName name) => (StringNames) name.Id;
    public static implicit operator string(CustomStringName name) => name.Value;
}
