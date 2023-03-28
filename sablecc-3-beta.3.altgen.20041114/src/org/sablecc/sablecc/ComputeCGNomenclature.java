/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This file is part of SableCC.                             *
 * See the file "LICENSE" for copyright information and the  *
 * terms and conditions for copying, distribution and        *
 * modification of SableCC.                                  *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/*
 * Last Modification date :: 28-October-2003
 * Add termtail to simple term and simple listterm
 * in order to support scripting generation for parser by 
 * the new scripting engine.
*/

package org.sablecc.sablecc;

import org.sablecc.sablecc.analysis.*;
import org.sablecc.sablecc.node.*;
import java.util.*;
import java.io.*;

public class ComputeCGNomenclature extends DepthFirstAdapter
{
  private String currentProd;
  private String currentAlt;
  private int counter;
  private ResolveIds ids;
  private ResolveProdTransformIds prodTransformIds;

  private final Map altTransformElemTypes = new TypedHashMap(
        NodeCast.instance,
        StringCast.instance);

  private final Map termNumbers = new TypedHashMap(NodeCast.instance,
                                  IntegerCast.instance);

  public ComputeCGNomenclature(ResolveIds ids, ResolveProdTransformIds prodTransformIds)
  {
    this.ids = ids;
    this.prodTransformIds = prodTransformIds;
  }

  public Map getAltTransformElemTypes()
  {
    return altTransformElemTypes;
  }

  public Map getTermNumbers()
  {
    return termNumbers;
  }

  public void caseAProd(final AProd production)
  {
    currentProd = "P" + ids.name(production.getId().getText());
    Object []temp = production.getAlts().toArray();
    for(int i = 0; i<temp.length; i++)
    {
      ((PAlt)temp[i]).apply(this);
    }
  }

  public void inAParsedAlt(AParsedAlt nodeAlt)
  {
    counter = 0;

    if(nodeAlt.getAltName() != null)
    {
      currentAlt = "A"+
                   ids.name( nodeAlt.getAltName().getText() )+
                   currentProd.substring(1);
    }
    else
    {
      currentAlt = "A" + currentProd.substring(1);
    }

    counter = 0;
  }

  boolean processingProdTransform = false;
  boolean processingAst = false;

  public void inAAst(AAst node)
  {
    processingAst = true;
  }

  public void outAAst(AAst node)
  {
    processingAst = false;
  }

  public void inAElem(AElem node)
  {
    if(!processingAst)
    {
      String elemType = (String)ids.elemTypes.get(node);

      if(node.getElemName() != null)
      {
        ids.altsElemNameTypes.put(currentAlt+"."+node.getElemName().getText(), elemType );
      }
    }
  }

  public void inANewTerm(ANewTerm node)
  {
    AProdName aProdName = (AProdName)node.getProdName();
    String type = "P" + ids.name(aProdName.getId().getText());

    altTransformElemTypes.put(node, type);
    termNumbers.put(node, new Integer(++counter));
  }

  public void inANewListTerm(ANewListTerm node)
  {
    AProdName aProdName = (AProdName)node.getProdName();
    String type = "P" + ids.name(aProdName.getId().getText());

    altTransformElemTypes.put(node, type);
    termNumbers.put(node, new Integer(++counter));
  }

  public void outAListTerm(AListTerm node)
  {
    if( node.getListTerms().size() > 0 )
    {
      Object[] temp = node.getListTerms().toArray();

      String firstTermType = (String)altTransformElemTypes.get(temp[0]);

      if(firstTermType != null)
      {
        if(!firstTermType.startsWith("L"))
        {
          altTransformElemTypes.put(node, "L" + firstTermType);
        }
        else
        {
          altTransformElemTypes.put(node, firstTermType);
        }
      }
    }
    else
    {
      altTransformElemTypes.put(node, "Lnull");
    }
    termNumbers.put(node, new Integer(++counter));
  }

