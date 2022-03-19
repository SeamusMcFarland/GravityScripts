using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGroundScript : MonoBehaviour
{

    PlayerScript playerS;
    public int orientation; // 0 for down, 1 for right, 2 for up, 3 for left

    // Start is called before the first frame update
    void Awake()
    {
        playerS = GameObject.Find("Player").GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(playerS == null)
            playerS = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        print("playerS: " + playerS);
        if (other.CompareTag("Player"))
            playerS.AddOrientationObject(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerS == null)
            playerS = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        print("playerS: " + playerS);
        if (other.CompareTag("Player"))
            playerS.RemoveOrientationObject(this);
    }

    public int GetOrientation()
    {
        return orientation;
    }
}
