using UnityEngine;

public class DirectorScript : MonoBehaviour
{
    [SerializeField] enemyMove enemy;
    public void cameraSpottedPlayer(Vector3 playerLocation)
    {
        UnityEngine.Debug.Log("seen");
        enemy.desiredLocation = playerLocation;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
