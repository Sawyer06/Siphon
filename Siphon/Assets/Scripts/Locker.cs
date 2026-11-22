using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Locker : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;

    [SerializeField] private TMP_Text openButtonPrompt;

    [SerializeField] private InputActionReference openAction;

    private bool isOpen = false;

    private bool canInteract;

    private PlayerController player;

    private void Start()
    {
        openButtonPrompt.enabled = false;
        openButtonPrompt.text = openAction.action.GetBindingDisplayString();

        player = GameObject.Find("Player").GetComponent<PlayerController>();

        openAction.action.started += ctx =>
        {
            if (canInteract)
                ToggleOpen();
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) // Player can activate distractions when in their area.
        {
            openButtonPrompt.enabled = true;
            canInteract = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            openButtonPrompt.enabled = false;
            canInteract = false;
        }
    }

    private void ToggleOpen()
    {
        isOpen = !isOpen;

        doorAnimator.SetBool("open", isOpen);
        //Invoke("CanInteractAgain", 0.1f);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!player)
                return;

            if (!isOpen)
                player.isHiding = true;
            else
                player.isHiding = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!player)
                return;

            player.isHiding = false;
        }
    }
}
