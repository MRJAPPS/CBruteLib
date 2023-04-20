using System;
using System.Collections.Generic;
using static CBrute.Helper.CommonErrorChecker;
namespace CBrute.Core
{
    /// <summary>
    /// This class is the most flexible class for password generation.
    /// Using this class, you can determine the constituent states of each position in the password regardless of the password length.
    /// In general, we use this class when we have more information about the password we want to obtain.
    /// However, this class is little bit slower than <see cref="SimpleBrute"/>.
    /// </summary>
    public sealed class ProBrute : BruteForce
    {
        #region PassTestInfo
        /// <summary>
        /// With this class, regardless of the password length, you can specify which objects sets each position of the password can include. This is only usable in the ProBrute class.
        /// </summary>
        public sealed class PassTestInfo
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            /// <summary>
            /// Constructor of the PassTestInfo class
            /// </summary>
            /// <param name="position">
            /// The position of an element in the password can be specified from the beginning or the end.
            /// Use numbers greater than or equal to 0 for distance from the beginning and use numbers less than 0 for distance from the end.
            /// Think of it as writing code in Python :) 
            /// The length of the password is not a concern, as Cbrute automatically detects what needs to be done.
            /// </param>
            /// <param name="testList">The set of states considered for position "<paramref name="position"/>" in the password</param>
            public PassTestInfo(int position, object[] testList)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            {
                Position = position;
                Test = testList;
            }
            #region private_R
            #region field_R
            private object[] test;
            private int position;
            #endregion
            #endregion
            #region public_R
            #region property_R
            /// <summary>
            /// The position of an element in the password can be specified from the beginning or the end.
            /// Use numbers greater than or equal to 0 for distance from the beginning and use numbers less than 0 for distance from the end.
            /// Think of it as writing code in Python :) 
            /// The length of the password is not a concern, as Cbrute automatically detects what needs to be done.
            /// </summary>
            public int Position
            {
                get => position;
                private set => position = value;
            }
            /// <summary>
            /// The set of states considered for position "<see cref="Position"/>" in the password
            /// </summary>
            public object[] Test
            {
                get => test;
                private set
                {
                    Helper.CommonErrorChecker.listChecker(value, nameof(test));
                    Helper.CommonErrorChecker.testE(value);
                    test = value;
                }
            }
            #endregion
            /// <summary>
            /// It calculates and returns the position that you previously specified in <see cref="Position"/> relative to a password with the size of <paramref name="length"/>.
            /// </summary>
            /// <param name="length">The length of password</param>
            /// <returns>Returns a position in the password</returns>
            public int GetPosition(int length)
            {
                int local_position = position;
                bool lastToFirst = local_position < 0;
                int ret;
                if (lastToFirst)
                {
                    local_position *= -1;
                    ret = length - local_position;
                    if (ret < length && ret >= 0) return ret; else return -1;
                }
                else
                {
                    ret = local_position;
                    if (ret < length && ret >= 0) return ret; else return -1;
                }
            }
            /// <summary>
            /// This method indicates whether two <see cref="PassTestInfo"/> objects are equal or not. The equality condition is based on the equality of their <see cref="Position"/> property.
            /// </summary>
            /// <param name="obj">An object</param>
            /// <returns>If the current object is equal to the <paramref name="obj"/> parameter, it returns true, otherwise false.</returns>
            public override bool Equals(object obj)
            {
                if (!(obj is PassTestInfo testInfo)) return false;
                return testInfo.position == this.position;
            }
            /// <summary>
            /// return <see cref="Position"/>.GetHashCode();
            /// </summary>
            /// <returns><see cref="Position"/>.GetHashCode();</returns>
            public override int GetHashCode()
            {
                return position.GetHashCode();
            }
            #endregion
        }
        #endregion
        /// <summary>
        /// Constructor of the ProBrute class
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
        /// <param name="maxPassLen">Maximum password length.</param>
        /// <param name="test">
        /// This array contains the states that make up the passwords.
        /// Cannot be empty, contain duplicate elements, or contain null elements.
        /// This array is used in positions that are not defined in <paramref name="testInfos"/>.
        /// </param>
        /// <param name="testInfos">
        /// By using this parameter, you can define more complex states for password generation.
        /// For example, you can specify that the last element in all passwords is always equal to "abcd",
        /// or you can specify that the eighth element in all passwords only contains the array {1,2,3}.
        /// Don't worry about the password length, as CBrute manages it. Can be null.
        /// </param>
        /// <param name="extraPassLengths">
        /// Put the sizes of passwords that you don't want to generate in this array. It can be null.
        /// Note that the size should be within the range [min, max].
        /// Also, it shouldn't be such that no password is generated!!
        /// </param>
        /// <param name="threadID">A unique identifier for each BruteForce instance in each thread.</param>
        public ProBrute(long startPos, long endPos, int minPassLen, int maxPassLen,
            object[] test, PassTestInfo[]? testInfos = null, int[]? extraPassLengths = null,
            int threadID = -1)
        {
            minMaxE(minPassLen, maxPassLen);
            extraLengthsE(extraPassLengths, minPassLen, maxPassLen);
            ProBruteE(startPos, ref endPos, minPassLen, maxPassLen, test, testInfos, extraPassLengths);
            this.startPos = startPos;
            this.endPos = endPos;
            this.min = minPassLen;
            this.max = maxPassLen;
            this.test = test;
            this.testInfos = testInfos;
            if (extraPassLengths != null)
                Array.Sort(extraPassLengths);
            this.extraLengths = extraPassLengths;
            this.threadID = threadID;
            //this.netID = netID;
        }
        #region errorChecking_R
        /// <summary>
        /// This function detects and reports the possible errors that may occur when instantiating the ProBrute class.
        /// </summary>
        /// <param name="startPos">determines the starting position for generating passwords.</param>
        /// <param name="endPos">If endPos is less than or equal to 0, it uses GetMax to determine the total number of possible states</param>
        /// <param name="min">The minimum password length.</param>
        /// <param name="max">The maximum password length.</param>
        /// <param name="test">This array contains the states that make up the passwords.</param>
        /// <param name="testInfos"><see cref="TestInfos"/></param>
        /// <param name="extraPassLengths"><see cref="ExtraLengths"/></param>
        internal static void ProBruteE(long startPos, ref long endPos, int min, int max, object[] test, PassTestInfo[]? testInfos,
            int[]? extraPassLengths)
        {
            passTestInfoE(testInfos!, test, min, max);//make sure "test" does is not null...
            testE(test);//make sure "test" has't any duplicate items...
            if (testInfos != null) testE(testInfos, nameof(testInfos));//make sure "testInfos" has't any duplicate items...
            checkStartEnd(startPos, ref endPos, GetMax(test, testInfos!, min, max)/*We don't use extralengths here!*/);
            if (extraPassLengths == null) return;
            if (checkStartEndRange(startPos, endPos))
                throw new Exception($"According to \"{nameof(extraPassLengths)}\", the range you selected cannot be generated!");
            //local methods:
            bool checkStartEndRange(long startPos, long endPos)
            {
                long[] wrongRanges = reduceRanges(extraPassLengths, testInfos, test, min, max);
                for (int i = 0; i < wrongRanges.Length; ++i)
                {
                    long start = wrongRanges[i];
                    long end = wrongRanges[i + 1];
                    if (startPos >= start && endPos <= end) return true;
                    ++i;
                }
                return false;
            }
        }
        /// <summary>
        /// This method checks for errors and ambiguities that may occur while working with <see cref="TestInfos"/> and reports them if any.
        /// Some of these errors are not serious and can be ignored.
        /// </summary>
        private static void passTestInfoE(PassTestInfo[]? testInfos, object[] test, int min, int max)
        {
            if (testInfos == null) goto testInfoIsNull;
            listChecker(testInfos, nameof(testInfos));
            if (!IgnoreTrivialErrors)
                for (int len = min; len <= max; ++len)//We check the {testInfos} based on the selected {min} and {max}.
                {
                    for (int selected = 0; selected < testInfos.Length; ++selected)
                    {
                        for (int i = 0; i < testInfos.Length; ++i)
                        {
                            if (i != selected && testInfos[selected].GetPosition(len) == testInfos[i].GetPosition(len))
                                throw new ArgumentException(
                                $"Generated \"{nameof(testInfos)}\" are ambiguous for passwords with \"{len}\" length!\n" +
                                $"Plase check \"{nameof(testInfos)}[{i}]\" and \"{nameof(testInfos)}[{selected}]\"."
                                , nameof(testInfos));
                            //In any case, you can ignore this error with proper prioritization(By Prioritize members of {testInfos})
                        }
                    }
                }
            testInfoIsNull:
            listChecker(test, nameof(test));
        }
        /// <summary>
        /// This method checks for possible errors that may occur while working with the <see cref="GetMax"/> function and reports them if any exist.
        /// </summary>
        private static void getMaxE(object[] test, PassTestInfo[]? testInfos, int min, int max, int[]? extraLengths = null)
        {
            minMaxE(min, max);
            passTestInfoE(testInfos, test, min, max);
            if (testInfos != null)
                testE(testInfos, nameof(testInfos));
            testE(test);
            extraLengthsE(extraLengths, min, max);
        }
        /// <summary>
        /// This method checks for possible errors that may occur while working with the GetPassByPos function and reports them if any exist.
        /// </summary>
        private static void getPassByPosE(long pos, object[] test, PassTestInfo[]? testInfos, int min, int max)
        {
            minMaxE(min, max);
            passTestInfoE(testInfos, test, min, max);
            long MAX = GetMax(test, testInfos, min, max);
            if (pos <= 0 || pos > MAX) throw new ArgumentException
                    ($"The \"{nameof(pos)}\" cannot be greater than MAX(\"{MAX}\")" +
                    $" or smaller or equal to 0!",
                    nameof(pos));
            testE(test);
            if (testInfos != null)
                testE(testInfos, nameof(testInfos));
        }
        /// <summary>
        /// If a password contains elements that are not present in testArray or testInfo, an error occurs.
        /// </summary>
        private static void checkPass(object[] pass, object[] test, PassTestInfo[]? testInfos)
        {
            object[][] testArrays = getTestArrays(testInfos, test, pass.Length);
            string emsg = "An invalid element was found in the password! Please check PassTestInfo and Test array.\n" +
                    "Make sure the password contains the elements you defined earlier.";
            for (int i = 0; i < testArrays.Length; ++i)
                if (Array.IndexOf(testArrays[i], pass[i]) == -1) throw new ArgumentException(emsg, nameof(pass));
        }
        /// <summary>
        /// Checks for possible errors that may occur while working with the <see cref="GetPosByPass"/> function and reports them if any exist.
        /// </summary>
        private static void getPosByPassE(object[] pass, object[] test, PassTestInfo[]? testInfos, int min, int max)
        {
            minMaxE(min, max);
            passTestInfoE(testInfos, test, min, max);
            testE(test);
            if (testInfos != null)
                testE(testInfos, nameof(testInfos));
            checkPass(pass, test, testInfos);

        }

