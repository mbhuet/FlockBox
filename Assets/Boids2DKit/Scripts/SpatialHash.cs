using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class SpatialHash<T>
{
    private Dictionary<int, List<T>> dict; //holds all T in int cell
    private Dictionary<T, List<int>> objects; //holds minCorner and maxCorner as ints
    private float cellSize;

    public SpatialHash(float cellSize)
    {
        this.cellSize = cellSize;
        dict = new Dictionary<int, List<T>>();
        objects = new Dictionary<T, List<int>>();
    }

    public void Remove(T obj)
    {
        if (objects.ContainsKey(obj))
        {
            List<int> occup = objects[obj];
            for (int i = 0; i < occup.Count; i++)
            {
                if (dict.ContainsKey(i))
                {
                    dict[i].Remove(obj);
                }
            }
        }
    }

    private void Insert(Vector3 vector, float radius, T obj, out List<int> keys)
    {
        GetOverlappingBuckets(vector, radius, out keys);
        for(int i=0; i<keys.Count; i++)
        {
            int key = keys[i];
            if (dict.ContainsKey(key))
            {
                dict[key].Add(obj);
            }
            else
            {
                dict[key] = new List<T> { obj };
            }
        }     
        objects[obj] = keys;
    }

    public void UpdatePosition(Vector3 vector, float radius, T obj, out List<int> buckets)
    {
        Remove(obj);
        Insert(vector, radius, obj, out buckets);
    }

    public void QueryPosition(Vector3 vector, float radius, out List<T> objects, out List<int> keys)
    {
        GetOverlappingBuckets(vector, radius, out keys);
        objects = new List<T>();

        for(int i =0; i<keys.Count; i++)
        {
            int key = keys[i];
            if (dict.ContainsKey(key)) objects.AddRange(dict[key]);
        }
    }

    public bool ContainsKey(Vector3 vector)
    {
        return dict.ContainsKey(Key(vector));
    }

    private void GetOverlappingBuckets(Vector3 vector, float radius, out List<int> bucketsObjIsIn)
    {
        bucketsObjIsIn = new List<int>();

        Vector3 min = vector - Vector3.one * radius;
        min.x = min.x - min.x % cellSize;
        min.y = min.y - min.y % cellSize;
        min.z = min.z - min.z % cellSize;

        Vector3 max = vector + Vector3.one * radius;
        max.x = max.x - max.x % cellSize + cellSize;
        max.y = max.y - max.y % cellSize + cellSize;
        max.z = max.z - max.z % cellSize + cellSize;

        for (float x = min.x + cellSize/2f; x<max.x; x+=cellSize)
        {
            for(float y = min.y + cellSize/2f; y<max.y; y+=cellSize)
            {
                for(float z = min.z + cellSize/2f; z<max.z; z += cellSize)
                {
                    bucketsObjIsIn.Add(Key(x, y, z));
                }
            }
        }
    }


    public void Clear()
    {
        var keys = dict.Keys.ToArray();
        for (var i = 0; i < keys.Length; i++)
            dict[keys[i]].Clear();
        objects.Clear();
    }

    public void Reset()
    {
        dict.Clear();
        objects.Clear();
    }

    private const int BIG_ENOUGH_INT = 16 * 1024;
    private const double BIG_ENOUGH_FLOOR = BIG_ENOUGH_INT + 0.0000;

    private static int FastFloor(float f)
    {
        return (int)(f + BIG_ENOUGH_FLOOR) - BIG_ENOUGH_INT;
    }

    private int Key(float x, float y, float z)
    {
        return ((FastFloor(x / cellSize) * 73856093) ^
                (FastFloor(y / cellSize) * 19349663) ^
                (FastFloor(z / cellSize) * 83492791));
    }

    private int Key(Vector3 v)
    {
        return Key(v.x, v.y, v.z);
    }
}