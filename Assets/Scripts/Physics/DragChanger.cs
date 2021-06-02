using UnityEngine;

namespace Physics
{
    [RequireComponent(typeof(Collider2D))]
    public class DragChanger : MonoBehaviour
    {
        public float quickDrag = 0.8f;

        [SerializeField] private MaterialDensity.MaterialType materialType;


        [HideInInspector] public float materialDensity;

        private void Awake()
        {
            materialDensity = MaterialDensity.GetMaterialDensity(materialType);
        }
    }
}