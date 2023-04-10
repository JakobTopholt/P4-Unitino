/* This file was generated by SableCC (http://www.sablecc.org/). */

using System;
using System.Collections;
using Moduino.node;

namespace Moduino.analysis {


public interface Analysis : Switch
{
    Object GetIn(Node node);
    void SetIn(Node node, Object inobj);
    Object GetOut(Node node);
    void SetOut(Node node, Object outobj);

    void CaseStart(Start node);
    void CaseAGrammar(AGrammar node);
    void CaseAUnit(AUnit node);
    void CaseASubunit(ASubunit node);
    void CaseAStmt(AStmt node);
    void CaseAPlusExp(APlusExp node);
    void CaseAMinusExp(AMinusExp node);
    void CaseADivExp(ADivExp node);
    void CaseAMultExp(AMultExp node);
    void CaseANumberExp(ANumberExp node);
    void CaseAId(AId node);

    void CaseTProg(TProg node);
    void CaseTTunit(TTunit node);
    void CaseTInt(TInt node);
    void CaseTLBkt(TLBkt node);
    void CaseTRBkt(TRBkt node);
    void CaseTLPar(TLPar node);
    void CaseTRPar(TRPar node);
    void CaseTLBrace(TLBrace node);
    void CaseTRBrace(TRBrace node);
    void CaseTPlus(TPlus node);
    void CaseTMinus(TMinus node);
    void CaseTSlash(TSlash node);
    void CaseTStar(TStar node);
    void CaseTEqual(TEqual node);
    void CaseTQMark(TQMark node);
    void CaseTBar(TBar node);
    void CaseTArrow(TArrow node);
    void CaseTComma(TComma node);
    void CaseTSemicolon(TSemicolon node);
    void CaseTColon(TColon node);
    void CaseTTid(TTid node);
    void CaseTChar(TChar node);
    void CaseTNumber(TNumber node);
    void CaseTHex(THex node);
    void CaseTString(TString node);
    void CaseTBlank(TBlank node);
    void CaseTComment(TComment node);
    void CaseEOF(EOF node);
}


public class AnalysisAdapter : Analysis
{
    private Hashtable inhash;
    private Hashtable outhash;

    public virtual Object GetIn(Node node)
    {
        if(inhash == null)
        {
            return null;
        }

        return inhash[node];
    }

    public virtual void SetIn(Node node, Object inobj)
    {
        if(this.inhash == null)
        {
            this.inhash = new Hashtable(1);
        }

        if(inobj != null)
        {
            this.inhash[node] = inobj;
        }
        else
        {
            this.inhash.Remove(node);
        }
    }
    public virtual Object GetOut(Node node)
    {
        if(outhash == null)
        {
            return null;
        }

        return outhash[node];
    }

    public virtual void SetOut(Node node, Object outobj)
    {
        if(this.outhash == null)
        {
            this.outhash = new Hashtable(1);
        }

        if(outobj != null)
        {
            this.outhash[node] = outobj;
        }
        else
        {
            this.outhash.Remove(node);
        }
    }
    public virtual void CaseStart(Start node)
    {
        DefaultCase(node);
    }

    public virtual void CaseAGrammar(AGrammar node)
    {
        DefaultCase(node);
    }
    public virtual void CaseAUnit(AUnit node)
    {
        DefaultCase(node);
    }
    public virtual void CaseASubunit(ASubunit node)
    {
        DefaultCase(node);
    }
    public virtual void CaseAStmt(AStmt node)
    {
        DefaultCase(node);
    }
    public virtual void CaseAPlusExp(APlusExp node)
    {
        DefaultCase(node);
    }
    public virtual void CaseAMinusExp(AMinusExp node)
    {
        DefaultCase(node);
    }
    public virtual void CaseADivExp(ADivExp node)
    {
        DefaultCase(node);
    }
    public virtual void CaseAMultExp(AMultExp node)
    {
        DefaultCase(node);
    }
    public virtual void CaseANumberExp(ANumberExp node)
    {
        DefaultCase(node);
    }
    public virtual void CaseAId(AId node)
    {
        DefaultCase(node);
    }

