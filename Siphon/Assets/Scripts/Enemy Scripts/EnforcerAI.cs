using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms.GameCenter;

public class EnforcerAI : enemyMove
{
    public Transform[] PatrolPoints;
    public Transform noiseSource;
    public Transform lastLocationDetected;

    public enum Behaviors { Idle, Patrol, Chase, Searching, CheckingLocker };
    public Behaviors enforcerAiBehaviors = Behaviors.Idle;

    // Multi-Agent
    public DirectorScript director;

    // Detection and Chase configurations
    public float detectionRange;
    public bool requireLineOfSight = true;
    public float eyeHeight = 1.2f;
    public LayerMask lineOfSightMask = ~0;
    public float chaseReportInterval = 0.5f;

    // Locker and Investigation config
    public float searchRadius = 10.0f;
    public float searchSpeedMultiplier = 1.1f;

    //Might want to tune this a little...
    public float lockerSearchAlertThreshold = 4.0f;
    public float lockerSearchRadius = 12.0f;
    public float lockerInteractDistance = 2.2f;
    public float lockerInspectDuration = 1.5f;
    public float lockerPlayerCheckRadius = 1.8f;

    // Patrol config
    bool ReversePath = false;
    bool RestartPath = false;
    public int curPatrolPoint = 0;
    public int lastPatrolPoint = 0; //Exists in case Patrol function gets interupted, so it has the option to return to a previous patrol point
    protected int closestPatrolPoint = 0;
    
    // State
    public NavMeshAgent navAgent;
    public bool isChase; //for when the enforcer is directly following the player
    public bool isSuspicious; //when the player's location is hinted at
    public float currentAlertLevel = 0.0f;

    protected float movementMultiplier = 0.9f;

    private Vector3 lastKnownPosition;
    private Transform chasedPlayerTransform;
    private PlayerController cachedPlayer;
    private bool atLastKnownPosition;
    private float nextChaseReportTime;
    private bool isWithGrunt = false;
    private float ignoreLockersUntil;

    private Locker targetLocker;
    private float lockerInspectTimer;
    private readonly HashSet<Locker> checkedLockers = new HashSet<Locker>();

    // So no enforcers claim the same locker and try to inspect the same locker at the same time.
    private static readonly HashSet<Locker> ClaimedLockers = new HashSet<Locker>();


    private static void ResetLockerState()
    {
        ClaimedLockers.Clear();
    }

    protected override void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        if (director == null)
            director = FindFirstObjectByType<DirectorScript>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            cachedPlayer = playerObj.GetComponent<PlayerController>();

        isChase = false;
        isSuspicious = false;
        RestartPath = true;

        UpdateMovementSpeed();

