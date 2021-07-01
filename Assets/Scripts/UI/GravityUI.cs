using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI
{
    public class GravityUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeBackground;
        [SerializeField] private Image fadeBackgroundImage;
        [SerializeField] private Image gravityDirectionArrow;
        [SerializeField] private float startAlpha = 0.5f;
        [SerializeField] private float fadeTime = 0.5f;
        [SerializeField] private float fadeDelay = 0.5f;

        private Coroutine _fadeAnimation = null;

        private void Awake()
        {
            fadeBackground.alpha = 0.0f;
        }

        private void OnEnable()
        {
            ChangeGravityAbility.OnGravityChange += OnGravityChange;
        }

        private void OnDisable()
        {
            ChangeGravityAbility.OnGravityChange -= OnGravityChange;
        }

        private void OnGravityChange(GravityDirection gravityDirection)
        {
            fadeBackground.alpha = startAlpha;
            fadeBackgroundImage.color = WorldManager.GravityDirectionToColor[gravityDirection];

            Quaternion arrowQuaternion = gravityDirectionArrow.rectTransform.rotation;
            switch (gravityDirection)
            {
                case GravityDirection.Left:
                    arrowQuaternion.eulerAngles = new Vector3(0, 0, -90);
                    break;
                case GravityDirection.Down:
                    arrowQuaternion.eulerAngles = new Vector3(0, 0, 0);
                    break;
                case GravityDirection.Right:
                    arrowQuaternion.eulerAngles = new Vector3(0, 0, 90);
                    break;
                case GravityDirection.Up:
                    arrowQuaternion.eulerAngles = new Vector3(0, 0, 180);
                    break;
                case GravityDirection.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gravityDirection), gravityDirection, null);
            }

            gravityDirectionArrow.rectTransform.rotation = arrowQuaternion;

            if (_fadeAnimation != null)
                StopCoroutine(_fadeAnimation);

            _fadeAnimation = StartCoroutine(ChangeGravityFade());
        }

        private IEnumerator ChangeGravityFade()
        {
            yield return new WaitForSeconds(fadeDelay);

            float startTime = Time.time;
            float progress = 0;
            while (startTime + fadeTime > Time.time)
            {
                fadeBackground.alpha = startAlpha - progress / fadeTime;
                progress += Time.deltaTime * startAlpha;
                yield return null;
            }

            fadeBackground.alpha = 0;
            _fadeAnimation = null;
        }
    }
}