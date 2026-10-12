using UnityEngine;
using UnityEngine.AI;

public class EnforcerAI : MonoBehaviour
{
    public Transform[] PatrolPoints;
    public Transform noiseSource;
    public Transform lastLocationDetected;
    public int curPatrolPoint = 0;
    public NavMeshAgent navAgent;
    Vector3 Destination;
    float distance;

    public bool isChase;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        isChase = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
