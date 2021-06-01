using UnityEngine;

namespace Physics
{
    [RequireComponent(typeof(Collider2D))]
    public class DragChanger : MonoBehaviour
    {
        [SerializeField] private MaterialDensity.MaterialType materialType;

        public float quickDrag = 0.8f;

        public float materialDensity;

        private void Awake()
        {
            materialDensity = MaterialDensity.GetMaterialDensity(materialType);
        }
    }
}