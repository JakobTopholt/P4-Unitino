/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This file is part of SableCC.                             *
 * See the file "LICENSE" for copyright information and the  *
 * terms and conditions for copying, distribution and        *
 * modification of SableCC.                                  *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/*
 * Last Modification date 17-11-2003
 * Addition of type checking for operators
 */
package org.sablecc.sablecc;

import org.sablecc.sablecc.analysis.*;
import org.sablecc.sablecc.node.*;
import java.util.*;
import java.io.*;

/*
 * ResolveTransformIds
 * 
 * This class computes semantic verifications for alternative transformations
 * in Production section. It makes sure that all transformations specified
 * for an alternative match with their corresponding prod_transform but also
 * the alternative transformation is itself correct.
 */

public class ResolveTransformIds extends DepthFirstAdapter
{

  private ResolveAstIds transformIds;
  private ResolveAltIds altIds;
  private ResolveProdTransformIds prodTransformIds;

  private int nbTransformAlt = 0;
  private int nbTransformProd = 0;
  private String currentAstAlt;
  private String currentAstProd;
  private String currentProd;
  private String currentAlt;
  private String currentAstProdName;

  //This Map contains the type of any term of alternative transformation(altTransform)
  public final Map altTransformElemTypes = new TypedHashMap(
        NodeCast.instance,
        StringCast.instance);

  public final Map mapSimpleTermProdTransformation = new TypedHashMap(
        StringCast.instance,
        ListCast.instance);

  public final Map simpleTermOrsimpleListTermTypes = new TypedHashMap(
        NodeCast.instance,
        StringCast.instance);

  public ResolveTransformIds(ResolveAstIds ast_ids, ResolveAltIds alt_ids, ResolveProdTransformIds p_ids)
  {
    transformIds = ast_ids;
    altIds = alt_ids;
    prodTransformIds = p_ids;
  }

  public ResolveProdTransformIds getProdTransformIds()
  {
    return prodTransformIds;
  }

  public void inAProd(final AProd production)
  {
    nbTransformProd = 0;
    currentProd = (String)altIds.ids.names.get(production);

    if(production.getArrow() != null)
    {
      nbTransformProd = production.getProdTransform().size();
    }
  }

  private LinkedList listCurrentAltGlobal;
  private LinkedList listCurrentAlt;
  private LinkedList listOfListCurrentAlt;

  public void inAParsedAlt(AParsedAlt nodeAlt)
  {
    nbTransformAlt = 0;

    currentAlt = (String)altIds.ids.names.get(nodeAlt);
    listCurrentAltGlobal = (LinkedList)((LinkedList)altIds.alts_elemsGlobal.get(currentAlt)).clone();
    listCurrentAlt = (LinkedList)((LinkedList)altIds.alts_elems.get(currentAlt)).clone();
    listOfListCurrentAlt = (LinkedList)((LinkedList)altIds.alts_elems_list.get(currentAlt)).clone();
  }

  public void inAAltTransform(AAltTransform node)
  {
    if(node.getTerms().size() == 0)
    {
      altTransformElemTypes.put(node, "nothing");
    }
    else
    {
      nbTransformAlt = node.getTerms().size();
    }
  }

  public void outAAltTransform(AAltTransform node)
  {
    if(nbTransformAlt != nbTransformProd)
    {
      error10( node.getRBrace() );
    }
  }

  public void outAParsedAlt(AParsedAlt node)
  {
    lastSimpleTerm = null;

    listCurrentAltGlobal = null;
    listCurrentAlt = null;
    listOfListCurrentAlt = null;
    mapSimpleTermProdTransformation.clear();
  }