  public void caseASimpleTerm(ASimpleTerm node)
  {
    String name;
    String elemType = (String)ids.altsElemNameTypes.get( currentAlt+"."+node.getId().getText() );
    boolean okTermtail = false;

    if(node.getSpecifier() != null)
    {
      if(node.getSpecifier() instanceof ATokenSpecifier)
      {
        name = "T" + ids.name(node.getId().getText());
      }
      else
      {
        if(node.getSimpleTermTail() == null)
        {
          name = "P" + ids.name(node.getId().getText());
        
	  //add termtail to the simpleterm
	  node.setSimpleTermTail( (TId)node.getId().clone() );
	}
        else
        {
          String termTail = node.getSimpleTermTail().getText();
          String localcurrentProd = "P" + ids.name(node.getId().getText());
          name = (String)prodTransformIds.prodTransformElemTypesString.get(localcurrentProd+"."+termTail);
        }
      }
    }
    else
    {
      String type;
      if( ( (elemType != null) && elemType.startsWith("T") ) ||
          ( (elemType == null) && ids.tokens.get("T" + ids.name(node.getId().getText())) != null ) )
      {
        if(elemType != null)
        {
          name = elemType;
        }
        else
        {
          name = "T" + ids.name(node.getId().getText());
        }
      }
      else
      {
        if(node.getSimpleTermTail() == null)
        {
          if(elemType != null)
          {
            name = elemType;
          }
          else
          {
            name = "P" + ids.name(node.getId().getText());
          }

	  //add termtail to the simpleterm
	  node.setSimpleTermTail( (TId)node.getId().clone() );
	}
        else
        {
          String prodType;
          if(elemType != null)
          {
            prodType = elemType;
          }
          else
          {
            prodType = "P" + ids.name(node.getId().getText());
          }

          String termTail = node.getSimpleTermTail().getText();
          name = (String)prodTransformIds.prodTransformElemTypesString.get(prodType+"."+termTail);
        }
      }
    }
    termNumbers.put(node, new Integer(++counter));
    if(name.endsWith("?"))
    {
      name = name.substring(0, name.length()-1);
    }
    altTransformElemTypes.put(node, name);
  }

  public void caseANullTerm(ANullTerm node)
  {
    altTransformElemTypes.put(node, "null");
    termNumbers.put(node, new Integer(++counter));
  }

  public void caseASimpleListTerm(ASimpleListTerm node)
  {
    String name;
    String strTermTail;

    if(node.getSpecifier() != null)
    {
      if(node.getSpecifier() instanceof ATokenSpecifier)
      {
        name = "T" + ids.name(node.getId().getText());
      }
      else
      {
        if(node.getSimpleTermTail() == null)
        {
          name = "P" + ids.name(node.getId().getText());

	  //add termtail to the simpleterm
	  node.setSimpleTermTail( (TId)node.getId().clone() ); 
        }
        else
        {
          String termTail = node.getSimpleTermTail().getText();

          String localcurrentProd = "P" + ids.name(node.getId().getText());
          name = (String)prodTransformIds.prodTransformElemTypesString.get(localcurrentProd+"."+termTail);
        }

      }
    }

    //the element has no specifier
    else
    {
      String  elemType = (String)ids.altsElemNameTypes.get( currentAlt+"."+node.getId().getText() );
      String type;
      if( ( (elemType != null) && elemType.startsWith("T") ) ||
          ( (elemType == null) && ids.tokens.get("T" + ids.name(node.getId().getText())) != null ) )
      {
        if(elemType != null)
        {
          name = elemType;
        }
        else
        {
          name = "T" + ids.name(node.getId().getText());
        }
      }
      //it seems to be a production without a specifier
      else
      {
        if(node.getSimpleTermTail() == null)
        {
          if(elemType != null)
          {
            name = elemType;
          }
          else
          {
            name = "P" + ids.name(node.getId().getText());
          }

	  //add termtail to the simpleterm
	  node.setSimpleTermTail( (TId)node.getId().clone() );
        }
        else
        {
          String prodType;
          if(elemType != null)
          {
            prodType = elemType;
          }
          else
          {
            prodType = "P" + ids.name(node.getId().getText());
          }

          String termTail = node.getSimpleTermTail().getText();
          name = (String)prodTransformIds.prodTransformElemTypesString.get(prodType+"."+termTail);

        }
      }
    }
    if(name.endsWith("?"))
    {
      name = name.substring(0, name.length()-1);
    }
    altTransformElemTypes.put(node, name);
    termNumbers.put(node, new Integer(++counter));
  }
}
