/* This file was generated by SableCC (http://www.sablecc.org/). */

using System;
using System.Collections;
using System.Text;
using System.IO;
using Moduino.node;
using Moduino.lexer;
using Moduino.analysis;

namespace Moduino.parser {

public class ParserException : ApplicationException
{
    Token token;

    public ParserException(Token token, String  message) : base(message)
    {
        this.token = token;
    }

    public Token Token
    {
      get { return token; }
    }
}

internal class State
{
    internal int state;
    internal ArrayList nodes;

    internal State(int state, ArrayList nodes)
    {
        this.state = state;
        this.nodes = nodes;
    }
}

internal class TokenIndex : AnalysisAdapter
{
    internal int index;

    public override void CaseTProg(TProg node)
    {
        index = 0;
    }

    public override void CaseTTunit(TTunit node)
    {
        index = 1;
    }

    public override void CaseTInt(TInt node)
    {
        index = 2;
    }

    public override void CaseTLBkt(TLBkt node)
    {
        index = 3;
    }

    public override void CaseTRBkt(TRBkt node)
    {
        index = 4;
    }

    public override void CaseTLPar(TLPar node)
    {
        index = 5;
    }

    public override void CaseTRPar(TRPar node)
    {
        index = 6;
    }

    public override void CaseTLBrace(TLBrace node)
    {
        index = 7;
    }

    public override void CaseTRBrace(TRBrace node)
    {
        index = 8;
    }

    public override void CaseTPlus(TPlus node)
    {
        index = 9;
    }

    public override void CaseTMinus(TMinus node)
    {
        index = 10;
    }

    public override void CaseTSlash(TSlash node)
    {
        index = 11;
    }

    public override void CaseTStar(TStar node)
    {
        index = 12;
    }

    public override void CaseTEqual(TEqual node)
    {
        index = 13;
    }

    public override void CaseTQMark(TQMark node)
    {
        index = 14;
    }

    public override void CaseTBar(TBar node)
    {
        index = 15;
    }

    public override void CaseTArrow(TArrow node)
    {
        index = 16;
    }

    public override void CaseTComma(TComma node)
    {
        index = 17;
    }

    public override void CaseTSemicolon(TSemicolon node)
    {
        index = 18;
    }

    public override void CaseTColon(TColon node)
    {
        index = 19;
    }

    public override void CaseTTid(TTid node)
    {
        index = 20;
    }

    public override void CaseTChar(TChar node)
    {
        index = 21;
    }

    public override void CaseTNumber(TNumber node)
    {
        index = 22;
    }

    public override void CaseTHex(THex node)
    {
        index = 23;
    }

    public override void CaseTString(TString node)
    {
        index = 24;
    }

    public override void CaseEOF(EOF node)
    {
        index = 25;
    }
}

public class Parser
{
    private Analysis ignoredTokens = new AnalysisAdapter();
    public Analysis IgnoredTokens
    {
      get { return ignoredTokens; }
    }

    protected ArrayList nodeList;

    private Lexer lexer;
    private Stack stack = new Stack();
    private int last_shift;
    private int last_pos;
    private int last_line;
    private Token last_token;
    private TokenIndex converter = new TokenIndex();
    private int[] action = new int[2];

    private const int SHIFT = 0;
    private const int REDUCE = 1;
    private const int ACCEPT = 2;
    private const int ERROR = 3;

    public Parser(Lexer lexer)
    {
        this.lexer = lexer;
    }

    private int GoTo(int index)
    {
        int state = State();
        int low = 1;
        int high = gotoTable[index].Length - 1;
        int value = gotoTable[index][0][1];

        while(low <= high)
        {
            int middle = (low + high) / 2;

            if(state < gotoTable[index][middle][0])
            {
                high = middle - 1;
            }
            else if(state > gotoTable[index][middle][0])
            {
                low = middle + 1;
            }
            else
            {
                value = gotoTable[index][middle][1];
                break;
            }
        }

        return value;
    }

    private void Push(int numstate, ArrayList listNode)
    {
        this.nodeList = listNode;

        stack.Push(new State(numstate, this.nodeList));
    }

    private int State()
    {
        State s = (State) stack.Peek();
        return s.state;
    }

    private ArrayList Pop()
    {
        return (ArrayList) ((State) stack.Pop()).nodes;
    }

    private int Index(Switchable token)
    {
        converter.index = -1;
        token.Apply(converter);
        return converter.index;
    }

