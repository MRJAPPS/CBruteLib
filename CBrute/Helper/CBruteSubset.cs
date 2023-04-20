using System;
using System.Collections.Generic;
using System.Threading;

namespace CBrute.Helper
{
    /// <summary>
    /// This class is used to obtain subsets of a set.
    /// </summary>
    internal static class CBruteSubset
    {
        #region private_R
        #region private_static_R
        /// <summary>
        /// This function takes an array of integers and an integer value as input and finds the first number in the array that is greater than this value.
        /// </summary>
        private static void getSmallestBigElement(int[] array, int value, int[] ret, int retIndex)//That's a weird name, ha?
        {
            //The array must be sorted first!
            //We have done this before
            for (int i = 0; i < array.Length; ++i)
            {
                if (array[i] > (value))
                {
                    ret[retIndex] = array[i];
                    return;
                }
            }
        }
        /// <summary>
        /// This function generates the next subset of an integer array, given the current subset and the original set.
        /// The function modifies the input parameter "<paramref name="first"/>, which represents the current subset, to generate the next subset.
        /// The function returns a boolean value indicating whether a next subset exists or not.
        /// </summary>
        /// <param name="first">The current subset</param>
        /// <param name="set">The Reference Collection</param>
        /// <param name="indexForChange">The index of "<paramref name="first"/>" array that needs to be changed.</param>
        /// <param name="distanceFromTheEnd">distance from the end of the <paramref name="set"/></param>
        /// <returns>If it is possible to calculate the subset after <paramref name="first"/>, it returns True; otherwise, it returns False.</returns>
        private static bool nextSubset(int[] first, int[] set, int indexForChange, int distanceFromTheEnd)
        {
            if (indexForChange < 0) return false;
            int lastItemIndex = Array.IndexOf(set, first[indexForChange]);
            if (lastItemIndex == set.Length - distanceFromTheEnd)
                return nextSubset(first, set, indexForChange - 1, distanceFromTheEnd + 1);
            first[indexForChange] = set[lastItemIndex + 1];
            for (int i = indexForChange + 1; i < first.Length; ++i)
                getSmallestBigElement(set, first[i - 1], first, i);
            return true;
        }