  public void outANewTerm(ANewTerm node)
  {
    LinkedList list = (LinkedList)prodTransformIds.prod_transforms.get(currentProd);
    AProdName prodNameNode = (AProdName)node.getProdName();

    currentAstProd = "P" + ResolveIds.name(prodNameNode.getId().getText());
    currentAstProdName = prodNameNode.getId().getText();

    if(prodNameNode.getProdNameTail() != null)
    {
      TId prodNameTailNode = prodNameNode.getProdNameTail();
      currentAstAlt = "A" + ResolveIds.name(prodNameTailNode.getText()) + ResolveIds.name(prodNameNode.getId().getText());
    }
    else
    {
      currentAstAlt = "A" + ResolveIds.name(prodNameNode.getId().getText());
    }

    String currentAstProdName_no_specifier;
    if(currentAstProdName.startsWith("P."))
    {
      currentAstProdName_no_specifier = currentAstProdName.substring(2);
    }
    else
    {
      currentAstProdName_no_specifier = currentAstProdName;
    }

    if(transformIds.ast_prods.get(currentAstProd) == null)
    {
      error15(((AProdName)node.getProdName()).getId(), ((AProdName)node.getProdName()).getId().getText());
    }

    if(transformIds.ast_alts.get(currentAstAlt) == null)
    {
      error14(((AProdName)node.getProdName()).getId(), currentAstAlt);
    }

    int sizeNewTerm = 0;
    if(node.getParams().size() > 0)
    {
      sizeNewTerm = node.getParams().size();
    }

    int sizeAstAlt = 0;
    if( ((AAstAlt)transformIds.ast_alts.get(currentAstAlt) ).getElems() != null)
    {
      sizeAstAlt = ( (List)((AAstAlt)transformIds.ast_alts.get(currentAstAlt) ).getElems()).size();
    }

    if(sizeNewTerm != sizeAstAlt)
    {
      error17( ((AProdName)node.getProdName()).getId(), currentAstAlt);
    }

    AProdName aProdName = (AProdName)node.getProdName();

    String type = "P" + ResolveIds.name(aProdName.getId().getText());

    altTransformElemTypes.put(node, type);

    AAstAlt astAlt = (AAstAlt)transformIds.ast_alts.get(currentAstAlt);

    if(node.getParams().size() > 0 && astAlt.getElems().size() > 0)
    {
      Object elemsTable[] = astAlt.getElems().toArray();
      Object paramsTable[] = node.getParams().toArray();

      String termType, elemType;

      //here, we're checking the type compabitlity between for a new node creation
      for(int j=0; j<elemsTable.length; j++)
      {
        termType = (String)altTransformElemTypes.get(paramsTable[j]);
        elemType = (String)transformIds.ast_elemTypes.get(elemsTable[j]);

	PUnOp elemOp = ((AElem)elemsTable[j]).getUnOp();

	if( !checkTypeCompability(elemType, termType, elemOp) )
        {
          error21(node.getLPar(), elemType, termType);
	}
      }
    }
    
  }


  public void outANewListTerm(ANewListTerm node)
  {
    LinkedList list = (LinkedList)prodTransformIds.prod_transforms.get(currentProd);
    AProdName prodNameNode = (AProdName)node.getProdName();

    currentAstProdName = prodNameNode.getId().getText();
    currentAstProd = "P" + ResolveIds.name(currentAstProdName);

    if(prodNameNode.getProdNameTail() != null)
    {
      TId prodNameTailNode = prodNameNode.getProdNameTail();
      currentAstAlt = "A" + ResolveIds.name(prodNameTailNode.getText()) + ResolveIds.name(prodNameNode.getId().getText());
    }
    else
    {
      currentAstAlt = "A" + ResolveIds.name(prodNameNode.getId().getText());
    }

    String currentAstProdName_no_specifier;
    if(currentAstProdName.startsWith("P."))
    {
      currentAstProdName_no_specifier = currentAstProdName.substring(2);
    }
    else
    {
      currentAstProdName_no_specifier = currentAstProdName;
    }

    if(transformIds.ast_prods.get(currentAstProd) == null)
    {
      error15(((AProdName)node.getProdName()).getId(), ((AProdName)node.getProdName()).getId().getText());
    }

    if(transformIds.ast_alts.get(currentAstAlt) == null)
    {
      error14(((AProdName)node.getProdName()).getId(), currentAstAlt);
    }

    int sizeNewTerm = 0;
    if(node.getParams().size() > 0)
    {
      sizeNewTerm = node.getParams().size();
    }

    int sizeAstAlt = 0;
    if( ((AAstAlt)transformIds.ast_alts.get(currentAstAlt) ).getElems() != null)
    {
      sizeAstAlt = ( (LinkedList)((AAstAlt)transformIds.ast_alts.get(currentAstAlt) ).getElems()).size();
    }

    if(sizeNewTerm != sizeAstAlt)
    {
      error17( ((AProdName)node.getProdName()).getId(), currentAstAlt);
    }

    AProdName aProdName = (AProdName)node.getProdName();

    String type = "P" + ResolveIds.name(aProdName.getId().getText());

    altTransformElemTypes.put(node, type);

    AAstAlt astAlt = (AAstAlt)transformIds.ast_alts.get(currentAstAlt);


    if(node.getParams().size() > 0 && astAlt.getElems().size() > 0)
    {
      Object elemsTable[] = astAlt.getElems().toArray();
      Object paramsTable[] = node.getParams().toArray();

      String termType, elemType;

      //here, we're checking the type compabitlity between for a new node creation
      for(int j=0; j<elemsTable.length; j++)
      {
        termType = (String)altTransformElemTypes.get(paramsTable[j]);
        elemType = (String)transformIds.ast_elemTypes.get(elemsTable[j]);

	PUnOp elemOp = ((AElem)elemsTable[j]).getUnOp();
	

	if( !checkTypeCompability(elemType, termType, elemOp) )
        {
          error21(node.getLPar(), elemType, termType);
	}

      }
    }
  }

