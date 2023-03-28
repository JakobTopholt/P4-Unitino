/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This file is part of SableCC.                             *
 * See the file "LICENSE" for copyright information and the  *
 * terms and conditions for copying, distribution and        *
 * modification of SableCC.                                  *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/*
 * Last Modification date : 17-11-2003
 * Fix simpleTerm and simpleListTerm bug related to
 * name confusion.
 * Now, elem_name is automatically added to an elem to avoid that.
 * The name has the form (production_name#alternative_name#elemId)
 * or (production_name#alternative_name#elemName)
 */

package org.sablecc.sablecc;

import java.util.*;
import org.sablecc.sablecc.analysis.*;
import org.sablecc.sablecc.node.*;

public class Inlining
{
  public static List productionsToBeRemoved =
    new TypedLinkedList(StringCast.instance);

  private AProd current_production;

  //The production to inline within current_production
  private In_Production prod_to_inline;

  public Inlining(AProd curr_prod, In_Production prod_to_inline)
  {
    this.current_production = curr_prod;
    this.prod_to_inline = prod_to_inline;
  }

  /*
   * The core of inlining is done here.
   * returns true if it succeed and false otherwise
  */
  public boolean inlineProduction()
  {
    AParsedAlt[] alts = (AParsedAlt[])current_production.getAlts().toArray(new AParsedAlt[0]);
    final BooleanEx prodMustBeInlined = new BooleanEx(false);

    /*
      We're trying to detect if the current production must be inlined.
      ie one of its alternatives contains production to inline
    */
    for(int i=0; i<alts.length; i++)
    {
      ((PAlt)alts[i]).apply( new DepthFirstAdapter()
                             {
                               public void caseAElem(AElem node)
                               {
                                 String elem_name = node.getId().getText();

                                 if(elem_name.equals(prod_to_inline.getName()) &&
                                     !(node.getSpecifier() instanceof ATokenSpecifier) )
                                 {
                                   prodMustBeInlined.setValue(true);
                                 }
                               }
                             }
                           );
      //We only need to know if one element within one of the production alternatives matches.
      if(prodMustBeInlined.getValue())
      {
        break;
      }
    }

    if(prodMustBeInlined.getValue())
    {
      //list of productions which inlining was a success.
      if( !productionsToBeRemoved.contains(ResolveIds.name(prod_to_inline.getName())) )
      {
        productionsToBeRemoved.add("P" + ResolveIds.name(prod_to_inline.getName()));
      }

      /*
      Once we detect that the production can be inline, 
      we try to inline each of its alternatives.
       */
      List listOfAlts = new TypedLinkedList(NodeCast.instance);
      for(int i=0; i<alts.length; i++)
      {
        listOfAlts.addAll( inlineAlternative(alts[i]) );
      }
      current_production.setAlts(listOfAlts);
      return true;
    }

    return false;
  }

  /*
   * Inlining of an alternative
   *
   */
  public List inlineAlternative(AParsedAlt alt)
  {
    AElem[] elems = (AElem[])alt.getElems().toArray(new AElem[0]);
    String elem_name ;
    // This list contains the names of elements to inline within an alternative
    // The elem name can be either a production name or name given to it by user
    LinkedList eventualProdIdOrNames = new LinkedList();
    int occurenceOfProductionToInlineWithinTheAlternative = 0;

    for(int i=0; i<elems.length; i++)
    {
      elem_name = elems[i].getId().getText();

      /*
      Element to inline within an alternative is added to
      a list of occurrences of the production to inline
      */
      if(elem_name.equals(prod_to_inline.getName()) &&
          !(elems[i].getSpecifier() instanceof ATokenSpecifier) )
      {
        occurenceOfProductionToInlineWithinTheAlternative++;
        if(elems[i].getElemName() != null)
        {
          eventualProdIdOrNames.add( elems[i].getElemName().getText() );
        }
        else
        {
          eventualProdIdOrNames.add( elems[i].getId().getText() );
        }
      }
    }

    List resultingListOfAlts = new TypedLinkedList();
    resultingListOfAlts.add(alt);
    for(int i=0; i<occurenceOfProductionToInlineWithinTheAlternative; i++)
    {
      resultingListOfAlts = inline(resultingListOfAlts, i+1);
    }

    return resultingListOfAlts;
  }

