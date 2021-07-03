using System;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace World
{
    [RequireComponent(typeof(Collider2D))]
    public class EndOfGame : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController playerController))
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}
