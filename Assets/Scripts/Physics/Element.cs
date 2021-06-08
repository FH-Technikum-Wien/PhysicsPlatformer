using UnityEngine;

namespace Physics
{
    [RequireComponent(typeof(Collider2D))]
    public class Element : MonoBehaviour
    {
        public float quickDrag = 0.8f;

        [SerializeField] private ElementDensity.ElementType elementType;

        public float ElementDensity { get; private set; }

        private void Awake()
        {
            ElementDensity = Physics.ElementDensity.GetMaterialDensity(elementType);
        }
    }
}