  String alt_elem_info = null;
  /*
   * whichOccurence is used to number element within the alternative
   */
  public List inline(List altsList, int whichOccurence)
  {
    List resultList = new LinkedList();
    AParsedAlt[] alts = (AParsedAlt[])altsList.toArray(new AParsedAlt[0]);
    AParsedAlt aParsed_alt;
    Map mapOfNewTermNames;


    for(int i=0; i<alts.length; i++)
    {
      aParsed_alt = alts[i];

      for(int j=0; j<prod_to_inline.getNbAlts(); j++)
      {
        mapOfNewTermNames = new TypedHashMap(StringCast.instance,
                                             StringCast.instance);

        List listElems = inlineList(aParsed_alt.getElems(),
                                    prod_to_inline.getAlternative(j).getElems(),
                                    mapOfNewTermNames);
        AAltTransform aAltTransform =
          (AAltTransform)((AAltTransform)aParsed_alt.getAltTransform()).clone();
        final Map currentMap = prod_to_inline.getAlternative(j).getProdTransform_AlTransformMap();

        aAltTransform.apply(new DepthFirstAdapter()
                            {
                              public void caseASimpleTerm(ASimpleTerm node)
                              {
                                if(node.getId().getText().equals(alt_elem_info)  &&
                                    !(node.getSpecifier() instanceof ATokenSpecifier) )
                                {
                                  String termTail;
                                  if(node.getSimpleTermTail() != null)
                                  {
                                    termTail = node.getSimpleTermTail().getText();
                                  }
                                  else
                                  {
                                    termTail = prod_to_inline.getName();
                                  }

                                  PTerm term = (PTerm)((PTerm)currentMap.get(termTail)).clone();

                                  if(currentMap.get(termTail) != null)
                                  {
                                    node.replaceBy(term);
                                  }
                                }
                              }

                              public void caseASimpleListTerm(final ASimpleListTerm node_)
                              {
                                if(node_.getId().getText().equals(alt_elem_info)  &&
                                    !(node_.getSpecifier() instanceof ATokenSpecifier) )
                                {
                                  String termTail;
                                  if(node_.getSimpleTermTail() != null)
                                  {
                                    termTail = node_.getSimpleTermTail().getText();
                                  }
                                  else
                                  {
                                    termTail = prod_to_inline.getName();
                                  }

                                  if(currentMap.get(termTail) != null)
                                  {
                                    PTerm term = (PTerm)currentMap.get(termTail);

                                    if( !(currentMap.get(termTail) instanceof ANewListTerm) &&
                                        !(currentMap.get(termTail) instanceof ASimpleListTerm)
                                      )
                                    {
                                      term.apply(new DepthFirstAdapter()
                                                 {
                                                   public void caseANewTerm(ANewTerm node)
                                                   {
                                                     node_.replaceBy( new ANewListTerm(   (AProdName)node.getProdName().clone(),
                                                                                          (TLPar)node.getLPar().clone(),
                                                                                          (LinkedList)cloneList(node.getParams())
                                                                                      )
                                                                    );
                                                   }

                                                   public void caseASimpleTerm(ASimpleTerm node)
                                                   {
                                                     PSpecifier specifier = null;
                                                     TId simpleTermTail = null;
                                                     if(node.getSpecifier() != null)
                                                     {
                                                       specifier = (PSpecifier)node.getSpecifier().clone();
                                                     }
                                                     if(node.getSimpleTermTail() != null)
                                                     {
                                                       simpleTermTail = (TId)node.getSimpleTermTail().clone();
                                                     }
                                                     node_.replaceBy( new ASimpleListTerm(  specifier,
                                                                                            (TId)node.getId().clone(),
                                                                                            simpleTermTail
                                                                                         )
                                                                    );
                                                   }

                                                   public void caseNullTerm(ANullTerm node)
                                                   {
                                                     node_.replaceBy( null );
                                                   }

                                                   public void caseAListTerm(AListTerm node)
                                                   {
                                                     AListTerm parent = (AListTerm)node_.parent();
                                                     List oldlistTerms = parent.getListTerms();
                                                     List newlistTerms = new LinkedList();

                                                     Object[] oldListTermsArray = (Object[]) oldlistTerms.toArray();
                                                     for(int i=0; i<oldListTermsArray.length; i++)
                                                     {
                                                       if(oldListTermsArray[i] != node_)
                                                       {
                                                         if(oldListTermsArray[i] instanceof PTerm)
                                                         {
                                                           newlistTerms.add( ((PTerm)oldListTermsArray[i]).clone() );
                                                         }
                                                         else
                                                         {
                                                           newlistTerms.add( ((PListTerm)oldListTermsArray[i]).clone() );
                                                         }
                                                       }
                                                       else
                                                       {
                                                         newlistTerms.addAll(cloneList(node.getListTerms()));
                                                       }
                                                     }
                                                     parent.setListTerms(newlistTerms);
                                                   }
                                                 }
                                                );
                                    }
                                    else
                                    {
                                      node_.replaceBy(term);
                                    }
                                  }
                                }
                              }
                            }
                           );

        AAltTransform tmpaAltTransform = (AAltTransform)aAltTransform.clone();
        fixSimpleTermOrSimpleListTermNames(tmpaAltTransform, mapOfNewTermNames);
        String newAltName;
        if(aParsed_alt.getAltName() != null)
        {
          newAltName = aParsed_alt.getAltName().getText()+ "$" +
                       prod_to_inline.getAlternative(j).getName() + whichOccurence;
        }
        else
        {
          newAltName = prod_to_inline.getAlternative(j).getName() + whichOccurence;
        }

        resultList.add( new AParsedAlt(new TId(newAltName),
                                       listElems,
				       tmpaAltTransform)
                      );
      }
    }
    return resultList;
  }

