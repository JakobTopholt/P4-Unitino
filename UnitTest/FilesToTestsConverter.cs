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
        return FileToTestCases().Aggregate(new List<TestCaseData>(), (list, data) =>
        {
            if (data.Ast == null)
                list.Add(new TestCaseData(data.Ast, data.CodeGen).SetName(data.Name).Ignore(data.Error));
            else if (!string.IsNullOrEmpty(data.CodeGen))
                list.Add(new TestCaseData(data.Ast, data.CodeGen).SetName(data.Name));
            return list;
        });
    }
    public static IEnumerable<TestCaseData> CodeGenDataForIno()
    {
        return FileToTestCases().Aggregate(new List<TestCaseData>(), (list, data) =>
        {
            if (data.Ast == null)
                list.Add(new TestCaseData(data.Name, data.CodeGen).SetName(data.Name).Ignore(data.Error));
            else if (!string.IsNullOrEmpty(data.CodeGen))
                list.Add(new TestCaseData(data.Name, data.CodeGen).SetName(data.Name));
            return list;
        });
    }

    public static IEnumerable<TestCaseData> PrettyPrintData()
    {
        return FileToTestCases().Aggregate(new List<TestCaseData>(), (list, data) =>
        {
            if (data.Ast == null)
                list.Add(new TestCaseData(data.Ast, data.PrettyPrint).SetName(data.Name).Ignore(data.Error));
            else if (!string.IsNullOrEmpty(data.PrettyPrint))
                list.Add(new TestCaseData(data.Ast, data.PrettyPrint).SetName(data.Name));
            return list;
        });
    }

    public static IEnumerable<TestCaseData> TypeVisitorData()
    {
        return FileToTestCases().Aggregate(new List<TestCaseData>(), (list, data) =>
        {
            if (data.Ast == null)
                list.Add(new TestCaseData(data.Ast, data.ShouldFail).SetName(data.Name).Ignore(data.Error));
            else if (data.ShouldFail != null)
                list.Add(new TestCaseData(data.Ast, data.ShouldFail).SetName(data.Name));
            return list;
        });
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
                    string name = fileName + ": " + test[0];
                    switch (test.Length)
                    {
                        case 0:
                        case 1:
                        case 2:
                            continue;
                        case 3:
                        {
                            string program = test[1];
                            Start? ast = Start(program, out string error);
                            
                            string prettyPrint = test[2];
                            TestData.Add(new Data(name, ast, error, prettyPrint));
                            break;
                        }
                        case 4:
                        {
                            string program = test[1];
                            Start? ast = Start(program, out string error);
                            
                            string prettyPrint = test[2];
                            string codeGen = test[3];
                            TestData.Add(new Data(name, ast, error, prettyPrint, codeGen));
                            break;
                        }
                        default:
                        {
                            string program = test[1];
                            Start? ast = Start(program, out string error);
                            string prettyPrint = test[2];
                            string codeGen = test[3];
                            bool? typedCorrect = StringToBool(test[4]);
                            TestData.Add(new Data(name, ast, error, prettyPrint, codeGen, typedCorrect));
                            break;
                        }
                    }
                } 
            }
            return TestData;
        }
    }

    private static bool? StringToBool(string test)
    {
        return test.ToLower() switch
        {
            "true" => true,
            "false" => false,
            _ => null
        };
    }

    private static Start? Start(string program, out string error)
    {
        Start ast;
        using MemoryStream stream = new(program.Select(c => (byte)c).ToArray());
        using StreamReader reader = new(stream);
        try
        {
            ast = new Parser(new Lexer(reader)).Parse();
        }
        catch (Exception e)
        {
            error = e.Message;
            return null;
        }

        error = "";
        return ast;
    }
}