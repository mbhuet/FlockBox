using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class Shape
    {
        public enum ShapeType
        {
            POINT, //occupy only one point, one neighborhood
            SPHERE, //occupy all neighborhoods within radius
            LINE, //occupy all neighborhoods along line
            CYLINDER,
            //COLLIDER,
        }
        public ShapeType type;

        public float radius = 1;
        public float length = 1;
        public Vector3Int dimensions = Vector3Int.one;
        //[SerializeField] private Collider collider;


#if UNITY_EDITOR
        public void DrawGizmo()
        {
            switch (type)
            {

                case ShapeType.POINT:
                    Gizmos.DrawWireSphere(Vector3.zero, radius);
                    break;
                case ShapeType.SPHERE:
                    Gizmos.DrawWireSphere(Vector3.zero, radius);
                    break;
                case ShapeType.LINE:
                    Gizmos.DrawLine(Vector3.zero, Vector3.forward * length);
                    UnityEditor.Handles.DrawWireDisc(Vector3.forward * length, Vector3.forward, radius);
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius);
                    break;
                case ShapeType.CYLINDER:
                    Gizmos.DrawLine(Vector3.zero, Vector3.forward * length);
                    UnityEditor.Handles.DrawWireDisc(Vector3.forward * length, Vector3.forward, radius);
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius);
                    break;
            }
        }
#endif
    }
}