  public void outAListTerm(AListTerm node)
  {
    if( (node.getListTerms() != null) && (node.getListTerms().size() != 0) )
    {
      Object temp[] = node.getListTerms().toArray();
      String firstTermType = (String)altTransformElemTypes.get(temp[0]);
      if(firstTermType.endsWith("?"))
      {
	firstTermType = firstTermType.substring(0, firstTermType.length()-1);
      }

      for(int i=1; i<temp.length; i++)
      {
        String termType = (String)altTransformElemTypes.get(temp[i]);
	if(termType.endsWith("?"))
        {
	  termType = termType.substring(0, termType.length()-1);
	}
	/*
        if( ( !termType.startsWith("L") && !firstTermType.startsWith("L") && !termType.equals(firstTermType) ) ||
            ( termType.startsWith("L")  && !firstTermType.startsWith("L") && !(termType.substring(1)).equals(firstTermType) ) ||
            ( !termType.startsWith("L") && firstTermType.startsWith("L")  && !(firstTermType.substring(1)).equals(termType) ) ||
            ( termType.startsWith("L")  && firstTermType.startsWith("L")  && !termType.equals(firstTermType) ) )
        {
	  System.out.println("$$$$$$$$$$$$ termtype : " + termType + " firstTermType : " + firstTermType);
          error20(node.getLBkt());
        }
	*/
	if(!termType.equals(firstTermType))
	{
	  System.out.println("$$$$$$$$$$$$ termtype : " + termType + " firstTermType : " + firstTermType);
	  error20(node.getLBkt());
	}
      }

      if(!firstTermType.startsWith("L"))
      {
        altTransformElemTypes.put(node, "L" + firstTermType);
      }
      else
      {
        altTransformElemTypes.put(node, firstTermType);
      }
    }
    else
    {
      altTransformElemTypes.put(node, "LNull");
    }
  }

  private LinkedList listL;
  private LinkedList listP;
  private String lastSimpleTerm;

