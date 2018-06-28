using System;
using System.Collections;
using System.Collections.Generic;
using Vexe.Runtime.Types;
using UnityEngine;

[System.Serializable]
public class RelationshipBehavior : SteeringBehavior {

    [VisibleWhen("isActive")]   public int maxFriends;
    [VisibleWhen("isActive")]   public float minFriendshipDuration = 5;
    [VisibleWhen("isActive")]   public float maxFriendshipDuration = 10;
    [VisibleWhen("isActive")]   public float maxStatusDifference = 10;

    const string friendArrayAttributeName = "friends";
    const string friendTimersAttributeName = "friendTimers";
    const string ignoreFriendAttributeName = "ignoreFriend";

    public static bool drawConnections = true;


    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(friendArrayAttributeName)) CreateFriendshipSlots(mine);
        List<int> expired = ExpiredFriendships(mine);
        Agent[] friends = (Agent[])mine.GetAttribute(friendArrayAttributeName);

        Vector3 sum = Vector3.zero;
        int friendCount = 0;
        for(int friendIndex = 0; friendIndex<friends.Length; friendIndex++)
        {
            if (expired.Contains(friendIndex)) EndFriendship(mine, friends, friendIndex);
            else if(friends[friendIndex] != null)
            {
                Agent friend = friends[friendIndex];
                Vector3 wrappedPosition = ClosestPositionWithWrap(mine.position, friend.position);
                sum += wrappedPosition;
                friendCount++;
                if(drawConnections) GLDebug.DrawLine(mine.position, wrappedPosition, Color.yellow);
            }
        }

        if (friendCount < maxFriends)
        {
            SearchForNewFriends(mine, friends, GetFilteredAgents(surroundings));
        }

        if (friendCount > 0)
        {
            sum /= (friendCount);
            return mine.seek(sum) * weight;  // Steer towards the position
        }
        else
        {
            return new Vector3(0, 0);
        }
    }

    List<int> ExpiredFriendships(Agent agent)
    {
        List<int> expiredFriendships = new List<int>();
        float[] timers = (float[])agent.GetAttribute(friendTimersAttributeName);
        for (int i = 0; i<timers.Length; i++)
        {
            timers[i] -= Time.deltaTime;
            if (timers[i] <= 0) expiredFriendships.Add(i);
        }
        agent.SetAttribute(friendTimersAttributeName, timers);
        return expiredFriendships;
    }

    void EndFriendship(Agent agent, int index)
        {   EndFriendship(agent, (Agent[])agent.GetAttribute(friendArrayAttributeName), index);   }
    void EndFriendship(Agent agent, Agent[] friends, int index)
    {
        agent.SetAttribute(ignoreFriendAttributeName, friends[index]);
        friends[index] = null;

    }

    void BeginFriendship(Agent agent, Agent newFriend, float friendshipDuration)
        {   BeginFriendship(agent, (Agent[])agent.GetAttribute(friendArrayAttributeName), newFriend, friendshipDuration);   }
    void BeginFriendship(Agent agent, Agent[] friends, Agent newFriend, float friendshipDuration)
    {
        for (int i = 0; i < friends.Length; i++)
            if (friends[i] == null)
            {
                friends[i] = newFriend;
                float[] timers = ((float[])agent.GetAttribute(friendTimersAttributeName));
                timers[i] = friendshipDuration;
                agent.SetAttribute(friendTimersAttributeName, timers);
                return;
            }
    }

    bool HasEmptyFriendSlot(Agent agent)
    {
        if (!agent.HasAttribute(friendArrayAttributeName)) return false;
        return HasEmptyFriendSlot((Agent[])agent.GetAttribute(friendArrayAttributeName));
    }

    bool HasEmptyFriendSlot(Agent[] friends)
    {
        for(int i = 0; i<friends.Length; i++)
        {
            if (friends[i] == null) return true;
        }
        return false;
    }


    void SearchForNewFriends(Agent agent, Agent[] friends, LinkedList<AgentWrapped> neighbors) {
        Agent ignore = (Agent)agent.GetAttribute(ignoreFriendAttributeName);
        List<Agent> alreadyFriends = new List<Agent>(friends);
        foreach(AgentWrapped neighbor_wrap in neighbors)
        {
            Agent neighbor = neighbor_wrap.agent;
            if (neighbor == agent || alreadyFriends.Contains(neighbor) || (ignore != null && neighbor != ignore)) continue;
            if(Vector3.Distance(agent.position, neighbor_wrap.wrappedPosition) <= effectiveRadius)
            {
                if (HasEmptyFriendSlot(neighbor))
                {
                    if(neighbor.HasAttribute(SocialStatusBehavior.statusAttributeName) && agent.HasAttribute(SocialStatusBehavior.statusAttributeName)){
                        if (!WithinSocialStatusRange(agent, neighbor)) continue;
                    }
                    float timerVal = GetFriendshipTimerStartingValue();
                    BeginFriendship(agent, neighbor, timerVal);
                    BeginFriendship(neighbor, agent, timerVal);
                }
            }
        }
    }

    void CreateFriendshipSlots(Agent agent)
    {
        agent.SetAttribute(friendArrayAttributeName, new Agent[maxFriends]);
        agent.SetAttribute(friendTimersAttributeName, new float[maxFriends]);
    }

    float GetFriendshipTimerStartingValue()
    {
        return UnityEngine.Random.Range(minFriendshipDuration, maxFriendshipDuration);
    }

    bool WithinSocialStatusRange(Agent a, Agent b)
    {
        float a_status = (float)a.GetAttribute(SocialStatusBehavior.statusAttributeName);
        float b_status = (float)b.GetAttribute(SocialStatusBehavior.statusAttributeName);
        return Mathf.Abs(a_status - b_status) <= maxStatusDifference;
    }

    //if two friends are on opposite sides of the screen because one just wrapped around, they should be drawn to the edges of the screen over the wrap, not to the middle of the screen
    Vector3 ClosestPositionWithWrap(Vector3 myPosition, Vector3 otherPosition)
    {

        if (Mathf.Abs(myPosition.x - otherPosition.x) > NeighborhoodCoordinator.size.x / 2f)
        {
            //Debug.Log("here " + Mathf.Abs(myPosition.x - otherPosition.x) + " " + NeighborhoodCoordinator.size.x / 2f);
            otherPosition.x += NeighborhoodCoordinator.size.x * (myPosition.x > otherPosition.x ? 1 : -1);
        }
        if(Mathf.Abs(myPosition.y - otherPosition.y) > NeighborhoodCoordinator.size.y / 2f)
        {
            otherPosition.y += NeighborhoodCoordinator.size.y * (myPosition.y > otherPosition.y ? 1 : -1);
        }
        return otherPosition;
    }
    
}
