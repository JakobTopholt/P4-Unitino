/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This file is part of SableCC.                             *
 * See the file "LICENSE" for copyright information and the  *
 * terms and conditions for copying, distribution and        *
 * modification of SableCC.                                  *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

package org.sablecc.sablecc;

import java.util.*;
import org.sablecc.sablecc.analysis.*;
import org.sablecc.sablecc.node.*;
import java.io.*;
import org.sablecc.sablecc.Grammar;
import java.util.Vector;
import java.util.Enumeration;

public class GenerateAlternativeCodeForParser extends DepthFirstAdapter
{
  String currentAlt;
  String realcurrentAlt;
  BufferedWriter file;
  private File pkgDir;

  private ResolveTransformIds transformIds;
  private ComputeCGNomenclature CG;
  private ComputeSimpleTermPosition CTP;
  private MacroExpander macros;
  private Map simpleTermTransform;
  private LinkedList listSimpleTermTransform;
  private Map simpleTermOrsimpleListTermTypes;
  private ParserData.Rule rule;

  GenerateAlternativeCodeForParser(File pkgDir, String aParsedAltName,
                                   String raParsedAltName,
                                   BufferedWriter file,
                                   ResolveTransformIds transformIds,
                                   ComputeCGNomenclature CG,
                                   ComputeSimpleTermPosition CTP,
                                   Map simpleTermTransform,
                                   MacroExpander macros,
                                   LinkedList listSimpleTermTransform,
                                   Map simpleTermOrsimpleListTermTypes,
                                   ParserData.Rule rule)
  {
    this.pkgDir = pkgDir;
    this.file = file;
    currentAlt = aParsedAltName;
    realcurrentAlt = raParsedAltName;
    this.transformIds = transformIds;
    this.CG = CG;
    this.CTP = CTP;
    this.simpleTermTransform = simpleTermTransform;
    this.macros = macros;
    this.listSimpleTermTransform = listSimpleTermTransform;
    this.simpleTermOrsimpleListTermTypes = simpleTermOrsimpleListTermTypes;
    this.rule = rule;
  }

  public void inAAltTransform(AAltTransform node)
  {
    Object temp[] = node.getTerms().toArray();
    String type_name;
    int position;

    for(int i = 0; i < temp.length; i++)
    {
      if(simpleTermTransform.get(temp[i]) != null)
      {
        type_name = (String)simpleTermTransform.get(temp[i]);
      }
      else
      {
        type_name = (String)CG.getAltTransformElemTypes().get(temp[i]);
      }

      position = ((Integer)CG.getTermNumbers().get(temp[i])).intValue();

      try
      {
        if(type_name.startsWith("L"))
        {
          macros.apply(file, "ParserListVariableDeclaration", new String[] {"" + position});
          rule.actions.add (new ParserData.Action (ParserData.Action.MAKELIST, "listNode" + position,
            -1, null, type_name.substring(1)));
        }
        else if(type_name.equals("null"))
        {
          macros.apply(file, "ParserNullVariableDeclaration", new String[] {"" + position});
        }
        else
        {
          macros.apply(file, "ParserSimpleVariableDeclaration", new String[] {type_name, type_name.toLowerCase(), "" + position});
        }
      }
      catch(IOException e)
      {
        throw new RuntimeException("An error occured while writing to " +
                                   new File(pkgDir, "Parser.java").getAbsolutePath());
      }
    }
  }

