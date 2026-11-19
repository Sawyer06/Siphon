using Unity.VisualScripting;
using UnityEngine;

public class DirectorScript : MonoBehaviour
{

    public void alertNearestEnforcer(Vector3 playerLocation)
    {
        EnforcerAI[] allEnforcers = FindObjectsByType<EnforcerAI>(FindObjectsSortMode.None);

        EnforcerAI closestEnforcerScript = null;
        float closestDistance = 99999;

        //go through all enforcers, find which is closest
        for (int i = 0; i < allEnforcers.Length; i++)
        {
            EnforcerAI thisEnforcerScript = allEnforcers[i];
            GameObject enforcer = thisEnforcerScript.GameObject();

            float dist = Vector3.Distance(enforcer.transform.position, playerLocation);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestEnforcerScript = thisEnforcerScript;
            }
        }

        //send message to enforcer
        if (closestEnforcerScript != null)
        {
            //////////////////////////////////

            //closestEnforcerScript.PLAYER_SPOTTED_GO_HERE();

            /////////////////////////////////
        }
    }

    public void alertNearestGrunt(Vector3 playerLocation)
    {
        GruntAI[] allGrunts = FindObjectsByType<GruntAI>(FindObjectsSortMode.None);

        GruntAI closestGruntScript = null;
        float closestDistance = 99999;

        //go through all enforcers, find which is closest
        for (int i = 0; i < allGrunts.Length; i++)
        {
            GruntAI thisGruntScript = allGrunts[i];
            GameObject gruntObject = thisGruntScript.GameObject();

            float dist = Vector3.Distance(gruntObject.transform.position, playerLocation);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestGruntScript = thisGruntScript;
            }
        }

        //send message to enforcer
        if (closestGruntScript != null)
        {
            //////////////////////////////////

            closestGruntScript.ScoutArea(playerLocation);

            /////////////////////////////////
        }
    }

    public void cameraSpottedPlayer(Vector3 playerLocation)
    {
        UnityEngine.Debug.Log("SEEN BY DIRECTOR");

        //alertNearestEnforcer(playerLocation);
        alertNearestGrunt(playerLocation);

    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
