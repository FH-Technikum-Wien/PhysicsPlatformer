using Player;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider2D))]
    public class KillOnTouch : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.collider.TryGetComponent(out PlayerController player))
                return;

            player.Kill();
        }
    }
}