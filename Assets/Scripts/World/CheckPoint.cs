using UnityEngine;

namespace World
{
    public class CheckPoint : MonoBehaviour
    {
        [SerializeField] [Tooltip("The index defines the order of the checkpoints")]
        private int checkPointIndex;

        public int Index => checkPointIndex;
    }
}