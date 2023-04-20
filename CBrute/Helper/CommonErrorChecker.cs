using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CBrute.Helper
{
    /// <summary>
    /// I have collected some common errors among all other classes in this class to prevent code duplication.
    /// The code for the functions in this class is self-explanatory and does not require any further explanation.
    /// </summary>
    internal static class CommonErrorChecker
    {
        internal static void checkStartEnd(long startPos, ref long endPos, long Max)
        {
            if (startPos <= 0) throw new ArgumentOutOfRangeException
                    ($"The \"{nameof(startPos)}\" cannot be less than or equals to 0!", nameof(startPos));
            if (endPos <= 0) endPos = Max;
            if (endPos > Max) throw new ArgumentOutOfRangeException
                        ($"The \"{nameof(endPos)}({endPos})\" cannot bigger than \"{nameof(Max)}({Max})\"!", nameof(endPos));
            if (startPos > endPos) throw new ArgumentOutOfRangeException
                    ($"The \"{nameof(startPos)}({startPos})\" cannot bigger than \"{nameof(endPos)}({endPos})\"!", nameof(startPos));
        }
        internal static void minMaxE(int min, int max)
        {
            string getParamError() => (min <= 0 ? nameof(min) : nameof(max));
            if (min <= 0 || max <= 0) throw new ArgumentException
                    ($"The \"{getParamError()}\"" +
                    $" cannot be less than or equal to 0!", getParamError());
            if (max < min) throw new ArgumentException
                    ($"The \"{nameof(max)}\" cannot be less than \"{nameof(min)}\"!");
        }
        internal static void testE(IList test, string name = "test")
        {
            if (test.HasDuplicateItem()) throw new ArgumentException
                    ($"The \"{name}\" cannot have duplicate elements!", name);
        }
        internal static void extraLengthsE(int[]? extraLengths, int min, int max)
        {
            if (extraLengths != null &&
                (extraLengths.Length >= (max - min + 1) || extraLengths.Any(len => len < min || len > max) ||
                extraLengths.HasDuplicateItem()))
                throw new ArgumentException
                    ($"Please check the following:\n" +
                    $"1. \"{nameof(extraLengths)}\" should hadn't any element that less than \"{nameof(min)}\"" +
                    $"or bigger than \"{nameof(max)}\"!\n" +
                    $"2. \"{nameof(extraLengths)}.{nameof(extraLengths.Length)}({extraLengths.Length})\"" +
                   $" cannot be greater than or equal to (max - min + 1)({max - min + 1})!" +
                   $"\n3. Also the {nameof(extraLengths)} cannot have duplicates items!", nameof(extraLengths));//need check
        }
        internal static void listChecker<T>(IList<T> list, string name)
        {
            if (list == null) throw new ArgumentNullException(name);
            if (list.Count == 0) throw new ArgumentException
                    ($"The \"{name}\" cannot be empty!", name);
            if (list.Any(x => x == null)) throw new ArgumentException
                    ($"The \"{name}\" cannot contain null elements!", name);
        }
        internal static void passTestE(object[] test, object[] pass, bool customMessage = false, string msg = "")
        {
            if (!pass.IsSubsetOf(test))
                if (!customMessage)
                    throw new ArgumentException
                        ($"The \"{nameof(pass)}\" cannot include elements that" +
                        $" are not in \"{nameof(test)}\"!", nameof(pass));
                else throw new ArgumentException(msg, nameof(pass));
        }
        internal static void checkPass(object[] pass, int min, int max)
        {
            if (pass.Length < min) throw new ArgumentException
                ($"The \"{pass}.{nameof(pass.Length)}({pass.Length})\" cannot less than " +
                $"\"{nameof(min)}({min})\"!", nameof(pass));
            if (pass.Length > max) throw new ArgumentException
                    ($"The \"{pass}.{nameof(pass.Length)}({pass.Length})\" cannot bigger than " +
                    $"\"{nameof(max)}({max})\"!", nameof(pass));
        }
    }
}
