using UnityEngine;

namespace PollingStation.Simulation
{
    public sealed class AgentView : MonoBehaviour
    {
        [SerializeField] private float speed = 4f;
        private Vector3 target;
        private bool movementPaused;
        private Renderer[] renderers;
        private Animator[] animators;
        private MaterialPropertyBlock colorProperties;
        private bool? wasMoving;

        public int AgentId { get; private set; }
        public string CurrentState { get; private set; } = "unknown";

        private void Awake()
        {
            EnsureRenderers();
            target = transform.position;
        }

        private void Update()
        {
            if (movementPaused)
            {
                SetMoving(false);
                return;
            }

            Vector3 previous = transform.position;
            transform.position = Vector3.MoveTowards(previous, target, speed * Time.deltaTime);
            Vector3 direction = target - previous;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    12f * Time.deltaTime);
            }

            SetMoving((target - transform.position).sqrMagnitude > 0.0001f);
        }

        public void Initialize(int agentId, float movementSpeed)
        {
            EnsureRenderers();
            AgentId = agentId;
            speed = movementSpeed;
            gameObject.name = $"Votante_{agentId}";
            wasMoving = null;
            SetMoving(false);
        }

        public void ApplyState(string state, Vector3 destination, bool immediate)
        {
            EnsureRenderers();
            CurrentState = state;
            target = destination;
            if (immediate)
            {
                transform.position = destination;
            }

            // El color identifica al votante, no la etapa del proceso.
            // Así un cambio de estado no hace que parezca un agente distinto.
            Color color = ColorForAgent(AgentId);
            foreach (Renderer item in renderers)
            {
                if (colorProperties == null)
                {
                    colorProperties = new MaterialPropertyBlock();
                }

                item.GetPropertyBlock(colorProperties);
                colorProperties.SetColor("_BaseColor", color);
                colorProperties.SetColor("_Color", color);
                item.SetPropertyBlock(colorProperties);
            }
        }

        private void EnsureRenderers()
        {
            if (renderers == null)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }

            if (animators == null)
            {
                animators = GetComponentsInChildren<Animator>();
            }
        }

        public void SetPaused(bool paused)
        {
            movementPaused = paused;
            if (paused)
            {
                SetMoving(false);
            }
        }

        private void SetMoving(bool moving)
        {
            EnsureRenderers();
            if (wasMoving == moving)
            {
                return;
            }

            wasMoving = moving;
            foreach (Animator animator in animators)
            {
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                foreach (AnimatorControllerParameter parameter in animator.parameters)
                {
                    if (parameter.name == "Moving" && parameter.type == AnimatorControllerParameterType.Bool)
                    {
                        animator.SetBool("Moving", moving);
                        break;
                    }
                }
            }
        }

        private static Color ColorForAgent(int id)
        {
            float hue = Mathf.Repeat(id * 0.173f, 1f);
            return Color.HSVToRGB(hue, 0.48f, 0.9f);
        }
    }
}
