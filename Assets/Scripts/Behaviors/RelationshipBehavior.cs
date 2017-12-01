using System;
using System.Collections;
using System.Collections.Generic;
using Vexe.Runtime.Types;
using UnityEngine;

[System.Serializable]
public class RelationshipBehavior : SteeringBehavior {

    [VisibleWhen("isActive")]   public int maxFriends;
    [VisibleWhen("isActive")]   public float minFriendshipDuration;
    [VisibleWhen("isActive")]   public float maxFriendshipDuration;

    const string friendArrayAttributeName = "friends";
    const string friendTimersAttributeName = "friendTimers";
    const string ignoreFriendAttributeName = "ignoreFriend";




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
                sum += friend.position;
                friendCount++;
                if(Vector3.Distance(mine.position, friend.position) < 30)Debug.DrawLine(mine.position, friend.position, Color.yellow);
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


    void SearchForNewFriends(SteeringAgent agent, SteeringAgent[] friends, LinkedList<SteeringAgent> neighbors) {
        SteeringAgent ignore = (SteeringAgent)agent.GetAttribute(ignoreFriendAttributeName);
        List<SteeringAgent> alreadyFriends = new List<SteeringAgent>(friends);
        foreach(SteeringAgent neighbor in neighbors)
        {
            if (neighbor == agent || alreadyFriends.Contains(neighbor) || (ignore != null && neighbor != ignore)) continue;
            if(Vector3.Distance(agent.position, neighbor.position) <= effectiveRadius)
            {
                if (HasEmptyFriendSlot(neighbor))
                {
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

    
}