    public Start Parse()
    {
        Push(0, null);

        IList ign = null;
        while(true)
        {
            while(Index(lexer.Peek()) == -1)
            {
                if(ign == null)
                {
                    ign = new TypedList(NodeCast.Instance);
                }

                ign.Add(lexer.Next());
            }

            if(ign != null)
            {
                ignoredTokens.SetIn(lexer.Peek(), ign);
                ign = null;
            }

            last_pos = lexer.Peek().Pos;
            last_line = lexer.Peek().Line;
            last_token = lexer.Peek();

            int index = Index(lexer.Peek());
            action[0] = actionTable[State()][0][1];
            action[1] = actionTable[State()][0][2];

            int low = 1;
            int high = actionTable[State()].Length - 1;

            while(low <= high)
            {
                int middle = (low + high) / 2;

                if(index < actionTable[State()][middle][0])
                {
                    high = middle - 1;
                }
                else if(index > actionTable[State()][middle][0])
                {
                    low = middle + 1;
                }
                else
                {
                    action[0] = actionTable[State()][middle][1];
                    action[1] = actionTable[State()][middle][2];
                    break;
                }
            }

            switch(action[0])
            {
                case SHIFT:
        {
            ArrayList list = new ArrayList();
            list.Add(lexer.Next());
                        Push(action[1], list);
                        last_shift = action[1];
                    }
        break;
                case REDUCE:
                    switch(action[1])
                    {
                    case 0:
        {
      ArrayList list = New0();
      Push(GoTo(0), list);
        }
        break;
                    case 1:
        {
      ArrayList list = New1();
      Push(GoTo(1), list);
        }
        break;
                    case 2:
        {
      ArrayList list = New2();
      Push(GoTo(1), list);
        }
        break;
                    case 3:
        {
      ArrayList list = New3();
      Push(GoTo(2), list);
        }
        break;
                    case 4:
        {
      ArrayList list = New4();
      Push(GoTo(3), list);
        }
        break;
                    case 5:
        {
      ArrayList list = New5();
      Push(GoTo(3), list);
        }
        break;
                    case 6:
        {
      ArrayList list = New6();
      Push(GoTo(4), list);
        }
        break;
                    case 7:
        {
      ArrayList list = New7();
      Push(GoTo(5), list);
        }
        break;
                    case 8:
        {
      ArrayList list = New8();
      Push(GoTo(5), list);
        }
        break;
                    case 9:
        {
      ArrayList list = New9();
      Push(GoTo(6), list);
        }
        break;
                    case 10:
        {
      ArrayList list = New10();
      Push(GoTo(7), list);
        }
        break;
                    case 11:
        {
      ArrayList list = New11();
      Push(GoTo(7), list);
        }
        break;
                    case 12:
        {
      ArrayList list = New12();
      Push(GoTo(7), list);
        }
        break;
                    case 13:
        {
      ArrayList list = New13();
      Push(GoTo(8), list);
        }
        break;
                    case 14:
        {
      ArrayList list = New14();
      Push(GoTo(8), list);
        }
        break;
                    case 15:
        {
      ArrayList list = New15();
      Push(GoTo(8), list);
        }
        break;
                    case 16:
        {
      ArrayList list = New16();
      Push(GoTo(9), list);
        }
        break;
                    case 17:
        {
      ArrayList list = New17();
      Push(GoTo(9), list);
        }
        break;
                    case 18:
        {
      ArrayList list = New18();
      Push(GoTo(10), list);
        }
        break;
                    }
                    break;
                case ACCEPT:
                    {
                        EOF node2 = (EOF) lexer.Next();
                        PGrammar node1 = (PGrammar) ((ArrayList)Pop())[0];
                        Start node = new Start(node1, node2);
                        return node;
                    }
                case ERROR:
                    throw new ParserException(last_token,
                        "[" + last_line + "," + last_pos + "] " +
                        errorMessages[errors[action[1]]]);
            }
        }
    }