    public virtual void CaseTProg(TProg node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTTunit(TTunit node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTInt(TInt node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTLBkt(TLBkt node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTRBkt(TRBkt node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTLPar(TLPar node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTRPar(TRPar node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTLBrace(TLBrace node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTRBrace(TRBrace node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTPlus(TPlus node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTMinus(TMinus node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTSlash(TSlash node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTStar(TStar node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTEqual(TEqual node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTQMark(TQMark node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTBar(TBar node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTArrow(TArrow node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTComma(TComma node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTSemicolon(TSemicolon node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTColon(TColon node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTTid(TTid node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTChar(TChar node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTNumber(TNumber node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTHex(THex node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTString(TString node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTBlank(TBlank node)
    {
        DefaultCase(node);
    }
    public virtual void CaseTComment(TComment node)
    {
        DefaultCase(node);
    }

    public virtual void CaseEOF(EOF node)
    {
        DefaultCase(node);
    }

    public virtual void DefaultCase(Node node)
    {
    }
}


public class DepthFirstAdapter : AnalysisAdapter
{
    public virtual void InStart(Start node)
    {
        DefaultIn(node);
    }

    public virtual void OutStart(Start node)
    {
        DefaultOut(node);
    }

    public virtual void DefaultIn(Node node)
    {
    }

    public virtual void DefaultOut(Node node)
    {
    }

    public override void CaseStart(Start node)
    {
        InStart(node);
        node.GetPGrammar().Apply(this);
        node.GetEOF().Apply(this);
        OutStart(node);
    }

    public virtual void InAGrammar(AGrammar node)
    {
        DefaultIn(node);
    }

    public virtual void OutAGrammar(AGrammar node)
    {
        DefaultOut(node);
    }

    public override void CaseAGrammar(AGrammar node)
    {
        InAGrammar(node);
        {
            Object[] temp = new Object[node.GetUnit().Count];
            node.GetUnit().CopyTo(temp, 0);
            for(int i = 0; i < temp.Length; i++)
            {
                ((PUnit) temp[i]).Apply(this);
            }
        }
        {
            Object[] temp = new Object[node.GetExp().Count];
            node.GetExp().CopyTo(temp, 0);
            for(int i = 0; i < temp.Length; i++)
            {
                ((PExp) temp[i]).Apply(this);
            }
        }
        OutAGrammar(node);
    }
    public virtual void InAUnit(AUnit node)
    {
        DefaultIn(node);
    }

    public virtual void OutAUnit(AUnit node)
    {
        DefaultOut(node);
    }

    public override void CaseAUnit(AUnit node)
    {
        InAUnit(node);
        if(node.GetId() != null)
        {
            node.GetId().Apply(this);
        }
        if(node.GetInt() != null)
        {
            node.GetInt().Apply(this);
        }
        {
            Object[] temp = new Object[node.GetSubunit().Count];
            node.GetSubunit().CopyTo(temp, 0);
            for(int i = 0; i < temp.Length; i++)
            {
                ((PSubunit) temp[i]).Apply(this);
            }
        }
        OutAUnit(node);
    }
    public virtual void InASubunit(ASubunit node)
    {
        DefaultIn(node);
    }

    public virtual void OutASubunit(ASubunit node)
    {
        DefaultOut(node);
    }

    public override void CaseASubunit(ASubunit node)
    {
        InASubunit(node);
        if(node.GetId() != null)
        {
            node.GetId().Apply(this);
        }
        if(node.GetExp() != null)
        {
            node.GetExp().Apply(this);
        }
        OutASubunit(node);
    }
    public virtual void InAStmt(AStmt node)
    {
        DefaultIn(node);
    }

    public virtual void OutAStmt(AStmt node)
    {
        DefaultOut(node);
    }

    public override void CaseAStmt(AStmt node)
    {
        InAStmt(node);
        if(node.GetExp() != null)
        {
            node.GetExp().Apply(this);
        }
        OutAStmt(node);
    }
    public virtual void InAPlusExp(APlusExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutAPlusExp(APlusExp node)
    {
        DefaultOut(node);
    }

    public override void CaseAPlusExp(APlusExp node)
    {
        InAPlusExp(node);
        if(node.GetL() != null)
        {
            node.GetL().Apply(this);
        }
        if(node.GetR() != null)
        {
            node.GetR().Apply(this);
        }
        OutAPlusExp(node);
    }
    public virtual void InAMinusExp(AMinusExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutAMinusExp(AMinusExp node)
    {
        DefaultOut(node);
    }

    public override void CaseAMinusExp(AMinusExp node)
    {
        InAMinusExp(node);
        if(node.GetL() != null)
        {
            node.GetL().Apply(this);
        }
        if(node.GetR() != null)
        {
            node.GetR().Apply(this);
        }
        OutAMinusExp(node);
    }
    public virtual void InADivExp(ADivExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutADivExp(ADivExp node)
    {
        DefaultOut(node);
    }

    public override void CaseADivExp(ADivExp node)
    {
        InADivExp(node);
        if(node.GetL() != null)
        {
            node.GetL().Apply(this);
        }
        if(node.GetR() != null)
        {
            node.GetR().Apply(this);
        }
        OutADivExp(node);
    }
    public virtual void InAMultExp(AMultExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutAMultExp(AMultExp node)
    {
        DefaultOut(node);
    }

    public override void CaseAMultExp(AMultExp node)
    {
        InAMultExp(node);
        if(node.GetL() != null)
        {
            node.GetL().Apply(this);
        }
        if(node.GetR() != null)
        {
            node.GetR().Apply(this);
        }
        OutAMultExp(node);
    }
    public virtual void InANumberExp(ANumberExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutANumberExp(ANumberExp node)
    {
        DefaultOut(node);
    }

    public override void CaseANumberExp(ANumberExp node)
    {
        InANumberExp(node);
        if(node.GetNumber() != null)
        {
            node.GetNumber().Apply(this);
        }
        OutANumberExp(node);
    }
    public virtual void InAId(AId node)
    {
        DefaultIn(node);
    }

    public virtual void OutAId(AId node)
    {
        DefaultOut(node);
    }

    public override void CaseAId(AId node)
    {
        InAId(node);
        if(node.GetTid() != null)
        {
            node.GetTid().Apply(this);
        }
        OutAId(node);
    }
}


public class ReversedDepthFirstAdapter : AnalysisAdapter
{
    public virtual void InStart(Start node)
    {
        DefaultIn(node);
    }

    public virtual void OutStart(Start node)
    {
        DefaultOut(node);
    }

    public virtual void DefaultIn(Node node)
    {
    }

    public virtual void DefaultOut(Node node)
    {
    }

    public override void CaseStart(Start node)
    {
        InStart(node);
        node.GetEOF().Apply(this);
        node.GetPGrammar().Apply(this);
        OutStart(node);
    }

    public virtual void InAGrammar(AGrammar node)
    {
        DefaultIn(node);
    }

    public virtual void OutAGrammar(AGrammar node)
    {
        DefaultOut(node);
    }

    public override void CaseAGrammar(AGrammar node)
    {
        InAGrammar(node);
        {
            Object[] temp = new Object[node.GetExp().Count];
            node.GetExp().CopyTo(temp, 0);
            for(int i = temp.Length - 1; i >= 0; i--)
            {
                ((PExp) temp[i]).Apply(this);
            }
        }
        {
            Object[] temp = new Object[node.GetUnit().Count];
            node.GetUnit().CopyTo(temp, 0);
            for(int i = temp.Length - 1; i >= 0; i--)
            {
                ((PUnit) temp[i]).Apply(this);
            }
        }
        OutAGrammar(node);
    }
    public virtual void InAUnit(AUnit node)
    {
        DefaultIn(node);
    }

    public virtual void OutAUnit(AUnit node)
    {
        DefaultOut(node);
    }

    public override void CaseAUnit(AUnit node)
    {
        InAUnit(node);
        {
            Object[] temp = new Object[node.GetSubunit().Count];
            node.GetSubunit().CopyTo(temp, 0);
            for(int i = temp.Length - 1; i >= 0; i--)
            {
                ((PSubunit) temp[i]).Apply(this);
            }
        }
        if(node.GetInt() != null)
        {
            node.GetInt().Apply(this);
        }
        if(node.GetId() != null)
        {
            node.GetId().Apply(this);
        }
        OutAUnit(node);
    }
    public virtual void InASubunit(ASubunit node)
    {
        DefaultIn(node);
    }

    public virtual void OutASubunit(ASubunit node)
    {
        DefaultOut(node);
    }

    public override void CaseASubunit(ASubunit node)
    {
        InASubunit(node);
        if(node.GetExp() != null)
        {
            node.GetExp().Apply(this);
        }
        if(node.GetId() != null)
        {
            node.GetId().Apply(this);
        }
        OutASubunit(node);
    }
    public virtual void InAStmt(AStmt node)
    {
        DefaultIn(node);
    }

    public virtual void OutAStmt(AStmt node)
    {
        DefaultOut(node);
    }

    public override void CaseAStmt(AStmt node)
    {
        InAStmt(node);
        if(node.GetExp() != null)
        {
            node.GetExp().Apply(this);
        }
        OutAStmt(node);
    }
    public virtual void InAPlusExp(APlusExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutAPlusExp(APlusExp node)
    {
        DefaultOut(node);
    }

    public override void CaseAPlusExp(APlusExp node)
    {
        InAPlusExp(node);
        if(node.GetR() != null)
        {
            node.GetR().Apply(this);
        }
        if(node.GetL() != null)
        {
            node.GetL().Apply(this);
        }
        OutAPlusExp(node);
    }
    public virtual void InAMinusExp(AMinusExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutAMinusExp(AMinusExp node)
    {
        DefaultOut(node);
    }

    public override void CaseAMinusExp(AMinusExp node)
    {
        InAMinusExp(node);
        if(node.GetR() != null)
        {
            node.GetR().Apply(this);
        }
        if(node.GetL() != null)
        {
            node.GetL().Apply(this);
        }
        OutAMinusExp(node);
    }
    public virtual void InADivExp(ADivExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutADivExp(ADivExp node)
    {
        DefaultOut(node);
    }

    public override void CaseADivExp(ADivExp node)
    {
        InADivExp(node);
        if(node.GetR() != null)
        {
            node.GetR().Apply(this);
        }
        if(node.GetL() != null)
        {
            node.GetL().Apply(this);
        }
        OutADivExp(node);
    }
    public virtual void InAMultExp(AMultExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutAMultExp(AMultExp node)
    {
        DefaultOut(node);
    }

    public override void CaseAMultExp(AMultExp node)
    {
        InAMultExp(node);
        if(node.GetR() != null)
        {
            node.GetR().Apply(this);
        }
        if(node.GetL() != null)
        {
            node.GetL().Apply(this);
        }
        OutAMultExp(node);
    }
    public virtual void InANumberExp(ANumberExp node)
    {
        DefaultIn(node);
    }

    public virtual void OutANumberExp(ANumberExp node)
    {
        DefaultOut(node);
    }

    public override void CaseANumberExp(ANumberExp node)
    {
        InANumberExp(node);
        if(node.GetNumber() != null)
        {
            node.GetNumber().Apply(this);
        }
        OutANumberExp(node);
    }
    public virtual void InAId(AId node)
    {
        DefaultIn(node);
    }

    public virtual void OutAId(AId node)
    {
        DefaultOut(node);
    }

    public override void CaseAId(AId node)
    {
        InAId(node);
        if(node.GetTid() != null)
        {
            node.GetTid().Apply(this);
        }
        OutAId(node);
    }
}
} // namespace Moduino.analysis
