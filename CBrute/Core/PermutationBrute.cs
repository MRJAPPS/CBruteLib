using CBrute.Helper;
using System;
using static CBrute.Helper.CBruteSubset;
using static CBrute.Helper.CommonErrorChecker;

namespace CBrute.Core
{
    /// <summary>
    /// By using this class, you can generate password permutations.
    /// You can even set the minimum and maximum length for password generation.
    /// In addition, you can specify the starting position for generating passwords and the ending position for stopping the password generation process.
    /// It may seem impossible at first glance, but with subsets, we can also set the minimum and maximum length of passwords for permutations.
    /// </summary>
    public sealed class PermutationBrute : BruteForce
    {
        /// <summary>
        /// Constructor of the PermutationBrute class
        /// </summary>
        /// <param name="startPos">
        /// This parameter determines the starting position for generating passwords.
        /// It should not be less than or equal to 0 or greater than <paramref name="endPos"/>.
        /// </param>
        /// <param name="endPos">
        /// This parameter determines the position of the last password to be generated. 
        /// If you want all passwords to be generated, you can use a number smaller than or equal to 0. 
        /// This parameter should not exceed the maximum number of passwords that can be generated. 
        /// </param>
        /// <param name="minPassLen">Minimum password length. This should not be grater than <paramref name="maxPassLen"/> or smaller than or equals to 0</param>
        /// <param name="maxPassLen">
        /// Maximum password length. In PermutationBrute, this parameter cannot be greater than the length of the <paramref name="test"/> parameter.
        /// Additionally, this parameter cannot be less than or equal to 0.
        /// </param>
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="threadID">A unique identifier for each BruteForce instance in each thread.</param>
        public PermutationBrute(long startPos, long endPos, int minPassLen, int maxPassLen,
            object[] test, int threadID = -1)
        {
            string[] strTestArray = PermutationBruteE(startPos, ref endPos, minPassLen, maxPassLen, test);
            this.test = strTestArray;
            min = minPassLen;
            max = maxPassLen;
            this.startPos = startPos;
            this.endPos = endPos;
            this.threadID = threadID;
            //this.netID = netID;
        }

        #region errorChecking_R
        /// <summary>
        /// This function detects and reports the possible errors that may occur when instantiating the PermutationBrute class.
        /// </summary>
        /// <param name="startPos">determines the starting position for generating passwords.</param>
        /// <param name="endPos">If endPos is less than or equal to 0, it uses <see cref="GetMax"/> to determine the total number of possible states</param>
        /// <param name="minPassLen">The minimum password length.</param>
        /// <param name="maxPassLen">The maximum password length.</param>
        /// <param name="test">This array contains the states that make up the passwords.</param>
        /// <returns>The function converts the <paramref name="test"/> array into an array of strings and returns it.</returns>
        internal static string[] PermutationBruteE(long startPos, ref long endPos, int minPassLen, int maxPassLen, object[] test)
        {
            listChecker(test, nameof(test));
            string[] strTestArray = test.ConvertToStringArray();
            testMinMaxE(strTestArray, minPassLen, maxPassLen);
            checkStartEnd(startPos, ref endPos, GetMax(test, minPassLen, maxPassLen));
            return strTestArray;

        }
        /// <summary>
        /// If this function determines that the <paramref name="minMax"/> parameter is greater than the length of the <paramref name="test"/> parameter, it throws an exception.
        /// </summary>
        private static void permutationMinMaxE(string[] test, int minMax, string parameterName)
        {
            if (minMax > test.Length) throw new ArgumentException($"In permutation, the \"{parameterName}({minMax})\"" +
                $" cannot be bigger than \"{nameof(test)}.{nameof(test.Length)}({test.Length})\"!", parameterName);
        }
        /// <summary>
        /// Checks whether the <paramref name="min"/>, <paramref name="max"/> and <paramref name="test"/> values are valid or not.
        /// </summary>
        private static void testMinMaxE(string[] test, int min, int max)
        {
            testE(test);
            permutationMinMaxE(test, max, nameof(max));
            minMaxE(min, max);
        }
        /// <summary>
        /// Identifies possible errors that may occur when working with the <see cref="GetPassByPos"/> function and reports them.
        /// </summary>
        private static void getPassByPosE(string[] test, long pos, int min, int max)
        {
            if (pos <= 0) throw new ArgumentException
                    ($"The \"{nameof(pos)}({pos})\" cannot be less than or equals to 0!", nameof(pos));
            long MAX = GetMax(test, min, max);
            if (pos > MAX) throw new ArgumentException
                    ($"The \"{nameof(pos)}({pos})\" cannot be bigger than \"{MAX}\"", nameof(pos));
        }
        /// <summary>
        /// Identifies possible errors that may occur when working with the <see cref="GetPosByPass"/> function and reports them.
        /// </summary>
        private static void getPosByPassE(string[] test, string[] pass, int min)
        {
            passTestE(test, pass);
            if (min > pass.Length) throw new ArgumentException($"The \"{nameof(min)}({min})\" cannot be bigger than " +
                $"\"{nameof(pass)}.{nameof(pass.Length)}({pass.Length})\"!", nameof(min));
            permutationMinMaxE(test, min, nameof(min));
        }

