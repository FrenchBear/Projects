// Solitaire Solver Test Application
// Extension Methods
//
// 2019-04-07   PV
// 2023-12-20   PV      Net8 C#12

using System;
using System.Collections.Generic;

namespace SolLib;

internal static class ExtensionMethods
{
    // Randomize the order of elements in a list
    // Takes a List<T> instead of an IList<T> to simplify the case with 1 element...
    public static List<T> Shuffle<T>(this List<T> list, int seed)
    {
        if (/*list == null ||*/ list.Count < 2)
            return list;

        var Rnd = new Random(seed);

        var shuffledList = new List<T>(list);
        for (int i = 0; i < list.Count; i++)
        {
            // It's OK to have p1==p2, for instance when shuffling a 2-element list
            // so that in 50% we return the original list, in 50% a swapped version
            var p1 = i;
            var p2 = Rnd.Next(list.Count);
            (shuffledList[p2], shuffledList[p1]) = (shuffledList[p1], shuffledList[p2]);
        }
        return shuffledList;
    }

    public static T[] InitializeArray<T>(this T[] array) where T : new()
    {
        for (int i = 0; i < array.Length; ++i)
            array[i] = new T();
        return array;
    }
}