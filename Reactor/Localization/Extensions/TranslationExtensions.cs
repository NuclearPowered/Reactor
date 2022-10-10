using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities.Extensions;
using Object = Il2CppSystem.Object;

namespace Reactor.Localization.Extensions;

public static class TranslationExtensions
{
    /// <summary>
    /// Invoking <see cref="TranslationController.GetString(StringNames,Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray{Il2CppSystem.Object})"/>
    /// directly will generally cause exceptions, this version makes it easier for the developer to invoke this method without worrying about IL2CPP boxing bs.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string GetStringFixed(this TranslationController self, StringNames name, params Il2CppInteropExtensions.Il2CppBoxedPrimitive[] args)
        => self.GetString(name, new Il2CppReferenceArray<Object>(args.Select(o => o.Object).ToArray()));
}
