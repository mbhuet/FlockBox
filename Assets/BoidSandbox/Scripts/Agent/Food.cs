using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]
public class FoodPellet : MonoBehaviour {

    private Agent _agent;

    private void Awake()
    {
        _agent = GetComponent<Agent>();
        _agent.OnCaught += Relocate;
    }

    

    void Relocate(Agent other)
    {
        transform.position = NeighborhoodCoordinator.Instance.RandomPosition();
    }

}