  public void inASimpleTerm(ASimpleTerm node)
  {
    String name = node.getId().getText();
    String typeOfExplicitElemName = (String)transformIds.astIds.altsElemNameTypes.get( currentAlt+"."+node.getId().getText() );
    String alternativeElemType = (String)transformIds.astIds.altsElemTypes.get( currentAlt+"."+node.getId().getText() );

    boolean okTermtail = false;
    String tmpName = name;

    if( (listCurrentAltGlobal == null) || !listCurrentAltGlobal.contains(name) )
    {
      error11(node.getId(), name);
    }
    else
    {
      ListIterator iter = null;
      iter = listCurrentAltGlobal.listIterator();

      while(iter.hasNext())
      {
        if( name.equals((String)iter.next()) )
        {
          if( node.getSimpleTermTail() == null )
          {
            iter.remove();
          }
          break;
        }
      }
    }

    if( (listCurrentAlt == null) || !listCurrentAlt.contains(name) )
    {
      error11(node.getId(), name);
    }
    else
    {
      ListIterator iter = null;
      iter = listCurrentAlt.listIterator();

      while(iter.hasNext())
      {
        if( name.equals((String)iter.next()) )
        {
          if( node.getSimpleTermTail() == null )
          {
            iter.remove();
          }
          break;
        }
      }
    }

    int position = 0;

    String type;
    
    if(alternativeElemType.startsWith("T"))
    {
      if(node.getSimpleTermTail() != null)
      {
        error12(node.getId());
      }

      altTransformElemTypes.put(node, alternativeElemType);
      position = 0;
    }
    //The element is a production
    else
    {
      listL = null;

      name = "P" + ResolveIds.name(node.getId().getText());

      if(!name.equals(lastSimpleTerm) )
      {
        if(typeOfExplicitElemName != null)
	{
          if( mapSimpleTermProdTransformation.get(currentAlt+"."+tmpName) == null)
	  {
            if(prodTransformIds.prod_transforms.get(typeOfExplicitElemName) != null)
	    {
              listL = (LinkedList)((LinkedList)prodTransformIds.prod_transforms.get(typeOfExplicitElemName)).clone();
	      mapSimpleTermProdTransformation.put(currentAlt+"."+tmpName, listL);
	    }
	  }
	  else
          {
            listL = (LinkedList)mapSimpleTermProdTransformation.get(currentAlt+"."+tmpName);
	  }
	}
	if( mapSimpleTermProdTransformation.get(currentAlt+".P"+tmpName) == null )
        {
          listP = (LinkedList)prodTransformIds.prod_transforms.get(name);
	  if(prodTransformIds.prod_transforms.get(name) != null)
          {
            listP = (LinkedList)((LinkedList)prodTransformIds.prod_transforms.get(name)).clone();
	    mapSimpleTermProdTransformation.put(currentAlt+".P"+tmpName, listP);
	  }
	}
	else
	{
          listP = (LinkedList)mapSimpleTermProdTransformation.get(currentAlt+".P"+tmpName);
	}
	
	listCurrentAlt.remove(lastSimpleTerm);
      }
      
      boolean blistL = false;

      if( ( (typeOfExplicitElemName != null) && (listL!= null) && (listL.size()==1) && ResolveIds.name((String)listL.getFirst()).equals(typeOfExplicitElemName.substring(1)) ) ||
	  ( (typeOfExplicitElemName == null) && (listP!= null) && (listP.size()==1) && listP.contains(node.getId().getText()) ) )
      {
        blistL = true;
      }

      //the production is transformed but it appears without a term tail.
      if( (node.getSimpleTermTail() == null) &&
	  ( (listL != null) || (listP != null) ) && !blistL )
      {
        if(typeOfExplicitElemName != null)
	{
          error9(node.getId(), typeOfExplicitElemName);
	}
	else
	{
          error9(node.getId(), node.getId().getText());
	}
      }
      
      //simpletermtail doesn't appear in the production transformation
      if( node.getSimpleTermTail() != null )
      {
        okTermtail = true;
	String strTermTail = node.getSimpleTermTail().getText();
	
	if( ( (listL == null) || !listL.contains(strTermTail) ) &&
	    ( (listP == null) || !listP.contains(strTermTail) ) )
        {
          error13(node.getId(), node.getId().getText(), strTermTail);
	}
	else
        {
          ListIterator iter = null;

	  if(listL != null)
          {
            iter = listL.listIterator();

	    position = listL.indexOf(strTermTail);
	    
	    while(iter.hasNext())
            {
              if( strTermTail.equals((String)iter.next()) )
	      {
                iter.remove();
		break;
	      }
	    }
	  }
	  
	  if(listP != null)
	  {
            iter = listP.listIterator();

            position = listP.indexOf(strTermTail);

	    while(iter.hasNext())
	    {
              if( strTermTail.equals((String)iter.next()) )
	      {
                iter.remove();
		break;
	      }
	    }
	  }
	  
	}
      }
      
      if(node.getSimpleTermTail() != null)
      {
        String termtail = node.getSimpleTermTail().getText();
	LinkedList listProdContains = null;
	if(typeOfExplicitElemName != null)
        {
          listProdContains = (LinkedList)prodTransformIds.mapProdTransformContainsList.get(typeOfExplicitElemName);
	}
	else
	{
          listProdContains = (LinkedList)prodTransformIds.mapProdTransformContainsList.get("P" + ResolveIds.name(node.getId().getText()));
	}
      }
      //The Type of the element without his eventual termtail (term.termtail :: (type of term))
      if(typeOfExplicitElemName != null)
      {
        simpleTermOrsimpleListTermTypes.put(node, typeOfExplicitElemName);
      }
      else
      {
        simpleTermOrsimpleListTermTypes.put(node, "P" + ResolveIds.name(node.getId().getText()));
      }

      if(node.getSimpleTermTail() == null)
      {
	altTransformElemTypes.put(node, alternativeElemType);
	position = 0;
      }
      else
      {
	String termTail = node.getSimpleTermTail().getText();
	//This boolean is used to ensures that optional operator is properly propagate.
	boolean qmark_op = false;
	if(alternativeElemType.endsWith("?"))
	{
	  alternativeElemType = alternativeElemType.substring(0, alternativeElemType.length()-1);
	  qmark_op = true;
	}
	String typeOfTerm = (String)prodTransformIds.prodTransformElemTypesString.get(alternativeElemType+"."+termTail);

	// The substring is done because we want to ensures that lists term should be wrapped by square brackets
	if(typeOfTerm.startsWith("L"))
	{
	  typeOfTerm = typeOfTerm.substring(1);
	}
	if(qmark_op && !typeOfTerm.endsWith("?"))
	{
	  typeOfTerm += "?";
	}
	altTransformElemTypes.put(node, typeOfTerm);
      }
    }
  }

