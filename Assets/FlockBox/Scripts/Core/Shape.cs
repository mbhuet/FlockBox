using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class Shape
    {
        public enum ShapeType
        {
            POINT,
            SPHERE,
        }
        public ShapeType type;

        public float radius = 1;


#if UNITY_EDITOR
        public void DrawGizmo()
        {
            Gizmos.DrawWireSphere(Vector3.zero, radius);
        }
#endif
    }
}
