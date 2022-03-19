using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private float health;
    private float maxHealth;
    public int orientation;

    Rigidbody rb;
    const float GRAVITY = 0.3f;

    float frameNormalizer;

    Collider[] allColliders;

    float playerDistance;
    GameObject player;

    float sightDistance;
    bool dead;

    bool locked;
    bool rotationLock;

    float maxSpeed;
    float speed;

    PlayerScript playerS;
    bool aggro;
    bool playerSeen;
    float stuckTimeout;
    float strikeDistance;
    Vector3 savedPosition;
    const float BEGIN_SLOW_DISTANCE = 2f;
    bool striking;
    bool active;
    float rotation;
    float acceleration;

    float xDiff, yDiff, zDiff;
    float passiveX, passiveY, passiveZ;
    Vector3 moveDestination;

    public bool omniscient;

    float attackTimer;
    const float ATTACK_TIMER_MAX = 2f;

    LaserEffectScript laserPSS;
    bool shaking;

    AudioControllerScript acS;

    // Start is called before the first frame update
    void Awake()
    {
        acS = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioControllerScript>();
        maxSpeed = 5f;
        laserPSS = GetComponentInChildren<LaserEffectScript>();
        active = true;
        acceleration = 0.2f;
        strikeDistance = 25f;
        savedPosition = transform.position;
        playerS = GameObject.Find("Player").GetComponent<PlayerScript>();
        sightDistance = 60f;
        player = GameObject.Find("Player");
        allColliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
        maxHealth = 2f;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        //print("aggroed: " + aggro + " speed: " + speed + " at: " + Time.time);
        playerDistance = Vector3.Distance(transform.position, player.transform.position);
        frameNormalizer = Time.deltaTime / 0.015f;

        if (!dead)
        {
            CheckGravity();
            CheckReaction();
            CheckMoveDestination();
            CheckShaking();
        }
    }

    private void CheckShaking()
    {
        if (shaking)
        {
            rb.velocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x + Random.Range(-0.03f, 0.03f), transform.position.y + Random.Range(-0.03f, 0.03f), transform.position.z + Random.Range(-0.03f, 0.03f));
        }
    }

    private void CheckGravity()
    {
        switch (orientation)
        {
            case 0:
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - GRAVITY * frameNormalizer, rb.velocity.z);
                break;
            case 1:
                rb.velocity = new Vector3(rb.velocity.x + GRAVITY * frameNormalizer, rb.velocity.y, rb.velocity.z);
                break;
            case 2:
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + GRAVITY * frameNormalizer, rb.velocity.z);
                break;
            case 3:
                rb.velocity = new Vector3(rb.velocity.x - GRAVITY * frameNormalizer, rb.velocity.y, rb.velocity.z);
                break;
        }
    }

    private void CheckReaction()
    {
        if (!rotationLock)
            CheckDirection();
        if (!locked)
        {
            if (!aggro)
            {
                if (PlayerWithinSight())
                {
                    if (playerDistance < sightDistance)
                    {
                        DetectedPlayer();
                        Aggroed();
                        playerSeen = true;
                    }
                }

            }
            else // if aggro true
            {
                attackTimer -= Time.deltaTime;

                if (Mathf.Pow(Mathf.Pow(savedPosition.x - transform.position.x, 2f) + Mathf.Pow(savedPosition.z - transform.position.z, 2f), 0.5f) > 3f)
                {
                    stuckTimeout = 0;
                    savedPosition = transform.position;
                }
                else
                {
                    stuckTimeout += Time.deltaTime;
                    if (stuckTimeout > 3f) // will de-aggro if in same place for specified seconds
                    {
                        DeAggro();
                    }
                }

                if (playerDistance < strikeDistance && attackTimer < 0)
                {
                    Attack();
                }
                SetSpeed();
                if (PlayerWithinSight() == true)
                {
                    playerSeen = true;
                    DetectedPlayer();
                }
                else
                {
                    playerSeen = false;
                }
                if (Mathf.Pow(Mathf.Pow(moveDestination.x - transform.position.x, 2f) + Mathf.Pow(moveDestination.z - transform.position.z, 2f), 0.5f) < 1f && playerSeen == false)
                {
                    DeAggro();
                }
            }
        }

    }

    private void SetSpeed()
    {
        if (striking)
            speed = 0;
        else
        {
            if (playerDistance > BEGIN_SLOW_DISTANCE)
                speed = maxSpeed;
            else
                speed = maxSpeed * Mathf.Pow(playerDistance / BEGIN_SLOW_DISTANCE, 0.05f);
        }
    }

    public void CheckDirection()
    {
        if (aggro && !locked)
        {
            if (playerSeen || omniscient)
            {
                xDiff = player.transform.position.x - transform.position.x;
                yDiff = player.transform.position.y - transform.position.y;
                zDiff = player.transform.position.z - transform.position.z;
            }
            else
            {
                xDiff = transform.position.x - passiveX;
                yDiff = transform.position.y - passiveY;
                zDiff = transform.position.z - passiveZ;
                passiveX = transform.position.x;
                passiveY = transform.position.y;
                passiveZ = transform.position.z;
            }
            switch (orientation)
            {
                case 0:
                    rotation = Mathf.Rad2Deg * Mathf.Atan2(zDiff, xDiff) + 90f;
                    transform.rotation = Quaternion.Euler(0, -rotation, 0);
                    break;
                case 1:
                    rotation = Mathf.Rad2Deg * Mathf.Atan2(zDiff, yDiff) + 90f;
                    transform.rotation = Quaternion.Euler(rotation, 0, 90f);
                    break;
                case 2:
                    rotation = Mathf.Rad2Deg * Mathf.Atan2(zDiff, xDiff) + 90f;
                    transform.rotation = Quaternion.Euler(0, rotation, 180f);
                    break;
                case 3:
                    rotation = Mathf.Rad2Deg * Mathf.Atan2(zDiff, yDiff) + 90f;
                    transform.rotation = Quaternion.Euler(-rotation, 0, 270f);
                    break;
            }
        }
    }

    public void DetectedPlayer()
    {
        moveDestination = player.transform.position; // sets position to be traveled to
    }

    private bool PlayerWithinSight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit))
        {
            if (hit.transform.CompareTag("Player"))
                return true;
            else
                return false;
        }
        else
            return false;
    }

    public void Aggroed()
    {
        if (active && !dead && !locked)
        {
            savedPosition = transform.position;
            stuckTimeout = 0;
            if (!aggro)
            {
                aggro = true; // prevents potential infinite loop
            }
            aggro = true;
        }
    }

    private void DeAggro()
    {
        playerSeen = false;
        aggro = false;
        moveDestination = transform.position;
        savedPosition = transform.position;
        speed = 0;
        rb.velocity = new Vector3(0, 0, 0);
    }

    private void CheckMoveDestination()
    {
        if (aggro)
        {
            switch (orientation)
            {
                case 0:
                    CheckMovement("x");
                    CheckMovement("z");
                    break;
                case 1:
                    CheckMovement("y");
                    CheckMovement("z");
                    break;
                case 2:
                    CheckMovement("x");
                    CheckMovement("z");
                    break;
                case 3:
                    CheckMovement("y");
                    CheckMovement("z");
                    break;
            }
        }
    }

    private void CheckMovement(string d)
    {
        switch (d)
        {
            case "x":
                if (moveDestination.x > transform.position.x && rb.velocity.x < speed)
                    rb.velocity = new Vector3(rb.velocity.x + acceleration, rb.velocity.y, rb.velocity.z);
                else if (moveDestination.x < transform.position.x && rb.velocity.x > -speed)
                    rb.velocity = new Vector3(rb.velocity.x - acceleration, rb.velocity.y, rb.velocity.z);
                break;

            case "y":
                if (moveDestination.y > transform.position.y && rb.velocity.y < speed)
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + acceleration, rb.velocity.z);
                else if (moveDestination.y < transform.position.y && rb.velocity.y > -speed)
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - acceleration, rb.velocity.z);
                break;

            case "z":
                if (moveDestination.z > transform.position.z && rb.velocity.z < speed)
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z + acceleration);
                else if (moveDestination.z < transform.position.z && rb.velocity.z > -speed)
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z - acceleration);
                break;
        }
    }

    private void Attack()
    {
        if (locked == false)
        {
            attackTimer = ATTACK_TIMER_MAX;
            locked = true;
            striking = true;
            speed = 0;
            rb.velocity = new Vector3(0, 0, 0);
            StartCoroutine("EndAttack");
        }
    }

    const float LASER_TORQUE_MAX = 10f;

    IEnumerator EndAttack()
    {
        shaking = true;
        yield return new WaitForSeconds(0.2f);
        shaking = false;
        FireGun();
        rb.velocity = rb.velocity + transform.forward * 8f;
        rb.AddTorque(new Vector3(Random.Range(-LASER_TORQUE_MAX, LASER_TORQUE_MAX), Random.Range(-LASER_TORQUE_MAX, LASER_TORQUE_MAX), Random.Range(-LASER_TORQUE_MAX, LASER_TORQUE_MAX)));
        laserPSS.ShowLaser(0.4f);
        yield return new WaitForSeconds(0.2f);
        locked = false;
        striking = false;
    }

    RaycastHit hit;
    EnemyScript storedES;

    private void FireGun()
    {
        acS.PlaySound("reverb");
        Debug.DrawRay(laserPSS.transform.position, -transform.forward, Color.red, 1000f);
        if (Physics.Raycast(laserPSS.transform.position, -transform.forward, out hit, 1000f))
        {
            if (hit.transform.CompareTag("Player"))
                playerS.Hurt(1f);
        }
    }

    public void Hurt(float dam)
    {
        acS.PlaySound("pop");
        DetectedPlayer();
        Aggroed();
        locked = true;
        playerSeen = false;
        speed = 0;
        health -= dam;
        print("I'm hurt!" + Time.time + " My health after: " + health);
        if (health <= 0)
            Death();
    }

    private void Death()
    {
        if (!dead)
        {
            print("death at: " + Time.time + " from: " + name);
            dead = true;
            shaking = false;
            laserPSS.ForceHide();
            foreach (Collider c in allColliders)
                c.enabled = false;
            rb.velocity = new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f));
            rb.angularVelocity = new Vector3(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
        }
    }
}