        #endregion
        #endregion
        #region public_R
        #region public_static_R
        /// <summary>
        /// This function computes all subsets of size <paramref name="n"/> from a given <paramref name="set"/>. If it cannot be done in the specified time(<paramref name="millisecondsTimeout"/>), an error occurs.
        /// </summary>
        /// <param name="set">The Reference Collection</param>
        /// <param name="n">Number of elements in each subset</param>
        /// <param name="millisecondsTimeout">Time constraint for the function to complete its task.</param>
        /// <returns>This function generates the subsets as a two-dimensional array. Each row represents a subset.</returns>
        /// <exception cref="TimeoutException"></exception>
        internal static string[][] GetSubsets(string[] set, int n, int millisecondsTimeout)
        {
            n -= 2;
            if (n < 0)
            {
                string[][] singleMemberSubsets = new string[set.Length][];
                for (int i = 0; i < set.Length; ++i)
                    singleMemberSubsets[i] = new string[1] { set[i] };
                return singleMemberSubsets;
            }
            Array.Sort(set);
            string[][]? ret = null;
            Thread thread = new Thread(() =>
            {
                List<string[]> subsets = new List<string[]>();
                int[] setIndexes = new int[set.Length];
                for (int i = 0; i < setIndexes.Length; ++i) setIndexes[i] = i;
                int[] beginningOfSubset = ListConverter.GetSubArray(setIndexes, 0, n);
                do
                {
                    int[] subsetIndexes = new int[beginningOfSubset.Length + 1];
                    for (int i = 0; i < beginningOfSubset.Length; i++) subsetIndexes[i] = beginningOfSubset[i];//Fill start
                    for (int i = 0; i < setIndexes.Length; ++i)
                    {
                        if (beginningOfSubset[beginningOfSubset.Length - 1] < setIndexes[i])
                        {
                            subsetIndexes[subsetIndexes.Length - 1] = setIndexes[i];
                            subsets.Add(ListConverter.ConvertToStringArrayByIndex(set, subsetIndexes));
                        }
                    }

                } while (nextSubset(beginningOfSubset, setIndexes, beginningOfSubset.Length - 1, 2));
                ret = new string[subsets.Count][];
                for (int i = 0; i < ret.Length; ++i) ret[i] = subsets[i];
            });
            thread.Start();
            if (!thread.Join(millisecondsTimeout)) throw new TimeoutException(nameof(GetSubsets));
            return ret!;
        }
        /*
         * Perhaps you're wondering why these codes are commented?
         * Because initially I intended to calculate the sum of all characters in all subsets to prevent the RAM from being filled, and if it exceeded a certain limit, I would stop generating subsets
         * However, after testing, I realized that even calculating the number of characters in all subsets is also slow and can further burden the CPU.
         * So, I just gave up on this matter altogether.
        */
        //internal static int GetSubsetsLength(string[] set, int n, int millisecondsTimeout)
        //{
        //    n -= 2;
        //    int subsetsLen = 0;
        //    if (n < 0)
        //    {
        //        foreach (string element in set)
        //            subsetsLen += element.Length;
        //        return subsetsLen;
        //    }
        //    Array.Sort(set);
        //    Thread thread = new Thread(() =>
        //    {
        //        int[] setIndexes = new int[set.Length];
        //        for (int i = 0; i < setIndexes.Length; ++i) setIndexes[i] = i;
        //        int[] beginningOfSubset = ListConverter.GetSubArray(setIndexes, 0, n);
        //        do
        //        {
        //            GC.Collect();
        //            int[] subsetIndexes = new int[beginningOfSubset.Length + 1];
        //            for (int i = 0; i < beginningOfSubset.Length; i++) subsetIndexes[i] = beginningOfSubset[i];//Fill start
        //            for (int i = 0; i < setIndexes.Length; ++i)
        //            {
        //                if (beginningOfSubset[beginningOfSubset.Length - 1] < setIndexes[i])
        //                {
        //                    subsetIndexes[subsetIndexes.Length - 1] = setIndexes[i];
        //                    string[] subset = ListConverter.ConvertToStringArrayByIndex(set, subsetIndexes);
        //                    foreach (string element in subset)
        //                        subsetsLen += element.Length;
        //                }
        //            }
        //        } while (nextSubset(beginningOfSubset, setIndexes, beginningOfSubset.Length - 1, 2));
        //    });
        //    thread.Start();
        //    if (!thread.Join(millisecondsTimeout)) throw new TimeoutException(nameof(GetSubsetsLength));
        //    return subsetsLen;
        //}
        //internal static string[][][] GetAllSubsets(string[] set, int millisecondsTimeout)
        //{
        //    string[][][]? subsets = null;
        //    Thread thread = new Thread(() =>
        //    {
        //        subsets = new string[set.Length][][];//We don't consider null set
        //        for (int i = 0; i < set.Length; ++i)
        //            subsets[i] = GetSubsets(set, i + 1, int.MaxValue);
        //    });
        //    thread.Start();
        //    if (!thread.Join(millisecondsTimeout)) throw new TimeoutException(nameof(GetAllSubsets));
        //    return subsets!;
        //}
        //internal static int GetAllSubsetsLenght(string[] set, int millisecondsTimeout)
        //{
        //    int subsetsLenght = 0;
        //    Thread thread = new Thread(() =>
        //    {
        //        for (int i = 0; i < set.Length; ++i)
        //            subsetsLenght += GetSubsetsLength(set, i + 1, int.MaxValue);
        //    });
        //    thread.Start();
        //    if (!thread.Join(millisecondsTimeout)) throw new TimeoutException(nameof(GetAllSubsetsLenght));
        //    return subsetsLenght;
        //}
        #endregion
        #endregion
    }
}
