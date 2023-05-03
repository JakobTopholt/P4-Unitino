using Moduino.lexer;
using Moduino.node;
using Moduino.parser;
using NUnit.Framework;

namespace UnitTest;

public static class FilesToTestsConverter
{
    private static readonly List<Data> TestData = new();
    private static readonly object ThreadLock = new ();

    public static IEnumerable<TestCaseData> CodeGenData()
    {
        return FileToTestCases().Select<Data, TestCaseData>(data => data.Ast != null
            ? new TestCaseData(data.Ast, data.CodeGen).SetName(data.Name)
            : new TestCaseData(data.Ast, data.CodeGen).SetName(data.Name).Ignore(data.Error));
    }

    public static IEnumerable<TestCaseData> PrettyPrintData()
    {
        return FileToTestCases().Select<Data, TestCaseData>(data => data.Ast != null
            ? new TestCaseData(data.Ast, data.PrettyPrint).SetName(data.Name)
            : new TestCaseData(data.Ast, data.PrettyPrint).SetName(data.Name).Ignore(data.Error));
    }

    public static IEnumerable<TestCaseData> TypeVisitorData()
    {
        return FileToTestCases().Select<Data, TestCaseData>(data => data.Ast != null
            ? new TestCaseData(data.Ast, data.ShouldFail).SetName(data.Name)
            : new TestCaseData(data.Ast, data.ShouldFail).SetName(data.Name).Ignore(data.Error));
    }

    private static List<Data> FileToTestCases()
    {
        // lock + check for count ensures that it is only once that testData is read from the files.
        lock (ThreadLock)
        {
            if (TestData.Count > 0)
                return TestData;
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory() + "/../../../Testcases"))
            {
                string fileName = file.Split('\\').Last().Split('.').First();
                using FileStream fileStream = File.Open(file, FileMode.Open);
                using StreamReader textReader = new(fileStream);
                string testCasesString = textReader.ReadToEnd();
                foreach (string oneTestCaseString in testCasesString.Split("//////"))
                {
                    if (string.IsNullOrWhiteSpace(oneTestCaseString)) continue;
                    string[] test = oneTestCaseString.Split("////").Select(str => str.Trim()).ToArray();
                    string name = test[0];
                    string program = test[1];
                    string prettyPrint = test[2];
                    string codeGen = test[3];
                    bool typedCorrect = test[4].Equals("true", StringComparison.OrdinalIgnoreCase);
                    using MemoryStream stream = new(program.Select(c => (byte)c).ToArray());
                    using StreamReader reader = new(stream);
                    Start? ast = null;
                    string error = "";
                    try
                    {
                        ast = new Parser(new Lexer(reader)).Parse();
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                    TestData.Add(new Data(ast, prettyPrint, codeGen,fileName + ": " + name, error, typedCorrect));
                } 
            }
            return TestData;
        }
    }
}