        #endregion
        #region private_R
        #region field_R
        //Determines the actual position of a password in all possible cases.
        private long realPosition = -1;
        private readonly int[]? extraLengths;
        private readonly object[] test;
        private readonly PassTestInfo[]? testInfos;

        #endregion
        #region private_static_R
        /// <summary>
        /// This function calculates the possible states of each position for a password with length "<paramref name="length"/>"
        /// using <paramref name="testInfos"/> and testArray(<paramref name="default"/>) and returns a two-dimensional array.
        /// Each row of this array represents a position of the password.
        /// </summary>
        private static object[][] getTestArrays(PassTestInfo[]? testInfos, object[] @default, int length)
        {
            object[][] result = new object[length][];
            for (int len = 0; len < result.Length; len++)
            {
                result[len] = @default!;
                if (testInfos != null)
                    foreach (PassTestInfo info in testInfos)
                        if (info.GetPosition(length) == len)
                        {
                            result[len] = info.Test;
                            break;
                        }
            }
            return result;
        }
        /// <summary>
        /// This function returns a set of ranges, each representing a password length.
        /// Each element with an even index represents "Start" and each element with an odd index represents "End".
        /// </summary>
        private static long[] getLengthRanges(PassTestInfo[]? passTestInfos, object[] test, int min, int max)
        {
            int len = max - min + 1;
            long[] ranges = new long[len];
            ranges[0] = 0;
            for (int i = 1; i < len; ++i)
            {
                int passLen = min + i - 1;
                ranges[i] = ranges[i - 1] + GetMax(test, passTestInfos, passLen, passLen);
            }
            return ranges;
        }
        /// <summary>
        /// Returns the length of passowrd at <paramref name="pos"/>
        /// </summary>
        /// <param name="pos">The position at which you intend to obtain the size of the producible password.</param>
        /// <param name="passTestInfos"><see cref="TestInfos"/></param>
        /// <param name="test">This array contains the states that make up the passwords.</param>
        /// <param name="min">The minimum password length.</param>
        /// <param name="max">The maximum password length.</param>
        /// <param name="firstPos">Returns the position of the first password with the obtained length.</param>
        private static int getPassLen(long pos, PassTestInfo[]? passTestInfos, object[] test, int min, int max, out long firstPos)
        {
            firstPos = 0;
            if (min == max) return max;
            long[] ranges = getLengthRanges(passTestInfos, test, min, max);
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
        /// This function calculates the extra ranges using the <paramref name="extraLengths"/> parameter.
        /// No password should be generated within these ranges.
        /// </summary>
        private static long[] getExtraRanges(PassTestInfo[]? testInfos, object[] test, int[]? extraLengths, int min, int max)
        {
            if (extraLengths == null) return new long[0];
            long[] extraRanges = new long[extraLengths.Length * 2];
            long[] ranges = getLengthRanges(testInfos, test, min, max);
            int extraRIndex = 0;
            for (int ELIndex = 0; ELIndex < extraLengths.Length; ++ELIndex)
            {
                int len = extraLengths[ELIndex];
                if (len >= min && len <= max)//Better safe than sorry :)
                {
                    int rangesIndex = ranges.Length - (max - len) - 1;
                    extraRanges[extraRIndex++] = ranges[rangesIndex] + 1;
                    extraRanges[extraRIndex++] = GetMax(test, testInfos, min, len);
                }
            }
            return extraRanges;
        }
        /// <summary>
        /// Assuming that you want to generate passwords with lengths from 3 to 10, but you do not want passwords with lengths 5, 6, or 7 to be generated.
        /// If you notice, 5, 6, and 7 are three consecutive numbers, and you want to exclude passwords with lengths ranging from 5 to 7.
        /// Therefore, instead of calculating the range for each of these lengths separately, it is better to consider the starting point of the first password with a length of 5 up to the last password with a length of 7 as a single unit.
        /// In other words, this function converts consecutive numbers into a unified range.
        /// </summary>
        private static long[] reduceRanges(int[] extraPassLengths, PassTestInfo[]? testInfos, object[] test, int min, int max)
        {
            int[] extraLenArrayIndexRanges = Helper.Helper.ConsecFinder(extraPassLengths);
            long[] ranges = getExtraRanges(testInfos, test, extraPassLengths, min, max);
            List<long> reduceRanges = new List<long>(ranges.Length);
            for (int i = 0; i < extraLenArrayIndexRanges.Length; ++i)
            {
                long start = ranges[extraLenArrayIndexRanges[i] * 2];
                long end = ranges[extraLenArrayIndexRanges[i + 1] * 2 + 1];
                reduceRanges.Add(start);
                reduceRanges.Add(end);
                ++i;
            }
            return reduceRanges.ToArray();
        }

        #endregion
        #endregion
        #region public_R
        #region public_static_R
        /// <summary>
        /// This function calculates the maximum number of passwords that can be generated.
        /// </summary>
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="testInfos">
        /// By using this parameter, you can define more complex states for password generation.
        /// For example, you can specify that the last element in all passwords is always equal to "abcd",
        /// or you can specify that the eighth element in all passwords only contains the array {1,2,3}. Can be null.
        /// Don't worry about the password length, as CBrute manages it. 
        /// </param>
        /// <param name="min">Minimum password length. This should not be grater than <paramref name="max"/> or smaller than or equals to 0</param>
        /// <param name="max">Maximum password length.</param>
        /// <param name="extraLengths">
        /// Put the sizes of passwords that you want to ignore here. It can be null.
        /// Note that the size should be within the range of [min, max].
        /// </param>
        /// <returns>Returns the maximum number of passwords that can be generated.</returns>
        public static long GetMax(object[] test, PassTestInfo[]? testInfos, int min, int max, int[]? extraLengths = null)
        {
            getMaxE(test, testInfos, min, max, extraLengths);
            long result = 0;
            for (int len = min; len <= max; len++)
            {
                if (extraLengths != null && Array.IndexOf(extraLengths, len) != -1) continue;
                object[][] testArrays = getTestArrays(testInfos, test, len);
                long multiplication = 1;
                for (int i = 0; i < testArrays.Length; ++i)
                    multiplication *= testArrays[i].Length;
                result += multiplication;
            }
            return result;
        }
        /// <summary>
        /// Using this function, you can obtain the password in all possible positions based on the actual position of that password.
        /// In other words, you need to know the actual position of that password. 
        /// </summary>
        /// <param name="pos">The actual position of the password that you want to obtain</param>
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="testInfos">See -> <see cref="TestInfos"/></param>
        /// <param name="min">Minimum password length. This should not be grater than <paramref name="max"/> or smaller than or equals to 0</param>
        /// <param name="max">Maximum password length.</param>
        /// <returns>If the function successfully executes its task, it returns the password located at position <paramref name="pos"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0056:Use index operator", Justification = "<Pending>")]
        public static object[] GetPassByPos(long pos, object[] test, PassTestInfo[]? testInfos, int min, int max)
        {
            getPassByPosE(pos, test, testInfos, min, max);
            int passLen = getPassLen(pos, testInfos, test, min, max, out long firstPos);
            if (passLen < 0) throw new UnexpectedException(0x3,"ProBrute.cs");
            object[] password = new object[passLen];
            object[][] testArrays = getTestArrays(testInfos, test, passLen);
            long[] changes = new long[passLen];
            pos -= firstPos;
            pos--;
            changes[changes.Length - 1] = pos;
            for (int i = changes.Length - 2; i >= 0; --i)
                changes[i] = changes[i + 1] / testArrays[i + 1].Length;
            for (int i = password.Length - 1; i >= 0; --i)
            {
                password[i] = testArrays[i][changes[i] % testArrays[i].Length];
            }
            return password;
        }
        /// <summary>
        /// You can use this function to find the position of <paramref name="pass"/> among all possible cases.
        /// </summary>
        /// <param name="pass">The password you want to find its position</param>
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="testInfos">See -> <see cref="TestInfos"/></param>
        /// <param name="min">Minimum password length. This should not be grater than <paramref name="max"/> or smaller than or equals to 0</param>
        /// <param name="max">Maximum password length.</param>
        /// <returns>If the function successfully executes its task, it returns the position of <paramref name="pass"/></returns>
        public static long GetPosByPass(object[] pass, object[] test, PassTestInfo[]? testInfos, int min, int max)
        {
            getPosByPassE(pass, test, testInfos, min, max);
            long[] ranges = getLengthRanges(testInfos, test, min, max);
            long firstPos = ranges[pass.Length - min] + 1;
            long ret = 0;
            object[][] testArrays = getTestArrays(testInfos, test, pass.Length);
            for (int i = 0; i < pass.Length; ++i)
            {
                int changes = Array.IndexOf(testArrays[i], pass[i]);
                ret += changes * calcFrontChanges(testArrays, i + 1);
            }
            return ret + firstPos;
            //local methods:
            static long calcFrontChanges(object[][] testArrays, int startIndex)
            {
                long ret = 1;
                for (int index = startIndex; index < testArrays.Length; ++index)
                    ret *= testArrays[index].Length;
                return ret;
            }
        }

        #endregion
        #region property_R
        /// <summary>
        /// If you don't want the ambiguities that arise when working with testInfo to be considered as an error, set its value to True.
        /// </summary>
        public static bool IgnoreTrivialErrors { get; set; } = false;
        /// <summary>
        /// Determines the actual position of a password in all possible cases.
        /// </summary>
        public long RealPos { get => realPosition; }
        /// <summary>
        /// Lengths that will never be generated.
        /// </summary>
        public int[]? ExtraLengths { get => extraLengths; }
        /// <summary>
        /// This array contains the states that make up the passwords.
        /// Cannot be empty, contain duplicate elements, or contain null elements.
        /// This array is used in positions that are not defined in <see cref="TestInfos"/>.
        /// However, you can't change this property.
        /// </summary>
        public object[] Test { get => test; }
        /// <summary>
        /// By using this Property, you can define more complex states for password generation.
        /// For example, you can specify that the last element in all passwords is always equal to "abcd",
        /// or you can specify that the eighth element in all passwords only contains the array {1,2,3}.
        /// Don't worry about the password length, as CBrute manages it. Can be null.
        /// However, you can't change this property.
        /// </summary>
        public PassTestInfo[]? TestInfos { get => testInfos; }

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
        /// This event occurs when a password is generated. The password should not be null.
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        /// <summary>
        /// What do you think this function does? :)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0056:Use index operator", Justification = "<Pending>")]
        protected override void StartBrute()
        {//Inspired by this page: https://www.csharpstar.com/csharp-brute-force-algorithm
            object[]? pass;
            long generated;
            total = CalculateTotal();
            started = true;
            OnStart?.Invoke(this);
        RESTART:
            pass = GetPassByPos(startPos, test, testInfos, min, max);
            generated = 0;
            realPosition = startPos - 1;
            resetStopPause();
            for (int len = pass.Length; len <= max; len++)
            {
                if (extraLengths != null && Array.IndexOf(extraLengths, len) != -1)
                {
                    realPosition = GetMax(test!, testInfos!, min, len);//Let's assume that we created the passwords :)
                    pass = null;
                    continue;//ignore this length
                }
                object[][] testArrays = getTestArrays(testInfos, test, len);
                if (pass == null)
                {//reset password
                    pass = new object[testArrays.Length];
                    for (int i = 0; i < pass.Length; ++i)
                        pass[i] = testArrays[i][0];
                }
                if (WorkerFoundSomething) generated = total;
                if (NeedToRestart()) goto RESTART;
                if (OnEndOrStop()) return;
                for (int forChange = pass.Length - 1; forChange > -1; --forChange)
                {
                    int forRep;
                    for (forRep = Array.IndexOf(testArrays[forChange], pass[forChange]) + 1;
                        forRep < testArrays[forChange].Length; ++forRep)
                    {
                        pass[forChange] = testArrays[forChange][forRep];
                        if (WorkerFoundSomething) generated = total;
                        if (NeedToRestart()) goto RESTART;
                        if (OnEndOrStop()) return;
                        for (int i = forChange + 1; i < pass.Length; ++i)
                        {
                            if (pass[i] != testArrays[forChange][testArrays[forChange].Length - 1])
                            {
                                forChange = len;
                                goto BREAK;
                            }
                        }
                    }
                BREAK:
                    if (forChange != len && forRep == testArrays[forChange].Length)
                        pass[forChange] = testArrays[forChange][0];
                }
                pass = null;//Reset
            }
            //We don't need to call resetStopPause here...
            //Codes written here never be executed!
            //We terminate the function using the OnEnd (or OnStop) event...
            //If a statement is executed in this part, it indicates a bug!
            throw new UnexpectedException(0x4,"ProBrute.cs");
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
                return this.stopped;
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
                if (realPosition >= endPos || total == generated)
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
        /// <summary>
        /// When StartPos or EndPos change (while passwords are being generated), this function predicts and handles errors.
        /// </summary>
        protected override void NeedErrorsChecking() => ProBruteE(startPos, ref endPos, min, max, test, TestInfos, extraLengths);
        /// <summary>
        /// This function calculates and returns the total number of possible passwords that can be generated.
        /// Unlike PermutationBrute, the situation here is a bit more complicated because of the extraLengths involved.
        /// </summary>
        protected override long CalculateTotal()
        {
            long ret = endPos - startPos + 1;
            if (extraLengths == null) return ret;
            long[] extraRanges = reduceRanges(extraLengths, testInfos, test, min, max);
            for (int i = 0; i < extraRanges.Length; ++i)
            {
                if ((extraRanges[i] >= startPos && extraRanges[i + 1] <= endPos))
                {
                    ret -= (extraRanges[i + 1] - extraRanges[i]) + 1;
                    goto CONTIUNE;
                }
                if ((extraRanges[i] < startPos && extraRanges[i + 1] >= startPos && extraRanges[i + 1] < endPos))
                {
                    ret -= (extraRanges[i + 1] - startPos) + 1;
                    goto CONTIUNE;
                }
                if ((extraRanges[i] > startPos && extraRanges[i] <= endPos && extraRanges[i + 1] > endPos))
                {
                    ret -= (endPos - extraRanges[i]) + 1;
                    goto CONTIUNE;
                }
            CONTIUNE: ++i;
            }
            return ret;
            //local methods:
            //bool isInRange(long extraStartPos, long extraEndPos) => 
            //    (extraStartPos >= StartPos && extraEndPos <= EndPos);
            //bool isBeginExtra(long extraStartPos, long extraEndPos) =>
            //    (extraStartPos < StartPos && extraEndPos >= StartPos);
            //bool isEndExtra(long extraStartPos, long extraEndPos) =>
            //    (extraStartPos > StartPos && extraEndPos > EndPos);
        }

        #endregion
    }
}
