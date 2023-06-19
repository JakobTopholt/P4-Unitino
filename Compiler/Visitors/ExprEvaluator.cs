using System.Globalization;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class ExprEvaluator : DepthFirstAdapter
{
    private Dictionary<Node, float> values = new();
    private float value;

    public ExprEvaluator(TDecimal value)
    {
        this.value = float.Parse(value.Text, CultureInfo.InvariantCulture);
    }

    public ExprEvaluator(TNumber value)
    {
        this.value = int.Parse(value.Text);
    }
    
    public string GetOutput(Node node)
    {
        return values[node].ToString(CultureInfo.InvariantCulture);
    }

    public override void OutANumberExp(ANumberExp node)
    {
        values.Add(node, int.Parse(node.GetNumber().Text));
    }
    public override void OutAExpExp(AExpExp node)
    {
        values.Add(node, values[node.GetExp()]);
    }
    
    public override void OutAPlusExp(APlusExp node)
    {
        values.Add(node, values[node.GetL()] + values[node.GetR()]);
    }

    public override void OutADecimalExp(ADecimalExp node)
    {
        values.Add(node, float.Parse(node.GetDecimal().ToString(), NumberStyles.Float));
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        values.Add(node, values[node.GetL()] - values[node.GetR()]);
    }

    public override void OutAMultiplyExp(AMultiplyExp node)
    {
        values.Add(node, values[node.GetL()] * values[node.GetR()]);
    }

    public override void OutAPrefixminusminusExp(APrefixminusminusExp node)
    {
        values.Add(node, --values[node.GetExp()]);
    }

    public override void OutADivideExp(ADivideExp node)
    {
        values.Add(node, values[node.GetL()] / values[node.GetR()]);
    }

    public override void OutAPrefixplusplusExp(APrefixplusplusExp node)
    {
        values.Add(node, ++values[node.GetExp()]);
    }

    public override void OutAValueExp(AValueExp node)
    {
        values.Add(node, value);
    }

    public override void OutASuffixplusplusExp(ASuffixplusplusExp node)
    {
        values.Add(node, values[node.GetExp()]++);
    }

    public override void OutAUnaryminusExp(AUnaryminusExp node)
    {
        values.Add(node, -values[node.GetExp()]);
    }

    public override void OutASuffixminusminusExp(ASuffixminusminusExp node)
    {
        values.Add(node, values[node.GetExp()]--);
    }

    public override void OutARemainderExp(ARemainderExp node)
    {
        values.Add(node, values[node.GetL()] % values[node.GetR()]);
    }

    public override void OutACastExp(ACastExp node)
    {
        values.Add(node, values[node.GetExp()]);
    }
}