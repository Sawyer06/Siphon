using System.Collections;
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

    public float scoutingGiveUpTime = 10.0f;
    public float scoutingRadius = 5.0f;
    private bool scouting;

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
        if (!scouting) // Grunt's default behavior, when not receiving orders from director.
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

                    //find nearest enforcer, make this the RunAwayLocation
                    /////////////////////////////////////
                    EnforcerAI[] allEnforcers = FindObjectsByType<EnforcerAI>(FindObjectsSortMode.None);

                    EnforcerAI closestEnforcerScript = null;
                    float closestDistance = 99999;

                    //go through all enforcers, find which is closest
                    for (int i = 0; i < allEnforcers.Length; i++)
                    {
                        EnforcerAI thisEnforcerScript = allEnforcers[i];
                        GameObject enforcer = thisEnforcerScript.GameObject();

                        float dist = Vector3.Distance(enforcer.transform.position, this.gameObject.transform.position);

                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            closestEnforcerScript = thisEnforcerScript;
                        }
                    }

                    RunAwayLocation = closestEnforcerScript.transform;
                    /////////////////////////////////////


                    Agent.destination = RunAwayLocation.position;
                    Agent.speed = runSpeed;
                    scouting = false;
                }
            }
        }
        else if (other.gameObject.CompareTag("Mess")) // If sees a mess, go clean up.
        {
            Distraction foundDistraction;
            if ((foundDistraction = other.gameObject.GetComponent<Distraction>()).isActivated)
            {
                foundDistraction.Deactivate();
                //scouting = false;
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

    /// Director calls this with player last seen position.
    public void ScoutArea(Vector3 position)
    {
        scouting = true;

        StartCoroutine(ScoutGiveUpTimer());
        StartCoroutine(ScoutSearch(position));
    }

    /// Timer to wait until returning to regular patrol.
    private IEnumerator ScoutGiveUpTimer()
    {
        yield return new WaitForSeconds(scoutingGiveUpTime);
        scouting = false;
    }

    /// Look around in the general area of the received player location.
    private IEnumerator ScoutSearch(Vector3 position)
    {
        Agent.destination = position + (new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y) * scoutingRadius);
        yield return new WaitForSeconds(5.0f);

        if (scouting)
            StartCoroutine(ScoutSearch(position));
        else
            Debug.Log("Grunt finished scouting");
    }
}
