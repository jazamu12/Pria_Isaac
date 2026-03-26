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
    [Header("Patrulla")]
    public Transform[] PatrolPoints;
    private int currentPoint = 0;

    private NavMeshAgent m_Agent;
    private Animator m_Animator;
    private float m_Distance;

    private enum State { Patrol, Chase, Attack }
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

                if (m_Distance <= AttackDistance)
                {
                    currentState = State.Attack;
                }
                else if (!CanSeePlayer() && m_Distance > AttackDistance)
                {
                    currentState = State.Patrol;
                }

                break;

            case State.Attack:
                Attack();
                if (m_Distance > AttackDistance)
                    currentState = State.Chase;

                break;
            
        }
    }

    void Patrol()
    {
        m_Animator.SetBool("isRunning", false);
        m_Animator.SetBool("isAttacking", false);

        if (!m_Agent.pathPending && m_Agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = Target.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > DetectionDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > FieldOfView / 2f)
            return false;

        Ray ray = new Ray(transform.position + Vector3.up, directionToPlayer.normalized);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, DetectionDistance))
        {
            if (hit.transform == Target)
                return true;
        }

        return false;
    }

    void GoToNextPoint()
    {
        if (PatrolPoints.Length == 0) return;

        m_Agent.destination = PatrolPoints[currentPoint].position;
        currentPoint = (currentPoint + 1) % PatrolPoints.Length;
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
        Debug.Log("ATACANDO");
        m_Animator.SetBool("isRunning", false);
        m_Animator.SetBool("isAttacking", true);

    }
    public void DealDamage()
    {
        if (Target == null) return;

        float distance = Vector3.Distance(transform.position, Target.position);

        if (distance <= AttackDistance)
        {
            Target.GetComponent<ThirdPersonController>()?.TakeDamage(damage);
        }
    }
    private void OnAnimatorMove()
    {
        if (!m_Animator.GetBool("isAttacking"))
        {
            m_Agent.speed = (m_Animator.deltaPosition / Time.deltaTime).magnitude;
        }
    }
}