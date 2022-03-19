using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedScript : MonoBehaviour
{
    PlayerScript playerS;

    List<GameObject> downGrounds = new List<GameObject>();
    List<GameObject> rightGrounds = new List<GameObject>();
    List<GameObject> upGrounds = new List<GameObject>();
    List<GameObject> leftGrounds = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        playerS = transform.parent.GetComponent<PlayerScript>();    
    }

    // Update is called once per frame
    void Update()
    {
        switch (playerS.GetOrientation())
        {
            case 0:
                if (downGrounds.Count > 0)
                    playerS.SetGrounded(true);
                else
                    playerS.SetGrounded(false);
                break;
            case 1:
                if (rightGrounds.Count > 0)
                    playerS.SetGrounded(true);
                else
                    playerS.SetGrounded(false);
                break;
            case 2:
                if (upGrounds.Count > 0)
                    playerS.SetGrounded(true);
                else
                    playerS.SetGrounded(false);
                break;
            case 3:
                if (leftGrounds.Count > 0)
                    playerS.SetGrounded(true);
                else
                    playerS.SetGrounded(false);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "GroundDown":
                downGrounds.Add(other.gameObject);
                break;
            case "GroundRight":
                rightGrounds.Add(other.gameObject);
                break;
            case "GroundUp":
                upGrounds.Add(other.gameObject);
                break;
            case "GroundLeft":
                leftGrounds.Add(other.gameObject);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case "GroundDown":
                downGrounds.Remove(other.gameObject);
                break;
            case "GroundRight":
                rightGrounds.Remove(other.gameObject);
                break;
            case "GroundUp":
                upGrounds.Remove(other.gameObject);
                break;
            case "GroundLeft":
                leftGrounds.Remove(other.gameObject);
                break;
        }
    }
}
