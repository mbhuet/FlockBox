using System;
using System.Collections;
using System.Collections.Generic;
using Vexe.Runtime.Types;
using UnityEngine;

[System.Serializable]
public class SocialStatusBehavior : CohesionBehavior
{
    [VisibleWhen("isActive")]
    public int maxStatus = 100;

    [fSlider(0,1)]
    public float speedDampening = .1f;

    public const string statusAttributeName = "socialStatus";

    public static float bankedStatus = 0;

    [VisibleWhen("isActive")]
    public AnimationCurve attractionCurve;

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(statusAttributeName)) mine.SetAttribute(statusAttributeName, GetRandomStatusValue());
        float myStatus = (float)mine.GetAttribute(statusAttributeName);
        myStatus = RemoveTaxes(myStatus);

        mine.visual.UpdateForAttribute(statusAttributeName, myStatus);

        mine.SetAttribute(statusAttributeName, myStatus);


        Vector3 sum = Vector3.zero;   // Start with empty vector to accumulate all positions
        float count = 0;
        foreach (AgentWrapped other_wrap in GetFilteredAgents(surroundings))
        {
            Agent other = other_wrap.agent;
            float d = Vector3.Distance(mine.position, other_wrap.wrappedPosition);
            if ((d > 0) && (d < effectiveRadius))
            {
                if (other.HasAttribute(statusAttributeName))
                {
                    float statusDifferential = (float)other.GetAttribute(statusAttributeName) - myStatus;
                    
                        Vector3 mineToOther = (other_wrap.wrappedPosition - mine.position).normalized;
                    float attraction = attractionCurve.Evaluate((statusDifferential / maxStatus + 1) * .5f);
                        sum += mineToOther.normalized * attraction;
                        count += attraction;
                    //Debug.Log("diff " + statusDifferential + " attract " + attraction);
                    
                }
            }
        }

        if (count > 0)
        {
            sum /= ((float)count);

            // Implement Reynolds: Steering = Desired - Velocity
            sum.Normalize();
            sum *= (mine.activeSettings.maxSpeed);
            Vector3 steer = sum - mine.velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
            //Debug.DrawRay(mine.position, steer, Color.yellow);
            return steer * weight;
        }
        else
        {
            return new Vector3(0, 0);
        }
    }

    private float RemoveTaxes(float initialValue)
    {
        float tax = initialValue * .1f * Time.deltaTime;
        bankedStatus += tax;
        return initialValue - tax;
    }

    private float GetRandomStatusValue()
    {
        return UnityEngine.Random.Range(0, maxStatus);
    }
}