  public void outAAltTransform(AAltTransform node)
  {
    Object temp[] = node.getTerms().toArray();
    String type_name;
    int position;

    try
    {
      for(int i = 0; i < temp.length; i++)
      {
        if(simpleTermTransform.get(temp[i]) != null)
        {
          type_name = (String)simpleTermTransform.get(temp[i]);
        }
        else
        {
          type_name = (String)CG.getAltTransformElemTypes().get(temp[i]);
        }

        position = ((Integer)CG.getTermNumbers().get(temp[i])).intValue();

        if(type_name.startsWith("L"))
        {
          type_name = "list";
          rule.actions.add (new ParserData.Action (ParserData.Action.RETURNLIST, null, -1,
            new String[] {"listNode" + position}, null));
        }
        else if(type_name.equals("null"))
        {
          type_name = "null";
          rule.actions.add (new ParserData.Action (ParserData.Action.RETURNNODE, null, -1,
            new String[] { null }, null));
        }
        else
        {
          rule.actions.add (new ParserData.Action (ParserData.Action.RETURNNODE, null, -1,
            new String[] {type_name.toLowerCase() + "Node" + position}, type_name));
          type_name = type_name.toLowerCase();
        }
        macros.apply(file, "ParserNewBodyListAdd", new String[] {type_name, "" + position});
      }
      macros.apply(file, "ParserNewTail");
    }
    catch(IOException e)
    {
      throw new RuntimeException("An error occured while writing to " +
                                 new File(pkgDir, "Parser.java").getAbsolutePath());
    }
  }

  public void inAParams(List list_param)
  {
    String type_name;
    int position;

    Object temp[] = list_param.toArray();

    for(int i = 0; i < temp.length; i++)
    {
      if(simpleTermTransform.get(temp[i]) != null)
      {
        type_name = (String)simpleTermTransform.get(temp[i]);
      }
      else
      {
        type_name = (String)CG.getAltTransformElemTypes().get(temp[i]);
      }
      position = ((Integer)CG.getTermNumbers().get(temp[i])).intValue();

      try
      {
        if(type_name.startsWith("L"))
        {
          macros.apply(file, "ParserListVariableDeclaration", new String[] {"" + position});
          rule.actions.add (new ParserData.Action (ParserData.Action.MAKELIST, "listNode" + position,
            -1, null, type_name.substring(1)));
        }
        else if(type_name.equals("null"))
        {
          macros.apply(file, "ParserNullVariableDeclaration", new String[] {"" + position});
        }
        else
        {
          macros.apply(file, "ParserSimpleVariableDeclaration", new String[] {type_name, type_name.toLowerCase(), "" + position});
        }
      }
      catch(IOException e)
      {
        throw new RuntimeException("An error occured while writing to " +
                                   new File(pkgDir, "Parser.java").getAbsolutePath());
      }
    }
  }

  public void inASimpleTerm(ASimpleTerm node)
  {
    try
    {
      String type_name;
      if(simpleTermTransform.get(node) != null)
      {
        type_name = (String)simpleTermTransform.get(node);
      }
      else
      {
        type_name = (String)CG.getAltTransformElemTypes().get(node);
      }
      int position = ((Integer)CG.getTermNumbers().get(node)).intValue();
      String termKey = currentAlt+"."+node.getId().getText();
      int elemPosition = ((Integer)CTP.elems_position.get(termKey)).intValue();
      int positionMap = 0;

      if(node.getSimpleTermTail() != null )
      {
        if( !listSimpleTermTransform.contains(node.getId().getText() ) )
        {
          String type = (String)CTP.positionsMap.get( realcurrentAlt+"."+node.getId().getText() );
          LinkedList list = (LinkedList)transformIds.getProdTransformIds().prod_transforms.get(type);
          if( list.indexOf( node.getSimpleTermTail().getText() ) >= 0 )
	  {
	    positionMap = list.indexOf( node.getSimpleTermTail().getText() );
	  }
        }

        if(simpleTermOrsimpleListTermTypes.get(node) != null)
        {
          String type = (String)simpleTermOrsimpleListTermTypes.get(node);
          LinkedList list = (LinkedList)transformIds.getProdTransformIds().prod_transforms.get(type);
          if( list.indexOf( node.getSimpleTermTail().getText() ) >= 0 )
	  {
	    positionMap = list.indexOf( node.getSimpleTermTail().getText() );
	  }
	}
      }

      String type;
      if(type_name.startsWith("L"))
      {
        String t = type_name.substring(1);
        type_name = "list";
        type = "TypedLinkedList";

        rule.actions.add (new ParserData.Action (ParserData.Action.FETCHLIST, type_name.toLowerCase() + "Node" + position,
          positionMap, new String[] {"nodeArrayList" +  elemPosition}, t));
      }
      else if(type_name.equals("null"))
      {
        type_name = "null";
        type = "Object";
      }
      else
      {
        type = type_name;

        rule.actions.add (new ParserData.Action (ParserData.Action.FETCHNODE, type_name.toLowerCase() + "Node" + position,
          positionMap, new String[] {"nodeArrayList" +  elemPosition}, type));
      }

      macros.apply(file, "ParserSimpleTerm", new String[]
                   {
                     type_name.toLowerCase(), ""+position,
                     type, ""+elemPosition, ""+positionMap
                   }
                  );
    }
    catch(IOException e)
    {
      throw new RuntimeException("An error occured while writing to " +
                                 new File(pkgDir, "Parser.java").getAbsolutePath());
    }
  }

