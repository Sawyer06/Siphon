using TMPro;
using UnityEngine;

//largely based on https://www.youtube.com/watch?v=kMHwy-unZ5M 

public class enemyVision : MonoBehaviour
{
    public DirectorScript director;
    private bool playerAlreadySpotted;

    public float detectRange = 15;
    public float detectAngle = 28;

    bool isInAngle, isInRange, isNotHidden;

    public GameObject Player;
    void Start()
    {
        playerAlreadySpotted = false;
    }

    // Update is called once per frame
    void Update()
    {
        isInAngle = false;
        isInRange = false;
        isNotHidden = false;

        //is player too far?
        if (Vector3.Distance(transform.position, Player.transform.position) < detectRange)
        {
            isInRange = true;
        }
        else { }

        //is player hidden?
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Player.transform.position - transform.position, out hit, Mathf.Infinity))
            //gives direction from turret to player
        {
            if (hit.transform == Player.transform)
            {
                isNotHidden = true;
            }
        }

        //is player in cone of sight?
        Vector3 side1 = Player.transform.position - transform.position;
        Vector3 side2 = transform.forward;
        float angle = Vector3.SignedAngle(side1,side2, Vector3.up);
        if (angle < detectAngle && angle > -1 * detectAngle)
        {
            isInAngle = true;
        }


        //player is seen
        if (isInAngle && isInRange &&  isNotHidden)
        {
            //tell director about it
            if (!playerAlreadySpotted)
            {
                /*                Vector3 lastSeenLocation = Player.transform.position;   //have to do this to get value instead of reference
                                director.cameraSpottedPlayer(lastSeenLocation);*/
                UnityEngine.Debug.Log("SEEN BY CAMERA");
                playerAlreadySpotted = true;
            }
            
        }
        else
        {
            playerAlreadySpotted = false;
        }
    }
}
