using System;
using System.Linq;
using System.Text;

namespace CBrute.Helper
{
    /// <summary>
    /// A helper class for converting and combining arrays.
    /// </summary>
    public static class ListConverter
    {
        /// <summary>
        /// Using this function, you can fill all the elements of an array with a specific value.
        /// </summary>
        /// <param name="array">The array whose elements' values you intend to change.</param>
        /// <param name="value">The new value that you intend to replace all the elements of the array with</param>
        internal static void Reset(this object[] array, object value)
        {
            for (int i = 0; i < array.Length; ++i) array[i] = value;
        }
        /// <summary>
        /// Extracts a specific range from the array "arr"
        /// </summary>
        /// <param name="arr">The array from which you want to extract a specific range.</param>
        /// <param name="start">The starting point for extracting the subarray.</param>
        /// <param name="end">The ending point for extracting the subarray.</param>
        /// <returns>It returns a subarray of <paramref name="arr"/> from <paramref name="start"/> to <paramref name="end"/>.</returns>
        internal static T[] GetSubArray<T>(this T[] arr, int start, int end)
        {
            T[] ret = new T[end - start + 1];
            int tmp = start;
            for (; start <= end; start++) ret[start - tmp] = arr[start];
            return ret;
        }
        /// <summary>
        /// It is used to concatenate multiple arrays sequentially.
        /// </summary>
        /// <param name="arrays">The list of arrays that you want to be concatenated together.</param>
        /// <returns>It returns the array that is the result of concatenating other arrays.</returns>
        public static object[] Merge(params object[][] arrays)
        {
            int len = 0;
            foreach (object[] cur in arrays) len += cur.Length;//We may be on the side with the jagged array
            int i = 0;
            object[] ret = new object[len];
            foreach (object[] cur in arrays)
                foreach (object obj in cur)
                    ret[i++ + 0] = obj;
            return ret;
        }
        /// <summary>
        /// It is used to append an array to the end of another array.
        /// </summary>
        /// <param name="array">The array to which you want to append another array to its end.</param>
        /// <param name="newArray">The array that you want to append to the end of the <paramref name="array"/>.</param>
        /// <returns>It returns the array that is the result of merging <paramref name="array"/> and <paramref name="newArray"/>.</returns>
        public static object[] Append(this object[] array, object[] newArray) => Merge(array, newArray);
        /// <summary>
        /// It is used to split an array.
        /// </summary>
        /// <param name="list">The array that you want to split.</param>
        /// <param name="count">Number of parts to split the array into.
        /// Note that it's better to be divisible by the length of the <paramref name="list"/>, otherwise the returned arrays will include an extra array to store the remaining items.</param>
        /// <returns>Converts the array <paramref name="list"/> into a two-dimensional array with a minimum row length of <paramref name="count"/>.</returns>
        public static object[][] Split(this object[] list, int count)
        {
            int remained = (list.Length % count);
            int len = (list.Length / count) + (remained == 0 ? 0 : 1);
            object[][] ret = new object[len][];
            int subArrayStartPos = 0;
            for (int i = 0; i < ret.Length - (remained == 0 ? 0 : 1); ++i)
            {
                ret[i] = GetSubArray(list, subArrayStartPos, subArrayStartPos + (count - 1));
                subArrayStartPos += (count - 1) + 1;
            }
            if (remained != 0)
                ret[ret.Length - 1] = GetSubArray(list, subArrayStartPos, list.Length - 1);
            return ret;
        }
        /// <summary>
        /// It is used to convert an array of strings to an array of objects.
        /// </summary>
        /// <param name="arr">The array that you want to convert to an array of type object.</param>
        /// <returns>An array of objects resulting from converting each element in <paramref name="arr"/> to an object.</returns>
        public static object[] ConvertToObjectArray(this string[] arr) => arr.Cast<object>().ToArray();
        /// <summary>
        /// It is used to convert an array of objects to an array of strings.
        /// </summary>
        /// <param name="arr">The array that you want to convert to an array of type strings.</param>
        /// <returns>An array of strings resulting from converting each element in <paramref name="arr"/> to an string.</returns>
        public static string[] ConvertToStringArray(this object[] arr) => arr.Select(item => item.ToString()).ToArray();
        /// <summary>
        /// Used to convert an array of objects to a string. If the <paramref name="arr"/> contains null elements, no error is raised.
        /// </summary>
        /// <param name="arr">The array that you want to convert to a string</param>
        /// <returns>"It converts all elements of the <paramref name="arr"/> to a string, combines them together, and returns a single string.</returns>
        public static string ConvertObjectArrayToString(this object[] arr)
        {
            StringBuilder SB = new StringBuilder();
            foreach (object obj in arr) SB.Append(obj ?? "<<null>>");
            return SB.ToString();
        }
        /// <summary>
        /// Used to convert an array of objects to a string. If the <paramref name="arr"/> contains null elements, error is raised.
        /// </summary>
        /// <param name="arr">The array that you want to convert to a string</param>
        /// <returns>"It converts all elements of the <paramref name="arr"/> to a string, combines them together, and returns a single string.</returns>
        public static string ConvertStringArrayToString(this object[] arr)
        {
            string[] tmp = ConvertToStringArray(arr);
            StringBuilder ret = new StringBuilder();
            foreach (string str in tmp)
                ret.Append(str);
            return ret.ToString();
        }
        /// <summary>
        /// This function takes an array of strings and an array of integers and selects the elements of the <paramref name="arr"/> based on the <paramref name="indexes"/> specified by the integers, adds them to a list, and returns it."
        /// </summary>
        /// <param name="arr">The string array</param>
        /// <param name="indexes">An array where each element contains an index of the <paramref name="arr"/>.</param>
        /// <returns>An array of strings that all its elements are present in the <paramref name="arr"/>.</returns>
        internal static string[] ConvertToStringArrayByIndex(string[] arr, int[] indexes)
        {
            string[] ret = new string[indexes.Length];
            for (int i = 0; i < indexes.Length; ++i)
                ret[i] = arr[indexes[i]];
            return ret;
        }
        /// <summary>
        /// This method takes an array of strings called <paramref name="subset"/> and an array of objects called <paramref name="pass"/>. It returns an integer array where each element represents the index of the corresponding element in <paramref name="pass"/> in the <paramref name="subset"/> array.
        /// </summary>
        /// <param name="subset">The array of strings to search in.</param>
        /// <param name="pass">The array of objects containing the strings to search for.</param>
        /// <returns>An array of integers representing the indexes of the strings in the original array.</returns>
        internal static int[] ConvertToIndexArray(string[] subset, object[] pass)
        {
            int[] ret = new int[pass.Length];
            for (int i = 0; i < pass.Length; ++i)
                ret[i] = Array.IndexOf(subset, pass[i].ToString());
            return ret;
        }
    }
}
