using System.IO;
using System.Linq;
using System.Text;

namespace Reactor.Greenhouse.Setup
{
    public static class GameVersionParser
    {
        public static int IndexOf(this byte[] source, byte[] pattern)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }

            return -1;
        }

        public static string Parse(string file)
        {
            var bytes = File.ReadAllBytes(file);

            var pattern = Encoding.UTF8.GetBytes("public.app-category.games");
            var index = bytes.IndexOf(pattern) + pattern.Length + 127;

            return Encoding.UTF8.GetString(bytes.Skip(index).TakeWhile(x => x != 0).ToArray());
        }
    }
}
