﻿using Compiler.Visitors;
using Moduino.lexer;
using Moduino.node;
using Moduino.parser;

namespace Compiler;

internal static class ModuinoCompiler
{
    public static async Task<string> CompileMinoToIno(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        string folder;
        if (name != path.Split(Path.DirectorySeparatorChar)[^2]) //Check if moduino file is in folder with same name ex: HelloWorld/HelloWorld.mino
        {
            // For some reason someFile.ino needs to be in someFile/someFile.ino - https://github.com/arduino/arduino-cli/issues/1968
            folder = Path.GetDirectoryName(path) + "\\" + name + "\\";
            Directory.CreateDirectory(folder);
        }
        else
            folder = Path.GetDirectoryName(path);
        string outPath = folder + name + ".ino";
        
        Start start;
        await using (FileStream fileStream = File.Open(path + ".mino", FileMode.Open))
        {
            using (TextReader textReader = new StreamReader(fileStream))
            {
                Lexer lexer = new(textReader);
                Parser parser = new(lexer);
                start = parser.Parse();
            }
        }

        //start.Apply(new PrettyPrint());

        List<SymbolTable> AllTables = new();

        SymbolTable symbolTable = new(null, AllTables);
        start.Apply(new UnitVisitor(symbolTable));
        start.Apply(new FunctionVisitor(symbolTable));
        start.Apply(new TypeChecker(symbolTable));

        // Codegen Visitor
        await using (FileStream stream = File.Create(outPath))
        await using (StreamWriter writer = new(stream))
        {
            using CodeGen codegen = new(writer, symbolTable);
            start.Apply(codegen);
            await writer.FlushAsync();
        }

        return folder;
    }
}