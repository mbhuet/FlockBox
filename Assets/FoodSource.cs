using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSource : MonoBehaviour {

    public float minStatus = 0;
    public float patchRadius = 10;
    public int numFoodSpawns = 10;
    public FoodTarget foodPrefab;
    private List<Target> activeFoods;
    private List<Target> cachedFoods;

    private float perFoodCost;

    private void Start()
    {
        perFoodCost = foodPrefab.nourishAmount;

        activeFoods = new List<Target>();
        cachedFoods = new List<Target>();
        SpawnWithinCircle(transform.position, numFoodSpawns);

    }

    private void Update()
    {
        int breaker = 0;
        while (breaker < 10 && SocialStatusBehavior.bankedStatus > perFoodCost)
        {
            breaker++;
            SpawnFromCachedFoods();
        }

    }

    void SpawnWithinCircle(Vector2 center, int numFoods)
    {
        for(int i = 0; i<numFoods; i++)
        {
            Vector2 position = GetRandomPosition();
            FoodTarget food = GameObject.Instantiate(foodPrefab, position, Quaternion.identity);
            food.minStatus = minStatus;
            food.OnCaught += AddFoodToCache;
            activeFoods.Add(food);
        }
    }

    Vector2 GetRandomPosition()
    {
        return (Vector2)transform.position + Random.insideUnitCircle * patchRadius;
    }

    void AddFoodToCache(Target food)
    {
        activeFoods.Remove(food);
        cachedFoods.Add(food);
    }

    void SpawnFromCachedFoods()
    {
        if (cachedFoods.Count == 0) return;
        if (SocialStatusBehavior.bankedStatus < perFoodCost) return;

        SocialStatusBehavior.bankedStatus -= perFoodCost;

        Target food = cachedFoods[0];
        food.Spawn(GetRandomPosition());

        cachedFoods.Remove(food);
        activeFoods.Add(food);
    }
}