  public void caseANullTerm(ANullTerm node)
  {
    altTransformElemTypes.put(node, "Null");
  }

  public void inASimpleListTerm(ASimpleListTerm node)
  {
    String name = node.getId().getText();
    String  typeOfExplicitElemName = (String)transformIds.astIds.altsElemNameTypes.get( currentAlt+"."+node.getId().getText() );
    String alternativeElemType = (String)transformIds.astIds.altsElemTypes.get( currentAlt+"."+node.getId().getText() );
    //System.out.println("$$$$$$$$$$$$$$$ premier toto");
    String strTermTail;
    String tmpName = name;

    if( (listCurrentAltGlobal == null) || !listCurrentAltGlobal.contains(name) )
    {
      error11(node.getId(), name);
    }
    else
    {
      ListIterator iter = null;
      iter = listCurrentAltGlobal.listIterator();

      while(iter.hasNext())
      {
        if( name.equals((String)iter.next()) )
        {
          if( node.getSimpleTermTail() == null )
          {
            iter.remove();
          }
          break;
        }
      }
    }

    if( ((listCurrentAlt == null) || !listCurrentAlt.contains(name)) && ((listOfListCurrentAlt == null) || !listOfListCurrentAlt.contains(name)) )
    {
      error11(node.getId(), name);
    }
    else
    {
      ListIterator iter = null;
      if( (listCurrentAlt != null) && listCurrentAlt.contains(name) )
      {
        iter = listCurrentAlt.listIterator();
      }
      else if( (listOfListCurrentAlt != null) && listOfListCurrentAlt.contains(name) )
      {
        iter = listOfListCurrentAlt.listIterator();
      }

      while(iter.hasNext())
      {
        if( name.equals((String)iter.next()) )
        {
          if( node.getSimpleTermTail() == null )
          {
            iter.remove();
          }
          break;
        }
      }
    }

    int position = 0;

    String type;
    if(alternativeElemType.startsWith("T"))
    {
      //A token can't have term tail
      if(node.getSimpleTermTail() != null)
      {
        error12(node.getId());
      }

      altTransformElemTypes.put(node, alternativeElemType);
      position = 0;
    }
    //it seems to be a production without a specifier
    else
    {
      name = "P" + ResolveIds.name(node.getId().getText());

      if(!name.equals(lastSimpleTerm) )
      {
        if(typeOfExplicitElemName != null)
        {
          if( mapSimpleTermProdTransformation.get(currentAlt+"."+tmpName) == null)
          {
            if(prodTransformIds.prod_transforms.get(typeOfExplicitElemName) != null)
            {
              listL = (LinkedList)((LinkedList)prodTransformIds.prod_transforms.get(typeOfExplicitElemName)).clone();
	      mapSimpleTermProdTransformation.put(currentAlt+"."+tmpName, listL);
	    }
	  }
	  else
          {
            listL = (LinkedList)mapSimpleTermProdTransformation.get(currentAlt+"."+tmpName);
	  }
	}
	if( mapSimpleTermProdTransformation.get(currentAlt+".P"+tmpName) == null )
        {
          listP = (LinkedList)prodTransformIds.prod_transforms.get(name);
	  if(prodTransformIds.prod_transforms.get(name) != null)
	  {
            listP = (LinkedList)((LinkedList)prodTransformIds.prod_transforms.get(name)).clone();
	    mapSimpleTermProdTransformation.put(currentAlt+".P"+tmpName, listP);
	  }
	}
	else
	{
	  listP = (LinkedList)mapSimpleTermProdTransformation.get(currentAlt+".P"+tmpName);
	}

	listCurrentAlt.remove(lastSimpleTerm);
      }

      boolean blistL = false;

      if( ( (typeOfExplicitElemName != null) && (listL!= null) && (listL.size()==1) && ResolveIds.name((String)listL.getFirst()).equals(typeOfExplicitElemName.substring(1)) ) ||
	  ( (typeOfExplicitElemName == null) && (listP!= null) && (listP.size()==1) && listP.contains(node.getId().getText()) ) )
      {
        blistL = true;
      }
      
      //the production is transformed but it appears without a term tail.
      if( (node.getSimpleTermTail() == null) &&
	  ( (listL != null) || (listP != null) ) && !blistL )
      {
        if(typeOfExplicitElemName != null)
	{
          error9(node.getId(), typeOfExplicitElemName);
	}
	else
	{
          error9(node.getId(), node.getId().getText());
	}
      }

      //the production is not transformed but it appears with a term tail.
      if(node.getSimpleTermTail() != null)
      {
        strTermTail = node.getSimpleTermTail().getText();

	if( ( (listL == null) || !listL.contains(strTermTail) ) &&
	    ( (listP == null) || !listP.contains(strTermTail) ) )
	{
          error13(node.getId(), node.getId().getText(), node.getSimpleTermTail().getText() );
	}
	else
	{
          ListIterator iter = null;

	  if(listL != null)
	  {
            iter = listL.listIterator();

	    position = listL.indexOf(strTermTail);

	    while(iter.hasNext())
            {
              if( strTermTail.equals((String)iter.next()) )
	      {
                iter.remove();
		break;
	      }
	    }
	  }
	  
	  if(listP != null)
	  {
            iter = listP.listIterator();
	    
	    position = listP.indexOf(strTermTail);
	   
	    while(iter.hasNext())
	    {
              if( strTermTail.equals((String)iter.next()) )
	      {
                iter.remove();
		break;
	      }
	    }
	  }
	}
      }

      if(node.getSimpleTermTail() != null)
      {
        String termtail = node.getSimpleTermTail().getText();
	LinkedList listProdContains = null;
	LinkedList prodContains = null;
	if(typeOfExplicitElemName != null)
	{
          listProdContains = (LinkedList)prodTransformIds.mapProdTransformContainsList.get(typeOfExplicitElemName);
	  prodContains = (LinkedList)prodTransformIds.prod_transforms.get(typeOfExplicitElemName);
	}
	else
	{
          listProdContains = (LinkedList)prodTransformIds.mapProdTransformContainsList.get("P" + ResolveIds.name(node.getId().getText()));
	  prodContains = (LinkedList)prodTransformIds.prod_transforms.get("P" + ResolveIds.name(node.getId().getText()));
	}

	LinkedList lst = (LinkedList)altIds.alts_elems_list.get(currentAlt);
      }
      
      //The Type of the element without his eventual termtail (term.termtail :: (type of term))

      if(typeOfExplicitElemName != null)
      {
	simpleTermOrsimpleListTermTypes.put(node, typeOfExplicitElemName);
      }
      else
      {
        simpleTermOrsimpleListTermTypes.put(node, "P" + ResolveIds.name(node.getId().getText()));
      }

      if(node.getSimpleTermTail() == null)
      {
	altTransformElemTypes.put(node, alternativeElemType);
	position = 0;
      }
      else
      {
	String termTail = node.getSimpleTermTail().getText();
	//This boolean is used to ensures that optional operator is properly propagate.
	boolean qmark_op = false;
	if(alternativeElemType.endsWith("?"))
	{
	  alternativeElemType = alternativeElemType.substring(0, alternativeElemType.length()-1);
	  qmark_op = true;
	}
	String typeOfTerm = (String)prodTransformIds.prodTransformElemTypesString.get(alternativeElemType+"."+termTail);
	//System.out.println("$$$$$$$$$$$$$$$toto -- typeOfTerm : " + typeOfTerm);
	// The substring is done because we want to ensures that lists term should be wrapped by square brackets
	if(typeOfTerm.startsWith("L"))
        {
          typeOfTerm = typeOfTerm.substring(1);
	}
	if(qmark_op && !typeOfTerm.endsWith("?"))
	{
	  typeOfTerm += "?";
	}
	altTransformElemTypes.put(node, typeOfTerm);
      }
    }
  }

