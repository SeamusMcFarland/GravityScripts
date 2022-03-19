using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEffectScript : MonoBehaviour
{
    ParticleSystem ps;


    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        ps.Stop();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowLaser()
    {
        ps.Play();
        StartCoroutine("HideLaser");
    }

    IEnumerator HideLaser()
    {
        yield return new WaitForSeconds(0.01f);
        ps.Stop();
        
    }

    public void ShowLaser(float length)
    {
        ps.Play();
        StartCoroutine("HideLaser", length);
    }

    IEnumerator HideLaser(float length)
    {
        yield return new WaitForSeconds(length);
        ps.Stop();

    }

    public void ForceHide()
    {
        ps.Stop();
    }

    /*
    private void FixedUpdate()
    {
        if (!shooting)
        {
            transform.position = gun.transform.position + gun.transform.forward;
            transform.rotation = Quaternion.Euler(playerS.GetRotationVector().x, playerS.GetRotationVector().y, playerS.GetRotationVector().z);
        }
    }
    */
}
