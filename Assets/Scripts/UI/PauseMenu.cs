using System;
using Physics;
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

        public void SetVisibility(bool isVisible)
        {
            canvasGroup.alpha = isVisible ? 1.0f : 0.0f;
            canvasGroup.interactable = isVisible;
            if (isVisible)
                EventSystem.current.SetSelectedGameObject(btnContinue.gameObject);
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