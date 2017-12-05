﻿using System;
using System.Collections;
using System.Collections.Generic;
using Vexe.Runtime.Types;
using UnityEngine;

[System.Serializable]
public class SocialStatusBehavior : CohesionBehavior
{
    [VisibleWhen("isActive")]
    public Gradient statusSpectrum;
    [VisibleWhen("isActive")]
    public int maxStatus = 100;

    public const string statusAttributeName = "socialStatus";

    [VisibleWhen("isActive")]
    public AnimationCurve attractionCurve;

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(statusAttributeName)) mine.SetAttribute(statusAttributeName, GetRandomStatusValue());
        float myStatus = (float)mine.GetAttribute(statusAttributeName);
        mine.visual.SetColor(statusSpectrum.Evaluate(myStatus / maxStatus));

        Vector3 sum = Vector3.zero;   // Start with empty vector to accumulate all positions
        float count = 0;
        foreach (SteeringAgentWrapped other_wrap in surroundings.neighbors)
        {
            SteeringAgent other = other_wrap.agent;
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
            sum *= (mine.settings.maxSpeed);
            Vector3 steer = sum - mine.velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.settings.maxForce);
            //Debug.DrawRay(mine.position, steer, Color.yellow);
            return steer * weight;
        }
        else
        {
            return new Vector3(0, 0);
        }
    }

    private float GetRandomStatusValue()
    {
        return UnityEngine.Random.Range(0, maxStatus);
    }
}