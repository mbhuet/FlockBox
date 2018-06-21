using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorAgent : EcosystemAgent {
    public BehaviorSettings huntSettings;
    public BehaviorSettings wanderSettings;

    public override void CatchAgent(SteeringAgent other)
    {
        base.CatchAgent(other);
        fsm.ChangeState(EcoState.EAT);
    }

}
