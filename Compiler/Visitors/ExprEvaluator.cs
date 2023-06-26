using System.Globalization;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class ExprEvaluator : DepthFirstAdapter
{
    private Dictionary<Node, object> values = new();
    private float value;
    private Node startNode;

    public ExprEvaluator(AUnitdecimalExp node, Node startNode)
    {
        value = float.Parse(node.GetDecimal().Text, CultureInfo.InvariantCulture);
        this.startNode = startNode;
    }

    public ExprEvaluator(AUnitnumberExp node, Node startNode)
    {
        value = int.Parse(node.GetNumber().Text);
        this.startNode = startNode;
    }
    
    public string GetOutput()
    {
        return ((float) values[startNode]).ToString(CultureInfo.InvariantCulture);
    }

    public override void OutANumberExp(ANumberExp node)
    {
        values.Add(node, int.Parse(node.GetNumber().Text));
    }

    public override void OutAParenthesisExp(AParenthesisExp node)
    {
        values.Add(node, values[node.GetExp()]);
    }
    
    public override void OutAPlusExp(APlusExp node)
    {
        values.Add(node, (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int, string
            (float a, float b) => a + b,
            (int a, int b) => a + b,
            (string a, string b) => a + b,
            //float, int
            (float a, int b) => a + b,
            (int a, float b) => a + b,
            //int, string
            (string a, int b) => a + b,
            (int a, string b) => a + b,
            //float, string
            (string a, float b) => a + b,
            (float a, string b) => a + b,
        });
    }

    public override void OutADecimalExp(ADecimalExp node)
    {
        values.Add(node, float.Parse(node.GetDecimal().ToString()));
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        values.Add(node, (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int
            (float a, float b) => a - b,
            (int a, int b) => a - b,
            
            (float a, int b) => a - b,
            (int a, float b) => a - b,
        });
    }

    public override void OutAMultiplyExp(AMultiplyExp node)
    {
        values.Add(node, (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int
            (float a, float b) => a * b,
            (int a, int b) => a * b,
            
            (float a, int b) => a * b,
            (int a, float b) => a * b,
        });
    }

    public override void OutADivideExp(ADivideExp node)
    {
        values.Add(node, (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int
            (float a, float b) => a / b,
            (int a, int b) => a / b,
            
            (float a, int b) => a / b,
            (int a, float b) => a / b,
        });
    }

    public override void OutAValueExp(AValueExp node)
    {
        values.Add(node, value);
    }

    public override void OutAUnaryminusExp(AUnaryminusExp node)
    {
        values.Add(node, values[node.GetExp()] switch
        {
            int a => -a,
            float a => -a,
            });
    }

    public override void OutARemainderExp(ARemainderExp node)
    {
        values.Add(node, (int)values[node.GetL()] % (int)values[node.GetR()]);
    }

    public override void OutACastExp(ACastExp node)
    {
        values.Add(node, (node.GetType(), values[node.GetExp()]) switch
        {
            (AIntType, int a) => a,
            (ADecimalType, float a) => a,
            (ABoolType, bool a) => a, 
            (ACharType, char a) => a, //TODO: Char everywhere else
            (AStringType, string a) => a,
            (ADecimalType, int a) => (float) a,
            (AIntType, decimal a) => (float) a,
            (AStringType, char a) => "" + a,
            (AStringType, int a) => a.ToString(),
            (AStringType, float a) => a.ToString(CultureInfo.InvariantCulture),
            (ACharType, int a) => (char) a,
        });
        values.Add(node, values[node.GetExp()]);
    }

    public override void OutABooleanExp(ABooleanExp node)
    {
        values.Add(node, values[node.GetBoolean()]);
    }

    public override void OutATrueBoolean(ATrueBoolean node)
    {
        values.Add(node, true);
    }

    public override void OutAFalseBoolean(AFalseBoolean node)
    {
        values.Add(node, false);
    }

    public override void CaseATernaryExp(ATernaryExp node)
    {
        node.GetCond().Apply(this);
        if ((bool)values[node.GetCond()])
        {
            node.GetTrue().Apply(this);
            values.Add(node, values[node.GetTrue()]);
        }
        else
        {
            node.GetFalse().Apply(this);
            values.Add(node, values[node.GetFalse()]);
        }
    }

    public override void CaseAAndExp(AAndExp node)
    {
        node.GetL().Apply(this);
        if ((bool)values[node.GetL()])
        {
            node.GetR().Apply(this);
            values.Add(node, values[node.GetR()]);
            return;
        }
        values.Add(node, false);
    }

    public override void CaseAOrExp(AOrExp node)
    {
        node.GetL().Apply(this);
        if (!(bool)values[node.GetL()])
        {
            node.GetR().Apply(this);
            values.Add(node, values[node.GetR()]);
            return;
        }
        values.Add(node, true);
    }

    public override void OutAStringExp(AStringExp node)
    {
        values.Add(node, node.GetString().Text);
    }

    public override void OutAEqualExp(AEqualExp node)
    {
        values.Add(node,  (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int, string
            (float a, float b) => Math.Abs(a - b) < 0.05f,
            (int a, int b) => a == b,
            (string a, string b) => a == b,
            //float, int
            (float a, int b) => Math.Abs(a - b) < 0.05f,
            (int a, float b) => Math.Abs(a - b) < 0.05f,
            //int, string
            (string a, int b) => a == b.ToString(),
            (int a, string b) => a.ToString() == b,
            //float, string
            (string a, float b) => a == b.ToString(CultureInfo.InvariantCulture),
            (float a, string b) => a.ToString(CultureInfo.InvariantCulture) == b,
            //bool
            (bool a, bool b) => a == b
        });
    }

    public override void OutALessequalExp(ALessequalExp node)
    {
        values.Add(node, (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int
            (float a, float b) => a <= b,
            (int a, int b) => a <= b,
            //float, int
            (float a, int b) => a <= b,
            (int a, float b) => a <= b,
        });
    }

    public override void OutALessExp(ALessExp node)
    {
        values.Add(node, (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int
            (float a, float b) => a < b,
            (int a, int b) => a < b,
            //float, int
            (float a, int b) => a < b,
            (int a, float b) => a < b,
        });
    }

    public override void OutAGreaterequalExp(AGreaterequalExp node)
    {
        values.Add(node, (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int
            (float a, float b) => a >= b,
            (int a, int b) => a >= b,
            //float, int
            (float a, int b) => a >= b,
            (int a, float b) => a >= b,
        });
    }

    public override void OutAGreaterExp(AGreaterExp node)
    {
        values.Add(node, (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int
            (float a, float b) => a > b,
            (int a, int b) => a > b,
            //float, int
            (float a, int b) => a > b,
            (int a, float b) => a > b,
        });
    }

    public override void OutANotequalExp(ANotequalExp node)
    {
        values.Add(node,  (values[node.GetL()], values[node.GetR()]) switch
        {
            // float, int, string
            (float a, float b) => !(Math.Abs(a - b) < 0.05f),
            (int a, int b) => a != b,
            (string a, string b) => a != b,
            //float, int
            (float a, int b) => !(Math.Abs(a - b) < 0.05f),
            (int a, float b) => !(Math.Abs(a - b) < 0.05f),
            //int, string
            (string a, int b) => a != b.ToString(),
            (int a, string b) => a.ToString() != b,
            //float, string
            (string a, float b) => a != b.ToString(CultureInfo.InvariantCulture),
            (float a, string b) => a.ToString(CultureInfo.InvariantCulture) != b,
            //bool
            (bool a, bool b) => a != b
        });
    }

    public override void OutALogicalnotExp(ALogicalnotExp node)
    {
        values.Add(node, !(bool)values[node.GetExp()]);
    }
}