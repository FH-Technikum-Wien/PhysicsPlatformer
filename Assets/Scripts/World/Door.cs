using UnityEngine;

namespace World
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private float timeToOpen;
        [SerializeField] private Vector3 openMovement;

        [Header("Debugging")] [SerializeField] private bool open;
        [Header("Debugging")] [SerializeField] private bool close;

        public bool IsOpen { get; private set; }
        public bool IsOpening { get; private set; }
        public bool IsAnimating { get; private set; }

        private float _animationTime;
        private Vector3 _closePosition;

        private void Awake()
        {
            _closePosition = transform.position;
        }

        private void Update()
        {
            if (open)
            {
                open = false;
                Open();
            }

            if (close)
            {
                close = false;
                Close();
            }

            if (IsAnimating)
            {
                transform.position = Vector3.Lerp(_closePosition, _closePosition + openMovement, _animationTime);
                _animationTime += IsOpening ? Time.deltaTime : -Time.deltaTime;
            }

            if (_animationTime >= timeToOpen)
            {
                transform.position = _closePosition + openMovement;
                IsOpen = true;
                IsAnimating = false;
                _animationTime = timeToOpen;
            }
            else if (_animationTime <= 0)
            {
                transform.position = _closePosition;
                IsOpen = false;
                IsAnimating = false;
                _animationTime = 0;
            }
        }

        public void Open()
        {
            IsOpening = true;
            IsAnimating = true;
        }

        public void Close()
        {
            IsOpening = false;
            IsAnimating = true;
        }
    }
}