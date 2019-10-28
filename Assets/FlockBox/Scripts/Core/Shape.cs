using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class Shape
    {
        public Agent.NeighborType shape;

        public float radius;
        public float length;
        public Vector3Int dimensions;
    }
}
