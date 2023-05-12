using System.Text.RegularExpressions;
using Compiler;
using NUnit.Framework;

namespace UnitTest;

public class CodegenChecker
{
    private static readonly Regex SWhitespace = new(@"\s+");

    [SetUp]
    public async Task DownloadCli()
    {
        await ArduinoCompiler.DownloadCliAsync(null);
    }

    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.CodeGenDataForIno))]
    public async Task CheckIfCodeGenWorksInArduino(string name, string codeGenText)
    {
        name = SWhitespace.Replace(name, "").Replace(':', '-');
        // For some reason someFile.ino needs to be in someFile/someFile.ino - https://github.com/arduino/arduino-cli/issues/1968
        string folder = Directory.GetCurrentDirectory() + "\\..\\" + name + "\\"; 
        {
            Directory.CreateDirectory(folder);
            await using StreamWriter textWriter = File.CreateText(folder + name + ".ino");
            await textWriter.WriteAsync(codeGenText);
        }
        string boardsTable = await ArduinoCompiler.GetBoards();

        // Parse the output to get the Fully Qualified Board Name(FQBN) and port name of the connected device
        string[] rows = boardsTable.Split( '\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (string row in rows)
        {
            string[] columns = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (columns.Length != 9 || columns[2] != "Serial") 
                continue;
            string boardFqbn = columns[7];
            Assert.That(await ArduinoCompiler.Compile(folder, null, boardFqbn), Is.True);
            return;
        }
        Assert.That(await ArduinoCompiler.Compile(folder, null, null), Is.True);
    }
}