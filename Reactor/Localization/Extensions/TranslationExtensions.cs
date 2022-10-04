using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Double = Il2CppSystem.Double;
using Int16 = Il2CppSystem.Int16;
using Int32 = Il2CppSystem.Int32;
using Int64 = Il2CppSystem.Int64;
using Object = Il2CppSystem.Object;
using Single = Il2CppSystem.Single;
using UInt16 = Il2CppSystem.UInt16;
using UInt32 = Il2CppSystem.UInt32;
using UInt64 = Il2CppSystem.UInt64;

namespace Reactor.Localization.Extensions;

public static class TranslationExtensions
{
    public readonly struct Il2CppObjectParsable
    {
        public readonly Object Object;

        public Il2CppObjectParsable(Object obj) => Object = obj;

        public static implicit operator Object(Il2CppObjectParsable parsable) => parsable.Object;

        public static implicit operator Il2CppObjectParsable(short value) => new(new Int16 {m_value = value}.BoxIl2CppObject());
        public static implicit operator Il2CppObjectParsable(int value) => new(new Int32 {m_value = value}.BoxIl2CppObject());
        public static implicit operator Il2CppObjectParsable(long value) => new(new Int64 {m_value = value}.BoxIl2CppObject());

        public static implicit operator Il2CppObjectParsable(ushort value) => new(new UInt16 {m_value = value}.BoxIl2CppObject());
        public static implicit operator Il2CppObjectParsable(uint value) => new(new UInt32 {m_value = value}.BoxIl2CppObject());
        public static implicit operator Il2CppObjectParsable(ulong value) => new(new UInt64 {m_value = value}.BoxIl2CppObject());

        public static implicit operator Il2CppObjectParsable(float value) => new(new Single {m_value = value}.BoxIl2CppObject());
        public static implicit operator Il2CppObjectParsable(double value) => new(new Double {m_value = value}.BoxIl2CppObject());

        public static implicit operator Il2CppObjectParsable(string value) => new(value);
    }

    /// <summary>
    /// Invoking <see cref="TranslationController.GetString(StringNames,Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray{Il2CppSystem.Object})"/>
    /// directly will generally cause exceptions, this version makes it easier for the developer to invoke this method without worrying about IL2CPP boxing bs.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string GetStringFixed(this TranslationController self, StringNames name, params Il2CppObjectParsable[] args)
        => self.GetString(name, new Il2CppReferenceArray<Object>(args.Select(o => o.Object).ToArray()));
}