        #endregion
        #region private_R
        #region private_types
        /// <summary>
        /// closed interval
        /// </summary>
        struct SubsetRange//I was not bored with the one-dimensional array anymore :_(
        {
            public long start, end;//closed interval
            public SubsetRange(long start = 0, long end = 0)
            {
                this.start = start;
                this.end = end;
            }
        }
        #endregion
        #region private_static_R
        /// <summary>
        /// Calculates the factorial of the <paramref name="number"/> and returns it.
        /// </summary>
        private static long longFactorial(long number)
        {
            long fact = 1;
            for (long i = 1; i <= number; ++i) fact *= i;
            return fact;
        }
        /// <summary>
        /// This function calculates the number of possible permutations of selecting <paramref name="r"/> items out of <paramref name="n"/> distinct items. 
        /// </summary>
        private static long permutation(long r, long n) => longFactorial(n) / longFactorial(n - r);
        /// <summary>
        /// This function calculates the number of possible combinations of selecting <paramref name="r"/> elements from a set of <paramref name="n"/> elements.
        /// </summary>
        private static long combination(long r, long n) => permutation(r, n) / longFactorial(r);
        /// <summary>
        /// This function returns a set of ranges, each representing a password length.
        /// Each element with an even index represents "Start" and each element with an odd index represents "End".
        /// </summary>
        private static long[] getLengthRanges(string[] test, int min, int max)
        {
            long[] ranges = new long[max - min + 1];
            ranges[0] = 0;
            for (int i = 1; i < ranges.Length; ++i)
            {
                long comb = combination(min + i - 1, test.Length);
                long fact = longFactorial(min + i - 1);
                ranges[i] = ranges[i - 1] + comb * fact;
            }
            return ranges;
        }
        /// <summary>
        /// Returns the length of passowrd at <paramref name="pos"/>
        /// </summary>
        /// <param name="test">This array contains the states that make up the passwords.</param>
        /// <param name="pos">The position at which you intend to obtain the size of the producible password.</param>
        /// <param name="min">The minimum password length.</param>
        /// <param name="max">The maximum password length.</param>
        /// <param name="firstPos">Returns the position of the first password with the obtained length.</param>
        private static int getPassLen(string[] test, long pos, int min, int max, out long firstPos)
        {
            firstPos = 0;
            long[] ranges = getLengthRanges(test, min, max);
            if (min == max) return min;
            for (int i = 1; i < ranges.Length; ++i)
            {
                if (i == ranges.Length - 1 && pos > ranges[i])
                {
                    firstPos = ranges[i];
                    return max;
                }
                if (pos >= ranges[i - 1] + 1 && pos <= ranges[i])
                {
                    firstPos = ranges[i - 1];
                    return min + i - 1;
                }
            }
            return -1;
        }
        /// <summary>
        /// Calculates and returns the password at position <paramref name="pos"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0056:Use index operator", Justification = "<Pending>")]
        private static string[] getPassByPos(string[] test, long pos)
        {
            if (test.Length == 1) return new string[] { test[0] };
            string[] pass = new string[test.Length];
            long[] factorials = new long[test.Length];
            for (int i = 1; i < factorials.Length; ++i) factorials[i - 1] = longFactorial(test.Length - i);
            for (int i = 0; i < pass.Length - 2; ++i)
            {
                int changes = (int)(pos / factorials[i] + (pos % factorials[i] == 0 ? 0 : 1)) - 1;
                int testIndex = changes % test.Length;
                pass[i] = test[testIndex];
                test = test.Remove(testIndex).ConvertToStringArray();
            }
            if (pass.Length >= 2 && test.Length >= 2)
            {
                bool isPosEven = pos % 2 == 0;
                int testI1 = isPosEven ? 1 : 0;
                int testI2 = isPosEven ? 0 : 1;
                pass[pass.Length - 2] = test[testI1];
                pass[pass.Length - 1] = test[testI2];
            }
            return pass;
        }
        /// <summary>
        /// This function returns the TestArray related to <paramref name="pos"/>.
        /// </summary>
        private static string[]? getTest(string[][] subsets, int testLen, long pos, int min)
        {
            int passLen = subsets[0].Length;
            long basePos = getBase();
            SubsetRange[] ranges = getSubsetRanges();
            for (int i = 0; i < ranges.Length; ++i)
                if (pos >= ranges[i].start && pos <= ranges[i].end)
                    return subsets[i];
            return null;
            //local methods:
            long getBase()
            {//This function calculates the base position of the password with a specific length.
             //Essentially, this position corresponds to the password with the value of subset[0]. 
                long @base = 0;
                for (int len = min; len < passLen; ++len) @base += combination(len, testLen) * longFactorial(len);
                ++@base;
                return @base;
            }
            SubsetRange[] getSubsetRanges()
            {//This function calculates and returns the ranges of each subset. Each subset has a specific range.
                SubsetRange[] ranges = new SubsetRange[subsets.Length];
                //Since basePos represents subset[0], the first range is from basePos to the number of permutations of subset[0] plus basePos.
                //Since a range is closed, we subtract one from the EndPos.
                ranges[0] = new SubsetRange(basePos, basePos + longFactorial(passLen) - 1);
                for (int i = 1; i < ranges.Length; ++i)
                {
#pragma warning disable IDE0017 // Simplify object initialization
                    ranges[i] = new SubsetRange();
#pragma warning restore IDE0017 // Simplify object initialization
                    ranges[i].start = ranges[i - 1].end + 1;
                    ranges[i].end = longFactorial(passLen) + ranges[i].start - 1;
                }
                return ranges;
            }
        }
        /// <summary>
        /// The following function takes three long values as input parameters: start, step, and end.
        /// It calculates and returns an array of long values that represents the ranges of positions based on the given parameters.
        /// </summary>
        private static long[] getPosRanges(long start, long step, long end)
        {
            // check for overflow during calculation
            checked
            {
                // calculate the length of the array
                int len = ((int)(end - start) + 1) / (int)step * 2;
                long[] ranges = new long[len];
                ranges[0] = start;
                // loop through the array to set the other values
                for (int i = 1; i < ranges.Length; ++i)
                    if (i % 2 != 0)
                        ranges[i] = ranges[i - 1] + step - 1;
                    else
                        ranges[i] = ranges[i - 1] + 1;
                return ranges;
            }
        }
        /// <summary>
        /// Calculates the position of a <paramref name="passIndexes"/> based on the password itself and the <paramref name="testIndexes"/>.
        /// The reason for using int arrays is their speed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0056:Use index operator", Justification = "<Pending>")]
        private static long getPosByPass(int[] testIndexes, int[] passIndexes)
        {
            if (testIndexes.Length == 1 && passIndexes.Length == 1) return 1;
            long[] factorials = new long[testIndexes.Length];
            for (int i = 1; i < factorials.Length; ++i) factorials[i - 1] = longFactorial(testIndexes.Length - i);
            long start = 1, end = longFactorial(passIndexes.Length), step;
            for (int i = 0; i < passIndexes.Length - 2; ++i)
            {
                step = factorials[i];
                long[] Pranges = getPosRanges(start, step, end);
                int testIndex = Array.IndexOf(testIndexes, passIndexes[i]);
                start = Pranges[testIndex * 2];
                end = Pranges[testIndex * 2 + 1];
                testIndexes = testIndexes.Remove(testIndex);
            }
            if (passIndexes[passIndexes.Length - 2] < passIndexes[passIndexes.Length - 1]) return start;
            else return end;
        }
        /// <summary>
        /// Finds a subset that <paramref name="pass"/> is made of.
        /// </summary>
        private static int getSubsetsIndex(string[][] subsets, string[] pass)
        {
            string[] sortedPass = new string[pass.Length];
            Array.Copy(pass, sortedPass, pass.Length);
            Array.Sort(sortedPass);
            int subsetsI;
            for (subsetsI = 0; subsetsI < subsets.Length; ++subsetsI)
            {
                bool founded = true;
                for (int i = 0; i < subsets[subsetsI].Length; ++i)
                {
                    if (subsets[subsetsI][i] != sortedPass[i])
                    {
                        founded = false;
                        break;
                    }
                }
                if (founded) break;
            }
            return subsetsI;
        }
        /// <summary>
        /// This function calculates and returns the number of generated passwords until reaching the sorted <paramref name="pass"/> password.
        /// </summary>
        /// <param name="test">This array contains the states that make up the passwords.</param>
        /// <param name="pass">The password that we are currently checking.</param>
        /// <param name="min">The minimum password length.</param>
        /// <param name="subsetsIndex">The index indicating the set from which the <paramref name="pass"/> is composed.</param>
        /// <param name="subsets">it calculates and returns the subsets with length equal to pass.length.</param>
        private static long getGeneratedPasswords(string[] test, string[] pass, int min, out int subsetsIndex, out string[][] subsets)
        {
            long generated = 0;
            //step 1
            for (int len = min; len <= pass.Length - 1; ++len) generated += combination(len, test.Length) * longFactorial(len);
            subsets = GetSubsets(test, pass.Length, MillisecondsTimeout);
            int subsetsI = getSubsetsIndex(subsets, pass);
            //step 2
            for (int i = 0; i < subsetsI; ++i) generated += longFactorial(pass.Length);
            subsetsIndex = subsetsI;
            return generated;
        }
        /// <summary>
        /// The function that implements the next permutation algorithm.
        /// </summary>
        /// <returns>If it can generate the next permutation of <paramref name="indexes"/>, it returns True, otherwise it returns False.</returns>
        private bool nextPermutation(int[] indexes)
        {
            int suffix = indexes.Length - 2;
            while (suffix >= 0 && indexes[suffix + 1] <= indexes[suffix]) suffix--;
            if (suffix < 0) return false;
            int pivot = indexes.Length - 1;
            while (pivot >= 0 && indexes[pivot] <= indexes[suffix]) pivot--;
            //Swap
#pragma warning disable IDE0180 // Use tuple to swap values
            int temp = indexes[suffix];
#pragma warning restore IDE0180 // Use tuple to swap values
            indexes[suffix] = indexes[pivot];
            indexes[pivot] = temp;
            //End of Swap
            Array.Reverse(indexes, suffix + 1, indexes.Length - (suffix + 1));
            return true;
        }

