using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private GameObject playerBody;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float crouchSpeed;

    private float speed;
    private Vector3 movement;

    [Space(10)]

    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference crouchAction;

    private void Update()
    {
        if (sprintAction.action.IsPressed())
        {
            speed = sprintSpeed;
        }
        else if (crouchAction.action.IsPressed())
        {
            speed = crouchSpeed;
        }
        else
        {
            speed = walkSpeed;
        }

        if (movement.x != 0 || movement.z != 0)
            playerBody.transform.rotation = Quaternion.LookRotation(-movement);
    }

    private void FixedUpdate()
    {
        playerRb.linearVelocity = movement * speed;
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        movement = new Vector3(input.x, 0, input.y);
    }
}