  public void inASimpleListTerm(ASimpleListTerm node)
  {
    try
    {
      String type_name;
      if(simpleTermTransform.get(node) != null)
      {
        type_name = (String)simpleTermTransform.get(node);
      }
      else
      {
        type_name = (String)CG.getAltTransformElemTypes().get(node);
      }

      String termKey = currentAlt+"."+node.getId().getText();
      int position = ((Integer)CG.getTermNumbers().get(node)).intValue();

      int elemPosition = ((Integer)CTP.elems_position.get(termKey)).intValue();

      int positionMap = 0;

      if(node.getSimpleTermTail() != null )
      {
        if( !listSimpleTermTransform.contains(node.getId().getText()) )
        {
          String type = (String)CTP.positionsMap.get( realcurrentAlt+"."+node.getId().getText() );
          LinkedList list = (LinkedList)transformIds.getProdTransformIds().prod_transforms.get(type);
          if( list.indexOf( node.getSimpleTermTail().getText() ) >= 0 )
	  {
	    positionMap = list.indexOf( node.getSimpleTermTail().getText() );
	  }
        }

        if(simpleTermOrsimpleListTermTypes.get(node) != null)
        {
          String type = (String)simpleTermOrsimpleListTermTypes.get(node);
          LinkedList list = (LinkedList)transformIds.getProdTransformIds().prod_transforms.get(type);
          if( list.indexOf( node.getSimpleTermTail().getText() ) >= 0 )
	  {
	    positionMap = list.indexOf( node.getSimpleTermTail().getText() );
	  }
        }
      }

      String type;
      if(type_name.startsWith("L"))
      {
        String t = type_name.substring(1);
        type_name = "list";
        type = "TypedLinkedList";

        rule.actions.add (new ParserData.Action (ParserData.Action.FETCHLIST, type_name.toLowerCase() + "Node" + position,
          positionMap, new String[] {"nodeArrayList" +  elemPosition}, t));
      }
      else if(type_name.equals("null"))
      {
        type_name = "null";
        type = "Object";
      }
      else
      {
        type = type_name;

        rule.actions.add (new ParserData.Action (ParserData.Action.FETCHNODE, type_name.toLowerCase() + "Node" + position,
          positionMap, new String[] {"nodeArrayList" +  elemPosition}, type));
      }

      macros.apply(file, "ParserSimpleTerm", new String[]
                   {
                     type_name.toLowerCase(), ""+position,
                     type, ""+elemPosition, ""+positionMap
                   }
                  );
    }
    catch(IOException e)
    {
      throw new RuntimeException("An error occured while writing to " +
                                 new File(pkgDir, "Parser.java").getAbsolutePath());
    }
  }

  public void inANewTerm(ANewTerm node)
  {
    try
    {
      macros.apply(file, "ParserBraceOpening");
    }
    catch(IOException e)
    {
      throw new RuntimeException("An error occured while writing to " +
                                 new File(pkgDir, "Parser.java").getAbsolutePath());
    }
    inAParams(node.getParams());
  }

  public void inANewListTerm(ANewListTerm node)
  {
    try
    {
      macros.apply(file, "ParserBraceOpening");
    }
    catch(IOException e)
    {
      throw new RuntimeException("An error occured while writing to " +
                                 new File(pkgDir, "Parser.java").getAbsolutePath());
    }
    inAParams(node.getParams());
  }

