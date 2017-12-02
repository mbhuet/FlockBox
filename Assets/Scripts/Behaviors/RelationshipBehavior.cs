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

    public static bool drawConnections = false;


    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(friendArrayAttributeName)) CreateFriendshipSlots(mine);
        List<int> expired = ExpiredFriendships(mine);
        SteeringAgent[] friends = (SteeringAgent[])mine.GetAttribute(friendArrayAttributeName);

        Vector3 sum = Vector3.zero;
        int friendCount = 0;
        for(int friendIndex = 0; friendIndex<friends.Length; friendIndex++)
        {
            if (expired.Contains(friendIndex)) EndFriendship(mine, friends, friendIndex);
            else if(friends[friendIndex] != null)
            {
                SteeringAgent friend = friends[friendIndex];
                Vector3 wrappedPosition = ClosestPositionWithWrap(mine.position, friend.position);
                sum += wrappedPosition;
                friendCount++;
                if(drawConnections) GLDebug.DrawLine(mine.position, wrappedPosition, Color.yellow);
            }
        }

        if (friendCount < maxFriends)
        {
            SearchForNewFriends(mine, friends, surroundings.neighbors);
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

    List<int> ExpiredFriendships(SteeringAgent agent)
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

    void EndFriendship(SteeringAgent agent, int index)
        {   EndFriendship(agent, (SteeringAgent[])agent.GetAttribute(friendArrayAttributeName), index);   }
    void EndFriendship(SteeringAgent agent, SteeringAgent[] friends, int index)
    {
        agent.SetAttribute(ignoreFriendAttributeName, friends[index]);
        friends[index] = null;

    }

    void BeginFriendship(SteeringAgent agent, SteeringAgent newFriend, float friendshipDuration)
        {   BeginFriendship(agent, (SteeringAgent[])agent.GetAttribute(friendArrayAttributeName), newFriend, friendshipDuration);   }
    void BeginFriendship(SteeringAgent agent, SteeringAgent[] friends, SteeringAgent newFriend, float friendshipDuration)
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

    bool HasEmptyFriendSlot(SteeringAgent agent)
    {
        if (!agent.HasAttribute(friendArrayAttributeName)) return false;
        return HasEmptyFriendSlot((SteeringAgent[])agent.GetAttribute(friendArrayAttributeName));
    }

    bool HasEmptyFriendSlot(SteeringAgent[] friends)
    {
        for(int i = 0; i<friends.Length; i++)
        {
            if (friends[i] == null) return true;
        }
        return false;
    }


    void SearchForNewFriends(SteeringAgent agent, SteeringAgent[] friends, LinkedList<SteeringAgentWrapped> neighbors) {
        SteeringAgent ignore = (SteeringAgent)agent.GetAttribute(ignoreFriendAttributeName);
        List<SteeringAgent> alreadyFriends = new List<SteeringAgent>(friends);
        foreach(SteeringAgentWrapped neighbor_wrap in neighbors)
        {
            SteeringAgent neighbor = neighbor_wrap.agent;
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

    void CreateFriendshipSlots(SteeringAgent agent)
    {
        agent.SetAttribute(friendArrayAttributeName, new SteeringAgent[maxFriends]);
        agent.SetAttribute(friendTimersAttributeName, new float[maxFriends]);
    }

    float GetFriendshipTimerStartingValue()
    {
        return UnityEngine.Random.Range(minFriendshipDuration, maxFriendshipDuration);
    }

    bool WithinSocialStatusRange(SteeringAgent a, SteeringAgent b)
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