    ArrayList New0()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList5 = (ArrayList) Pop();
        ArrayList nodeArrayList4 = (ArrayList) Pop();
        ArrayList nodeArrayList3 = (ArrayList) Pop();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TypedList listNode3 = new TypedList();
        TypedList listNode5 = new TypedList();
        TypedList listNode2 = (TypedList)nodeArrayList1[0];
        if ( listNode2 != null )
        {
            listNode3.AddAll(listNode2);
        }
        TypedList listNode4 = (TypedList)nodeArrayList4[0];
        if ( listNode4 != null )
        {
            listNode5.AddAll(listNode4);
        }
        AGrammar pgrammarNode1 = new AGrammar (
              listNode3,
              listNode5
        );
        nodeList.Add(pgrammarNode1);
        return nodeList;
    }
    ArrayList New1()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TypedList listNode3 = new TypedList();
        PUnit punitNode1 = (PUnit)nodeArrayList1[0];
        TypedList listNode2 = (TypedList)nodeArrayList2[0];
        if ( punitNode1 != null )
        {
            listNode3.Add(punitNode1);
        }
        if ( listNode2 != null )
        {
            listNode3.AddAll(listNode2);
        }
        nodeList.Add(listNode3);
        return nodeList;
    }
    ArrayList New2()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TypedList listNode2 = new TypedList();
        PUnit punitNode1 = (PUnit)nodeArrayList1[0];
        if ( punitNode1 != null )
        {
            listNode2.Add(punitNode1);
        }
        nodeList.Add(listNode2);
        return nodeList;
    }
    ArrayList New3()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList7 = (ArrayList) Pop();
        ArrayList nodeArrayList6 = (ArrayList) Pop();
        ArrayList nodeArrayList5 = (ArrayList) Pop();
        ArrayList nodeArrayList4 = (ArrayList) Pop();
        ArrayList nodeArrayList3 = (ArrayList) Pop();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TypedList listNode5 = new TypedList();
        PId pidNode2 = (PId)nodeArrayList2[0];
        TInt tintNode3 = (TInt)nodeArrayList4[0];
        TypedList listNode4 = (TypedList)nodeArrayList6[0];
        if ( listNode4 != null )
        {
            listNode5.AddAll(listNode4);
        }
        AUnit punitNode1 = new AUnit (
              pidNode2,
              tintNode3,
              listNode5
        );
        nodeList.Add(punitNode1);
        return nodeList;
    }
    ArrayList New4()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TypedList listNode3 = new TypedList();
        PSubunit psubunitNode1 = (PSubunit)nodeArrayList1[0];
        TypedList listNode2 = (TypedList)nodeArrayList2[0];
        if ( psubunitNode1 != null )
        {
            listNode3.Add(psubunitNode1);
        }
        if ( listNode2 != null )
        {
            listNode3.AddAll(listNode2);
        }
        nodeList.Add(listNode3);
        return nodeList;
    }
    ArrayList New5()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TypedList listNode2 = new TypedList();
        PSubunit psubunitNode1 = (PSubunit)nodeArrayList1[0];
        if ( psubunitNode1 != null )
        {
            listNode2.Add(psubunitNode1);
        }
        nodeList.Add(listNode2);
        return nodeList;
    }
    ArrayList New6()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList4 = (ArrayList) Pop();
        ArrayList nodeArrayList3 = (ArrayList) Pop();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        PId pidNode2 = (PId)nodeArrayList1[0];
        PExp pexpNode3 = (PExp)nodeArrayList3[0];
        ASubunit psubunitNode1 = new ASubunit (
              pidNode2,
              pexpNode3
        );
        nodeList.Add(psubunitNode1);
        return nodeList;
    }
    ArrayList New7()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList3 = (ArrayList) Pop();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TypedList listNode3 = new TypedList();
        PExp pexpNode1 = (PExp)nodeArrayList1[0];
        TypedList listNode2 = (TypedList)nodeArrayList3[0];
        if ( pexpNode1 != null )
        {
            listNode3.Add(pexpNode1);
        }
        if ( listNode2 != null )
        {
            listNode3.AddAll(listNode2);
        }
        nodeList.Add(listNode3);
        return nodeList;
    }
    ArrayList New8()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TypedList listNode2 = new TypedList();
        PExp pexpNode1 = (PExp)nodeArrayList1[0];
        if ( pexpNode1 != null )
        {
            listNode2.Add(pexpNode1);
        }
        nodeList.Add(listNode2);
        return nodeList;
    }
    ArrayList New9()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        PExp pexpNode1 = (PExp)nodeArrayList1[0];
        nodeList.Add(pexpNode1);
        return nodeList;
    }
    ArrayList New10()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList3 = (ArrayList) Pop();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        PExp pexpNode2 = (PExp)nodeArrayList1[0];
        PExp pexpNode3 = (PExp)nodeArrayList3[0];
        APlusExp pexpNode1 = new APlusExp (
              pexpNode2,
              pexpNode3
        );
        nodeList.Add(pexpNode1);
        return nodeList;
    }
    ArrayList New11()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList3 = (ArrayList) Pop();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        PExp pexpNode2 = (PExp)nodeArrayList1[0];
        PExp pexpNode3 = (PExp)nodeArrayList3[0];
        AMinusExp pexpNode1 = new AMinusExp (
              pexpNode2,
              pexpNode3
        );
        nodeList.Add(pexpNode1);
        return nodeList;
    }
    ArrayList New12()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        PExp pexpNode1 = (PExp)nodeArrayList1[0];
        nodeList.Add(pexpNode1);
        return nodeList;
    }
    ArrayList New13()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList3 = (ArrayList) Pop();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        PExp pexpNode2 = (PExp)nodeArrayList1[0];
        PExp pexpNode3 = (PExp)nodeArrayList3[0];
        AMultExp pexpNode1 = new AMultExp (
              pexpNode2,
              pexpNode3
        );
        nodeList.Add(pexpNode1);
        return nodeList;
    }
    ArrayList New14()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList3 = (ArrayList) Pop();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        PExp pexpNode2 = (PExp)nodeArrayList1[0];
        PExp pexpNode3 = (PExp)nodeArrayList3[0];
        ADivExp pexpNode1 = new ADivExp (
              pexpNode2,
              pexpNode3
        );
        nodeList.Add(pexpNode1);
        return nodeList;
    }
    ArrayList New15()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        PExp pexpNode1 = (PExp)nodeArrayList1[0];
        nodeList.Add(pexpNode1);
        return nodeList;
    }
    ArrayList New16()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList3 = (ArrayList) Pop();
        ArrayList nodeArrayList2 = (ArrayList) Pop();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        PExp pexpNode1 = (PExp)nodeArrayList2[0];
        nodeList.Add(pexpNode1);
        return nodeList;
    }
    ArrayList New17()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TNumber tnumberNode2 = (TNumber)nodeArrayList1[0];
        ANumberExp pexpNode1 = new ANumberExp (
              tnumberNode2
        );
        nodeList.Add(pexpNode1);
        return nodeList;
    }
    ArrayList New18()
    {
        ArrayList nodeList = new ArrayList();
        ArrayList nodeArrayList1 = (ArrayList) Pop();
        TTid ttidNode2 = (TTid)nodeArrayList1[0];
        AId pidNode1 = new AId (
              ttidNode2
        );
        nodeList.Add(pidNode1);
        return nodeList;
    }

    private static int[][][] actionTable = {
      new int[][] {
        new int[] {-1, 3, 0},
        new int[] {1, 0, 1},
      },
      new int[][] {
        new int[] {-1, 3, 1},
        new int[] {20, 0, 5},
      },
      new int[][] {
        new int[] {-1, 3, 2},
        new int[] {25, 2, -1},
      },
      new int[][] {
        new int[] {-1, 3, 3},
        new int[] {0, 0, 7},
      },
      new int[][] {
        new int[] {-1, 1, 2},
        new int[] {1, 0, 1},
      },
      new int[][] {
        new int[] {-1, 1, 18},
      },
      new int[][] {
        new int[] {-1, 3, 6},
        new int[] {19, 0, 9},
      },
      new int[][] {
        new int[] {-1, 3, 7},
        new int[] {7, 0, 10},
      },
      new int[][] {
        new int[] {-1, 1, 1},
      },
      new int[][] {
        new int[] {-1, 3, 9},
        new int[] {2, 0, 11},
      },
      new int[][] {
        new int[] {-1, 3, 10},
        new int[] {5, 0, 12},
        new int[] {22, 0, 13},
      },
      new int[][] {
        new int[] {-1, 3, 11},
        new int[] {7, 0, 19},
      },
      new int[][] {
        new int[] {-1, 3, 12},
        new int[] {5, 0, 12},
        new int[] {22, 0, 13},
      },
      new int[][] {
        new int[] {-1, 1, 17},
      },
      new int[][] {
        new int[] {-1, 3, 14},
        new int[] {8, 0, 21},
      },
      new int[][] {
        new int[] {-1, 1, 8},
        new int[] {18, 0, 22},
      },
      new int[][] {
        new int[] {-1, 1, 9},
        new int[] {9, 0, 23},
        new int[] {10, 0, 24},
      },
      new int[][] {
        new int[] {-1, 1, 12},
        new int[] {11, 0, 25},
        new int[] {12, 0, 26},
      },
      new int[][] {
        new int[] {-1, 1, 15},
      },
      new int[][] {
        new int[] {-1, 3, 19},
        new int[] {20, 0, 5},
      },
      new int[][] {
        new int[] {-1, 3, 20},
        new int[] {6, 0, 30},
        new int[] {9, 0, 23},
        new int[] {10, 0, 24},
      },
      new int[][] {
        new int[] {-1, 1, 0},
      },
      new int[][] {
        new int[] {-1, 3, 22},
        new int[] {5, 0, 12},
        new int[] {22, 0, 13},
      },
      new int[][] {
        new int[] {-1, 3, 23},
        new int[] {5, 0, 12},
        new int[] {22, 0, 13},
      },
      new int[][] {
        new int[] {-1, 3, 24},
        new int[] {5, 0, 12},
        new int[] {22, 0, 13},
      },
      new int[][] {
        new int[] {-1, 3, 25},
        new int[] {5, 0, 12},
        new int[] {22, 0, 13},
      },
      new int[][] {
        new int[] {-1, 3, 26},
        new int[] {5, 0, 12},
        new int[] {22, 0, 13},
      },
      new int[][] {
        new int[] {-1, 3, 27},
        new int[] {8, 0, 36},
      },
      new int[][] {
        new int[] {-1, 1, 5},
        new int[] {20, 0, 5},
      },
      new int[][] {
        new int[] {-1, 3, 29},
        new int[] {16, 0, 38},
      },
      new int[][] {
        new int[] {-1, 1, 16},
      },
      new int[][] {
        new int[] {-1, 1, 7},
      },
      new int[][] {
        new int[] {-1, 1, 10},
        new int[] {11, 0, 25},
        new int[] {12, 0, 26},
      },
      new int[][] {
        new int[] {-1, 1, 11},
        new int[] {11, 0, 25},
        new int[] {12, 0, 26},
      },
      new int[][] {
        new int[] {-1, 1, 14},
      },
      new int[][] {
        new int[] {-1, 1, 13},
      },
      new int[][] {
        new int[] {-1, 1, 3},
      },
      new int[][] {
        new int[] {-1, 1, 4},
      },
      new int[][] {
        new int[] {-1, 3, 38},
        new int[] {5, 0, 12},
        new int[] {22, 0, 13},
      },
      new int[][] {
        new int[] {-1, 3, 39},
        new int[] {18, 0, 40},
      },
      new int[][] {
        new int[] {-1, 1, 6},
      },
    };

    private static int[][][] gotoTable  = {
      new int[][] {
        new int[] {-1, 2},
      },
      new int[][] {
        new int[] {-1, 3},
        new int[] {4, 8},
      },
      new int[][] {
        new int[] {-1, 4},
      },
      new int[][] {
        new int[] {-1, 27},
        new int[] {28, 37},
      },
      new int[][] {
        new int[] {-1, 28},
      },
      new int[][] {
        new int[] {-1, 14},
        new int[] {22, 31},
      },
      new int[][] {
        new int[] {-1, 15},
        new int[] {38, 39},
      },
      new int[][] {
        new int[] {-1, 16},
        new int[] {12, 20},
      },
      new int[][] {
        new int[] {-1, 17},
        new int[] {23, 32},
        new int[] {24, 33},
      },
      new int[][] {
        new int[] {-1, 18},
        new int[] {25, 34},
        new int[] {26, 35},
      },
      new int[][] {
        new int[] {-1, 29},
        new int[] {1, 6},
      },
    };

    private static String[] errorMessages = {
      "expecting: 'unit'",
      "expecting: tid",
      "expecting: EOF",
      "expecting: 'Prog'",
      "expecting: 'Prog', 'unit'",
      "expecting: '=>', ':'",
      "expecting: ':'",
      "expecting: '{'",
      "expecting: 'int'",
      "expecting: '(', number",
      "expecting: ')', '}', '+', '-', '/', '*', ';'",
      "expecting: '}'",
      "expecting: '}', ';'",
      "expecting: '}', '+', '-', ';'",
      "expecting: ')', '+', '-'",
      "expecting: '}', tid",
      "expecting: '=>'",
      "expecting: ';'",
    };

    private static int[] errors = {
      0, 1, 2, 3, 4, 5, 6, 7, 3, 8, 9, 7, 9, 10, 11, 12, 
      13, 10, 10, 1, 14, 2, 9, 9, 9, 9, 9, 11, 15, 16, 10, 11, 
      10, 10, 10, 10, 4, 11, 9, 17, 15, 
    };
}
}
