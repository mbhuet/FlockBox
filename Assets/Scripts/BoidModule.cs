using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoidVector
{
    SEPARATE,
    ALIGN,
    COHESION,
    AVOID
}


[System.Serializable]
public abstract class BoidModule {
    protected Boid mine;
    public BoidModule() { }

    public virtual void SetOwner(Boid owner) {
        mine = owner;
    }

    public float GetModFactor(Boid mine, Boid other, BoidVector vectorType)
    {
        switch (vectorType)
        {
            case BoidVector.SEPARATE:
                return SeparationFactor(other);
                break;
            case BoidVector.ALIGN:
                return AlignmentFactor(other);
                break;
            case BoidVector.COHESION:
                return CohesionFactor(other);
                break;
        }
        return 1;
    }
    public float GetModFactor(Boid mine, Obstacle other, BoidVector vectorType)
    {
        switch (vectorType)
        {
            case BoidVector.AVOID:
                return AvoidanceFactor(other);
                break;
        }
        return 1;
    }

    protected virtual float AvoidanceFactor(Obstacle other) { return 1; }
    protected virtual float SeparationFactor(Boid other) { return 1; }
    protected virtual float CohesionFactor(Boid other) { return 1; }
    protected virtual float AlignmentFactor(Boid other) { return 1; }
}
