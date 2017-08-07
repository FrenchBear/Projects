// Bonza Generator
// Various extension methods to simplify code
// 2017-07-24   PV      Split from program.cs to prepare new merged application

using System;
using System.Collections.Generic;


namespace Bonza.Generator
{
    public static class ExtensionMethods
    {
        private static readonly Random Rnd = new Random();

        // Randomize the order of elements in a list
        // Takes a List<T> instead of an IList<T> to simplify the case with 1 element...
        public static List<T> Shuffle<T>(this List<T> list)
        {
            if (list == null || list.Count < 2) return list;

            var shuffledList = new List<T>(list);
            for (int i = 0; i < list.Count; i++)
            {
                // It's Ok to have p1==p2, for instance when shuffling a 2-element list
                // so that in 50% we return the original list, in 50% a swapped version
                var p1 = i;
                var p2 = Rnd.Next(list.Count);
                var temp = shuffledList[p1];
                shuffledList[p1] = shuffledList[p2];
                shuffledList[p2] = temp;
            }
            return shuffledList;
        }


        /*
        // Shuffle any (I)List with an extension method based on the Fisher-Yates shuffle
        // https://stackoverflow.com/questions/273313/randomize-a-listt
        public static void ShuffleInPlace<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // Return a random element from a list
        public static T TakeRandom<T>(this List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (list.Count == 0)
                throw new ArgumentException("Can't select a random element from a list of zero element");
            return list[Rnd.Next(list.Count)];
        }

        // Execute an action on every element of an enumeration
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }
        */
    }
}
