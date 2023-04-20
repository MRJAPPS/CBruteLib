using System;
using System.Collections;
using System.Collections.Generic;

namespace CBrute.Helper
{
    /// <summary>
    /// I have implemented here some helper functions that assist other classes.
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Checks whether a duplicate element exists in the <paramref name="array"/> or not.
        /// </summary>
        /// <param name="array">Array to be checked for duplicates.</param>
        /// <returns>If there is a duplicate element in the array, it returns True, and False otherwise.</returns>
        internal static bool HasDuplicateItem(this IList array)
        {
            for (int i = 0; i < array.Count; ++i)
                for (int j = 0; j < array.Count; ++j)
                    if (i != j && array[i]!.Equals(array[j])) return true;
            return false;
        }
        /// <summary>
        /// Checks if <paramref name="array1"/> is a subset of <paramref name="array2"/> or not.
        /// </summary>
        /// <param name="array1">The array that we want to check if it is a subset.</param>
        /// <param name="array2">The array that we think array1 is a subset of it.</param>
        /// <returns>We return True if <paramref name="array1"/> is a subset of <paramref name="array2"/>, and False otherwise.</returns>
        internal static bool IsSubsetOf(this IList array1, Array array2)
        {
            foreach (var item in array1)
                if (Array.IndexOf(array2, item) == -1) return false;
            return true;
        }
        /// <summary>
        /// This function finds consecutive sequences of integers in an input array and returns their start and end indices as an integer array.
        /// </summary>
        /// <param name="array">The array that we want to identify consecutive elements in it.</param>
        /// <returns>It returns an array where each element represents an index in the <paramref name="array"/> parameter, and even elements indicate the start position, while odd elements indicate the end position.</returns>
        internal static int[] ConsecFinder(int[] array)
        {
            List<int> ret = new List<int>(array.Length);
            int start = 0, end = 0;
            for (int i = 0; i < array.Length; ++i)
            {
                start = i;
                end = i;
                for (int j = start + 1; j < array.Length; ++j)
                {
                    if (array[j] == array[j - 1] + 1) end = j;
                    else break;
                }
                ret.Add(start);
                ret.Add(end);
                i = end;
            }
            return ret.ToArray();
        }
        /// <summary>
        /// Removes an element from an array.
        /// </summary>
        /// <param name="array">The array from which we want to remove an element.</param>
        /// <param name="index">Index of the element that we want to remove from the <paramref name="array"/>.</param>
        /// <returns>It returns an array with the specified element removed from it.</returns>
        internal static object[] Remove(this object[] array, int index)
        {
            ArrayList ret = new ArrayList();
            for (int i = 0; i < array.Length; ++i)
                if (i != index) ret.Add(array[i]);
            return ret.ToArray();
        }
        /// <summary>
        /// Removes an element from an array.
        /// </summary>
        /// <param name="array">The array from which we want to remove an element.</param>
        /// <param name="index">Index of the element that we want to remove from the <paramref name="array"/>.</param>
        /// <returns>It returns an array with the specified element removed from it.</returns>
        internal static int[] Remove(this int[] array, int index)
        {
            List<int> ret = new List<int>();
            for (int i = 0; i < array.Length; ++i)
                if (i != index) ret.Add(array[i]);
            return ret.ToArray();
        }
    }
}
