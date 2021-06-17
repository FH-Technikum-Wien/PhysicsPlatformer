using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Events;

namespace World.Puzzles
{
    public class Puzzle : MonoBehaviour
    {
        [SerializeField] private Camera puzzleCamera;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private CanvasGroup cbLevelName;

        public Camera PuzzleCamera => puzzleCamera;
        public Transform PlayerSpawnPoint => playerSpawnPoint;

        public event UnityAction<Puzzle> OnPlayerEnter;

        private bool _firstTime = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                OnPlayerEnter?.Invoke(this);
                if (_firstTime)
                {
                    _firstTime = false;
                    StartCoroutine(FadeOutLevelName(1.0f, 1.0f));
                }
            }

            IEnumerator FadeOutLevelName(float delay, float fadeTime)
            {
                yield return new WaitForSeconds(delay);

                float startTime = Time.time;
                float progress = 0;
                while (startTime + fadeTime > Time.time)
                {
                    cbLevelName.alpha = 1 - progress / fadeTime;
                    progress += Time.deltaTime;
                    yield return null;
                }

                cbLevelName.alpha = 0;
            }
        }

        public void SetEnabled(bool isEnabled)
        {
            foreach (MonoBehaviour behaviour in GetComponentsInChildren<MonoBehaviour>())
            {
                behaviour.enabled = isEnabled;
            }

            foreach (Rigidbody2D rb in GetComponentsInChildren<Rigidbody2D>())
            {
                rb.simulated = isEnabled;
            }
        }
    }
}