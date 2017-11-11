using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialModule : BoidModule {
    public float status;
    const float maxStatus = 100;
    const float minStatus = 0;
    static Color lowStatusColor = Color.blue;
    static Color highStatusColor = Color.red;

    const float maxCohesionFactor = 100;

    public SocialModule() : base()
    {
        status = UnityEngine.Random.Range(minStatus, maxStatus);
    }

    public override void SetOwner(Boid owner)
    {
        base.SetOwner(owner);
        owner.sprite.color = Color.Lerp(lowStatusColor, highStatusColor, status / maxStatus);
    }

    protected override float AvoidanceFactor(Obstacle other) { return 1; }
    protected override float SeparationFactor(Boid other) {
        //separate more from lower status, 
        if (other.HasModuleOfType<SocialModule>())
        {
            //float mineTheirsStatusRatio = this.status / other.GetModuleOfType<SocialModule>().status;
            //Debug.Log("mine " + status + " theirs " + other.GetModuleOfType<SocialModule>().status + " ratio " + mineTheirsStatusRatio);
            return this.status / other.GetModuleOfType<SocialModule>().status;
        }
        return 1;

    }
    protected override float CohesionFactor(Boid other) {
        //attract to higher status, 
        if (other.HasModuleOfType<SocialModule>())
        {
            return Mathf.Clamp(other.GetModuleOfType<SocialModule>().status / this.status, 0, maxCohesionFactor) ;
        }
        return 1;
    }
    protected override float AlignmentFactor(Boid other) {
        return 1;
    }
}
