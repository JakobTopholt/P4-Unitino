using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// TODO: Create UnitVisitor
// The idea is that we want to convert example: 50ms => UnitTimems(50). For this branch we only need to handle the
// declaring part of this. So create a dictionary<string, stmtNode> such that in the future the value 50ms can use the
// ms to find the function UnitTimems(). Make sure to throw compileerror if there's 2 declarations of the same string
// Also create a dictionary for Dictionary<string, List<string>> so that Time a = 50ms; or Time b = (6+5*7/2)ms can be
// recognized in the future
/*
public class UnitVisitor : DepthFirstAdapter
{
    enum Type
    {
        Exception,
        Int
    }
    
    // Dictionary used to store information on the type of each node
    // Used then to evaluate which type an expression should yield.
    private Dictionary<Node, Type> nodeTypes = new();
    public void EvaluateType(Node node)
    {
        Type l = nodeTypes[node.GetL()];
        Type r = nodeTypes[node.GetR()];
        switch (l, r)
        {
            case { l: Type.Int, r: Type.Int }:
            {
                
                break;
            }
        }
    }

    public override void OutANumberExp(ANumberExp node)
    {
        if (int.TryParse(node.ToString(), out int a))
        {
            nodeTypes.Add(node, Type.Int);
        }
    }
    
}*/