using UnityEngine;

namespace FromZeroToHero.Core
{
    public sealed class MovingActor : MonoBehaviour
    {
        [SerializeField] private float speed = 3.1f;
        [SerializeField] private float turnSpeed = 8f;

        public Vector3 TargetPosition { get; private set; }
        public bool HasTarget { get; private set; }

        private Transform visual;

        public void SetVisual(Transform root)
        {
            visual = root;
        }

        public void MoveTo(Vector3 worldPosition)
        {
            TargetPosition = worldPosition;
            HasTarget = true;
        }

        private void Update()
        {
            if (!HasTarget)
                return;

            Vector3 current = transform.position;
            Vector3 next = Vector3.MoveTowards(current, TargetPosition, speed * Time.deltaTime);
            transform.position = next;

            Vector3 delta = TargetPosition - current;
            delta.y = 0f;
            if (delta.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(delta.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
            }

            if (visual != null)
            {
                float bob = Mathf.Sin(Time.time * 6f) * 0.02f;
                visual.localPosition = new Vector3(0f, bob, 0f);
            }

            if ((TargetPosition - next).sqrMagnitude < 0.001f)
            {
                HasTarget = false;
            }
        }
    }
}
