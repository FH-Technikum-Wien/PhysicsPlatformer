using UnityEngine;

namespace World
{
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] private Door doorToOpen;

        [SerializeField] private Plate plateToPushDown;

        [SerializeField] private Vector2 platePressedLocalPosition;
        [SerializeField] private float platePressTime;

        private Vector2 _plateLocalStartPosition;
        private float _animationTime;

        private void Awake()
        {
            _plateLocalStartPosition = plateToPushDown.transform.localPosition;
        }

        private void Update()
        {
            plateToPushDown.transform.localPosition =
                Vector3.Lerp(_plateLocalStartPosition, platePressedLocalPosition, _animationTime / platePressTime);
            _animationTime += plateToPushDown.bodies.Count > 0 ? Time.deltaTime : -Time.deltaTime;

            _animationTime = Mathf.Clamp(_animationTime, 0, platePressTime);

            if (_animationTime > 0)
            {
                doorToOpen.Open();
            }
            else
            {
                doorToOpen.Close();
            }
        }

        public void ResetPressurePlate()
        {
            _animationTime = 0;
            plateToPushDown.bodies.Clear();
        }
    }
}