using Player;
using UnityEngine;

namespace World
{
    /// <summary>
    /// Prevents the player from taking objects through. Harmless version of the portal gate.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerGate : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController playerController))
                return;

            playerController.CanPickUpObjects = false;
            playerController.ForceDropPickedUpObject();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController playerController))
                return;

            playerController.CanPickUpObjects = true;
        }
    }
}