using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodTarget : Target {

    public float minStatus;
    public float nourishAmount = 1;

    protected new void Start()
    {
        Vector3 pos = transform.position;
        transform.position = ZLayering.GetZLayeredPosition(pos);
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

    public override void CaughtBy(SteeringAgent agent)
    {
        base.CaughtBy(agent);
        Nourish(agent);
    }

    public override void Spawn(Vector2 position)
    {
        base.Spawn(position);
        visual.enabled = true;
    }

    void Nourish(SteeringAgent agent)
    {
        if (agent.HasAttribute(SocialStatusBehavior.statusAttributeName))
        {
            float status = (float)agent.GetAttribute(SocialStatusBehavior.statusAttributeName);
            status += nourishAmount;
            agent.SetAttribute(SocialStatusBehavior.statusAttributeName, status);
        }
    }


}
