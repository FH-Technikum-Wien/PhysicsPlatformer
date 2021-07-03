using System;
using System.Collections;
using Physics;
using Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using World;

namespace MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        /// <summary>
        /// All title letter springs. Used for start animation.
        /// </summary>
        [SerializeField] [Tooltip("All title letter springs. Used for start animation")]
        private PhysicsBody2D[] letterSprings;

        [SerializeField] [Tooltip("The gravity scale to move the letter outside the screen")]
        private float startGravityScale = -400.0f;

        [SerializeField] [Tooltip("The start animation delay")]
        private float startAnimationDelay = 1.0f;

        [SerializeField] [Tooltip("The pause between each letter spring")]
        private float letterSpringAnimationPause = 0.5f;

        [Header("Buttons")] [SerializeField] [Tooltip("Start Button")]
        private Button btnStart;

        [SerializeField] [Tooltip("Quit Button")]
        private Button btnQuit;

        [Header("Player")] [SerializeField] [Tooltip("The player controller component")]
        private PlayerController playerController;

        private void OnEnable()
        {
            btnStart.onClick.AddListener(OnStartClick);
            btnQuit.onClick.AddListener(OnQuitClick);
        }

        private void OnDisable()
        {
            btnStart.onClick.RemoveListener(OnStartClick);
            btnQuit.onClick.RemoveListener(OnQuitClick);
        }

        private void Start()
        {
            playerController.InputAction.Enable();
            playerController.InputAction.Player.Disable();
            playerController.InputAction.PlayerMainMenu.Enable();
            
            // Hide buttons
            btnStart.gameObject.SetActive(false);
            btnQuit.gameObject.SetActive(false);
            
            // Reset gravity
            PhysicsBody2D.SetGlobalGravityDirection(Vector2.up);
            WorldManager.GravityDirection = GravityDirection.Down;

            foreach (PhysicsBody2D spring in letterSprings)
            {
                spring.transform.localPosition -=
                    new Vector3(0, startGravityScale * PhysicsBody2D.GlobalGravityAcceleration / 4f, 0);
                // Set gravity scale, to hide spring letter
                spring.gravityScale = startGravityScale;
            }

            StartCoroutine(StartAnimation());
        }

        private void Update()
        {
            // If no button is selected, select start
            if (!playerController.IsUsingMouse)
            {
                if (btnStart.gameObject != EventSystem.current.currentSelectedGameObject &&
                    btnQuit.gameObject != EventSystem.current.currentSelectedGameObject)
                {
                    EventSystem.current.SetSelectedGameObject(btnStart.gameObject);
                }
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private IEnumerator StartAnimation()
        {
            yield return new WaitForSeconds(startAnimationDelay);
            foreach (PhysicsBody2D spring in letterSprings)
            {
                // Reset gravity scale
                spring.gravityScale = 100;
                yield return new WaitForSeconds(letterSpringAnimationPause);
            }

            yield return new WaitForSeconds(1.0f);
            btnStart.gameObject.SetActive(true);
            btnQuit.gameObject.SetActive(true);
        }

        private void OnStartClick()
        {
            SceneManager.LoadScene(1);
        }

        private void OnQuitClick()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}