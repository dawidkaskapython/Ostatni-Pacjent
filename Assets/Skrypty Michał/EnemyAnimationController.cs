using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Sprawdzamy prędkość agenta. 
        // velocity.magnitude mówi nam jak szybko obiekt porusza się w świecie.
        float currentSpeed = agent.velocity.magnitude;

        // Jeśli prędkość jest większa niż 0.1, ustawiamy bool na true
        if (currentSpeed > 0.1f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }
}