using CBrute.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CBrute.Helper
{
    /*
First mode: In this case, the string starts with a hyphen (-). Consider the following variables that can come after the hyphen:
                                                   “-X,Y,Z,T”
The red and green variables are mandatory, and the blue ones are optional. However, Y is optional only in one case, which you will understand soon.
If X is a number, Y (optional) must also be a number. The output of the function in this case is numbers from X to Y. The Z variable (a number) also determines the minimum length of each string and, if necessary, fills the left side of the string. The T variable is a character that in default mode is considered as " " (Space). If you want to fill the left side of the string with a special character, replace it with T. If only the X variable is used, numbers from 0 to X are generated.
If X is a character, then Y is mandatory, and both must be a character. The function returns a range of characters that you have requested. For example, "-a,z" generates characters from 'a' to 'z'. To understand the application of Z and T variables, please refer to the previous paragraph.
Note that do not use too much "," (comma) character and X should always be smaller than Y.
For example, consider the following code:
--------------------------------------
// .Net 6
using CBrute.Helper;
string[] strings = VariableReader.generateStringArray("-5");//0,5
Console.Write("-5 => ");
for (int i = 0; i < strings.Length; i++)
{
    Console.Write(strings[i]);
    if (i != strings.Length - 1) Console.Write(", ");
}
Console.WriteLine("\n--5,5,3,? :");
strings = VariableReader.generateStringArray("--5,5,3,?");//-5 to 5
foreach (string s in strings) Console.WriteLine(s);
Console.ReadKey();
--------------------------------------
Output:
-5 => 0, 1, 2, 3, 4, 5
--5,5,3,? :
?-5
?-4
?-3
?-2
?-1
??0
??1
??2
??3
??4
??5
--------------------------------------
second mode: the string only contains "@". In this case, the returned array contains some printable characters.
--------------------------------------
third mode: The string starts with "file=>X". In this case, a valid path to a file must exist instead of X. The function reads the lines of the file and returns an array of strings.
--------------------------------------
fourth mode: The string starts with “_”. After the “_” character, a word or sentence must come. The function converts all English characters in the word or sentence to lowercase or uppercase and returns the result.
For example (case four), consider the following code:
--------------------------------------
// .Net 6
using CBrute.Helper;
object[][] strings = VariableReader.generateStringArray("_flutter").ConvertToObjectArray().Split(8);
foreach (var row in strings)
{
    foreach (var col in row)
        Console.Write(col + " ");
    Console.WriteLine();
}
Console.ReadKey();
--------------------------------------
Output:
flutter flutteR fluttEr fluttER flutTer flutTeR flutTEr flutTER
fluTter fluTteR fluTtEr fluTtER fluTTer fluTTeR fluTTEr fluTTER
flUtter flUtteR flUttEr flUttER flUtTer flUtTeR flUtTEr flUtTER
flUTter flUTteR flUTtEr flUTtER flUTTer flUTTeR flUTTEr flUTTER
fLutter fLutteR fLuttEr fLuttER fLutTer fLutTeR fLutTEr fLutTER
fLuTter fLuTteR fLuTtEr fLuTtER fLuTTer fLuTTeR fLuTTEr fLuTTER
fLUtter fLUtteR fLUttEr fLUttER fLUtTer fLUtTeR fLUtTEr fLUtTER
fLUTter fLUTteR fLUTtEr fLUTtER fLUTTer fLUTTeR fLUTTEr fLUTTER
Flutter FlutteR FluttEr FluttER FlutTer FlutTeR FlutTEr FlutTER
FluTter FluTteR FluTtEr FluTtER FluTTer FluTTeR FluTTEr FluTTER
FlUtter FlUtteR FlUttEr FlUttER FlUtTer FlUtTeR FlUtTEr FlUtTER
FlUTter FlUTteR FlUTtEr FlUTtER FlUTTer FlUTTeR FlUTTEr FlUTTER
FLutter FLutteR FLuttEr FLuttER FLutTer FLutTeR FLutTEr FLutTER
FLuTter FLuTteR FLuTtEr FLuTtER FLuTTer FLuTTeR FLuTTEr FLuTTER
FLUtter FLUtteR FLUttEr FLUttER FLUtTer FLUtTeR FLUtTEr FLUtTER
FLUTter FLUTteR FLUTtEr FLUTtER FLUTTer FLUTTeR FLUTTEr FLUTTER
--------------------------------------
However, note that if you pass an invalid string to the function, the function will throw an exception.

    */
    /// <summary>
    /// By using this class, you can perform routine tasks in generating password lists more quickly.
    /// For more information, please visit the project's GitHub page and read the "VariableReader.cs" file.
    /// </summary>
    public static class VariableReader
    {
        /// <summary>
        /// Generates a sequence of strings representing the numbers between <paramref name="start"/> and <paramref name="end"/> (inclusive).
        /// </summary>
        /// <param name="start">The starting number of the sequence.</param>
        /// <param name="end">The ending number of the sequence.</param>
        /// <param name="len">Optional. The length to pad each number to with the given character <paramref name="ch"/>.</param>
        /// <param name="ch">Optional. The character to use for padding the numbers to <paramref name="len"/>.</param>
        /// <returns>An IEnumerable of strings representing the numbers in the specified range.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="start"/> is greater than or equal to <paramref name="end"/>.</exception>
        private static IEnumerable<string> getNumbers(long start, long end, int len = 0, char ch = ' ')
        {
            if (start >= end)
                throw new ArgumentException($"{nameof(start)}({start}) cannot be greater than or equal to {nameof(end)}({end})");
            for (long number = start; number <= end; ++number)
                if (len <= 0) yield return number.ToString();
                else yield return number.ToString().PadLeft(len, ch);
        }
        /// <summary>
        /// Generates an IEnumerable of strings containing characters from <paramref name="start"/> to <paramref name="end"/> inclusive.
        /// </summary>
        /// <param name="start">The starting character of the sequence.</param>
        /// <param name="end">The ending character of the sequence.</param>
        /// <param name="len">The length to which each string element should be padded with the <paramref name="ch"/> character.</param>
        /// <param name="ch">The character to use for padding each string element.</param>
        /// <returns>An IEnumerable of strings containing characters from <paramref name="start"/> to <paramref name="end"/> inclusive.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="start"/> is greater than or equal to <paramref name="end"/>.</exception>
        private static IEnumerable<string> getChars(char start, char end, int len = 0, char ch = ' ')
        {
            if (start >= end)
                throw new ArgumentException($"{nameof(start)}({start}({(int)start})) cannot be greater than or equal to {nameof(end)}({end}({(int)end}))");
            for (char @char = start; @char <= end; ++@char)
                if (len <= 0) yield return @char.ToString();
                else yield return @char.ToString().PadLeft(len, ch);
        }
        /// <summary>
        /// Returns an array of some ASCII characters.
        /// </summary>
        /// <returns>An array of ASCII characters.</returns>
        private static string[] getAsciiChars() =>
            "! \" # $ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? @ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z [ \\ ] ^ _ ` a b c d e f g h i j k l m n o p q r s t u v w x y z { | } ~"
                .Replace(" ", "").ToCharArray().Select(ch => ch.ToString()).ToArray();
        /// <summary>
        /// Accepts a string as input and converts all English letters in it to uppercase and lowercase.
        /// </summary>
        /// <param name="str">The input string</param>
        /// <returns>An array containing all possible cases of uppercasing and lowercasing the characters of <paramref name="str"/>.</returns>
        private static string[] mangleString(string str)
        {
            string lower = str.ToLower();
            int length = str.Length;
            List<string> result = new List<string>();
            ProBrute.PassTestInfo[] testInfos = new ProBrute.PassTestInfo[length];
            for (int i = 0; i < length; ++i)
                testInfos[i] = new ProBrute.PassTestInfo(i,
                    (lower[i] >= 'a' && lower[i] <= 'z') ? new object[] { lower[i], char.ToUpper(lower[i]) } : new object[] { str[i] });
            ProBrute PB = new ProBrute(1, 0, length, length, ProBrute.JunkArray, testInfos);
            PB.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
        {
            result.Add(pass.ConvertObjectArrayToString());
            return false;
        };
            PB.Start();
            return result.ToArray();
        }
        /// <summary>
        /// Loads the contents of a text file at the specified path and returns them as an array of strings.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>An array of strings containing the lines of the file.</returns>
        private static string[] loadFile(string path)
        {
            List<string> ret = new List<string>();
            using (StreamReader SR = new StreamReader(path))
                while (!SR.EndOfStream)
                    ret.Add(SR.ReadLine());
            return ret.ToArray();
        }
        /// <summary>
        /// Generates an array of strings based on the provided expression.
        /// </summary>
        /// <param name="expression">The expression to be parsed and used for generating the array of strings.</param>
        /// <returns>An array of strings generated based on the provided expression.</returns>
        /// <exception cref="FormatException">Thrown when the provided expression is invalid.</exception>
        /// <exception cref="Exception">Thrown when too many commas are found in the provided expression.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0057:Use range operator", Justification = "<Pending>")]
        public static string[] generateStringArray(string expression)
        {
            if (expression.StartsWith("-"))
            {
                expression = expression.Substring(1);
                if (expression.IndexOf(",") != -1)
                {
                    string[] range = expression.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        Convert.ToInt64(range[0]);
                        goto Number;
                    }
                    catch { goto Chars; }
                Number:
                    return range.Length switch
                    {
                        2 => getNumbers(Convert.ToInt64(range[0]), Convert.ToInt64(range[1])).ToArray(),
                        3 => getNumbers(Convert.ToInt64(range[0]), Convert.ToInt64(range[1]),
                                                        Convert.ToInt32(range[2])).ToArray(),
                        4 => getNumbers(Convert.ToInt64(range[0]), Convert.ToInt64(range[1]),
                                                        Convert.ToInt32(range[2]), Convert.ToChar(range[3])).ToArray(),
                        _ => throw new Exception("Too many commas found in the string expression." +
                                                        " Please check the input and try again."),
                    };
                Chars:
                    return range.Length switch
                    {
                        2 => getChars(Convert.ToChar(range[0]), Convert.ToChar(range[1])).ToArray(),
                        3 => getChars(Convert.ToChar(range[0]), Convert.ToChar(range[1]),
                                                        Convert.ToInt32(range[2])).ToArray(),
                        4 => getChars(Convert.ToChar(range[0]), Convert.ToChar(range[1]),
                                                        Convert.ToInt32(range[2]), Convert.ToChar(range[3])).ToArray(),
                        _ => throw new Exception("Too many commas found in the string expression." +
                                                        " Please check the input and try again."),
                    };
                }
                else return getNumbers(0, Convert.ToInt64(expression)).ToArray();
            }
            else if (expression.Equals("@")) return getAsciiChars();
            else if (expression.ToLower().StartsWith("file=>"))
                return loadFile(expression.ToLower().Substring("file=>".Length).TrimStart().TrimEnd());
            else if (expression.StartsWith("_")) return mangleString(expression.Substring(1));
            else
                throw new FormatException("The entered expression is invalid. Please check the expression and try again.");

        }
    }
}
