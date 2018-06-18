using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPatch : MonoBehaviour {

    public float minStatus = 0;
    public float patchRadius = 10;
    public int numFoodSpawns = 10;
    public FoodPlant foodPrefab;
    private List<FoodPlant> activePlants;
    private List<FoodPlant> cachedPlants;

    private float perFoodCost;

    private void Start()
    {
        perFoodCost = 1;//.nourishAmount;

        activePlants = new List<FoodPlant>();
        cachedPlants = new List<FoodPlant>();
        SpawnWithinCircle(transform.position, numFoodSpawns);

    }

    private void Update()
    {
        int breaker = 0;
        while (breaker < 10 && SocialStatusBehavior.bankedStatus > perFoodCost)
        {
            breaker++;
            SpawnFromCachedPlants();
        }

    }

    void SpawnWithinCircle(Vector2 center, int numFoods)
    {
        for(int i = 0; i<numFoods; i++)
        {
            FoodPlant plant = GameObject.Instantiate(foodPrefab);//, position, Quaternion.identity);
            plant.Grow(GetRandomPosition(), GetRandomStatusRequirement());
            plant.OnExhausted += AddFoodToCache;
            activePlants.Add(plant);
        }
    }

    Vector2 GetRandomPosition()
    {
        return (Vector2)transform.position + Random.insideUnitCircle * patchRadius;
    }

    float GetRandomStatusRequirement()
    {
        float rand =  Random.Range(-100, 100);
        return Mathf.Clamp(rand, 0, 100);
        //return 0;
    }

    void AddFoodToCache(FoodPlant plant)
    {
        activePlants.Remove(plant);
        cachedPlants.Add(plant);
    }

    void SpawnFromCachedPlants()
    {
        if (cachedPlants.Count == 0) return;
        if (SocialStatusBehavior.bankedStatus < perFoodCost) return;

        SocialStatusBehavior.bankedStatus -= perFoodCost;

        FoodPlant plant = cachedPlants[0];
        plant.Grow(GetRandomPosition(), GetRandomStatusRequirement());


        cachedPlants.Remove(plant);
        activePlants.Add(plant);
    }
}
