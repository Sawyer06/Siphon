using UnityEngine;
using UnityEngine.AI;

public class GruntAI : enemyMove
{
    public Transform[] PatrolPoints;
    private NavMeshAgent Agent;
    private int CurrentPatrolPoint = 0;
    private Transform LastPlayerLocation;
    public Transform RunAwayLocation;
    protected override void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.destination = PatrolPoints[CurrentPatrolPoint].position;
        desiredLocation = PatrolPoints[CurrentPatrolPoint].position;
        CurrentPatrolPoint = (CurrentPatrolPoint + 1) % PatrolPoints.Length;
    }

    protected override void Update()
    {
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            Agent.destination = PatrolPoints[CurrentPatrolPoint].position;
            desiredLocation = PatrolPoints[CurrentPatrolPoint].position;
            CurrentPatrolPoint = (CurrentPatrolPoint + 1) % PatrolPoints.Length;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            LastPlayerLocation = other.transform;
            Debug.Log(LastPlayerLocation.transform.position.x + " " + LastPlayerLocation.transform.position.y + " " + LastPlayerLocation.transform.position.z);
            Agent.destination = RunAwayLocation.position;
        }
    }
}
