using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    public static class AgentExtensions
    {
        public static void SetTargetAgent(this SteeringAgent mine, string attributeName, Agent target)
        {
            mine.SetAgentIntProperty(attributeName, target.agentID);
        }

        public static void ClearTargetAgent(this SteeringAgent mine, string attributeName)
        {
            mine.SetAgentIntProperty(attributeName, -1);
        }

        public static bool HasTargetAgent(this SteeringAgent mine, string attributeName)
        {
            if (!mine.HasAgentIntProperty(attributeName)) return false;
            return mine.GetAgentIntProperty(attributeName) >= 0;
        }

        public static Agent GetTargetAgent(this SteeringAgent mine, string attributeName)
        {
            if (!mine.HasAgentIntProperty(attributeName))
            {
                return null;
            }
            int chosenTargetID = mine.GetAgentIntProperty(attributeName);
            if (chosenTargetID != -1)
            {
                return Agent.GetAgentById(chosenTargetID);
            }
            return null;
        }

        //TODO - rename something like "contact target"
        public static void AttemptCatch(this SteeringAgent mine, Agent target)
        {
            if (mine.Overlaps(target))
            {
                mine.CatchAgent(target);
            }
        }
    }
}