  private Object curr_prodTransformElems[];
  private boolean curr_prod_has_prodTransform;
  private boolean firstProduction = false;

  public void outAProductions(AProductions node)
  {
    List list = node.getProds();
    AProd prod = (AProd)list.get(0);
    firstProduction = true;

    if( prodTransformIds.listProdTransformList.contains("P"+ ResolveIds.name( prod.getId().getText()) ) )
    {
      errorFirst(prod.getId());
    }

    Object temp[] = node.getProds().toArray();
    for(int i = 0; i < temp.length; i++)
    {

      ((PProd) temp[i]).apply(new DepthFirstAdapter()
                              {
                                public void inAProd(AProd production)
                                {
                                  List prodTransform = production.getProdTransform();
                                  String prodTransformElemType = "";
                                  curr_prodTransformElems = null;
                                  curr_prod_has_prodTransform = false;

                                  if(production.getArrow() != null && prodTransform.size() >= 1)
                                  {
				    curr_prod_has_prodTransform = true;
                                    curr_prodTransformElems = prodTransform.toArray();
                                    prodTransformElemType = (String)prodTransformIds.prodTransformElemTypes.get(curr_prodTransformElems[0]);
                                  }
                                  else if(production.getArrow() == null)
                                  {
                                    curr_prod_has_prodTransform = false;
                                    String []tab = new String[1];
                                    tab[0] = "P" + ResolveIds.name(production.getId().getText());
                                    curr_prodTransformElems = (Object[])tab;
                                    prodTransformElemType = (String)curr_prodTransformElems[0];
                                  }
                                  else
                                  {
                                    curr_prod_has_prodTransform = false;
                                    String []tab = new String[1];
                                    tab[0] = "nothing";
                                    curr_prodTransformElems = (Object[])tab;
                                    prodTransformIds.prodTransformElemTypes.put(prodTransform, "nothing");
                                    prodTransformElemType = (String)curr_prodTransformElems[0];
                                  }

                                  if(firstProduction)
                                  {
                                    if( !prodTransformElemType.equals(transformIds.getFirstAstProduction()) )
                                    {
                                      error22();
                                    }
                                    firstProduction = false;
                                  }
                                }

                                public void inAAltTransform(AAltTransform node)
                                {
				  Object curr_altTransformTerms[] = node.getTerms().toArray();

				  for(int k = 0; k < curr_altTransformTerms.length; k++)
				  {
                                    String prodTransformElemType, altTransformTermType;

				    if(curr_prod_has_prodTransform)
				    {
				      prodTransformElemType = (String)prodTransformIds.prodTransformElemTypes.get(curr_prodTransformElems[k]);
				    }
				    else
				    {
				      prodTransformElemType = (String)curr_prodTransformElems[k];
				    }
				    altTransformTermType = (String)altTransformElemTypes.get(curr_altTransformTerms[k]);

				    PUnOp elemOp = ((AElem)curr_prodTransformElems[k]).getUnOp();

				    if( !checkTypeCompability(prodTransformElemType, altTransformTermType, elemOp) )
				    {

				      error21(node.getLBrace(), prodTransformElemType, altTransformTermType);
				    }

				  }
				  
				  if(curr_altTransformTerms.length == 0)
                                  {
                                    String prodTransformElemType = (String)curr_prodTransformElems[0];
                                    String altTransformTermType = (String)altTransformElemTypes.get(node);
                                    if(!prodTransformElemType.equals(altTransformTermType))
                                    {
                                      error21(node.getLBrace(), prodTransformElemType, altTransformTermType);
                                    }
                                  }
			      }

	  }
			      );
    }
  }

