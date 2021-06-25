using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Events;

namespace World.Puzzles
{
    public class Puzzle : MonoBehaviour
    {
        [SerializeField] private Transform puzzleCameraPosition;
        [SerializeField] private Transform playerSpawnPoint;

        [SerializeField] [Tooltip("Objects to reset on level reset")]
        private GameObject[] objectsToReset;

        public Vector3 PuzzleCameraPosition => puzzleCameraPosition.position;
        public Transform PlayerSpawnPoint => playerSpawnPoint;

        public event UnityAction<Puzzle> OnPlayerEnter;

        private readonly List<Transform> _resetTransforms = new List<Transform>();

        private void Awake()
        {
            // Save all positions
            foreach (GameObject objectToReset in objectsToReset)
            {
                _resetTransforms.Add(objectToReset.transform);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                OnPlayerEnter?.Invoke(this);
            }
        }

        /// <summary>
        /// (Dis)Enables all MonoBehaviour scripts in the level tree.
        /// </summary>
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

        /// <summary>
        /// Resets all defined objects to their start position, rotation and scale.
        /// </summary>
        public void ResetObjects()
        {
            // Reset all objects to start position
            for (int i = 0; i < objectsToReset.Length; i++)
            {
                objectsToReset[i].transform.position = _resetTransforms[i].position;
                objectsToReset[i].transform.rotation = _resetTransforms[i].rotation;
                objectsToReset[i].transform.localScale = _resetTransforms[i].localScale;
            }
        }
    }
}