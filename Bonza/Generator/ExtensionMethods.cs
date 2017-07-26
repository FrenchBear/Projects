// Bonza Generator
// Various extension methods to simplify code
// 2017-07-24   PV      Split from program.cs to prepare new merged application

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bonza.Generator
{
    public static class ExtensionMethods
    {
        static readonly Random rnd = new Random();

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
                int p1, p2;
                p1 = i; // rnd.Next(list.Count);
                p2 = rnd.Next(list.Count);
                T temp = shuffledList[p1];
                shuffledList[p1] = shuffledList[p2];
                shuffledList[p2] = temp;
            }
            return shuffledList;
        }


        // Shuffle any (I)List with an extension method based on the Fisher-Yates shuffle
        // https://stackoverflow.com/questions/273313/randomize-a-listt
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
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
            return list[rnd.Next(list.Count)];
        }


    }
}