        #endregion
        #region field_R

        private readonly string[] test;
        //Determines the actual position of a password in all possible cases.
        long realPosition = -1;

        #endregion
        #endregion
        #region public_R
        #region property_R
        /// <summary>
        /// Determines the actual position of a password in all possible cases.
        /// When generating passwords, using this property allows you to obtain the actual position of the generated password among all possible states.
        /// However, you can't change this property.
        /// </summary>
        public long RealPos { get => realPosition; }
        /// <summary>
        /// This array contains the states that make up the passwords.
        /// However, you can't change this property.
        /// </summary>
        public object[] Test { get => test; }

        #endregion
        #region public_static_R
        /// <summary>
        /// Using this function, you can obtain the password in all possible positions based on the actual position of that password.
        /// In other words, you need to know the actual position of that password.
        /// </summary>
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="pos">The actual position of the password that you want to obtain</param>
        /// <param name="min">Minimum password length. This should not be grater than <paramref name="max"/> or smaller than or equals to 0</param>
        /// <param name="max">
        /// Maximum password length. In PermutationBrute, this parameter cannot be greater than the length of the <paramref name="test"/> parameter.
        /// Additionally, this parameter cannot be less than or equal to 0.
        /// </param>
        /// <returns>If the function successfully executes its task, it returns the password located at position <paramref name="pos"/>.</returns>
        public static object[] GetPassByPos(long pos, object[] test, int min, int max)
        {
            listChecker(test, nameof(test));
            string[] strTestArray = test.ConvertToStringArray();
            getPassByPosE(strTestArray, pos, min, max);
            int len = getPassLen(strTestArray, pos, min, max, out long firstPos);
            string[][] subsets = GetSubsets(strTestArray, len, MillisecondsTimeout);
            return getPassByPos(getTest(subsets, strTestArray.Length, pos, min)!, pos - firstPos);
        }
        /// <summary>
        /// You can use this function to find the position of <paramref name="pass"/> among all possible cases.
        /// </summary>
        /// <param name="pass">The password you want to find its position</param>
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="min">Minimum password length. This should not be grater than <paramref name="test"/> length or smaller than or equals to 0</param>
        /// <returns>If the function successfully executes its task, it returns the position of <paramref name="pass"/></returns>
        public static long GetPosByPass(object[] pass, object[] test, int min)
        {
            listChecker(test, nameof(test));
            listChecker(pass, nameof(pass));
            string[] strTestArray = test.ConvertToStringArray();
            string[] strPassArray = pass.ConvertToStringArray();
            getPosByPassE(strTestArray, strPassArray, min);
            int[] passIndexes = new int[pass!.Length];
            long generated = getGeneratedPasswords(strTestArray, strPassArray, min, out int subsetsIndex, out string[][] subsets);
            int[] testIndexes = new int[subsets[subsetsIndex].Length];
            for (int i = 0; i < testIndexes.Length; ++i) testIndexes[i] = i;
            for (int i = 0; i < pass.Length; ++i) passIndexes[i] = Array.IndexOf(subsets[subsetsIndex], pass[i]);
            long permutationPos = getPosByPass(testIndexes, passIndexes);
            return permutationPos + generated;
        }
        /// <summary>
        /// This function calculates the maximum number of passwords that can be generated.
        /// </summary>
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="min">Minimum password length. This should not be grater than <paramref name="max"/> or smaller than or equals to 0</param>
        /// <param name="max">
        /// Maximum password length. In PermutationBrute, this parameter cannot be greater than the length of the <paramref name="test"/> parameter.
        /// Additionally, this parameter cannot be less than or equal to 0.
        /// </param>
        /// <returns>Returns the maximum number of passwords that can be generated.</returns>
        public static long GetMax(object[] test, int min, int max)
        {
            listChecker(test, nameof(test));
            string[] testArray = test.ConvertToStringArray();
            testMinMaxE(testArray, min, max);
            long Max = 0;
            for (int len = min; len <= max; ++len) Max += combination(len, testArray.Length) * longFactorial(len);
            return Max;
        }

