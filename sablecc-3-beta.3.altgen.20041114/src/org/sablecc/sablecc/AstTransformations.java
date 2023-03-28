/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This file is part of SableCC.                             *
 * See the file "LICENSE" for copyright information and the  *
 * terms and conditions for copying, distribution and        *
 * modification of SableCC.                                  *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

package org.sablecc.sablecc;

import org.sablecc.sablecc.analysis.*;
import org.sablecc.sablecc.node.*;
import java.util.*;
import java.io.*;

/*
 * AstTransformations
 *
 * Here we're removing ? operator and 
 * replacing + operator by * operator.
 */

public class AstTransformations extends DepthFirstAdapter
{
  public void caseAProductions(AProductions node)
  {}

  public void caseAElem(AElem node)
  {
    if(node.getUnOp() != null)
    {
      node.getUnOp().apply(new AnalysisAdapter()
                           {
                             public void caseAQMarkUnOp(AQMarkUnOp node)
                             {
                               node.replaceBy(null);
                             }

                             public void caseAPlusUnOp(APlusUnOp node)
                             {
                               node.replaceBy(new AStarUnOp(new TStar(node.getPlus().getLine(), node.getPlus().getPos())));
                             }

                           }
                          );
    }
  }
}
