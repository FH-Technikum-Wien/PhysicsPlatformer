using Player;
using UnityEngine;
using UnityEngine.Events;

namespace World.Puzzles
{
    public class Puzzle : MonoBehaviour
    {
        [SerializeField] private Camera puzzleCamera;
        [SerializeField] private Transform playerSpawnPoint;

        public Camera PuzzleCamera => puzzleCamera;
        public Transform PlayerSpawnPoint => playerSpawnPoint;

        public event UnityAction<Puzzle> OnPlayerEnter;

        private bool _isCompleted;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController player))
                OnPlayerEnter?.Invoke(this);
        }

        public void SetEnabled(bool isEnabled)
        {
            foreach (MonoBehaviour behaviour in GetComponentsInChildren<MonoBehaviour>())
            {
                behaviour.enabled = isEnabled;
            }
        }
    }
}