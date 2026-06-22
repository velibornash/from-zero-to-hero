using UnityEngine;

namespace FromZeroToHero.Core
{
    public sealed class SelectionCursor : MonoBehaviour
    {
        public void Place(Vector3 worldPosition, float width)
        {
            transform.position = worldPosition + new Vector3(0f, 0.03f, 0f);
            transform.localScale = new Vector3(width, 1f, width);
        }
    }
}