        #region field_R
        /// <summary>
        /// When we want to calculate subsets of a set, it is possible that the RAM memory becomes full.
        /// To prevent this, we can set a time and if the subsets are not ready by that time, an error will occur and the process of finding subsets will be stopped.
        /// </summary>
        public static int MillisecondsTimeout = 10000;
        #endregion

        #endregion
        #region events_R
        /// <summary>
        /// This event occurs before the password generation process starts.
        /// </summary>
        public event delegate_OnStart? OnStart;
        /// <summary>
        /// When <see cref="BruteForce.StartPos"/> changes, this event occurs and the password generation process starts from the beginning.
        /// <see cref="BruteForce.StartPos"/> must be changed by another thread.
        /// </summary>
        public event delegate_OnStopOrRestart? OnRestart;
        /// <summary>
        /// This event occurs when the password generation process is completely halted.
        /// </summary>
        public event delegate_OnStopOrRestart? OnStop;
        /// <summary>
        ///  This event occurs when the password generation process finishes.
        /// </summary>
        public event delegate_OnEnd? OnEnd;
        /// <summary>
        /// This event occurs when the password generation process is paused.
        /// </summary>
        public event delegate_OnPauseOrResume? OnPause;
        /// <summary>
        /// This event occurs when a password is generated. This event should not be null.
        /// </summary>
        public event delegate_PasswordGenerated? PasswordGenerated;
        /// <summary>
        /// This event occurs when an error happens. In case the try/catch error handling approach is used, this event is not applicable.
        /// </summary>
        public event delegate_OnError? OnError;
        /// <summary>
        /// This event occurs when the paused process is resumed.
        /// </summary>
        public event delegate_OnPauseOrResume? OnResume;

