using UnityEngine;

namespace ThirdPersonController
{
    public class LadderScript : MonoBehaviour
    {
        [SerializeField, Tooltip("Position at the base of the ladder")]
        Vector3 positionOffset = new Vector3();
        [SerializeField, Tooltip("Y rotation in opposite drection of the ladder")]
        float rotation = 0f;

        public Vector3 BasePosition => transform.position + positionOffset;
        public float Rotation => rotation;
        public Vector3 Normal => Quaternion.Euler(0f, Rotation, 0f) * Vector3.forward;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(BasePosition, 0.2f);
            Gizmos.DrawRay(BasePosition, Normal);
        }
    }
}