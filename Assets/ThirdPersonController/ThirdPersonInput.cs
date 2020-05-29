namespace ThirdPersonController.Input
{
    using UnityEngine;
    public class ThirdPersonInput : MonoBehaviour
    {
        [SerializeField] ThirdPersonMovement movement = null;

        void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            movement.inputDirection.Set(
                Input.GetAxis("Horizontal"),
                0f,
                Input.GetAxis("Vertical")
            );
            movement.inputDirection.Normalize();

            movement.mouse.Set(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );

            movement.walkingState.SetSprinting(Input.GetKey(KeyCode.LeftShift));
        }
    }

}