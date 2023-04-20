using CBrute;
using CBrute.Core;
using CBrute.Helper;
using CBrute.Worker;
using System.Diagnostics;
using System.Reflection;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args) => new Program().main();
        const string ERROR = "NOOOOO! GOOOOD!!! NOOOOO!";
        const string OK = "<<<Looks like it's OK!>>>\nPress any key...";
        readonly object locker = new();
        readonly ConsoleColor default_foreground = Console.ForegroundColor,
            default_background = Console.BackgroundColor;

        static void changeConsoleColor(ConsoleColor foreground, ConsoleColor background)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
        }
        void resetColors()
        {
            Console.ForegroundColor = default_foreground;
            Console.BackgroundColor = default_background;
        }
        #region TestFunctions
        void SBGetPassOrPos_By_PosOrPassT()
        {
            Console.Title = nameof(SBGetPassOrPos_By_PosOrPassT);
            int min = 3, max = 8;
            object[] test = new object[] { 1, 2, 3, 4, 5 };
            long numOfGeneratablePasswords = SimpleBrute.GetMax(test, min, max);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (long pos = 0; pos < numOfGeneratablePasswords; pos++)
            {
                changeConsoleColor(ConsoleColor.Black, ConsoleColor.DarkGray);
                long temp = pos + 1;
                object[] pass = SimpleBrute.GetPassByPos(temp, test, min, max);
                long passPos = SimpleBrute.GetPosByPass(pass, test, min);
                if (passPos != temp)
                    throw new Exception($"{ERROR}\n{nameof(SBGetPassOrPos_By_PosOrPassT)}");
                string password = pass.ConvertStringArrayToString();
                Console.Write($"{string.Format("{0,4}", passPos)}=>" +
                    $" {string.Format($"{'{'}0,{max}{'}'}", password)}");
                resetColors();
                Console.WriteLine();
            }
            stopwatch.Stop();
            resetColors();
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
        }
        void SBSingleThreadT(int[]? extraLenList, long start, long end)
        {
            Console.Title = nameof(SBSingleThreadT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            object[] test = new object[] { 1, 2, 3, 4, 5 };
            int min = 3, max = 9;
            //Console.WriteLine(ListConverter.ConvertStringArrayToString(
            //    SimpleBrute.GetPassByPos(3875, test, min, max)
            //    ));
            //return;
            SimpleBrute brute = new(start, end, min, max, test, extraLenList);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                string password = pass.ConvertStringArrayToString();
                SimpleBrute senderSB = (SimpleBrute)sender;
                Console.Write($"{string.Format("{0,5}", senderSB.RealPos)}=>" +
                     $" {string.Format($"{'{'}0,{max}{'}'}", password)} ");
                Console.WriteLine($"{generated}/{total}");
                object[] passItems = SimpleBrute.GetPassByPos(senderSB.RealPos, test, min, max);
                string passForCheck = passItems.ConvertStringArrayToString();
                if (!password.Equals(passForCheck) ||
                senderSB.RealPos != SimpleBrute.GetPosByPass(passItems, test, min)
                || generated > total) throw new Exception(ERROR);
                return false;
            };
            Stopwatch stopwatch = Stopwatch.StartNew();
            brute.Start();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
            resetColors();
        }
        void SBPauseStopT(int[]? extraLenList, long start, long end)
        {
            Console.Title = nameof(SBPauseStopT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            object[] test = new object[] { 1, 2, 3, 4, 5, 7 };
            int min = 3, max = 6;
            SimpleBrute brute = new(start, end, min, max, test, extraLenList);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                string password = pass.ConvertStringArrayToString();
                SimpleBrute senderSB = (SimpleBrute)sender;
                Console.Write($"{string.Format("{0,5}", senderSB.RealPos)}=>" +
                     $" {string.Format($"{'{'}0,{max}{'}'}", password)} ");
                Console.WriteLine($"{generated}/{total}");
                object[] passItems = SimpleBrute.GetPassByPos(senderSB.RealPos, test, min, max);
                string passForCheck = passItems.ConvertStringArrayToString();
                if (!password.Equals(passForCheck) ||
                senderSB.RealPos != SimpleBrute.GetPosByPass(passItems, test, min)
                || generated > total) throw new Exception(ERROR);
                return false;
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("Paused");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            brute.OnStop += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("Stoped");
                resetColors();
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
            };
            brute.OnResume += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("Resumed");
                new Thread(() =>
                {
                    Thread.Sleep(555);
                    sender.Stop();
                }).Start();
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            new Thread(() =>
            {
                brute.Start();
            }).Start();
            Thread.Sleep(1000);
            brute.Pause = true;
            for (int i = 0; i < 5; i++)
            {
                Console.Beep();
                Thread.Sleep(1000 - 20);//-20: To make up for the 20 milliseconds wasted in "waitUntil Pause" method
            }
            brute.Pause = false;

        }
        void SBMultithreadChangeEndPosT(int[]? extraLenList, long start, long end)
        {
            Console.Title = nameof(SBMultithreadChangeEndPosT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            object[] test = new object[] { 1, 2, 3, 4, 5, 7 };
            int min = 3, max = 12;
            SimpleBrute brute = new(start, end, min, max, test, extraLenList);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                SimpleBrute B = ((SimpleBrute)sender);
                string password = pass.ConvertStringArrayToString();
                string passOnPos = SimpleBrute.GetPassByPos(B.RealPos, B.Test, B.MinimumPassLength, B.MaximumPassLength).ConvertStringArrayToString();
                Console.Write($"/G {generated} /T {total} __ {(int)((generated / (double)total) * 100D)}%");
                Console.WriteLine($"   {password} _\'\'_ {passOnPos}");
                if (!password.Equals(passOnPos))
                {
                    Console.WriteLine(ERROR);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                }
                Thread.Sleep(1);
                return false;
            };
            brute.OnEnd += (BruteForce sender, object[] pass, bool result) =>
            {
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
                resetColors();
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                if (sender.Pause) Console.WriteLine("PAUSED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            new Thread(() => brute.Start()).Start();
            Thread.Sleep(3000);
            brute.EndPos = brute.RealPos + 100;//The work is done faster
            //brute.EndPos = brute.RealPos - 100;//Generate passwords ends
            //brute.EndPos++;//It causes an error
        }
        void SBMultithreadChangeStartPosT(int[]? extraLenList, long start, long end)
        {
            Console.Title = nameof(SBMultithreadChangeStartPosT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            object[] test = new object[] { 1, 2, 3, 4, 5, 7 };
            int min = 3, max = 12;
            SimpleBrute brute = new(start, end, min, max, test, extraLenList);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                SimpleBrute B = ((SimpleBrute)sender);
                string password = pass.ConvertStringArrayToString();
                string passOnPos = SimpleBrute.GetPassByPos(B.RealPos, B.Test, B.MinimumPassLength, B.MaximumPassLength).ConvertStringArrayToString();
                Console.Write($"/G {generated} /T {total} __ {(int)((generated / (double)total) * 100D)}%");
                Console.WriteLine($"   {password} _\'\'_ {passOnPos}");
                if (!password.Equals(passOnPos))
                {
                    Console.WriteLine(ERROR);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                }
                Thread.Sleep(1);
                return false;
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                if (sender.Pause) Console.WriteLine("PAUSED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            brute.OnRestart += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("RESTARTED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            brute.OnStop += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Blue, default_background);
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
                resetColors();
            };
            new Thread(() => brute.Start()).Start();
            Thread.Sleep(3000);
            brute.StartPos = 1000;
            Thread.Sleep(5000);
            brute.Stop();
            //brute.StartPos = brute.EndPos;
            //brute.StartPos = 0;//It causes an error
        }
        void SBMultithreadChangesStartPosAndEndPosAtSameTime(int[]? extraLenList, long start, long end)
        {
            Console.Title = nameof(SBMultithreadChangesStartPosAndEndPosAtSameTime);
            changeConsoleColor(ConsoleColor.Green, default_background);
            object[] test = new object[] { 1, 2, 3, 4, 5, 7 };
            int min = 3, max = 12;
            SimpleBrute brute = new(start, end, min, max, test, extraLenList);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                SimpleBrute B = ((SimpleBrute)sender);
                string password = pass.ConvertStringArrayToString();
                string passOnPos = SimpleBrute.GetPassByPos(B.RealPos, B.Test, B.MinimumPassLength, B.MaximumPassLength).ConvertStringArrayToString();
                Console.Write($"/G {generated} /T {total} __ {(int)((generated / (double)total) * 100D)}%");
                Console.WriteLine($"   {password} _\'\'_ {passOnPos}");
                if (!password.Equals(passOnPos))
                {
                    Console.WriteLine(ERROR);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                }
                Thread.Sleep(1);
                return false;
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                if (sender.Pause) Console.WriteLine("PAUSED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            brute.OnRestart += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("RESTARTED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            brute.OnStop += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Blue, default_background);
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
                resetColors();
            };
            brute.OnEnd += (BruteForce sender, object[] pass, bool result) =>
            {
                changeConsoleColor(ConsoleColor.Blue, default_background);
                Console.WriteLine("OnEnd");
                changeConsoleColor(ConsoleColor.Green, default_background);
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
                resetColors();
            };
            new Thread(() => brute.Start()).Start();
            Thread.Sleep(3000);
            //This work is not recommended at all!
            new Thread(() => brute.EndPos = brute.RealPos + 1000).Start();
            new Thread(() => brute.StartPos = 1000).Start();
            Thread.Sleep(5000);
            brute.Stop();
        }

        static void SBSingleThreadSpeedTest(long start, long end, int min, int max, object[] test)
        {
            SimpleBrute brute = new(start, end, min, max, test);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                Console.Title = (((double)generated / total) * 100D).ToString("0.00");
                return false;
            };
            Stopwatch stopwatch = Stopwatch.StartNew();
            brute.Start();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
        }
        void PassTestInfoT()
        {
            Console.Title = nameof(PassTestInfoT);
            object[] testArray = new object[4] { 1, 2, 3, 4 };
            ProBrute.PassTestInfo testInfo;
            testInfo = new ProBrute.PassTestInfo(7, testArray);
            Console.WriteLine(testInfo.GetPosition(8));//7
            testInfo = new ProBrute.PassTestInfo(-1, testArray);
            Console.WriteLine(testInfo.GetPosition(9));//8
            testInfo = new ProBrute.PassTestInfo(-3, testArray);
            Console.WriteLine(testInfo.GetPosition(8));//5
        }
        void PBgetTestArraysT()
        {
            Console.Title = nameof(PBgetTestArraysT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            MethodInfo? getTestArrays = typeof(ProBrute).GetMethod("getTestArrays", BindingFlags.Static | BindingFlags.NonPublic);
            ProBrute.PassTestInfo[] infos = new ProBrute.PassTestInfo[6]
            {
                new ProBrute.PassTestInfo(-1,new object[]{1,2,3 }),
                new ProBrute.PassTestInfo(0,new object[]{4,5,6}),
                new ProBrute.PassTestInfo(1,new object[]{'_'}),
                new ProBrute.PassTestInfo(2,new object[]{0}),
                new ProBrute.PassTestInfo(3,new object[]{'*','='}),
                new ProBrute.PassTestInfo(4,new object[]{'/','-','='})
            };
            object[] @default = new object[] { 9, 8, 7 };
            int len = 8;
            object[][]? tests = (object[][]?)getTestArrays?.Invoke(null, new object[] { infos, @default, len });
            if (tests == null)
            {
                Console.WriteLine(ERROR);
                Console.ReadKey();
                resetColors();
                return;
            }
            for (int i = 0; i < tests.Length; i++)
            {
                Console.Write(i + ". ");
                for (int j = 0; j < tests[i].Length; j++)
                {
                    Console.Write(tests[i][j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
        }
        void PBgetMaxT()
        {
            Console.Title = nameof(PBgetMaxT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            ProBrute.PassTestInfo[] infos = {
            new ProBrute.PassTestInfo(0,new object[]{1,2,3}),
            new ProBrute.PassTestInfo(4,new object[]{5,6,7}),
            new ProBrute.PassTestInfo(-1,new object[]{6,7,5,9}),
            };
            object[] test = new object[] { 9, 10, 25 };
            int min = 4, max = 6;
            int[] extra = { 5 };
            long value = ProBrute.GetMax(test, infos, min, max, extra);
            resetColors();
            Console.WriteLine(value);
            Console.WriteLine(value == 1080 ? OK : ERROR);
            Console.ReadKey();
            Console.Clear();
        }
        void PBgetRangesT()
        {
            string funName = "getLengthRanges";
            Console.Title = nameof(PBgetRangesT);
            changeConsoleColor(ConsoleColor.Blue, default_background);
            ProBrute.PassTestInfo[] infos = { new ProBrute.PassTestInfo(-1, new object[] { 1, 2, 3 }) };
            object[] test = new object[] { 9, 10, 40 };
            int min = 4, max = 6;
            MethodInfo? getLengthRanges = typeof(ProBrute).GetMethod(funName, BindingFlags.NonPublic | BindingFlags.Static);
            if (getLengthRanges == null)
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("WTF!?");
                return;
            }
            long[] ranges = (long[])getLengthRanges.Invoke(null,
                new object[] { infos, test, min, max })!;
            foreach (var range in ranges)
            {
                Console.WriteLine(range);
            }
            Console.WriteLine(OK);
            resetColors();
            Console.ReadKey();
        }
        void PBgetPassOrPos_By_PosOrPassT()
        {
            Console.Title = nameof(PBgetPassOrPos_By_PosOrPassT);
            int min = 3, max = 8;
            object[] test = new object[] { 1, 2, 3, 4, 5 };
            ProBrute.PassTestInfo[] infos = { new ProBrute.PassTestInfo(-1, new object[] { 78, 92 }), new ProBrute.PassTestInfo(1, new object[] { "List", "MRJ" }) };
            long numOfGeneratablePasswords = ProBrute.GetMax(test, infos, min, max);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (long pos = 0; pos < numOfGeneratablePasswords; pos++)
            {
                changeConsoleColor(ConsoleColor.Black, ConsoleColor.DarkGray);
                long temp = pos + 1;
                object[] pass = ProBrute.GetPassByPos(temp, test, infos, min, max);
                long passPos = ProBrute.GetPosByPass(pass, test, infos, min, max);
                if (passPos != temp)
                    throw new Exception($"{ERROR}\n{nameof(PBgetPassOrPos_By_PosOrPassT)}");
                string password = pass.ConvertStringArrayToString();
                Console.Write($"{string.Format("{0,4}", passPos)}=>" +
                    $" {string.Format($"{'{'}0,{max}{'}'}", password)}");
                resetColors();
                Console.WriteLine();
            }
            stopwatch.Stop();
            resetColors();
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
        }
        void PBSingleThreadT(int[]? extra, long start, long end)
        {
            Console.Title = nameof(PBSingleThreadT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            object[] test = { " HHHH ", " 3333 ", " VVV ", " Java ", " Gholam " };
            ProBrute.PassTestInfo[] testInfos = { new ProBrute.PassTestInfo(-1, new object[] { " 13 " }), new ProBrute.PassTestInfo(0, new object[] { " KKKK " }) };
            int min = 3, max = 9;
            ProBrute brute = new(start, end, min, max, test, testInfos, extra);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                string password = pass.ConvertStringArrayToString();
                ProBrute senderPB = (ProBrute)sender;
                Console.Write($"{string.Format("{0,5}", senderPB.RealPos)}=>" +
                     $" {string.Format($"{'{'}0,{max}{'}'}", password)} ");
                Console.WriteLine($"{generated}/{total}");
                object[] passItems = ProBrute.GetPassByPos(senderPB.RealPos, senderPB.Test, senderPB.TestInfos, min, max);
                string passForCheck = passItems.ConvertStringArrayToString();
                if (!password.Equals(passForCheck) ||
                senderPB.RealPos != ProBrute.GetPosByPass(passItems, senderPB.Test, senderPB.TestInfos, min, max)
                || generated > total)
                    throw new Exception(ERROR);
                return false;
            };
            Stopwatch stopwatch = Stopwatch.StartNew();
            brute.Start();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
            resetColors();
        }
        void PBPauseStopT(int[]? extra, long start, long end)
        {
            Console.Title = nameof(PBPauseStopT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            object[] test = { " Pishro ", " Ho3in ", " Putaker ", " Java ", " Gholam " };
            ProBrute.PassTestInfo[] testInfos = { new ProBrute.PassTestInfo(-1, new object[] { " 13 " }), new ProBrute.PassTestInfo(0, new object[] { " Hichkas " }) };
            int min = 3, max = 9;
            ProBrute brute = new(start, end, min, max, test, testInfos, extra);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                string password = pass.ConvertStringArrayToString();
                ProBrute senderPB = (ProBrute)sender;
                Console.Write($"{string.Format("{0,5}", senderPB.RealPos)}=>" +
                     $" {string.Format($"{'{'}0,{max}{'}'}", password)} ");
                Console.WriteLine($"{generated}/{total}");
                object[] passItems = ProBrute.GetPassByPos(senderPB.RealPos, senderPB.Test, senderPB.TestInfos, min, max);
                string passForCheck = passItems.ConvertStringArrayToString();
                if (!password.Equals(passForCheck) ||
                senderPB.RealPos != ProBrute.GetPosByPass(passItems, senderPB.Test, senderPB.TestInfos, min, max)
                || generated > total)
                    throw new Exception(ERROR);
                return false;
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("Paused");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            brute.OnStop += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("Stoped");
                resetColors();
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
            };
            brute.OnResume += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("Resumed");
                new Thread(() =>
                {
                    Thread.Sleep(1555);
                    sender.Stop();
                }).Start();
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            new Thread(() =>
            {
                brute.Start();
            }).Start();
            Thread.Sleep(1000);
            brute.Pause = true;
            for (int i = 0; i < 5; i++)
            {
                Console.Beep();
                Thread.Sleep(1000 - 20);//-20: To make up for the 20 milliseconds wasted in "waitUntil Pause" method
            }
            brute.Pause = false;
        }
        void PBMultithreadChangeEndPosT(int[]? extraLenList, long start, long end)
        {
            Console.Title = nameof(PBMultithreadChangeEndPosT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            object[] test = new object[] { 1, 2, 3, 4, 5, 7 };
            ProBrute.PassTestInfo[] testInfos = { new ProBrute.PassTestInfo(-1, new object[] { "Test" }), new ProBrute.PassTestInfo(2, new object[] { '@' }) };
            int min = 3, max = 12;
            ProBrute brute = new(start, end, min, max, test, testInfos, extraLenList);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                ProBrute B = ((ProBrute)sender);
                string password = pass.ConvertStringArrayToString();
                string passOnPos = ProBrute.GetPassByPos(B.RealPos, B.Test, B.TestInfos, B.MinimumPassLength, B.MaximumPassLength).ConvertStringArrayToString();
                Console.Write($"/G {generated} /T {total} __ {(int)((generated / (double)total) * 100D)}%");
                Console.WriteLine($"   {password} _\'\'_ {passOnPos}");
                if (!password.Equals(passOnPos))
                {
                    Console.WriteLine(ERROR);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                }
                Thread.Sleep(1);
                return false;
            };
            brute.OnEnd += (BruteForce sender, object[] pass, bool result) =>
            {
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
                resetColors();
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                if (sender.Pause) Console.WriteLine("PAUSED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            new Thread(() => brute.Start()).Start();
            Thread.Sleep(3100);
            brute.EndPos = brute.RealPos + 100;//The work is done faster
            //brute.EndPos = brute.RealPos - 100;//Generate passwords ends
            //brute.EndPos++;//It causes an error
        }
        void PBMultithreadChangeStartPosT(int[]? extraLenList, long start, long end)
        {
            Console.Title = nameof(PBMultithreadChangeStartPosT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            object[] test = new object[] { 1, 2, 3, 4, 5, 7 };
            ProBrute.PassTestInfo[] testInfos = { new ProBrute.PassTestInfo(-1, new object[] { "Test" }), new ProBrute.PassTestInfo(2, new object[] { '@' }) };
            int min = 3, max = 12;
            ProBrute brute = new(start, end, min, max, test, testInfos, extraLenList);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                ProBrute B = ((ProBrute)sender);
                string password = pass.ConvertStringArrayToString();
                string passOnPos = ProBrute.GetPassByPos(B.RealPos, B.Test, B.TestInfos, B.MinimumPassLength, B.MaximumPassLength).ConvertStringArrayToString();
                Console.Write($"/G {generated} /T {total} __ {(int)((generated / (double)total) * 100D)}%");
                Console.WriteLine($"   {password} _\'\'_ {passOnPos}");
                if (!password.Equals(passOnPos))
                {
                    Console.WriteLine(ERROR);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                }
                Thread.Sleep(1);
                return false;
            };
            brute.OnStop += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Blue, default_background);
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
                resetColors();
            };
            brute.OnRestart += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("RESTARTED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                if (sender.Pause) Console.WriteLine("PAUSED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            new Thread(() => brute.Start()).Start();
            Thread.Sleep(3100);
            brute.StartPos = 1000;
            Thread.Sleep(5000);
            brute.Stop();
            //brute.StartPos = brute.EndPos;
            //brute.StartPos = 0;//It causes an error
        }

        static void PBSingleThreadSpeedTest(long start, long end, int min, int max)
        {
            object[] test = new object[] { 1, 2, 3, 4, 5, 7, 8 };
            ProBrute.PassTestInfo[] testInfos = { new ProBrute.PassTestInfo(-1, new object[] { "Test" }), new ProBrute.PassTestInfo(2, new object[] { '@' }) };
            ProBrute brute = new(start, end, min, max, test, testInfos);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                Console.Title = (((double)generated / total) * 100D).ToString("0.00");
                return false;
            };
            Stopwatch stopwatch = Stopwatch.StartNew();
            brute.Start();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
        }
        void PEBgetPassOrPos_By_PosOrPassT()
        {
            Console.Title = nameof(PEBgetPassOrPos_By_PosOrPassT);
            int min = 1, max = 9, lastPassLen = min - 1;
            object[] test = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            long numOfGeneratablePasswords = PermutationBrute.GetMax(test, min, max);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (long pos = 0; pos < numOfGeneratablePasswords; pos++)
            {
                long realPos = pos + 1;
                object[] pass = PermutationBrute.GetPassByPos(realPos, test, min, max);
                long passPos = PermutationBrute.GetPosByPass(pass, test, min);
                if (passPos != realPos)
                    throw new Exception($"{ERROR}\n{nameof(PEBgetPassOrPos_By_PosOrPassT)}");
                if (lastPassLen != pass.Length)
                {
                    lastPassLen = pass.Length;
                    Console.WriteLine($"LEN => {lastPassLen}");
                }
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);//00:10:52.4655505
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
        }
        void PEBSingleThreadT(long start = 1, long end = 0)
        {
            int min = 1, max = 7;
            object[] test = { 1, "sfksnfs", "rtgfdfvlksolek", "MRJ", "I", "AM", 77, "o0o", 9 };
            Console.Title = nameof(PEBSingleThreadT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            PermutationBrute brute = new(start, end, min, max, test);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                string password = pass.ConvertStringArrayToString();
                PermutationBrute senderPEB = (PermutationBrute)sender;
                Console.Write($"{string.Format("{0,5}", senderPEB.RealPos)}=>" +
                     $" {string.Format($"{'{'}0,{max}{'}'}", password)} ");
                Console.WriteLine($"{generated}/{total}");
                object[] passItems = PermutationBrute.GetPassByPos(senderPEB.RealPos, test, min, max);
                string passForCheck = passItems.ConvertStringArrayToString();
                if (!password.Equals(passForCheck) ||
                senderPEB.RealPos != PermutationBrute.GetPosByPass(passItems, senderPEB.Test, min)
                || generated > total)
                    throw new Exception(ERROR);
                return false;
            };
            Stopwatch stopwatch = Stopwatch.StartNew();
            brute.Start();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
            resetColors();
        }
        void PEBPauseStopT(long start = 1, long end = 0)
        {
            Console.Title = nameof(PBPauseStopT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            int min = 1, max = 7;
            object[] test = { 1, "sfksnfs", "rtgfdfvlksolek", "MRJ", "I", "AM", 77, "o0o", 9 };
            PermutationBrute brute = new(start, end, min, max, test);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                string password = pass.ConvertStringArrayToString();
                PermutationBrute senderPEB = (PermutationBrute)sender;
                Console.Write($"{string.Format("{0,5}", senderPEB.RealPos)}=>" +
                     $" {string.Format($"{'{'}0,{max}{'}'}", password)} ");
                Console.WriteLine($"{generated}/{total}");
                object[] passItems = PermutationBrute.GetPassByPos(senderPEB.RealPos, test, min, max);
                string passForCheck = passItems.ConvertStringArrayToString();
                if (!password.Equals(passForCheck) ||
                senderPEB.RealPos != PermutationBrute.GetPosByPass(passItems, senderPEB.Test, min)
                || generated > total)
                    throw new Exception(ERROR);
                return false;
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("Paused");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            brute.OnStop += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("Stoped");
                resetColors();
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
            };
            brute.OnResume += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("Resumed");
                new Thread(() =>
                {
                    Thread.Sleep(1555);
                    sender.Stop();
                }).Start();
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            new Thread(() =>
            {
                brute.Start();
            }).Start();
            Thread.Sleep(1000);
            brute.Pause = true;
            for (int i = 0; i < 5; i++)
            {
                Console.Beep();
                Thread.Sleep(1000 - 20);//-20: To make up for the 20 milliseconds wasted in "waitUntil Pause" method
            }
            brute.Pause = false;

        }
        void PEBMultithreadChangeEndPosT(long start = 1, long end = 0)
        {
            Console.Title = nameof(PEBMultithreadChangeEndPosT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            int min = 1, max = 7;
            object[] test = { 1, 2, 3, 4, 5, 6, 7, 8 };
            PermutationBrute brute = new(start, end, min, max, test);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                PermutationBrute peb = (PermutationBrute)sender;
                string password = pass.ConvertObjectArrayToString();
                string passOnPos = PermutationBrute.GetPassByPos(peb.RealPos, peb.Test, peb.MinimumPassLength, peb.MaximumPassLength).ConvertObjectArrayToString();
                Console.Write($"/G {generated} /T {total} __ {(int)((generated / (double)total) * 100D)}%");
                Console.WriteLine($"   {password} _\'\'_ {passOnPos}");
                if (!password.Equals(passOnPos))
                {
                    Console.WriteLine(ERROR);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                }
                //Thread.Sleep(1);
                return false;
            };
            brute.OnEnd += (BruteForce sender, object[] pass, bool result) =>
            {
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
                resetColors();
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                /*if (sender.Pause)*/
                Console.WriteLine("PAUSED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            new Thread(() => brute.Start()).Start();
            Thread.Sleep(3100);
            brute.EndPos = brute.RealPos + 100;//The work is done faster
            //brute.EndPos = brute.RealPos - 100;//Generate passwords ends
            //brute.EndPos++;//It causes an error
        }
        void PEBMultithreadChangeStartPosT(long start = 1, long end = 0)
        {
            Console.Title = nameof(PEBMultithreadChangeStartPosT);
            changeConsoleColor(ConsoleColor.Green, default_background);
            int min = 1, max = 7;
            object[] test = { 1, 2, 3, 4, 5, 6, 7, 8 };
            PermutationBrute brute = new(start, end, min, max, test);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                PermutationBrute peb = (PermutationBrute)sender;
                string password = pass.ConvertObjectArrayToString();
                string passOnPos = PermutationBrute.GetPassByPos(peb.RealPos, peb.Test, peb.MinimumPassLength, peb.MaximumPassLength).ConvertObjectArrayToString();
                Console.Write($"/G {generated} /T {total} __ {(int)((generated / (double)total) * 100D)}%");
                Console.WriteLine($"   {password} _\'\'_ {passOnPos}");
                if (!password.Equals(passOnPos))
                {
                    Console.WriteLine(ERROR);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                }
                //Thread.Sleep(1);
                return false;
            };
            brute.OnEnd += (BruteForce sender, object[] pass, bool result) =>
            {
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
                resetColors();
            };
            brute.OnPause += (BruteForce sender, long generated, long total) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                /*if (sender.Pause)*/
                Console.WriteLine("PAUSED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            brute.OnStop += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Blue, default_background);
                Console.WriteLine(OK);
                Console.ReadKey();
                Console.Clear();
                resetColors();
            };
            brute.OnRestart += (BruteForce sender, object[] pass) =>
            {
                changeConsoleColor(ConsoleColor.Red, default_background);
                Console.WriteLine("RESTARTED");
                changeConsoleColor(ConsoleColor.Green, default_background);
            };
            new Thread(() => brute.Start()).Start();
            Thread.Sleep(3100);
            brute.StartPos = 2144;
            Thread.Sleep(5000);
            brute.Stop();
            //brute.StartPos = brute.EndPos;
            //brute.StartPos = 0;//It causes an error
        }

        static void PEBSingleThreadSpeedTest(long start, long end, int min, int max, object[] test)
        {
            PermutationBrute brute = new(start, end, min, max, test);
            brute.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            {
                Console.Title = (((double)generated / total) * 100D).ToString("0.00");
                return false;
            };
            Stopwatch stopwatch = Stopwatch.StartNew();
            brute.Start();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
        }
        void PBWTest(long start, long end, int min, int max, object[] test, ProBrute.PassTestInfo[]? testInfos)
        {
            ProBruteWorker SBW = new(start, end, min, max, test, testInfos,
               Environment.ProcessorCount * 2, checker);
            SBW.OnStart += OnStart;
            SBW.OnPause += OnPause;
            SBW.OnResume += OnResume;
            SBW.OnEnd += OnEnd;
            SBW.OnStop += OnStop;
            SBW.OnError += SBW_OnError;
            SBW.OnThreadStart += OnThreadStart;
            SBW.OnThreadPause += OnThreadPause;
            SBW.OnThreadResume += OnThreadResume;
            SBW.OnThreadEnd += OnThreadEnd;
            SBW.OnThreadStop += OnThreadStop;
            SBW.OnThreadError += OnThreadError;
            SBW.OnThreadRestart += OnThreadRestart;
            SBW.DoWork(false);
            Thread.Sleep(1000);
            SBW.Pause = true;
            Thread.Sleep(2000);
            SBW.Pause = false;
            Thread.Sleep(5000);
            SBW[0].StartPos = 1;
            Thread.Sleep(1000);
            SBW[0].EndPos = 700;
            Thread.Sleep(1000);
            SBW.Pause = true;
            Thread.Sleep(1666);
            SBW.Pause = false;
            Thread.Sleep(2666);
            SBW.Stop();
        }
        void SBWTest(long start, long end, int min, int max, object[] test)
        {
            SimpleBruteWorker SBW = new(start, end, min, max, test,
                Environment.ProcessorCount * 2, checker);
            SBW.OnStart += OnStart;
            SBW.OnPause += OnPause;
            SBW.OnResume += OnResume;
            SBW.OnEnd += OnEnd;
            SBW.OnStop += OnStop;
            SBW.OnError += SBW_OnError;
            SBW.OnThreadStart += OnThreadStart;
            SBW.OnThreadPause += OnThreadPause;
            SBW.OnThreadResume += OnThreadResume;
            SBW.OnThreadEnd += OnThreadEnd;
            SBW.OnThreadStop += OnThreadStop;
            SBW.OnThreadError += OnThreadError;
            SBW.OnThreadRestart += OnThreadRestart;
            SBW.DoWork(false);
            Thread.Sleep(1000);
            SBW.Pause = true;
            Thread.Sleep(2000);
            SBW.Pause = false;
            Thread.Sleep(5000);
            SBW[0].StartPos = 1;
            Thread.Sleep(1000);
            SBW[0].EndPos = 700;
            Thread.Sleep(1000);
            SBW.Pause = true;
            Thread.Sleep(1666);
            SBW.Pause = false;
            Thread.Sleep(2666);
            SBW.Stop();
        }
        void PEBWTest(long start, long end, int min, int max, object[] test)
        {
            PermutationBruteWorker SBW = new(start, end, min, max, test,
                Environment.ProcessorCount * 2, checker);
            SBW.OnStart += OnStart;
            SBW.OnPause += OnPause;
            SBW.OnResume += OnResume;
            SBW.OnEnd += OnEnd;
            SBW.OnStop += OnStop;
            SBW.OnError += SBW_OnError;
            SBW.OnThreadStart += OnThreadStart;
            SBW.OnThreadPause += OnThreadPause;
            SBW.OnThreadResume += OnThreadResume;
            SBW.OnThreadEnd += OnThreadEnd;
            SBW.OnThreadStop += OnThreadStop;
            SBW.OnThreadError += OnThreadError;
            SBW.OnThreadRestart += OnThreadRestart;
            SBW.DoWork(false);
            Thread.Sleep(1000);
            SBW.Pause = true;
            Thread.Sleep(2000);
            SBW.Pause = false;
            Thread.Sleep(5000);
            SBW[0].StartPos = 1;
            Thread.Sleep(1000);
            SBW[0].EndPos = 700;
            Thread.Sleep(1000);
            SBW.Pause = true;
            Thread.Sleep(1666);
            SBW.Pause = false;
            Thread.Sleep(2666);
            SBW.Stop();
        }
        #endregion
        #region WorkerEevents

        private static void theEnd()
        {
            Console.WriteLine(OK);
            Console.ReadKey();
            Console.Clear();
        }
        private bool checker(Worker sender, BruteForce brute, object[] pass, long generated, long total)
        {
            brute.Tag ??= new StreamWriter(new MemoryStream());
            StreamWriter SW = (StreamWriter)brute.Tag;
            SW.WriteLine(pass.ConvertObjectArrayToString());
            SW.Close();
            brute.Tag = null;
            return false;
        }
        private void OnStart(Worker sender)
        {
            BaseClass tmp = (BaseClass)sender;
            changeConsoleColor(ConsoleColor.Red, default_background);
            Console.WriteLine(nameof(OnStart));
            resetColors();
            tmp.Tag = Stopwatch.StartNew();
        }
        private void OnPause(Worker sender)
        {
            changeConsoleColor(ConsoleColor.Red, default_background);
            Console.WriteLine(nameof(OnPause));
            resetColors();
        }
        private void OnResume(Worker sender)
        {
            changeConsoleColor(ConsoleColor.Red, default_background);
            Console.WriteLine(nameof(OnResume));
            resetColors();
        }
        private void OnEnd(Worker sender, bool r)
        {
            Stopwatch? tmp = (sender).Tag as Stopwatch;
            changeConsoleColor(ConsoleColor.Red, default_background);
            Console.WriteLine(nameof(OnEnd));
            Console.WriteLine(tmp?.Elapsed);
            resetColors();
            theEnd();
        }
        private void OnStop(Worker sender, bool result)
        {
            Stopwatch? tmp = (sender).Tag as Stopwatch;
            changeConsoleColor(ConsoleColor.Red, default_background);
            Console.WriteLine(nameof(OnStop));
            Console.WriteLine(tmp?.Elapsed);
            resetColors();
            theEnd();
        }
        private void OnThreadStart(Worker sender, BruteForce brute)
        {
            lock (locker)
            {
                changeConsoleColor(ConsoleColor.Green, default_background);
                Console.WriteLine(nameof(OnThreadStart) + $" {brute.ThreadID}");
                resetColors();
            }
        }
        private void OnThreadPause(Worker sender, BruteForce brute, long generated, long total)
        {
            lock (locker)
            {
                changeConsoleColor(ConsoleColor.Blue, default_background);
                Console.WriteLine(nameof(OnThreadPause) + $" {brute.ThreadID}");
                resetColors();
            }
        }
        private void OnThreadResume(Worker sender, BruteForce brute, long generated, long total)
        {
            lock (locker)
            {
                changeConsoleColor(ConsoleColor.DarkBlue, default_background);
                Console.WriteLine(nameof(OnThreadResume) + $" {brute.ThreadID}");
                resetColors();
            }
        }
        private bool OnThreadEnd(Worker sender, BruteForce brute, object[] pass, bool result)
        {
            lock (locker)
            {
                (brute.Tag as StreamWriter)?.Close();
                changeConsoleColor(ConsoleColor.Magenta, default_background);
                Console.WriteLine(nameof(OnThreadEnd) + $" {brute.ThreadID}");
                resetColors();
            }
            return result;
        }
        private void OnThreadStop(Worker sender, BruteForce brute, object[] pass)
        {
            lock (locker)
            {
                changeConsoleColor(ConsoleColor.Magenta, default_background);
                Console.WriteLine(nameof(OnThreadStop) + $" {brute.ThreadID}");
                resetColors();
            }
        }
        private void OnThreadError(Worker sender, BruteForce brute, Exception e, ref bool tryAgain)
        {
            lock (locker)
            {
                changeConsoleColor(ConsoleColor.Green, default_background);
                Console.WriteLine(nameof(OnThreadError) + $" {brute.ThreadID}");
                changeConsoleColor(ConsoleColor.Red, ConsoleColor.White);
                Console.WriteLine(e);
                resetColors();
            }
        }
        private void OnThreadRestart(Worker sender, BruteForce brute, object[] pass)
        {
            lock (locker)
            {
                changeConsoleColor(ConsoleColor.Green, default_background);
                Console.WriteLine(nameof(OnThreadRestart) + $" {brute.ThreadID}");
                resetColors();
            }
        }
        private void SBW_OnError(Worker sender, Exception lastEx) => throw lastEx;
        #endregion
        private void main()
        {
            ProBrute.IgnoreTrivialErrors = true;
            //SBGetPassOrPos_By_PosOrPassT();
            //SBSingleThreadT(new int[] { 4, 6, 5 }, 1, 0);
            //SBSingleThreadT(new int[] { 5, 6, 3, 9 }, 1, 0);
            //SBSingleThreadT(new int[] { }, 70, 90);
            //SBPauseStopT(null, 1, 0);
            //PassTestInfoT();
            //PBgetTestArraysT();
            //PBgetMaxT();
            //PBgetRangesT();
            //PBgetPassOrPos_By_PosOrPassT();
            //PBSingleThreadT(null, 1, 0);
            //PBSingleThreadT(new int[] { 5, 6, 8 }, 1, 0);
            //PBSingleThreadT(new int[] { 5, 6, 8 }, 70, 90);//dose not work :-)
            //PBSingleThreadT(new int[] { 5, 6, 8 }, 1000, 10000);
            //PBPauseStopT(new int[] { 5, 6 }, 1, 0);
            //PBPauseStopT(null, 1, 0);
            //SBMultithreadChangeEndPosT(new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 1, 0);
            //PBMultithreadChangeEndPosT(new int[] { 3, 4 }, 1, 0);
            //SBMultithreadChangeStartPosT(null, 1, 10000);
            //PBMultithreadChangeStartPosT(null, 1, 2000);
            //SBMultithreadChangesStartPosAndEndPosAtSameTime(null, 1, 0);
            //PEBgetPassOrPos_By_PosOrPassT();
            //PEBSingleThreadT();
            //PEBSingleThreadT(7, 54);
            //PEBPauseStopT();
            //PEBMultithreadChangeEndPosT();
            //PEBMultithreadChangeStartPosT();
            //PEBSingleThreadSpeedTest(1, 0, 1, 9, new object[] { 1, "sfksnfs", "rtgfdfvlksolek", "MRJ", "I", "AM", 77, "o0o", 9, "*.-", "<<>>" });//00:05:51.5960389
            //PBSingleThreadSpeedTest(1, 0, 1, 10);//Not good!
            //SBSingleThreadSpeedTest(1, 0, 1, 9, new object[] { 1, "sfksnfs", "rtgfdfvlksolek", "MRJ", "I", "AM", 77, "o0o", 9, "*.-", "<<>>" });//Not good!
            //SBWTest(1, 0, 4, 10, new object[] { 1, 2, 3, 4, 5, 6, 7 });
            //PBWTest(1, 0, 4, 11, new object[] { 1, 2, 3, 4, 5, 6 }, new ProBrute.PassTestInfo[]
            //{
            //    new ProBrute.PassTestInfo(-1,new object[]{1,2}),
            //    new ProBrute.PassTestInfo(2,new object[]{5,6,7})
            //});
            //PEBWTest(1, 0, 4, 11, new object[] { 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14 });
        }
    }
}