using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnforcerAI : enemyMove
{
    public Transform[] PatrolPoints;
    public Transform noiseSource;
    public Transform lastLocationDetected;

    bool ReversePath = false;
    bool RestartPath = false;
    public int curPatrolPoint = 0;
    public int lastPatrolPoint = 0; //Exists in case Patrol function gets interupted, so it has the option to return to a previous patrol point
    protected int closestPatrolPoint = 0;
    public NavMeshAgent navAgent;
    Vector3 Destination;

    protected float movementMultiplier = 0.9f;
    float distance;
    float inspectionRange;
    float seekingTimer;
    float timeUntilUnaware;

    public bool isChase; //for when the enforcer is directly following the player
    public bool isSuspicious; //when the player's location is hinted by either grunts or the player making a noise
    protected override void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = walkSpeed * movementMultiplier;
        navAgent.destination = PatrolPoints[curPatrolPoint].position;


        isChase = false;
        isSuspicious = false;
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

        if(isSuspicious)
            timeUntilUnaware += Time.deltaTime;

        
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
        if(distance <= 5)
        {
            isChase = true;
            timeUntilUnaware = 0.0f;
        }
        else if(distance > 5)
        {
            
        }
    }
    void Patrol() //Specifics of control for moving between PatrolPoints
    {
        distance = Vector3.Distance(gameObject.transform.position, PatrolPoints[curPatrolPoint].position);

        if(distance > 2.0f)
        {
            Destination = PatrolPoints[curPatrolPoint].position;
            navAgent.SetDestination(Destination);
        }
        else{
            if(ReversePath && !RestartPath)
            {
                if(curPatrolPoint <= PatrolPoints.Length - 1)
                {
                    lastPatrolPoint = curPatrolPoint;
                    curPatrolPoint += 1;
                    Destination = PatrolPoints[curPatrolPoint].position;
                    navAgent.SetDestination(Destination);
                }
                else
                {
                    
                }
            }
            else if(!ReversePath && RestartPath)
            {
                if(curPatrolPoint >= PatrolPoints.Length - 1)
                {
                    lastPatrolPoint = curPatrolPoint;
                    curPatrolPoint = 0;
                    Destination = PatrolPoints[curPatrolPoint].position;
                    navAgent.SetDestination(Destination);
                }
                else
                {
                    
                }
            }
            else
            {
                if (curPatrolPoint >= PatrolPoints.Length - 1)
                {
                    lastPatrolPoint = curPatrolPoint;
                    curPatrolPoint = 0;
                    Destination = PatrolPoints[curPatrolPoint].position;
                    navAgent.SetDestination(Destination);
                    RestartPath = true;
                }
                else
                {
                    lastPatrolPoint = curPatrolPoint;
                    curPatrolPoint++;
                    Destination = PatrolPoints[curPatrolPoint].position;
                    navAgent.SetDestination(Destination);
                }
            }
        }

    }

    void Wander() //Implements a random Ai Patrolling path in a specified area, based on an example in https://www.youtube.com/watch?v=dYs0WRzzoRc
    {
        Vector3 point;
        if (RandomPoint(lastLocationDetected.position, inspectionRange, out point))
        {   
            Debug.DrawRay(point, Vector3.up, Color.green, 1.0f); //Makes it visible with gizmos
            navAgent.SetDestination(point);
        }
    }
    bool RandomPoint(Vector3 center, float inspectionRange, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * inspectionRange;
        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
    
    void CheckAware() //Specifics of logic for chasing mechanics, leads to either chasing, wandering, or returning to PatrolPoints
    {
        
    }
}
