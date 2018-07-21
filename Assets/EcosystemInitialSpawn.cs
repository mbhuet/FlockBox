using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcosystemInitialSpawn : MonoBehaviour {

    public FaunaAgent predatorPrefab;
    public FaunaAgent preyPrefab;
    public FloraTarget floraPrefab;

    public Agent[] stampPalette;

    private Agent selectedAgent;

    public bool initialSpawn = true;
    public int initialPredators = 1;
    public int initialPrey = 10;
    public int initialFlora = 3;


    private void Start()
    {
        SelectAgentForStamping(0);
        if(initialSpawn) InitialSpawn();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StampAgent();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectAgentForStamping(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectAgentForStamping(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectAgentForStamping(2);
        }
    }

    private void StampAgent()
    {
        if (selectedAgent == null) return;
        SpawnAgent(selectedAgent, Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    private void SelectAgentForStamping(int index)
    {
        if (index >= stampPalette.Length) return;
        selectedAgent = stampPalette[index];
    }

    void InitialSpawn()
    {
        float randomSpawnRadius = Camera.main.orthographicSize;
        for (int i = 0; i < initialPredators; i++)
        {
            SpawnAgent(predatorPrefab, Random.insideUnitCircle * randomSpawnRadius);
        }
        for (int i = 0; i < initialPrey; i++)
        {
            SpawnAgent(preyPrefab, Random.insideUnitCircle * randomSpawnRadius);
        }

        for (int i = 0; i < initialFlora; i++)
        {
            SpawnAgent(floraPrefab, Random.insideUnitCircle * randomSpawnRadius);
        }

    }



    void SpawnAgent(Agent prefab, Vector2 pos)
    {
        Agent instance = prefab.GetInstance();
        instance.Spawn(pos);
    }

    
}