  public void inAListTerm(AListTerm node)
  {
    try
    {
      macros.apply(file, "ParserBraceOpening");
      Object temp[] = node.getListTerms().toArray();

      for(int i = 0; i < temp.length; i++)
      {
        String type_name;
        if(simpleTermTransform.get(temp[i]) != null)
        {
          type_name = (String)simpleTermTransform.get(temp[i]);
        }
        else
        {
          type_name = (String)CG.getAltTransformElemTypes().get(temp[i]);
        }
        int position = ((Integer)CG.getTermNumbers().get(temp[i])).intValue();

        if(type_name.startsWith("L"))
        {
          macros.apply(file, "ParserListVariableDeclaration", new String[] {"" + position});
          // we declare as we fetch so no MAKELIST neccessary
        }
        else if(type_name.equals("null"))
        {
          macros.apply(file, "ParserNullVariableDeclaration", new String[] {"" + position});
        }
        else
        {
          macros.apply(file, "ParserSimpleVariableDeclaration", new String[] {type_name, type_name.toLowerCase(), "" + position});
        }
      }
    }
    catch(IOException e)
    {
      throw new RuntimeException("An error occured while writing to " +
                                 new File(pkgDir, "Parser.java").getAbsolutePath());
    }
  }

  public void outAListTerm(AListTerm node)
  {
    try
    {
      Object temp[] = node.getListTerms().toArray();
      int listPosition = ((Integer)CG.getTermNumbers().get(node)).intValue();

      for(int i = 0; i < temp.length; i++)
      {
        String type_name;
        if(simpleTermTransform.get(temp[i]) != null)
        {
          type_name = (String)simpleTermTransform.get(temp[i]);
        }
        else
        {
          type_name = (String)CG.getAltTransformElemTypes().get(temp[i]);
        }
        int position = ((Integer)CG.getTermNumbers().get(temp[i])).intValue();

        if(!type_name.equals("null"))
        {
          if(type_name.startsWith("L"))
          {
            macros.apply(file, "ParserTypedLinkedListAddAll", new String[] {"list", ""+listPosition, "list", ""+ position});
            rule.actions.add (new ParserData.Action (ParserData.Action.ADDLIST, null, -1,
              new String[] {"listNode" + listPosition, "listNode" + position}, null));
          }
          else
          {
            macros.apply(file, "ParserTypedLinkedListAdd", new String[] {"list", ""+listPosition, type_name.toLowerCase(), ""+ position});
            rule.actions.add (new ParserData.Action (ParserData.Action.ADDNODE, null, -1,
              new String[] {"listNode" + listPosition,  type_name.toLowerCase() + "Node" + position}, type_name));
          }
        }
      }
      macros.apply(file, "ParserBraceClosing");
    }
    catch(IOException e)
    {
      throw new RuntimeException("An error occured while writing to " +
                                 new File(pkgDir, "Parser.java").getAbsolutePath());
    }
  }

