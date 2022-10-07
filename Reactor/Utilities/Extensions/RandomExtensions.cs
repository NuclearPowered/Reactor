using System;
using System.Collections.Generic;
using System.Linq;

namespace Reactor.Utilities.Extensions;

public static class RandomExtensions
{
    /// <summary>
    /// Returns random <typeparamref name="T"/> from <paramref name="input"/> using <paramref name="random"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> input, Random random)
    {
        var list = input as IList<T> ?? input.ToList();
        return list.Count == 0 ? default : list[random.Next(0, list.Count)];
    }

    /// <summary>Returns a random double that is within a specified range.</summary>
    public static double NextDouble(this Random random, double minValue, double maxValue)
    {
        return random.NextDouble() * (maxValue - minValue) + minValue;
    }

    /// <summary>
    /// Returns random <typeparamref name="T"/> from <paramref name="input"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> input)
    {
        var list = input as IList<T> ?? input.ToList();
        return list.Count == 0 ? default : list[UnityEngine.Random.Range(0, list.Count)];
    }
}