        #endregion
        #endregion
        #region protected_R
        /// <summary>
        /// This function calculates and returns the total number of possible passwords that can be generated.
        /// </summary>
        protected override long CalculateTotal() => endPos - startPos + 1;
        /// <summary>
        /// When <see cref="BruteForce.StartPos"/> or <see cref="BruteForce.EndPos"/> change (while passwords are being generated), this function predicts and handles errors.
        /// </summary>
        protected override void NeedErrorsChecking() => PermutationBruteE(startPos, ref endPos, min, max, test);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        /// <summary>
        /// What do you think this function does? :)
        /// </summary>
        protected override void StartBrute()
        {//Inspired by this page: https://www.nayuki.io/page/next-lexicographical-permutation-algorithm
         // To generate passwords, we first need to get the password at StartPos and start generating from there.
         // Then, using the next permutation algorithm, we generate the rest of the passwords.
         // It is better to use int arrays instead of string arrays for faster generation.
         // To generate passwords of lengths from MinimumPasswordLength to MaximumPasswordLength,
         // we need to get subsets of Test array with min elements and calculate permutations of each of them.
         // This needs to be done for other sizes as well.
            object[]? pass;
            long generated;
            string[][] subsets;
            int passSubsetI = 0;
            total = CalculateTotal();
            started = true;
            OnStart?.Invoke(this);
        RESTART:
            pass = GetPassByPos(startPos, test, min, max);
            subsets = GetSubsets(test, pass.Length, MillisecondsTimeout);
            passSubsetI = getSubsetsIndex(subsets, pass.ConvertToStringArray());
            realPosition = startPos - 1;
            generated = 0;
            resetStopPause();
            bool helper = false;
            for (int len = pass.Length; len <= max; len++)
            {
                if (len != pass.Length)
                {
                    subsets = GetSubsets(test, len, MillisecondsTimeout);
                    passSubsetI = 0;
                }
                for (; passSubsetI < subsets.Length; ++passSubsetI)
                {
                    if (helper)
                        pass = subsets[passSubsetI];
                    helper = true;
                    if (WorkerFoundSomething) generated = total;
                    if (NeedToRestart()) goto RESTART;
                    if (OnEndOrStop()) return;
                    int[] indexes = ListConverter.ConvertToIndexArray(subsets[passSubsetI], pass);
                    while (nextPermutation(indexes))
                    {
                        pass = ListConverter.ConvertToStringArrayByIndex(subsets[passSubsetI], indexes);
                        if (WorkerFoundSomething) generated = total;
                        if (NeedToRestart()) goto RESTART;
                        if (OnEndOrStop()) return;
                    }
                }
            }
            //We don't need to call resetStopPause here...
            //Codes written here never be executed!
            //We terminate the function using the OnEnd (or OnStop) event...
            //If a statement is executed in this part, it indicates a bug!
            throw new UnexpectedException(0x5, "PermutationBrute.cs");
            //Local methods:
            bool NeedToRestart()
            {
                if (needToRestart)
                {
                    needToRestart = false;
                    OnRestart?.Invoke(this, pass);
                    return true;
                }
                return false;
            }
            bool checkStop()
            {
                if (Pause)
                {
                    OnPause?.Invoke(this, generated, total);
                    bool stoped = waitUntilPause() || this.stopped;
                    if (stoped) return true;
                    OnResume?.Invoke(this, generated, total);
                }
                return stopped;
            }
            bool Pgenerated()
            {
                generated++; realPosition++;
                return PasswordGenerated(this, pass!, generated, total);
            }
            bool OnEndOrStop()
            {
                if (WorkerFoundSomething) goto WFS;
                if (Pgenerated())
                {
                    OnEnd?.Invoke(this, pass, true);
                    started = false;
                    resetStopPause();
                    return true;
                }
                if (checkStop())
                {
                    OnStop?.Invoke(this, pass);
                    started = false;
                    resetStopPause();
                    return true;
                }
            WFS:
                if (realPosition >= endPos || generated == total)
                {
                    OnEnd?.Invoke(this, pass, false);
                    started = false;
                    resetStopPause();
                    return true;
                }
                return false;
            }
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        /// <summary>
        /// Starts the password generation process in a way that is manageable through the <see cref="OnError"/> event.
        /// </summary>
        protected override void StartBruteForceEvent()
        {
            try { StartBrute(); }
            catch (Exception ex)
            {
                resetStopPause();
                started = false;
                OnError?.Invoke(this, ex);
            }
        }

        #endregion
    }
}
