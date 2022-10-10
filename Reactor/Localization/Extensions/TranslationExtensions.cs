using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities.Extensions;
using Object = Il2CppSystem.Object;

namespace Reactor.Localization.Extensions;

/// <summary>
/// Extension methods for <see cref="TranslationController"/>.
/// </summary>
public static class TranslationExtensions
{
    /// <summary>
    /// Invoking <see cref="TranslationController.GetString(StringNames,Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray{Il2CppSystem.Object})"/>
    /// directly will generally cause exceptions, this version makes it easier for the developer to invoke this method without worrying about IL2CPP boxing bs.
    /// </summary>
    /// <param name="self">The <see cref="TranslationController"/> instance.</param>
    /// <param name="name">The <see cref="StringNames"/> to convert into <see cref="string"/>.</param>
    /// <param name="args">Optional arguments to be used in <see cref="string.Format(System.IFormatProvider?,string,object?)"/>.</param>
    /// <returns>The <see cref="string"/> representation of the <see cref="StringNames"/>.</returns>
    public static string GetStringFixed(this TranslationController self, StringNames name, params Il2CppInteropExtensions.Il2CppBoxedPrimitive[] args)
        => self.GetString(name, new Il2CppReferenceArray<Object>(args.Select(o => o.Object).ToArray()));
}
