using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyAgent : FaunaAgent {


    protected static List<PreyAgent> preyCache;
    protected static int numPrey = 0;



    protected void Update()
    {
        base.Update();
        if (fsm.State!= EcoState.FLEE && (bool)GetAttribute(FleeBehavior.fleeAttributeName))
        {
            fsm.ChangeState(EcoState.FLEE);
        }
    }



    


    


}
