using StarterAssets;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public Transform Target;

    [Header("Distancias")]
    public float DetectionDistance = 10f;
    public float FieldOfView = 120f;
    public float AttackDistance = 2f;
    public float damage = 10f;

    [Header("Detección de Sonido")]
    public float HearingRadius = 15f;         // Radio en el que escucha sonidos
    public float InvestigateWaitTime = 3f;    // Segundos mirando antes de volver a patrullar
    public bool DrawGizmos = true;

    [Header("Memoria")]
    public float ChaseMemoryTime = 2f;   // Segundos que recuerda al jugador sin verlo
    private float _chaseMemoryTimer = 0f;

    [Header("Patrulla")]
    public Transform[] PatrolPoints;
    private int currentPoint = 0;

    private NavMeshAgent m_Agent;
    private Animator m_Animator;
    private float m_Distance;

    private Vector3 m_SoundPosition;
    private float m_InvestigateTimer = 0f;
    private bool m_IsLookingAround = false;
    private float m_LookAngle = 0f;

    private enum State { Patrol, Chase, Attack, Investigate }
    private State currentState;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();

        currentState = State.Patrol;
        GoToNextPoint();
    }

    void Update()
    {
        m_Distance = Vector3.Distance(transform.position, Target.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (CanSeePlayer())
                    currentState = State.Chase;
                break;

            case State.Chase:
                Chase();

                if (CanSeePlayer())
                    _chaseMemoryTimer = ChaseMemoryTime;   // Resetea la memoria al verlo
                else
                    _chaseMemoryTimer -= Time.deltaTime;

                if (m_Distance <= AttackDistance)
                {
                    currentState = State.Attack;
                }
                else if (_chaseMemoryTimer <= 0f)           // Solo suelta si agotó la memoria
                {
                    _chaseMemoryTimer = 0f;
                    currentState = State.Patrol;
                }
                break;

            case State.Attack:
                Attack();
                if (m_Distance > AttackDistance)
                    currentState = State.Chase;
                break;

            case State.Investigate:
                Investigate();
                // Si ve al jugador mientras investiga, lo persigue
                if (CanSeePlayer())
                    currentState = State.Chase;
                break;
        }
    }

    // ─────────────────────────────────────────────
    //  MÉTODO PÚBLICO — llámalo desde otro script
    //  cuando ocurra un sonido en el mundo
    // ─────────────────────────────────────────────
    /// <summary>
    /// Llama este método cuando ocurra un sonido.
    /// Si está dentro del radio de escucha, el enemigo irá a investigar.
    /// </summary>
    public void HearSound(Vector3 soundPosition)
    {
        float distToSound = Vector3.Distance(transform.position, soundPosition);

        if (distToSound > HearingRadius) return;

        // Solo interrumpe patrulla o investigación previa, no un combate activo
        if (currentState == State.Chase || currentState == State.Attack) return;

        m_SoundPosition = soundPosition;
        m_InvestigateTimer = InvestigateWaitTime;
        m_IsLookingAround = false;

        currentState = State.Investigate;
        Debug.Log($"[EnemyAI] Sonido escuchado a {distToSound:F1}m → Investigando");
    }

    // ─────────────────────────────────────────────
    //  ESTADOS
    // ─────────────────────────────────────────────
    void Patrol()
    {
        m_Animator.SetBool("isRunning", false);
        m_Animator.SetBool("isAttacking", false);

        if (!m_Agent.pathPending && m_Agent.remainingDistance < 0.5f)
            GoToNextPoint();
    }

    void Chase()
    {
        m_Agent.isStopped = false;
        m_Agent.destination = Target.position;
        m_Animator.SetBool("isRunning", true);
        m_Animator.SetBool("isAttacking", false);
    }

    void Attack()
    {
        m_Agent.isStopped = true;
        m_Animator.SetBool("isRunning", false);
        m_Animator.SetBool("isAttacking", true);
    }

    void Investigate()
    {
        m_Animator.SetBool("isAttacking", false);

        if (!m_IsLookingAround)
        {
            // Fase 1: Caminar hacia el origen del sonido
            m_Agent.isStopped = false;
            m_Agent.destination = m_SoundPosition;
            m_Animator.SetBool("isRunning", true);

            bool arrivedAtSound = !m_Agent.pathPending && m_Agent.remainingDistance < 0.6f;

            if (arrivedAtSound)
            {
                // Llegó → empieza a mirar alrededor
                m_IsLookingAround = true;
                m_Agent.isStopped = true;
                m_Animator.SetBool("isRunning", false);
                m_LookAngle = 0f;
                Debug.Log("[EnemyAI] Llegué al punto, mirando alrededor...");
            }
        }
        else
        {
            // Fase 2: Girar 360° lentamente buscando al jugador
            m_LookAngle += 90f * Time.deltaTime; // 90°/seg → una vuelta cada ~4 seg
            transform.rotation = Quaternion.Euler(0f, m_LookAngle, 0f);

            m_InvestigateTimer -= Time.deltaTime;

            if (m_InvestigateTimer <= 0f)
            {
                // No encontró nada → volver a patrullar
                Debug.Log("[EnemyAI] Nada aquí. Volviendo a patrullar.");
                m_Agent.isStopped = false;
                currentState = State.Patrol;
                GoToNextPoint();
            }
        }
    }

    // ─────────────────────────────────────────────
    //  UTILIDADES
    // ─────────────────────────────────────────────
    bool CanSeePlayer()
    {
        // Apunta al centro del CharacterController en vez de a los pies
        CharacterController playerCC = Target.GetComponent<CharacterController>();
        Vector3 playerCenter = playerCC != null
            ? Target.position + playerCC.center
            : Target.position + Vector3.up;          // fallback

        Vector3 directionToPlayer = playerCenter - (transform.position + Vector3.up);
        float distance = directionToPlayer.magnitude;

        if (distance > DetectionDistance) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > FieldOfView / 2f) return false;

        Ray ray = new Ray(transform.position + Vector3.up, directionToPlayer.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, DetectionDistance))
            if (hit.transform == Target) return true;

        return false;
    }

    void GoToNextPoint()
    {
        if (PatrolPoints.Length == 0) return;
        m_Agent.destination = PatrolPoints[currentPoint].position;
        currentPoint = (currentPoint + 1) % PatrolPoints.Length;
    }

    public void DealDamage()
    {
        if (Target == null) return;
        if (Vector3.Distance(transform.position, Target.position) <= AttackDistance)
            Target.GetComponent<ThirdPersonController>()?.TakeDamage(damage);
    }

    private void OnAnimatorMove()
    {
        if (!m_Animator.GetBool("isAttacking"))
            m_Agent.speed = (m_Animator.deltaPosition / Time.deltaTime).magnitude;
    }

    // ─────────────────────────────────────────────
    //  GIZMOS — visualización en el Editor
    // ─────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (!DrawGizmos) return;

        // Radio de escucha (amarillo)
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, HearingRadius);

        // Radio de detección visual (rojo)
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, DetectionDistance);
    }
}