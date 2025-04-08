using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    public Transform InteractionPoint;
    public LayerMask InteractionLayer;
    public float InteractionPoinRadius = 1f;
    public bool IsInteracting { get; private set; }

    private IInteractable currentInteractable;
    
    private void Update()
    {
        var colliders = Physics.OverlapSphere(InteractionPoint.position, InteractionPoinRadius, InteractionLayer);

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                var interactable = colliders[i].GetComponent<IInteractable>();

                if(interactable != null && !IsInteracting) {
                    StartInteraction(interactable);
                    break;
                }
            }
        }

        if (Keyboard.current.tabKey.wasReleasedThisFrame) {
            EndInteraction();
        }
    }

    void StartInteraction(IInteractable interactable)
    {
        currentInteractable = interactable;
        interactable.Interact(this,out bool interactionSuccessful);
        interactable.OnInteractionComplete += HandleInteractionComplete;
        IsInteracting = true;
    }

    private void HandleInteractionComplete(IInteractable arg0) {
        EndInteraction();
    }

    void EndInteraction()
    {
        if (currentInteractable != null) {
            currentInteractable.OnInteractionComplete -= HandleInteractionComplete;
            currentInteractable.EndInteraction();
            currentInteractable = null;
        }
        IsInteracting = false;
    }
}
