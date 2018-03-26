using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodTarget : Target {

    public float minStatus;

    protected new void Start()
    {
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, pos.y, ZLayering.YtoZPosition(pos.y));
        visual.transform.localPosition = Vector3.up * (TallVisual.StatusToHeight(minStatus) - 1);
        base.Start();
    }

    public override bool CanBePursuedBy(SteeringAgent agent)
    {
        bool sufficientStatus = true;
        if (agent.HasAttribute(SocialStatusBehavior.statusAttributeName))
        {
            float status = (float)agent.GetAttribute(SocialStatusBehavior.statusAttributeName);
            sufficientStatus = status >= minStatus;
        }
        return sufficientStatus && base.CanBePursuedBy(agent) ;
    }


}
