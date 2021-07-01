using System;
using System.Collections;
using System.Collections.Generic;
using Physics;
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

        private readonly List<Vector3> _resetPositions = new List<Vector3>();

        private void Start()
        {
            // Save all positions
            foreach (GameObject objectToReset in objectsToReset)
            {
                _resetPositions.Add(objectToReset.transform.position);
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
                objectsToReset[i].transform.position = _resetPositions[i];

                if (objectsToReset[i].TryGetComponent(out PhysicsBody2D body))
                {
                    body.ResetBody();
                }
                
                if (objectsToReset[i].TryGetComponent(out PressurePlate plate))
                {
                    plate.ResetPressurePlate();
                }
                
                if (objectsToReset[i].TryGetComponent(out Door door))
                {
                    door.CloseInstant();
                }

                if (objectsToReset[i].TryGetComponent(out MovingPlatform platform))
                {
                    platform.ResetPlatform();
                }
                
                if (objectsToReset[i].TryGetComponent(out Pendulum pendulum))
                {
                    pendulum.ResetPendulum();
                }
            }
        }
    }
}