using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GruntAI : enemyMove
{
    public Transform[] PatrolPoints;
    private NavMeshAgent Agent;
    private bool Hiding = false;
    private int CurrentPatrolPoint = 0;
    private Vector3 LastPlayerLocation;
    private float PatrolTime = 0;
    private float TimeInSafePlace = 0;
    public float SafePlaceTimeLimit;
    public Transform RunAwayLocation;

    protected override void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = walkSpeed;
        desiredLocation = PatrolPoints[CurrentPatrolPoint].position;
        Agent.destination = desiredLocation;
        CurrentPatrolPoint = (CurrentPatrolPoint + 1) % PatrolPoints.Length;
    }

    protected override void Update()
    {
        if (CurrentPatrolPoint == 0 && PatrolTime != 0 && Agent.remainingDistance <= Agent.stoppingDistance)
        {
            Debug.Log(PatrolTime.ToString());
            PatrolTime = 0;
        }
        PatrolTime += Time.deltaTime;
        if (Agent.isStopped && Hiding)
        {
            TimeInSafePlace += Time.deltaTime;
        }
        if (TimeInSafePlace > SafePlaceTimeLimit)
        {
            Hiding = false;
            TimeInSafePlace = 0;
            Agent.isStopped = false;
            desiredLocation = PatrolPoints[CurrentPatrolPoint].position;
            Agent.destination = desiredLocation;
        }
        if (Hiding == false)
        {
            if (Agent.remainingDistance <= Agent.stoppingDistance)
            {
                Agent.speed = walkSpeed;
                Agent.destination = PatrolPoints[CurrentPatrolPoint].position;
                desiredLocation = PatrolPoints[CurrentPatrolPoint].position;
                CurrentPatrolPoint = (CurrentPatrolPoint + 1) % PatrolPoints.Length;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) // If sees player, run away and report.
        {
            if (other.TryGetComponent<PlayerController>(out PlayerController Player))
            {
                if (Player.isHiding == false)
                {
                    LastPlayerLocation = other.transform.position;
                    Agent.destination = RunAwayLocation.position;
                    Agent.speed = runSpeed;
                }
            }
        }
        else if (other.gameObject.CompareTag("Mess")) // If sees a mess, go clean up.
        {
            Distraction foundDistraction;
            if ((foundDistraction = other.gameObject.GetComponent<Distraction>()).isActivated)
            {
                foundDistraction.Deactivate();
            }
        }
        if (other.gameObject.transform == RunAwayLocation) // Stops Grunt when reaching RunAwayLocation.*Notice* Give LastPlayerLocation to Director in this check *Notice*
        {
            Agent.isStopped = true;
            Hiding = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Mess")) // If sees a mess, go clean up.
        {
            Distraction foundDistraction;
            if ((foundDistraction = other.gameObject.GetComponent<Distraction>()).isActivated)
            {
                Agent.destination = other.transform.position;
            }
        }
    }
}
