using UnityEngine;
using UnityEngine.AI;

public class EnforcerAI : enemyMove
{
    public Transform[] PatrolPoints;
    public Transform noiseSource;
    public Transform lastLocationDetected;
    public int curPatrolPoint = 0;
    public int lastPatrolPoint = 0;
    protected int closestPatrolPoint = 0;
    public NavMeshAgent navAgent;
    Vector3 Destination;
    protected float movementMultiplier = 0.9f;
    float distance;
    float seekingTimer;
    float timeUntilUnaware;

    public bool isChase;
    protected override void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = walkSpeed * movementMultiplier;
        navAgent.destination = PatrolPoints[curPatrolPoint].position;


        isChase = false;
    }

    protected override void Update()
    {
        if(isChase == true)
        {
            movementMultiplier = 1.5f;
            navAgent.speed = walkSpeed * movementMultiplier;

        }
        else
        {
            movementMultiplier = 0.9f;   
            navAgent.speed = walkSpeed * movementMultiplier;
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            lastLocationDetected = other.transform; 
            //if(<= 2.0f){ //if the player is close enough... use that .distance method
            //  isChase = true;
            //}
        }
    }
}
