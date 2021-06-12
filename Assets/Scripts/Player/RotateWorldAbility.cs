using System.Collections;
using Physics;
using UnityEngine;

namespace Player
{
    public class RotateWorldAbility : MonoBehaviour
    {
        [SerializeField] private float rotationTime = 1.0f;

        private float _time;
        private bool _isRotating;

        public void RotateWorldLeft()
        {
            StartCoroutine(RotateWordCoroutine(-90));
        }

        public void RotateWorldRight()
        {
            StartCoroutine(RotateWordCoroutine(90));
        }

        public void RotateWorldUp()
        {
            StartCoroutine(RotateWordCoroutine(180));
        }

        private IEnumerator RotateWordCoroutine(int rotationAmount)
        {
            // Skip if already rotating
            if (_isRotating)
                yield break;
            _isRotating = true;

            // Stop everything
            Time.timeScale = 0.0f;

            Quaternion transformRotation = transform.rotation;
            Vector3 currentRotation = transformRotation.eulerAngles;

            Vector3 targetRotation = currentRotation;
            targetRotation.z += rotationAmount;

            _time = 0.0f;
            // Lerp to new rotation
            while (_time < rotationTime)
            {
                transformRotation.eulerAngles = Vector3.Lerp(currentRotation,
                    targetRotation, _time / rotationTime);

                transform.rotation = transformRotation;
                // As everything is stopped (by timeScale being 0), use unscaled
                _time += Time.unscaledDeltaTime;
                yield return null;
            }

            // Set to correct value (if lerp was inaccurate)
            transformRotation.eulerAngles = targetRotation;
            transform.rotation = transformRotation;

            // Set gravity to match new camera perspective
            PhysicsBody2D.GlobalGravity = transform.rotation * Vector3.up * PhysicsBody2D.GlobalGravityAcceleration;

            // Resume everything
            Time.timeScale = 1.0f;

            _isRotating = false;
        }
    }
}