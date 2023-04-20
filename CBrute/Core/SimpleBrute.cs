using CBrute.Helper;
using System;
using System.Collections.Generic;
using static CBrute.Helper.CommonErrorChecker;

namespace CBrute.Core
{
    /// <summary>
    /// This class implements the simplest password generation method.
    /// Using this class, you can generate passwords like any other tool, but with the difference that strings and other elements can be combined.
    /// Use this class when you have the minimum information about the password.
    /// </summary>
    public sealed class SimpleBrute : BruteForce
    {
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
        /// </param>
        /// <param name="extraPassLengths">
        /// Put the sizes of passwords that you don't want to generate in this array. It can be null.
        /// Note that the size should be within the range [min, max].
        /// Also, it shouldn't be such that no password is generated!!
        /// </param>
        /// <param name="threadID">A unique identifier for each BruteForce instance in each thread.</param>
        public SimpleBrute(long startPos, long endPos, int minPassLen, int maxPassLen,
            object[] test, int[]? extraPassLengths = null, int threadID = -1)
        {
            minMaxE(minPassLen, maxPassLen);
            extraLengthsE(extraPassLengths, minPassLen, maxPassLen);
            SimpleBruteE(startPos, ref endPos, minPassLen, maxPassLen, test, extraPassLengths);
            this.startPos = startPos;
            this.endPos = endPos;
            this.min = minPassLen;
            this.max = maxPassLen;
            this.test = test;
            if (extraPassLengths != null)
                Array.Sort(extraPassLengths);
            this.extraLengths = extraPassLengths;
            this.threadID = threadID;
            //this.netID = netID;
        }
        #region errorChecking_R
        /// <summary>
        /// This function detects and reports the possible errors that may occur when instantiating the SimpleBrute class.
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos">If endPos is less than or equal to 0, it uses GetMax to determine the total number of possible states</param>
        /// <param name="min">The minimum password length.</param>
        /// <param name="max">The maximum password length.</param>
        /// <param name="test">This array contains the states that make up the passwords.</param>
        /// <param name="extraPassLengths"><see cref="ExtraLengths"/></param>
        internal static void SimpleBruteE(long startPos, ref long endPos, int min, int max, object[] test, int[]? extraPassLengths)
        {
            listChecker(test, nameof(test));//make sure "test" does is not null...
            testE(test);//make sure "test" has't any duplicate items...
            checkStartEnd(startPos, ref endPos, GetMax(test, min, max)/*We don't use extralengths here!*/);
            if (extraPassLengths == null) return;
            if (checkStartEndRange(startPos, endPos))
                throw new Exception($"According to {nameof(extraPassLengths)}, the range you selected cannot be generated!");
            //local methods: 
            bool checkStartEndRange(long startPos, long endPos)
            {
                long[] wrongRanges = reduceRanges(extraPassLengths, test, min, max);
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
        /// Checks for possible errors that may occur while working with the <see cref="GetPassByPos"/> function and reports them if any exist.
        /// </summary>
        private static void getPassByPosE(long pos, object[] test, int min, int max)
        {
            minMaxE(min, max);
            listChecker(test, nameof(test));
            long MAX = GetMax(test, min, max);
            if (pos <= 0 || pos > MAX) throw new ArgumentException
                    ($"The \"{nameof(pos)}\" cannot be greater than MAX(\"{MAX}\")" +
                    $" or smaller or equal to 0!",
                    nameof(pos));
            testE(test);
        }
        /// <summary>
        /// Checks for possible errors that may occur while working with the <see cref="GetMax"/> function and reports them if any exist.
        /// </summary>
        private static void getMaxE(int testLen, int min, int max)
        {
            if (testLen <= 0) throw new ArgumentException
                    ($"The \"{testLen}\" cannot be less than or equal to 0!", nameof(testLen));
            minMaxE(min, max);
        }
        /// <summary>
        ///  Checks for possible errors that may occur while working with the <see cref="GetPosByPass"/> function and reports them if any exist.
        /// </summary>
        private static void getPosByPassE(object[] pass, object[] test, int min, int max)
        {
            minMaxE(min, max);
            listChecker(pass, nameof(pass));
            listChecker(test, nameof(test));
            checkPass(pass, min, max);
            testE(test);
            passTestE(test, pass);
        }

        #endregion
        #region private_R
        #region field_R
        //Determines the actual position of a password in all possible cases.
        private long realPosition = -1;
        private readonly int[]? extraLengths;
        private readonly object[] test;

        #endregion
        #region private_static_R
        /// <summary>
        /// Assuming that you want to generate passwords with lengths from 3 to 10, but you do not want passwords with lengths 5, 6, or 7 to be generated.
        /// If you notice, 5, 6, and 7 are three consecutive numbers, and you want to exclude passwords with lengths ranging from 5 to 7.
        /// Therefore, instead of calculating the range for each of these lengths separately, it is better to consider the starting point of the first password with a length of 5 up to the last password with a length of 7 as a single unit.
        /// In other words, this function converts consecutive numbers into a unified range.
        /// </summary>
        private static long[] reduceRanges(int[] extraPassLengths, object[] test, int min, int max)
        {
            int[] extraLenArrayIndexRanges = Helper.Helper.ConsecFinder(extraPassLengths);
            long[] ranges = getExtraRanges(test, extraPassLengths, min, max);
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
        /// <summary>
        /// This function returns a set of ranges, each representing a password length.
        /// Each element with an even index represents "Start" and each element with an odd index represents "End".
        /// </summary>
        private static long[] getLengthRanges(object[] test, int min, int max)
        {
            int testLen = test.Length;
            int len = max - min + 1;
            long[] ranges = new long[len];
            ranges[0] = 0;
            for (int i = 1; i < len; ++i)
                ranges[i] = ranges[i - 1] + (long)Math.Pow(testLen, i - 1 + min);
            return ranges;
        }
        /// <summary>
        /// Returns the length of passowrd at <paramref name="pos"/>
        /// </summary>
        /// <param name="pos">The position at which you intend to obtain the size of the producible password.</param>
        /// <param name="test">This array contains the states that make up the passwords.</param>
        /// <param name="min">The minimum password length.</param>
        /// <param name="max">The maximum password length.</param>
        /// <param name="firstPos">Returns the position of the first password with the obtained length.</param>
        private static int getPassLen(long pos, object[] test, int min, int max, out long firstPos)
        {
            int testLen = test.Length;
            firstPos = 0;
            if (min == max) return max;
            if (pos < testLen) return min;
            long[] ranges = getLengthRanges(test, min, max);
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
        /// This function calculates the extra ranges using the <paramref name="extraLens"/> parameter.
        /// No password should be generated within these ranges.
        /// </summary>
        private static long[] getExtraRanges(object[] test, int[]? extraLens, int min, int max)
        {
            if (extraLens == null) return new long[0];
            long[] extraRanges = new long[extraLens.Length * 2];
            long[] ranges = getLengthRanges(test, min, max);
            int extraRIndex = 0;
            for (int ELIndex = 0; ELIndex < extraLens.Length; ++ELIndex)
            {
                int len = extraLens[ELIndex];
                if (len >= min && len <= max)//Better safe than sorry
                {
                    int rangesIndex = ranges.Length - (max - len) - 1;
                    extraRanges[extraRIndex++] = ranges[rangesIndex] + 1;
                    extraRanges[extraRIndex++] = GetMax(test, min, len);//bazeh baste
                }
            }
            return extraRanges;
        }

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
        /// Lengths that will never be generated.
        /// However, you can't change this property.
        /// </summary>
        public int[]? ExtraLengths { get => extraLengths; }
        /// <summary>
        /// This array contains the states that make up the passwords.
        /// However, you can't change this property.
        /// </summary>
        public object[] Test { get => test; }

        #endregion
        #region public_static_R
        /// <summary>
        /// This function calculates the maximum number of passwords that can be generated.
        /// </summary>
        /// <param name="test">
        /// This array contains the states that make up the passwords.
        /// Cannot be empty, contain duplicate elements, or contain null elements. 
        /// </param>
        /// <param name="min">Minimum password length. This should not be grater than <paramref name="max"/> or smaller than or equals to 0</param>
        /// <param name="max">Maximum password length.</param>
        /// <param name="extraLengths">
        /// Put the sizes of passwords that you don't want to generate in this array. It can be null.
        /// Note that the size should be within the range [min, max].
        /// Also, it shouldn't be such that no password is generated!!
        /// </param>
        /// <returns>Returns the maximum number of passwords that can be generated.</returns>
        public static long GetMax(object[] test, int min, int max, int[]? extraLengths = null)
        {
            listChecker(test, nameof(test));
            getMaxE(test.Length, min, max);
            extraLengthsE(extraLengths, min, max);
            long numberOfGeneratablePasswords = 0;
            for (int len = min; len <= max; len++)
                if (extraLengths != null && Array.IndexOf(extraLengths, len) != -1)
                    continue;
                else
                    numberOfGeneratablePasswords += (long)Math.Pow(test.Length, len);
            return numberOfGeneratablePasswords;
        }
        /// <summary>
        /// Using this function, you can obtain the password in all possible positions based on the actual position of that password.
        /// In other words, you need to know the actual position of that password. 
        /// </summary>
        /// <param name="pos">The actual position of the password that you want to obtain</param>
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="min">Minimum password length. This should not be grater than <paramref name="max"/> or smaller than or equals to 0</param>
        /// <param name="max">Maximum password length.</param>
        /// <returns>If the function successfully executes its task, it returns the password located at position <paramref name="pos"/>.</returns>
        public static object[] GetPassByPos(long pos, object[] test, int min, int max)
        {
            getPassByPosE(pos, test, min, max);
            int passLen = getPassLen(pos, test, min, max, out long firstPos);
            if (passLen < 0) throw new UnexpectedException(0x0, "SimpleBrute.cs");
            object[] pass = new object[passLen];
            long[] changes = new long[passLen];
            if (pos == firstPos)
            {
                pass.Reset(test[test.Length - 1]);
                return pass;
            }
            changes[changes.Length - 1] = pos - firstPos - 1;
            for (int i = passLen - 2; i >= 0; i--)
                changes[i] = changes[i + 1] / test.Length;
            for (int i = passLen - 1; i >= 0; i--)
            {
                int testIndex = (int)(changes[i] % test.Length);
                object testElement = test[testIndex];
                pass[i] = testElement;
            }
            return pass;
        }
        /// <summary>
        /// You can use this function to find the position of <paramref name="pass"/> among all possible cases.
        /// </summary>
        /// <param name="pass">The password you want to find its position</param>
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="min">Minimum password length. This should not be grater than the length of the <paramref name="pass"/> or smaller than or equals to 0</param>
        /// <returns>If the function successfully executes its task, it returns the position of <paramref name="pass"/></returns>
        public static long GetPosByPass(object[] pass, object[] test, int min)
        {
            getPosByPassE(pass, test, min, pass.Length);
            bool isLastPass = false;
            foreach (object obj in pass)
                if (!(isLastPass = obj.Equals(test[test.Length - 1]))) break;
            if (isLastPass) return GetMax(test, min, pass.Length);
            long position = 0;
            for (int passI = 0, reversePassI = pass.Length - 1;
                passI < pass.Length; passI++, reversePassI--)
            {
                long numberOfChanges = Array.IndexOf(test, pass[passI]);
                if (numberOfChanges < 0) throw new UnexpectedException(0x1,"SimpleBrute.cs");
                long calculated = (long)Math.Pow(test.Length, reversePassI);
                position += numberOfChanges * calculated;
            }
            long generated = 0;
            for (int i = pass.Length - 1; i >= min; i--)
                generated += (long)Math.Pow(test.Length, i);
            return position + generated + 1;
        }

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
        protected override void StartBrute()
        {//Inspired by this page: https://www.csharpstar.com/csharp-brute-force-algorithm
            object[]? pass;
            long generated;
            total = CalculateTotal();
            started = true;
            OnStart?.Invoke(this);
        RESTART:
            pass = GetPassByPos(startPos, test, min, max);
            generated = 0;
            realPosition = startPos - 1;
            resetStopPause();
            for (int len = pass.Length; len <= max; len++)
            {
                if (extraLengths != null && Array.IndexOf(extraLengths, len) != -1)
                {
                    realPosition = GetMax(test!, min, len);//Let's assume that we created the passwords
                    pass = null;
                    continue;//ignore this length
                }
                if (pass == null)
                {
                    pass = new object[len];
                    pass.Reset(test[0]);
                }
                if (WorkerFoundSomething) generated = total;
                if (NeedToRestart()) goto RESTART;
                if (OnEndOrStop()) return;
                for (int forChange = pass.Length - 1; forChange > -1; --forChange)
                {
                    int forRep;
                    for (forRep = Array.IndexOf(test, pass[forChange]) + 1;
                        forRep < test.Length; ++forRep)
                    {
                        pass[forChange] = test[forRep];
                        if (WorkerFoundSomething) generated = total;
                        if (NeedToRestart()) goto RESTART;
                        if (OnEndOrStop()) return;
                        for (int i = forChange + 1; i < pass.Length; ++i)
                        {
                            if (pass[i] != test[test.Length - 1])
                            {
                                forChange = len;
                                goto BREAK;
                            }
                        }
                    }
                BREAK:
                    if (forRep == test.Length)
                        pass[forChange] = test[0];
                }
                pass = null;//Reset password!
            }
            //We don't need to call resetStopPause here...
            //Codes written here never be executed!
            //We terminate the function using the OnEnd (or OnStop) event...
            //If a statement is executed in this part, it indicates a bug!
            throw new UnexpectedException(0x2, "SimpleBrute.cs");
            //Local methods:
            bool NeedToRestart()
            {
                if (needToRestart)
                {
                    needToRestart = false;
                    OnRestart?.Invoke(this, pass!);
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
        /// When StartPos or EndPos change (while passwords are being generated), this function predicts and handles errors.
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
        ///  When StartPos or EndPos change (while passwords are being generated), this function predicts and handles errors.
        /// </summary>
        protected override void NeedErrorsChecking() => SimpleBruteE(startPos, ref endPos, min, max, test!, extraLengths);
        /// <summary>
        /// This function calculates and returns the total number of possible passwords that can be generated.
        /// Unlike PermutationBrute, the situation here is a bit more complicated because of the <see cref="ExtraLengths"/> involved.
        /// </summary>
        protected override long CalculateTotal()
        {
            long ret = endPos - startPos + 1;
            if (extraLengths == null) return ret;
            long[] extraRanges = reduceRanges(extraLengths, test!, min, max);
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
