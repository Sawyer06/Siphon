using UnityEngine;
using UnityEngine.AI;

public class enemyMove : MonoBehaviour
{
    //public Transform desiredLocation;
    public Vector3 desiredLocation;
    private NavMeshAgent agent;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(desiredLocation != null)
        {
            agent.destination = desiredLocation;  
        }
        
    }
}
