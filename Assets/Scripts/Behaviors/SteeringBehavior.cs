using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Each SteeringBehavior will be instantiated ONCE
//That instance will be used by all SteeringAgents



public abstract class SteeringBehavior {

    public abstract Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings, float effectiveDistance);
    public virtual void ModifyAttributes(SteeringAgent mine, SurroundingsInfo surroundings) { }
    //public abstract void CreatRequiredAttributes();
}