  private boolean checkTypeCompability(String elemType, String termType, PUnOp elemOp)
  {
    //System.out.println("elemOp :: " + elemOp);
    if( elemOp != null )
    {
      if( elemOp instanceof AStarUnOp)
      {
        if(!elemType.startsWith("L") )
	{
          elemType = "L" + elemType;
	}
	if(!elemType.endsWith("?"))
        {
	  elemType = elemType + "?";
	}
      }
      else if( elemOp instanceof APlusUnOp)
      {
        if(!elemType.startsWith("L"))
	{
	  elemType = "L" + elemType;
	}
      }
      else
      {
	if(!elemType.endsWith("?"))
	{
	  elemType = elemType + "?";
	}
      }
    }
    /* 
    if(elemType.endsWith("?") && (termType.equals("LNull") || termType.equals("Null") ) )
    {
      if(elemType.startsWith("L") && termType.equals("LNull") || 
	 !elemType.startsWith("L") && termType.equals("Null") )
      {
	return true;
      }
    }
    if(elemType.endsWith("?"))
    {
      elemType = elemType.substring(0,elemType.length()-1);
    }
    return termType.equals(elemType);
    */
    //System.out.println("@@@@@@@@@   termType :: " + termType + " elemType :: " + elemType);
    if(!elemType.endsWith("?"))
    {
      //System.out.println("%%%%%  passe dans !elemType.endsWith(\"?\")");
      return termType.equals(elemType);
    }
    else
    {
      if(elemType.startsWith("L"))
      {
	//System.out.println("%%%%%  passe dans elemType.startsWith(\"L\")" );
	return termType.equals(elemType) ||
	       termType.equals(elemType.substring(0, elemType.length()-1)) ||
	       termType.equals("LNull") ;
      }
      //System.out.println("%%%%%  passe dans not elemType.startsWith(\"L\")" );
      return termType.equals(elemType) || 
	     termType.equals(elemType.substring(0, elemType.length()-1)) ||
	     termType.equals("Null") ;
    }
  }


