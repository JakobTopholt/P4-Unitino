using Moduino.lexer;
using Moduino.node;
using Moduino.parser;
using NUnit.Framework;

namespace UnitTest;

public static class TestUtils
{
    public static IEnumerable<TestCaseData> GetTestsFromFile(string path) //expected path input: 0Prettyprint, 1Typecheck, 2Codegen
    {
        foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory() + "/../../../VisitorTests/" + path))
        {
            string fileName = file.Split('\\').Last().Split('.').First();
            using FileStream fileStream = File.Open(file, FileMode.Open);
            using StreamReader textReader = new(fileStream);
            string testCasesString = textReader.ReadToEnd();
            foreach (string oneTestCaseString in testCasesString.Split("//////"))
            {
                if (string.IsNullOrWhiteSpace(oneTestCaseString)) continue;
                string[] test = oneTestCaseString.Split("////").Select(str => str.Trim()).ToArray();
                if (test.Length != 3)
                    yield return new TestCaseData(null, null).SetName(test[0]).SetCategory(fileName).Ignore("Wrong amount of //// or missing a ////// between tests");
                using MemoryStream stream = new(test[1].Select(c => (byte)c).ToArray());
                using StreamReader reader = new(stream);
                Start? ast = null;
                string message = "";
                try
                {
                    ast = new Parser(new Lexer(reader)).Parse();
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
                if (ast == null)
                    yield return new TestCaseData(ast, null).SetName(test[0]).SetCategory(fileName).Ignore(message);
                yield return new TestCaseData(ast, test[2]).SetName(test[0]).SetCategory(fileName);
            }
        }
    }
}