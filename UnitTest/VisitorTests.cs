using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private static readonly Regex whiteSpace = new (@"\s+");
    private readonly List<TestCase> testCases = new ();
    [SetUp]
    public void Setup()
    {
        {
            using FileStream fileStream = File.Open(Directory.GetCurrentDirectory() + "/../../../PrettyPrint.mino", FileMode.Open);
            using StreamReader textReader = new (fileStream);
            string testCasesString = textReader.ReadToEnd();
            foreach (string oneTestCaseString in testCasesString.Split("//////"))
            {
                if (string.IsNullOrWhiteSpace(oneTestCaseString))
                    continue;
                string[] test = oneTestCaseString.Split("////");
                
                string program = test[0];
                string prettyPrint = test[1];
                string codeGen = test[2];
                if (string.IsNullOrWhiteSpace(program) || string.IsNullOrWhiteSpace(prettyPrint) || string.IsNullOrWhiteSpace(codeGen))
                    continue;
                using MemoryStream stream = new (program.Select(c => (byte) c ).ToArray());
                using StreamReader reader = new StreamReader(stream);
                
                Start ast = new Parser(new Lexer(reader)).Parse();
                testCases.Add(new TestCase(ast, 
                    whiteSpace.Replace(prettyPrint, ""), 
                    codeGen.Remove(0, 2).ReplaceLineEndings()));
            }
        }
    }

    [Test]
    public void PrettyPrint()
    {
        foreach (TestCase testCase in testCases)
        {
            StringBuilder sb = new();
            TextWriter output = new StringWriter(sb);
            testCase.Ast.Apply(new PrettyPrint(output));
            Assert.That(whiteSpace.Replace(sb.ToString(), ""), 
                Is.EqualTo(testCase.PrettyPrint));
        }
    }

    [Test]
    public void CodeGen()
    {
        foreach (TestCase testCase in testCases)
        {
            Console.WriteLine(testCase.Ast + "\n" + testCase.CodeGen + "\n" + testCase.PrettyPrint);
            using MemoryStream stream = new();
            using StreamWriter writer = new(stream);
            CodeGen codeGen = new(writer);
            testCase.Ast.Apply(codeGen);
            writer.Flush();
            Console.WriteLine(stream.Length);
            string code = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int) stream.Length);
            Assert.That(code.ReplaceLineEndings(), Is.EqualTo(testCase.CodeGen));
        }
    }
}