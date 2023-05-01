using Compiler.Visitors;
using NUnit.Framework;
using System.Text;
using System.Text.RegularExpressions;
using Moduino.lexer;
using Moduino.node;
using Moduino.parser;

namespace UnitTest;

public class VisitorTests
{
    private static readonly Regex WhiteSpace = new (@"\s+");
    private static IEnumerable<TestCaseData> TestCases() {
        {
            using FileStream fileStream = File.Open(Directory.GetCurrentDirectory() + "/../../../PrettyPrint.mino", FileMode.Open);
            using StreamReader textReader = new (fileStream);
            string testCasesString = textReader.ReadToEnd();
            foreach (string oneTestCaseString in testCasesString.Split("//////"))
            {
                if (string.IsNullOrWhiteSpace(oneTestCaseString))
                    continue;
                string[] test = oneTestCaseString.Split("////");

                string name = test[0];
                string program = test[1];
                string prettyPrint = test[2];
                string codeGen = test[3];
                if (string.IsNullOrWhiteSpace(program) || string.IsNullOrWhiteSpace(prettyPrint) || string.IsNullOrWhiteSpace(codeGen))
                    continue;
                using MemoryStream stream = new (program.Select(c => (byte) c ).ToArray());
                using StreamReader reader = new StreamReader(stream);
                
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
                TestCaseData testCaseData = new TestCaseData(
                        ast, 
                        WhiteSpace.Replace(prettyPrint, ""), 
                        codeGen.TrimStart().ReplaceLineEndings()
                        ).SetName(name);
                yield return ast != null ? testCaseData : testCaseData.Ignore(error);
            }
        }
    }

    [TestCaseSource(nameof(TestCases))]
    public void PrettyPrint(Start ast, string prettyPrint, string codeGenText)
    {
        StringBuilder sb = new();
        TextWriter output = new StringWriter(sb);
        ast.Apply(new PrettyPrint(output));
        Assert.That(WhiteSpace.Replace(sb.ToString(), ""), 
            Is.EqualTo(prettyPrint));
    }

    [TestCaseSource(nameof(TestCases))]
    public void CodeGen(Start ast, string prettyPrint, string codeGenText)
    {
        Console.WriteLine(ast + "\n" + codeGenText + "\n");
        using MemoryStream stream = new();
        using StreamWriter writer = new(stream);
        CodeGen codeGen = new(writer);
        ast.Apply(codeGen);
        writer.Flush();
        //Console.WriteLine(stream.Length);
        string code = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int) stream.Length);
        Assert.That(code.ReplaceLineEndings(), Is.EqualTo(codeGenText));
    }
}