  public void outANewTerm(ANewTerm node)
  {
    String type_name = (String)CG.getAltTransformElemTypes().get(node);
    if(simpleTermTransform.get(node) != null)
    {
      type_name = (String)simpleTermTransform.get(node);
    }
    else
    {
      type_name = (String)CG.getAltTransformElemTypes().get(node);
    }
    int position = ((Integer)CG.getTermNumbers().get(node)).intValue();
    String newAltName = name((AProdName)node.getProdName());

    try
    {
      if(type_name.startsWith("L"))
      {
        type_name = "list";
      }
      else
      {
        type_name = type_name.toLowerCase();
      }
      macros.apply(file, "ParserNewBodyNew", new String[] {type_name, ""+position, newAltName});

      Vector args = new Vector();
      String result_name = type_name + "Node" + position;

      if(node.getParams().size() > 0)
      {
        Object temp[] = node.getParams().toArray();
        String isNotTheFirstParam = "";

        for(int i = 0; i < temp.length; i++)
        {
          if(simpleTermTransform.get(temp[i]) != null)
          {
            type_name = (String)simpleTermTransform.get(temp[i]);
          }
          else
          {
            type_name = (String)CG.getAltTransformElemTypes().get(temp[i]);
          }
          position = ((Integer)CG.getTermNumbers().get(temp[i])).intValue();

          if(i != 0)
          {
            isNotTheFirstParam = ", ";
          }

          if(type_name.equals("null"))
          {
            macros.apply(file, "ParserNew&ListBodyParamsNull", new String[] {isNotTheFirstParam+"null"});
            args.add(null);
          }
          else
          {
            if(type_name.startsWith("L"))
            {
              type_name = "list";
            }
            else
            {
              type_name = type_name.toLowerCase();
            }
            macros.apply(file, "ParserNew&ListBodyParams", new String[] {isNotTheFirstParam+type_name, ""+position});
            args.add(type_name + "Node" + position);
          }

        }
      }
      macros.apply(file, "ParserNewBodyNewTail");
      macros.apply(file, "ParserBraceClosing");

      String[] sargs = new String[args.size()];
      for ( int i = 0; i < args.size(); i++ ) sargs[i] = (String)args.get(i);
      rule.actions.add (new ParserData.Action (ParserData.Action.MAKENODE, result_name,
        -1, sargs, newAltName));
          
    }
    catch(IOException e)
    {
      throw new RuntimeException("An error occured while writing to " +
                                 new File(pkgDir, "TokenIndex.java").getAbsolutePath());
    }
  }

  public void outANewListTerm(ANewListTerm node)
  {
    String type_name;
    if(simpleTermTransform.get(node) != null)
    {
      type_name = (String)simpleTermTransform.get(node);
    }
    else
    {
      type_name = (String)CG.getAltTransformElemTypes().get(node);
    }
    int position = ((Integer)CG.getTermNumbers().get(node)).intValue();
    String newAltName = name((AProdName)node.getProdName());
    try
    {
      if(type_name.startsWith("L"))
      {
        type_name = "list";
      }
      else
      {
        type_name = type_name.toLowerCase();
      }

      macros.apply(file, "ParserNewBodyNew", new String[] {type_name, ""+position, newAltName});

      Vector args = new Vector();
      String result_name = type_name + "Node" + position;

      if(node.getParams().size() > 0)
      {
        Object temp[] = node.getParams().toArray();
        String isNotTheFirstParam = "";

        for(int i = 0; i < temp.length; i++)
        {
          if(simpleTermTransform.get(temp[i]) != null)
          {
            type_name = (String)simpleTermTransform.get(temp[i]);
          }
          else
          {
            type_name = (String)CG.getAltTransformElemTypes().get(temp[i]);
          }
          position = ((Integer)CG.getTermNumbers().get(temp[i])).intValue();

          if(i != 0)
          {
            isNotTheFirstParam = ", ";
          }

          if(type_name.equals("null"))
          {
            macros.apply(file, "ParserNew&ListBodyParamsNull", new String[] {isNotTheFirstParam+"null"});
            args.add(null);
          }
          else
          {
            if(type_name.startsWith("L"))
            {
              type_name = "list";
            }
            else
            {
              type_name = type_name.toLowerCase();
            }
            macros.apply(file, "ParserNew&ListBodyParams", new String[] {isNotTheFirstParam+type_name, ""+position});
            args.add(type_name + "Node" + position);
          }
        }
      }
      macros.apply(file, "ParserNewBodyNewTail");
      macros.apply(file, "ParserBraceClosing");

      String[] sargs = new String[args.size()];
      for ( int i = 0; i < args.size(); i++ ) sargs[i] = (String)args.get(i);
      rule.actions.add (new ParserData.Action (ParserData.Action.MAKENODE, result_name,
        -1, sargs, newAltName));
    }
    catch(IOException e)
    {
      throw new RuntimeException("An error occured while writing to " +
                                 new File(pkgDir, "TokenIndex.java").getAbsolutePath());
    }
  }

  public String name(AProdName node)
  {
    if(node.getProdNameTail() != null)
    {
      return "A" +
             ResolveIds.name(node.getProdNameTail().getText()) +
             ResolveIds.name(node.getId().getText());
    }
    return "A" + ResolveIds.name(node.getId().getText());
  }

}
