using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Distraction : MonoBehaviour
{
    public bool isActivated;

    [SerializeField] private float timeToDeactivate;

    [SerializeField] private GameObject normalObject;
    [SerializeField] private GameObject distractionObject;

    [SerializeField] private TMP_Text activateButtonPrompt;

    [SerializeField] private InputActionReference activateDistractionAction;

    private void Start()
    {
        activateButtonPrompt.enabled = false;
        
        normalObject.SetActive(true);
        distractionObject.SetActive(false);

        activateButtonPrompt.text = activateDistractionAction.action.GetBindingDisplayString();
    }

    public void Activate()
    {
        Debug.Log("Activating");
        isActivated = true;
        normalObject.SetActive(false);
        distractionObject.SetActive(true);
    }

    public void Deactivate()
    {
        if (isActivated)
        {
            Debug.Log("Deactivating");
            StartCoroutine(WaitToDeactivate());
        }
    }

    private IEnumerator WaitToDeactivate()
    {
        yield return new WaitForSeconds(timeToDeactivate);
        isActivated = false;
        normalObject.SetActive(true);
        distractionObject.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) // Player can activate distractions when in their area.
        {
            if (!isActivated)
            {
                activateButtonPrompt.enabled = true;
                if (activateDistractionAction.action.IsPressed())
                {
                    Activate();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            activateButtonPrompt.enabled = false;
        }
    }
}