  private static void error9(Token token, String name)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "production " + name + " was transformed. It can't appear here again");
  }

  private  void error10(Token token)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "this alternative transformation is incorrect because the number of "+
      " production transformation and alternatives ones doesn't match");
  }

  private static void error11(Token token, String name)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      name + " must be one of the elements on the left side of the arrow "+
      " or is already refered to in this alternative" );
  }

  private static void error12(Token token)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "tokens are never transformed. This syntax is incorrect");
  }

  private static void error13(Token token, String prod_name, String name)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "production " + prod_name + " was never transformed to "+name+
      " or #"+name+"# cannot be refered by #" + prod_name + "# twice in the same alternative");
  }

  private static void error14(Token token, String name)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "alternative "+ name +" doesn't exist in section AST");
  }

  private static void error15(Token token, String name)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "Production "+ name + " doesn't exist in section AST");
  }

  private static void error16(Token token, String name)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "this transformation is incorrect. The current production "+
      "can not be transformed to production "+ name);
  }

  private static void error17(Token token, String name)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "The number of parameters requested for alternative "+name+" in "+
      "the AST section doesn't match");
  }

  private static void error19(Token token)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "The use of specifier here is not permit");
  }

  private static void error20(Token token)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "The type of elements in a list must the same");
  }

  private static void error21(Token token, String prodName, String altName)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "expecting " + prodName + " when " + altName + " was found");
  }

  private static void error22()
  {
    throw new RuntimeException(
      "The first production transformation must refered to the first production in section AST"
    );
  }

  private static void errorFirst(Token token)
  {
    throw new RuntimeException(
      "[" + token.getLine() + "," + token.getPos() + "] " +
      "The first production's transformation must be only one elements without an operator.");
  }

  public String toString()
  {
    StringBuffer s = new StringBuffer();
    String nl = System.getProperty("line.separator");

    s.append("ast_elems");
    s.append(nl);
    s.append(nl);

    return s.toString();
  }
}
