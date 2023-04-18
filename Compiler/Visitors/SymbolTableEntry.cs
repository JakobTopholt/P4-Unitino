namespace Compiler.Visitors;

public class SymbolTableEntry {
    public string Name { get; }
    public string Type { get; }
    public int ScopeLevel { get; }

    public SymbolTableEntry(string name, string type, int scopeLevel) {
        Name = name;
        Type = type;
        ScopeLevel = scopeLevel;
    }
}