  public List inlineList(List oldElemsList,
                         AElem[] inliningProductionsElems,
                         Map mapOfNewTermNames)
  {
    int position = 0;
    AElem[] listElems = (AElem[]) oldElemsList.toArray(new AElem[0]);
    for(int i=0; i<listElems.length; i++)
    {
      //We are looking for the position of the element inside the alternative.
      if( listElems[i].getId().getText().equals(prod_to_inline.getName()) )
      {
        position = i;
        if(listElems[i].getElemName() != null)
        {
          alt_elem_info = listElems[i].getElemName().getText();
        }
        else
        {
          alt_elem_info = listElems[i].getId().getText();
        }
        break;
      }
    }

    LinkedList list = new LinkedList();
    int elemPosition = 1;

    //Before the inlined element (old alternative elements)
    for(int i=0; i<position; i++)
    {
      list.add(((AElem)oldElemsList.get(i)).clone() );
    }

    // The inline element (new element added to the alternative)
    for(int i=0; i<inliningProductionsElems.length; i++)
    {
      list.add(inliningProductionsElems[i].clone());
    }

    // After the inlined element (old alternative elements)
    for(int i=position+1; i<listElems.length; i++)
    {
      list.add(((AElem)oldElemsList.get(i)).clone());
    }

    AElem[] listOfAltElems = (AElem[]) list.toArray(new AElem[0]);
    for(int i=0; i<listOfAltElems.length; i++)
    {
      String old_name = listOfAltElems[i].getId().getText();
      TId elemName = (TId)listOfAltElems[i].getElemName();
      if(elemName != null)
      {
        elemName = (TId)elemName;
        old_name = elemName.getText();
      }

      String elemNameString = (elemName != null ? elemName.getText() : "@elem@" );
      elemNameString += (i+1);
      listOfAltElems[i].setElemName(new TId(elemNameString));
      mapOfNewTermNames.put(old_name, elemNameString);
    }

    return list;
  }

  private void fixSimpleTermOrSimpleListTermNames(AAltTransform tmpaAltTransform,
      final Map mapOldNameNewNames)
  {
    tmpaAltTransform.apply(new DepthFirstAdapter()
                           {
                             public void caseASimpleTerm(ASimpleTerm node)
                             {
                               if(mapOldNameNewNames.get(node.getId().getText()) != null)
                               {
                                 node.setId(new TId( (String)mapOldNameNewNames.get(node.getId().getText()) ));
                               }
                             }

                             public void caseASimpleListTerm(ASimpleListTerm node)
                             {
                               if(mapOldNameNewNames.get(node.getId().getText()) != null)
                               {
                                 node.setId(new TId( (String)mapOldNameNewNames.get(node.getId().getText()) ));
                               }
                             }
                           }
                          );
  }

  private List cloneList(List list)
  {
    List clone = new LinkedList();

    for(Iterator i = list.iterator(); i.hasNext();)
    {
      clone.add(((Node) i.next()).clone());
    }

    return clone;
  }

  class BooleanEx
  {
    boolean value;

    BooleanEx(boolean value)
    {
      this.value = value;
    }

    void setValue(boolean value)
    {
      this.value = value;
    }

    boolean getValue()
    {
      return value;
    }
  }

}
