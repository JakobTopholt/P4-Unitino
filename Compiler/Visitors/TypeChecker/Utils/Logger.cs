using System.Text;
using Moduino.node;

namespace Compiler.Visitors.TypeChecker.Utils;

public class Logger
{
    private readonly List<string> _currentLocationErrors = new();
    private readonly Stack<string> _locations = new();
    private readonly List<string> _errorResults = new();
    private readonly TextWriter _output;
    public Logger(TextWriter output)
    {
        _output = output;
    }
    // Locations to be skipped so that errors are written without "In PGrammar" for example
    private bool Skip(Node node)
    {
        switch (node)
        {
            case AArg:
            case PGrammar:
            case Start:
                return true;
            default:
                return false;
        }
    }
    public void PrintAllErrors()
    {
        foreach (string error in _errorResults)
        {
            _output.WriteLine(error);
        }

        // Ready for reuse
        _locations.Clear();
        _errorResults.Clear();
    }
    public void EnterNode(Node node)
    {
        if (Skip(node))
            return;
        _locations.Push($"in {node.GetType()}");
    }

    public void ExitNode(Node node)
    {
        if (Skip(node))
            return;
        _locations.Pop();
        if (_currentLocationErrors.Count == 0)
            return;
        StringBuilder sb = new();
        foreach (string error in _currentLocationErrors) 
            sb.AppendLine(error);
        int indent = 1;
        foreach (string s in _locations) 
            sb.AppendLine(new string(' ', indent++ * 3) + s);

        _errorResults.Add(sb.ToString());
        _currentLocationErrors.Clear();
    }
    public void ThrowError(string error)
    {
        _currentLocationErrors.Add(error);
    }

}