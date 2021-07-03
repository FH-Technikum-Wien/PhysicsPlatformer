using System;
using Physics;
using Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using World;
using World.Puzzles;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private Button btnContinue;
        [SerializeField] private Button btnRestartLevel;
        [SerializeField] private Button btnRestartGame;
        [SerializeField] private Button btnQuit;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private PlayerController playerController;

        public event UnityAction OnContinue;

        private void OnEnable()
        {
            btnContinue.onClick.AddListener(OnContinueClicked);
            btnRestartLevel.onClick.AddListener(OnRestartLevelClicked);
            btnRestartGame.onClick.AddListener(OnRestartGameClicked);
            btnQuit.onClick.AddListener(OnQuitClicked);
        }

        private void OnDisable()
        {
            btnContinue.onClick.RemoveListener(OnContinueClicked);
            btnRestartLevel.onClick.RemoveListener(OnRestartLevelClicked);
            btnRestartGame.onClick.RemoveListener(OnRestartGameClicked);
            btnQuit.onClick.RemoveListener(OnQuitClicked);
        }

        private void Update()
        {
            if(!canvasGroup.interactable)
                return;
            
            // If no button is selected, select start
            if (!playerController.IsUsingMouse)
            {
                if (btnContinue.gameObject != EventSystem.current.currentSelectedGameObject &&
                    btnQuit.gameObject != EventSystem.current.currentSelectedGameObject)
                {
                    EventSystem.current.SetSelectedGameObject(btnContinue.gameObject);
                }
            }
            else
            {
                // Deselect all
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void SetVisibility(bool isVisible)
        {
            canvasGroup.alpha = isVisible ? 1.0f : 0.0f;
            canvasGroup.interactable = isVisible;
        }

        private void OnContinueClicked()
        {
            OnContinue?.Invoke();
        }

        private void OnRestartLevelClicked()
        {
            PuzzleManager.ResetCurrentLevel();
            OnContinue?.Invoke();
        }

        private void OnRestartGameClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            OnContinue?.Invoke();
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
