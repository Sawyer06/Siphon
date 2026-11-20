using Unity.VisualScripting;
using UnityEngine;

public class DirectorScript : MonoBehaviour
{
    private float alertLevel;
    private float maxAlertLevel;
    private float timeUntilAlertReduction; //every x seconds, alertLevel is reduced a little
    private float maxTimeUntilAlertReduction;

    //private bool playerChaseInProgress;

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

        float alertLevelIncrease = 1.0f;

        if ( this.alertLevel + alertLevelIncrease <= this.maxAlertLevel )
        {
            this.alertLevel += alertLevelIncrease;
        }
        
    }

    public void enforcerReportsFailedChase(Vector3 playerLastLocation, EnforcerAI thisEnforcer)
    {
        //tell thisEnforcer to go back to post
        /////DO STUFF HERE, MAKE SURE TO SET STATE TO NOT CHASING

        //loop through all enforcers, make sure no one is still chasing
        EnforcerAI[] allEnforcers = FindObjectsByType<EnforcerAI>(FindObjectsSortMode.None);
        bool isAnyoneChasing = false;
        for (int i = 0; i < allEnforcers.Length; i++)
        {
            if (allEnforcers[i].isChase == true)
            {
                isAnyoneChasing = true;
            }
        }
        //if no one is chasing, player got away
        if (!isAnyoneChasing)
        {
            //this.playerChaseInProgress = false;

            GruntAI[] allGrunts = FindObjectsByType<GruntAI>(FindObjectsSortMode.None);
            for (int i = 0; i < allGrunts.Length; i++)
            {
                //tell all grunts to go back to post
                /////DO STUFF HERE
            }
        }
            




    }

    public void enforcerReportsChaseHasStarted(Vector3 playerLastLocation)
    {
        //set enforcer alert level
        //DO STUFF HERE

        //this.playerChaseInProgress = true;
    }


    void Start()
    {
        this.alertLevel = 0.0f;
        this.maxAlertLevel = 10f;

        this.maxTimeUntilAlertReduction = 5.0f; 
        this.timeUntilAlertReduction = 0.0f;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        //as time passes, alert level reduces
        //
        if (this.timeUntilAlertReduction > 0.0)
        {
            this.timeUntilAlertReduction -= Time.deltaTime;
        }
        else //time limit reached, reduce level
        {
            this.timeUntilAlertReduction = this.maxTimeUntilAlertReduction;

            float reduceAlertByThisMuch = 1.0f;
            if (this.alertLevel - reduceAlertByThisMuch >= 0.0f)
            {
                this.alertLevel -= reduceAlertByThisMuch;
                //UnityEngine.Debug.Log("CURRENT ALERT LEVEL: " + this.alertLevel);
            }
            else
            {
                this.alertLevel = 0.0f;
            }
        }


    }
}
