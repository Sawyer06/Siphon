using System.Collections;
using System.Collections.Generic;
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
    float inspectionRange;
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

    void SearchForTarget()
    {
        Destination = lastLocationDetected.transform.position;
        navAgent.SetDestination(Destination);
        distance = Vector3.Distance(gameObject.transform.position, Destination);

    }
    void Patrol() //Specifics of control for moving between PatrolPoints
    {
        distance = Vector3.Distance(gameObject.transform.position, PatrolPoints[curPatrolPoint].position);

        if (curPatrolPoint >= PatrolPoints.Length - 1)
        {
            curPatrolPoint = 0;
            Destination = PatrolPoints[curPatrolPoint].position;
            navAgent.SetDestination(Destination);
        }


    }

    void Wander() //Implements a random Ai Patrolling path in a specified area, based on an example in https://www.youtube.com/watch?v=dYs0WRzzoRc
    {
        Vector3 point;
        if (RandomPoint(lastLocationDetected.position, inspectionRange, out point))
        {
            
        }
    }
    bool RandomPoint(Vector3 center, float inspectionRange, out Vector3 result)
    {
        
        result = Vector3.zero;
        return false;
    }
    void CheckAware() //Specifics of logic for chasing mechanics, leads to either chasing, wandering, or returning to PatrolPoints
    {
        
    }
}
