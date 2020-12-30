using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnhollowerBaseLib;

namespace Reactor
{
    public class CustomStringName
    {
        private static int _lastId = -1;

        private static readonly List<CustomStringName> _list = new List<CustomStringName>();

        public static IReadOnlyList<CustomStringName> List => _list.AsReadOnly();

        public static CustomStringName Register(string value)
        {
            var customStringName = new CustomStringName(_lastId--, value);
            _list.Add(customStringName);

            return customStringName;
        }

        public int Id { get; }

        public string Value { get; }

        private CustomStringName(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public static implicit operator StringNames(CustomStringName name) => (StringNames) name.Id;
        public static explicit operator CustomStringName(StringNames name) => List.SingleOrDefault(x => x.Id == (int) name);

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>))]
        private static class GetStringPatch
        {
            public static bool Prefix([HarmonyArgument(0)] StringNames stringId, [HarmonyArgument(1)] Il2CppReferenceArray<Il2CppSystem.Object> parts, ref string __result)
            {
                var customStringName = (CustomStringName) stringId;

                if (customStringName != null)
                {
                    __result = string.Format(customStringName.Value, parts);
                    return false;
                }

                return true;
            }
        }
    }
}
