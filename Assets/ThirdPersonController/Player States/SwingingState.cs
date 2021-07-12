using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class SwingingState : PlayerState
    {
        [SerializeField, Tooltip("Additional force applied downwards")]
        float additionalGravity = 0f;

        [SerializeField] float yOffset = 0f;

        [SerializeField] float jointSpring = 0f;
        [SerializeField] float jointDamper = 0f;
        [SerializeField] float jointMassScale = 0f;

        Vector3 anchorPoint => movement.transform.position - new Vector3(0, yOffset, 0);

        Vector3 connectionPoint = new Vector3(0, 16, 0);

        SpringJoint joint;

        public override PlayerState Process(Vector3 inputWorldDirection)
        {
            if (Input.GetMouseButtonUp(0)) return movement.inAirState;

            Debug.DrawLine(anchorPoint, connectionPoint, Color.red);
            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            movement.rigidbody.AddForce(Vector3.down * additionalGravity);
        }

        protected override void EnterImpl()
        {
            joint = movement.gameObject.AddComponent<SpringJoint>();
            joint.anchor = new Vector3(0, yOffset, 0);
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = connectionPoint;

            float targetDistance = Vector3.Distance(connectionPoint, anchorPoint);

            joint.minDistance = targetDistance * 0.9f;
            joint.maxDistance = targetDistance * 0.9f;

            joint.spring = jointSpring;
            joint.damper = jointDamper;
            joint.massScale = jointMassScale;
        }

        protected override void ExitImpl()
        {
            GameObject.Destroy(joint);
        }
    }
}