        if (PatrolPoints != null && PatrolPoints.Length > 0)
        {
            SetDestination(PatrolPoints[curPatrolPoint].position);
            ChangeBehavior(Behaviors.Patrol);
        }
        else
        {
            ChangeBehavior(Behaviors.Idle);
        }
    }

    protected override void Update()
    {
        if (isWithGrunt)
            return;

        UpdateMovementSpeed();
        RunBehaviors();
    }

    private void OnDisable()
    {
        ReleaseTargetLocker();
    }

    // Decided to add this since there a few speed changes depending on enforcer state atp.
    private void UpdateMovementSpeed()
    {
        if (isChase)
        {
            navAgent.speed = runSpeed;
        }
        else if (enforcerAiBehaviors == Behaviors.Searching || enforcerAiBehaviors == Behaviors.CheckingLocker)
        {
            navAgent.speed = walkSpeed * searchSpeedMultiplier;
        }
        else
        {
            navAgent.speed = walkSpeed * movementMultiplier;
        }
    }

    private void SetDestination(Vector3 target)
    {
        desiredLocation = target;
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(target);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (!CanSeePlayer(other.transform))
            return;

        lastLocationDetected = other.transform;
        lastKnownPosition = other.transform.position;
    }

    private bool CanSeePlayer(Transform playerTransform)
    {
        if (playerTransform == null)
            return false;

        if (cachedPlayer == null)
            cachedPlayer = playerTransform.GetComponent<PlayerController>();

        if (cachedPlayer != null && cachedPlayer.isHiding && Time.time >= ignoreLockersUntil)
            return false;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > detectionRange)
            return false;

        if (!requireLineOfSight)
            return true;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = playerTransform.position + Vector3.up * eyeHeight;
        Vector3 direction = target - origin;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, dist, lineOfSightMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == playerTransform || hit.transform.CompareTag("Player") || hit.transform.root == playerTransform.root;
        }

        return true;
    }

    // ---------------------
    // Grunt to Enforcer API
    // ---------------------
    
    // Allows for checking what behavior the enforcer agent is currently following
    public Behaviors CurrentBehavior => enforcerAiBehaviors;

    // Is true when the enforcer is not mid-chase 
    public bool CanReceiveGruntReport => !isChase;

    // Pause patrol so enforcer can stop and face grunt (for visual feedback purposes...) when receiving report
    public void SetGruntInteractionPause(bool paused, Vector3? faceTarget = null)
    {
        if (isChase && paused)
            return; // Chase is more important than receiving a report

        isWithGrunt = paused;

        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = paused;
        }

        if (paused && faceTarget.HasValue)
        {
            Vector3 faceDirection = faceTarget.Value - transform.position;
            faceDirection.y = 0f;
            if (faceDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(faceDirection);
            }
        }
    }

    public void ReceiveGruntReport(Vector3 reportedPlayerPosition)
    {
        isWithGrunt = false;
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
        }

        isSuspicious = true;

        // Relay the report to the director
        if (director != null)
        {
            director.enforcerReportsChaseHasStarted(reportedPlayerPosition);
        }

        if (!isChase)
        {
            InvestigateLocation(reportedPlayerPosition, currentAlertLevel);
        }
    }
    
    // ------------------------
    // Director to Enforcer API
    // ------------------------

    // Called when a report of a player location comes in
    public void InvestigateLocation(Vector3 reportedPosition, float alertLevel)
    {
        currentAlertLevel = alertLevel;
        isSuspicious = true;

        // Dont wanna pull away an enforcer who is already chasing
        if (isChase)
            return;

        ReleaseTargetLocker();
        checkedLockers.Clear();

        lastKnownPosition = reportedPosition;
        atLastKnownPosition = false;

        Debug.Log($"{name}: Investigating {reportedPosition} at alert level {alertLevel}");

        SetDestination(lastKnownPosition);
        ChangeBehavior(Behaviors.Searching);
    }

    // Called to update the alert level (obviously...)
    public void UpdateAlertLevel(float alertLevel)
    {
        currentAlertLevel = alertLevel;
        
        if (enforcerAiBehaviors == Behaviors.CheckingLocker && currentAlertLevel < lockerSearchAlertThreshold)
        {
            ReleaseTargetLocker();
            ChangeBehavior(Behaviors.Searching);
        }
    }

    // Call when alert level is low enough for the search to be called off
    public void ReturnToPost()
    {
        if (isChase)
            return;

        Debug.Log($"{name}: Returning to patrol.");

        currentAlertLevel = 0.0f;
        isSuspicious = false;
        chasedPlayerTransform = null;
        lastLocationDetected = null;
        ReleaseTargetLocker();
        checkedLockers.Clear();

        if (PatrolPoints != null && PatrolPoints.Length > 0)
        {
            SetDestination(PatrolPoints[curPatrolPoint].position);
            ChangeBehavior(Behaviors.Patrol);
        }
        else
        {
            ChangeBehavior(Behaviors.Idle);
        }
    }

    // --------------
    // Behavior State
    // --------------

    private void StartChase(Transform playerTransform)
    {
        ReleaseTargetLocker();
        checkedLockers.Clear();

        isChase = true;
        isSuspicious = true;
        chasedPlayerTransform = playerTransform;
        lastLocationDetected = playerTransform;
        lastKnownPosition = playerTransform.position;

        SetDestination(lastKnownPosition);
        ChangeBehavior(Behaviors.Chase);

        ReportChasePositionToDirector();
    }

    private void ReportChasePositionToDirector()
    {
        nextChaseReportTime = Time.time + chaseReportInterval;

        if (director != null)
        {
            director.enforcerReportsChaseHasStarted(lastKnownPosition);
        }
    }

    private void RunBehaviors()
    {
        switch (enforcerAiBehaviors)
        {
            case Behaviors.Idle:
                Idle();
                break;

            case Behaviors.Patrol:
                Patrol();
                break;

            case Behaviors.Chase:
                CheckAware();
                break;

            case Behaviors.Searching:
                SearchArea();
                break;

            case Behaviors.CheckingLocker:
                CheckLocker();
                break;
        }
    }
    private void ChangeBehavior(Behaviors newBehavior)
    {
        enforcerAiBehaviors = newBehavior;
    }

    private void Idle()
    {

    }

    private void Patrol() //Specifics of control for moving between PatrolPoints
    {
        if (PatrolPoints == null || PatrolPoints.Length == 0)
            return;

        float dist = Vector3.Distance(transform.position, PatrolPoints[curPatrolPoint].position);

        if (dist > 2.0f)
        {
            SetDestination(PatrolPoints[curPatrolPoint].position);
        }
        else
        {
            lastPatrolPoint = curPatrolPoint;

            if (ReversePath && !RestartPath)
            {
                curPatrolPoint = (curPatrolPoint <= 0) ? PatrolPoints.Length - 1 : curPatrolPoint - 1;
            }
            else
            {
                curPatrolPoint = (curPatrolPoint >= PatrolPoints.Length - 1) ? 0 : curPatrolPoint + 1;
            }

            SetDestination(PatrolPoints[curPatrolPoint].position);
        }

    }

    private void CheckAware()
    {
        if (chasedPlayerTransform == null)
        {
            LosePlayer();
            return;
        }

        if (CanSeePlayer(chasedPlayerTransform))
        {
            lastKnownPosition = chasedPlayerTransform.position;
            SetDestination(lastKnownPosition);

            // Update director
            if (Time.time >= nextChaseReportTime)
            {
                ReportChasePositionToDirector();
            }
        }
        else
        {
            LosePlayer();
        }
    }

    private void LosePlayer()
    {
        isChase = false;
        chasedPlayerTransform = null;
        lastLocationDetected = null;
        checkedLockers.Clear();
        atLastKnownPosition = false;

        Debug.Log($"{name}: Lost sight of player. Investigating around {lastKnownPosition}");

        SetDestination(lastKnownPosition);
        ChangeBehavior(Behaviors.Searching);

        if (director != null)
        {
            director.enforcerReportsFailedChase(lastKnownPosition, this);
        }
    }

    private void SearchArea()
    {
        if (navAgent.pathPending)
            return;

        if (navAgent.remainingDistance > Mathf.Max(navAgent.stoppingDistance, 1.0f))
            return;

        if (!atLastKnownPosition)
        {
            atLastKnownPosition = true;
        }

        // When alert level is high, check nearby lockers.
        if (currentAlertLevel >= lockerSearchAlertThreshold)
        {
            Locker nextLocker = FindNearbyUncheckedLocker();
            if (nextLocker != null)
            {
                targetLocker = nextLocker;
                ClaimedLockers.Add(targetLocker);
                lockerInspectTimer = 0.0f;
                SetDestination(targetLocker.transform.position);
                ChangeBehavior(Behaviors.CheckingLocker);
                return;
            }
        }

        // Otherwise just wander around the last known position untill recalled by director.
        Wander(lastKnownPosition, searchRadius);
    }

    private void Wander(Vector3 center, float range) //Implements a random Ai Patrolling path in a specified area, based on an example in https://www.youtube.com/watch?v=dYs0WRzzoRc
    {
        Vector3 point;
        // Uses the last player location as the center of the search area.
        if (RandomPoint(center, range, out point))
        {
            Debug.DrawRay(point, Vector3.up, Color.green, 1.0f); //Makes it visible with gizmos
            navAgent.SetDestination(point);

            Debug.Log("New Wander Destination: " + point);
        }
    }

    private void CheckLocker()
    {
        if (targetLocker == null || IsLockerAlreadyOpen(targetLocker))
        {
            if (targetLocker != null)
                checkedLockers.Add(targetLocker);

            ReleaseTargetLocker();
            ChangeBehavior(Behaviors.Searching);
            return;
        }

        float distanceToLocker = Vector3.Distance(transform.position, targetLocker.transform.position);
        if (distanceToLocker > lockerInteractDistance)
        {
            SetDestination(targetLocker.transform.position);
            return;
        }

        navAgent.ResetPath();
        lockerInspectTimer += Time.deltaTime;

        if (lockerInspectTimer < lockerInspectDuration)
            return;

        Locker inspectedLocker = targetLocker;
        checkedLockers.Add(inspectedLocker);
        ReleaseTargetLocker();

        // Locker animation
        Animator lockerAnim = inspectedLocker.GetComponentInChildren<Animator>();
        if (lockerAnim != null)
        {
            lockerAnim.SetBool("open", true);
        }

        if (IsPlayerHidingInLocker(inspectedLocker, out Transform foundPlayer))
        {
            Debug.Log($"{name}: Found player hiding in locker.");

            if (cachedPlayer != null)
                cachedPlayer.isHiding = false;

            ignoreLockersUntil = Time.time + 2.0f;
            StartChase(foundPlayer);
        }
        else
        {
            Debug.Log($"{name}: Locker was empty.");
            ChangeBehavior(Behaviors.Searching);
        }
    }

    // Finds the closest unchecked locker.
    private Locker FindNearbyUncheckedLocker()
    {
        Locker[] allLockers = FindObjectsByType<Locker>(FindObjectsSortMode.None);
        Locker closest = null;
        float closestDistance = float.MaxValue;

        foreach (Locker locker in allLockers)
        {
            if (locker == null || ClaimedLockers.Contains(locker) || checkedLockers.Contains(locker))
                continue;

            if (IsLockerAlreadyOpen(locker))
                continue;

            float distFromLastKnown = Vector3.Distance(lastKnownPosition, locker.transform.position);
            if (distFromLastKnown <= lockerSearchRadius)
            {
                float distFromSelf = Vector3.Distance(transform.position, locker.transform.position);
                if (distFromSelf < closestDistance)
                {
                    closestDistance = distFromSelf;
                    closest = locker;
                }
            }
        }

        return closest;
    }

    private bool IsLockerAlreadyOpen(Locker locker)
    {
        Animator lockerAnim = locker.GetComponentInChildren<Animator>();
        return lockerAnim != null && lockerAnim.GetBool("open");
    }

    private bool IsPlayerHidingInLocker(Locker locker, out Transform playerTransform)
    {
        if (cachedPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                cachedPlayer = playerObj.GetComponent<PlayerController>();
        }

        if (cachedPlayer != null && cachedPlayer.isHiding)
        {
            float dist = Vector3.Distance(cachedPlayer.transform.position, locker.transform.position);
            if (dist <= lockerPlayerCheckRadius)
            {
                playerTransform = cachedPlayer.transform;
                return true;
            }
        }

        playerTransform = null;
        return false;
    }

    private void ReleaseTargetLocker()
    {
        if (targetLocker != null)
        {
            ClaimedLockers.Remove(targetLocker);
            targetLocker = null;
        }
    }


    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }
}
