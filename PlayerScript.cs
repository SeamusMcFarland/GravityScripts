using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    Rigidbody rb;
    float maxSpeed; // can change if both axis are being held
    float typeSpeed;
    float acceleration;
    float jumpSpeed;

    const float MAX_MOUSE_Y = 60f;
    float verticalRotation;
    float horizontalRotation;
    float horRotX, horRotY;

    GameObject camObj;

    const float VERTICAL_MOUSE_SENSITIVITY = 2.5f;
    const float HORIZONTAL_MOUSE_SENSITIVITY = 5f;

    bool grounded;
    float stateAcceleration;

    bool sprinting;
    float energy;
    float maxEnergy;
    const float SPRINT_MODIFIER = 2f;
    float energyTimer;
    const float ENERGY_TIMER_DELAY = 1f;
    const float MINIMUM_ENERGY_PERCENTAGE = 0.15f;

    bool shiftK;

    public Image staminaBarRemaining;
    public Image staminaBarOuter;
    public Image staminaBarInner;
    float savedStaminaX;
    bool staminaFilled;

    Vector3 relativeVelocity;

    int orientation;
    List<GravityGroundScript> groundS = new List<GravityGroundScript>();
    float[] oCooldown = { 0, 0, 0, 0 };
    const float ORIENTATION_COOLDOWN = 0.5f;
    GravityGroundScript currentGS;
    bool changedOrientation;
    float zRotation;
    const float FLIP_SPEED = 10f;
    const float GRAVITY = 0.5f;
    const float FRICTION_VELOCITY = 0.5f;
    float frameNormalizer;
    float flipSpeed;

    bool doubleJump;

    bool leftMouseDown;
    int ammo;
    int maxAmmo;
    float gunCooldown;
    const float GUN_COOLDOWN = 0.3f;
    float damage;

    List<EnemyScript> enemyS = new List<EnemyScript>();

    bool noisy;

    public SparkEffectScript sparkEffectS;
    public SparkEffectScript bloodSparkEffectS;
    LaserEffectScript leS;

    float health;

    public GameObject[] winObjects;
    public GameObject[] loseObjects;
    bool dead;

    AudioControllerScript acS;

    // Start is called before the first frame update
    void Awake()
    {
        acS = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioControllerScript>();
        noisy = false;
        health = 5f;
        leS = GetComponentInChildren<LaserEffectScript>();
        damage = 1f;
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Enemy"))
            enemyS.Add(o.GetComponent<EnemyScript>());
        maxAmmo = 99999;
        ammo = maxAmmo;
        Cursor.lockState = CursorLockMode.Locked;
        doubleJump = true;
        staminaFilled = false;
        SetStaminaBarEnabled(false);
        savedStaminaX = staminaBarRemaining.rectTransform.localPosition.x;

        maxEnergy = 5f;
        energy = maxEnergy;

        camObj = GetComponentInChildren<Camera>().gameObject;

        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        typeSpeed = 20f;
        jumpSpeed = 13f;
    }

    // Update is called once per frame
    void Update()
    {
        frameNormalizer = Time.deltaTime / 0.015f;

        CheckGravity();
        CheckFriction();
        CheckSwitchOrientation();
        CheckOrientation();
        CheckInputs();
        CheckSprinting();
        CheckEnergy();
        RefreshStaminaBar();
        CheckVelocity();
        CheckRotation();
        CheckAttack();
        CheckJump();
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

    float friction;

    private void CheckFriction()
    {
        if (grounded)
        {
            friction = FRICTION_VELOCITY * frameNormalizer;

            if (Mathf.Abs(transform.InverseTransformDirection(rb.velocity).x) < friction)
                rb.velocity = transform.TransformDirection(new Vector3(0, transform.InverseTransformDirection(rb.velocity).y, transform.InverseTransformDirection(rb.velocity).z));
            else if (transform.InverseTransformDirection(rb.velocity).x < -friction)
                rb.velocity += transform.right * friction;
            else if (transform.InverseTransformDirection(rb.velocity).x > friction)
                rb.velocity -= transform.right * friction;
            else
                print("ERROR! INVALID RB.VELOCITY COMPARISON");

            if (Mathf.Abs(transform.InverseTransformDirection(rb.velocity).z) < friction)
                rb.velocity = transform.TransformDirection(new Vector3(transform.InverseTransformDirection(rb.velocity).x, transform.InverseTransformDirection(rb.velocity).y, 0));
            else if (transform.InverseTransformDirection(rb.velocity).z < -friction)
                rb.velocity += transform.forward * friction;
            else if (transform.InverseTransformDirection(rb.velocity).z > friction)
                rb.velocity -= transform.forward * friction;
            else
                print("ERROR! INVALID RB.VELOCITY COMPARISON");
        }
    }

    /*
    private void CheckFriction()
    {
        if (grounded)
        {
            switch (orientation)
            {
                case 0:
                    ApplyFrictionToAxis("x");
                    ApplyFrictionToAxis("z");
                    break;
                case 1:
                    ApplyFrictionToAxis("y");
                    ApplyFrictionToAxis("z");
                    break;
                case 2:
                    ApplyFrictionToAxis("x");
                    ApplyFrictionToAxis("z");
                    break;
                case 3:
                    ApplyFrictionToAxis("y");
                    ApplyFrictionToAxis("z");
                    break;
            }
        }
    }

    float friction;

    private void ApplyFrictionToAxis(string s)
    {
        friction = FRICTION_VELOCITY * frameNormalizer;

        switch (s)
        {
            case "x":
                if (Mathf.Abs(rb.velocity.x) < friction)
                    rb.velocity = new Vector3(0, rb.velocity.y, rb.velocity.z);
                else if (rb.velocity.x < -friction)
                    rb.velocity = new Vector3(rb.velocity.x + friction, rb.velocity.y, rb.velocity.z);
                else if (rb.velocity.x > friction)
                    rb.velocity = new Vector3(rb.velocity.x - friction, rb.velocity.y, rb.velocity.z);
                else
                    print("ERROR! INVALID RB.VELOCITY COMPARISON");
                break;
            case "y":
                if (Mathf.Abs(rb.velocity.y) < friction)
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                else if (rb.velocity.y < -friction)
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + friction, rb.velocity.z);
                else if (rb.velocity.y > friction)
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - friction, rb.velocity.z);
                else
                    print("ERROR! INVALID RB.VELOCITY COMPARISON");
                break;
            case "z":
                if (Mathf.Abs(rb.velocity.z) < friction)
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0);
                else if (rb.velocity.z < -friction)
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z + friction);
                else if (rb.velocity.z > friction)
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z - friction);
                else
                    print("ERROR! INVALID RB.VELOCITY COMPARISON");
                break;
            default:
                print("ERROR! INVALID SWITCH IN APPLYFRICTIONTOAXIS!");
                break;
        }
    }*/

    private void CheckSwitchOrientation()
    {
        if (currentGS == null && groundS.Count > 0)
            currentGS = groundS[0];

        if (groundS.Count > 0 && !grounded && !changedOrientation)
        {
            int chosenSwitch = -1;
            for (int i = 0; i < groundS.Count; i++)
                if (groundS[i].GetOrientation() != orientation && oCooldown[groundS[i].GetOrientation()] <= 0)
                    chosenSwitch = i;

            if (chosenSwitch != -1)
            {
                changedOrientation = true;

                oCooldown[currentGS.GetOrientation()] = ORIENTATION_COOLDOWN;
                currentGS = groundS[chosenSwitch];
                orientation = currentGS.GetOrientation();
            }

        }

        for (int i = 0; i < 4; i++)
            oCooldown[i] -= Time.deltaTime;
    }

    private void CheckOrientation()
    {
        if (grounded)
        {
            switch (orientation)
            {
                case 0:
                    zRotation = 0;
                    break;
                case 1:
                    zRotation = 90f;
                    break;
                case 2:
                    zRotation = 180f;
                    break;
                case 3:
                    zRotation = -90f;
                    break;
            }
        }
        else
        {
            flipSpeed = FLIP_SPEED * frameNormalizer;

            switch (orientation)
            {
                case 0:
                    if (Mathf.Abs(zRotation) > 3f)
                    {
                        if (Mathf.Abs(zRotation) < 6f)
                            flipSpeed /= 2f;

                        if (zRotation > 0)
                            zRotation -= flipSpeed;
                        else
                            zRotation += flipSpeed;
                    }
                    break;
                case 1:
                    if (zRotation > 93f || zRotation < 87f)
                    {
                        if (zRotation < 96f && zRotation > 84f)
                            flipSpeed /= 2f;

                        if (zRotation > 90f || zRotation < -90f)
                            zRotation -= flipSpeed;
                        else
                            zRotation += flipSpeed;
                    }
                    break;
                case 2:
                    if ((zRotation > -177f && zRotation <= 0) || (zRotation < 177f && zRotation > 0))
                    {
                        if (zRotation < -174f || zRotation > 174f)
                            flipSpeed /= 2f;

                        if (zRotation < 0)
                            zRotation -= flipSpeed;
                        else
                            zRotation += flipSpeed;
                    }
                    break;
                case 3:
                    if (zRotation > -87f || zRotation < -93f)
                    {
                        print("correcting left at zRotation: " + zRotation + " at time: " + Time.time);
                        if (zRotation < -84f && zRotation > -96f)
                            flipSpeed /= 2f;

                        if (zRotation > -90f && zRotation < 90f)
                            zRotation -= flipSpeed;
                        else
                            zRotation += flipSpeed;
                    }
                    break;
            }

            if (zRotation > 180f)
                zRotation -= 360f;
            if (zRotation <= -180f)
                zRotation += 360f;
        }
    }

    private void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            shiftK = true;
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            shiftK = false;
    }

    private void CheckSprinting()
    {
        if ((energy / maxEnergy) > MINIMUM_ENERGY_PERCENTAGE && shiftK)
            sprinting = true;
        else if (energy < 0 || !shiftK)
            sprinting = false;

        if (sprinting)
            SetStaminaBarEnabled(true);
    }

    private void CheckEnergy()
    {
        if (sprinting)
        {
            energyTimer = ENERGY_TIMER_DELAY;
            energy -= Time.deltaTime;
        }

        if (energyTimer < 0)
        {
            if (energy < maxEnergy)
                energy += Time.deltaTime;
        }
        else
        {
            energyTimer -= Time.deltaTime;
        }
    }

    private void RefreshStaminaBar()
    {
        if (energy >= maxEnergy - 0.01f)
        {
            SetStaminaBarEnabled(false);
            staminaFilled = true;
        }
        else
        {
            staminaBarRemaining.transform.localScale = new Vector2(energy / maxEnergy, staminaBarRemaining.transform.localScale.y);
            staminaBarRemaining.rectTransform.localPosition = new Vector2(savedStaminaX - ((50f / maxEnergy) * (maxEnergy - energy)), staminaBarRemaining.rectTransform.localPosition.y);
        }
    }

    private void SetStaminaBarEnabled(bool e)
    {
        staminaBarRemaining.enabled = e;
        staminaBarOuter.enabled = e;
        staminaBarInner.enabled = e;
    }

    private void CheckVelocity()
    {
        HorizontalDirectionAdjust();

        maxSpeed = typeSpeed;

        if (sprinting)
            maxSpeed *= SPRINT_MODIFIER;

        switch (orientation)
        {
            case 0:
                if (Mathf.Abs(transform.InverseTransformDirection(rb.velocity).x) > 0.01f && Mathf.Abs(transform.InverseTransformDirection(rb.velocity).z) > 0.01f)
                    maxSpeed /= 1.41f; // 1.41 is roughly sqrt of 2
                break;
            case 1:
                if (Mathf.Abs(transform.InverseTransformDirection(rb.velocity).y) > 0.01f && Mathf.Abs(transform.InverseTransformDirection(rb.velocity).z) > 0.01f)
                    maxSpeed /= 1.41f; // 1.41 is roughly sqrt of 2
                break;
            case 2:
                if (Mathf.Abs(transform.InverseTransformDirection(rb.velocity).x) > 0.01f && Mathf.Abs(transform.InverseTransformDirection(rb.velocity).z) > 0.01f)
                    maxSpeed /= 1.41f; // 1.41 is roughly sqrt of 2
                break;
            case 3:
                if (Mathf.Abs(transform.InverseTransformDirection(rb.velocity).y) > 0.01f && Mathf.Abs(transform.InverseTransformDirection(rb.velocity).z) > 0.01f)
                    maxSpeed /= 1.41f; // 1.41 is roughly sqrt of 2
                break;

        }

        if (grounded)
            acceleration = 2f;
        else
            acceleration = 0.4f;

        if (Mathf.Abs(Input.GetAxis("Horizontal") * frameNormalizer) > 0.01f)
        {
            if (Input.GetAxis("Horizontal") * frameNormalizer > 0.01f)
            {
                if (transform.InverseTransformDirection(rb.velocity).x < maxSpeed)
                    rb.velocity = rb.velocity + transform.right * acceleration * frameNormalizer;
            }
            else
            {
                if (transform.InverseTransformDirection(rb.velocity).x > -maxSpeed)
                    rb.velocity = rb.velocity - transform.right * acceleration * frameNormalizer;
            }
        }
        if (Mathf.Abs(Input.GetAxis("Vertical") * frameNormalizer) > 0.01f)
        {
            if (Input.GetAxis("Vertical") * frameNormalizer > 0.01f)
            {
                if (transform.InverseTransformDirection(rb.velocity).z < maxSpeed)
                    rb.velocity = rb.velocity + transform.forward * acceleration * frameNormalizer;
                // rb.velocity = rb.velocity + new Vector3(moveRotX, 0, moveRotZ) * acceleration;
            }
            else
            {
                if (transform.InverseTransformDirection(rb.velocity).z > -maxSpeed)
                    rb.velocity = rb.velocity - transform.forward * acceleration * frameNormalizer;
                //rb.velocity = rb.velocity - new Vector3(moveRotX, 0, moveRotZ) * acceleration;
            }
        }
    }

    float moveRotX, moveRotZ;

    private void HorizontalDirectionAdjust()
    {
        if (Mathf.Abs(horizontalRotation) > 90f)
            moveRotZ = -Mathf.Abs(Mathf.Abs(horizontalRotation) - 90f) / 90f; // value of 0 to -1
        else
            moveRotZ = Mathf.Abs(Mathf.Abs(horizontalRotation) - 90f) / 90f; // value of 0 to 1

        float tempHorizAdjust = horizontalRotation - 90f;
        if (tempHorizAdjust <= -180f)
            tempHorizAdjust += 360f;

        if (Mathf.Abs(tempHorizAdjust) > 90f)
            moveRotX = -Mathf.Abs(Mathf.Abs(tempHorizAdjust) - 90f) / 90f; // value of 0 to -1
        else
            moveRotX = Mathf.Abs(Mathf.Abs(tempHorizAdjust) - 90f) / 90f; // value of 0 to 1
    }

    /*private void CheckVelocity()
    {
        maxSpeed = typeSpeed;

        if (sprinting)
            maxSpeed *= SPRINT_MODIFIER;

        switch(orientation)
        {
            case 0:
                if (Mathf.Abs(rb.velocity.x) > 0.01f && Mathf.Abs(rb.velocity.z) > 0.01f)
                    maxSpeed /= 1.41f; // 1.41 is roughly sqrt of 2
                break;
            case 1:
                if (Mathf.Abs(rb.velocity.y) > 0.01f && Mathf.Abs(rb.velocity.z) > 0.01f)
                    maxSpeed /= 1.41f; // 1.41 is roughly sqrt of 2
                break;
            case 2:
                if (Mathf.Abs(rb.velocity.x) > 0.01f && Mathf.Abs(rb.velocity.z) > 0.01f)
                    maxSpeed /= 1.41f; // 1.41 is roughly sqrt of 2
                break;
            case 3:
                if (Mathf.Abs(rb.velocity.y) > 0.01f && Mathf.Abs(rb.velocity.z) > 0.01f)
                    maxSpeed /= 1.41f; // 1.41 is roughly sqrt of 2
                break;

        }

        if (grounded)
            acceleration = 0.8f;
        else
            acceleration = 0.1f;

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f)
        {
            if (Input.GetAxis("Horizontal") > 0.01f)
            {
                switch (orientation)
                {
                    case 0:
                        if (rb.velocity.x < maxSpeed)
                            rb.velocity = rb.velocity + transform.right * acceleration;
                        break;
                    case 1:
                        if (rb.velocity.z < maxSpeed)
                            rb.velocity = rb.velocity + transform.right * acceleration;
                        break;
                    case 2:
                        if (rb.velocity.x > -maxSpeed)
                            rb.velocity = rb.velocity + transform.right * acceleration;
                        break;
                    case 3:
                        if (rb.velocity.z > -maxSpeed)
                            rb.velocity = rb.velocity + transform.right * acceleration;
                        break;
                }
            }
            else
            {
                switch (orientation)
                {
                    case 0:
                        if (rb.velocity.x > -maxSpeed)
                            rb.velocity = rb.velocity - transform.right * acceleration;
                        break;
                    case 1:
                        if (rb.velocity.z > -maxSpeed)
                            rb.velocity = rb.velocity - transform.right * acceleration;
                        break;
                    case 2:
                        if (rb.velocity.x < maxSpeed)
                            rb.velocity = rb.velocity - transform.right * acceleration;
                        break;
                    case 3:
                        if (rb.velocity.z < maxSpeed)
                            rb.velocity = rb.velocity - transform.right * acceleration;
                        break;
                }
            }
        }
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f)
        {
            if (Input.GetAxis("Vertical") > 0.01f)
            {
                if (rb.velocity.z < maxSpeed)
                    rb.velocity = rb.velocity + transform.forward * acceleration;
            }
            else
            {
                if (rb.velocity.z > -maxSpeed)
                    rb.velocity = rb.velocity - transform.forward * acceleration;
            }
        }
    }

    private void CheckVelocity()
    {
        relativeVelocity = transform.InverseTransformDirection(rb.velocity);

        maxSpeed = typeSpeed;

        if (sprinting)
            maxSpeed *= SPRINT_MODIFIER;

        if (Mathf.Abs(relativeVelocity.x) < 0.01f && Mathf.Abs(relativeVelocity.z) < 0.01f)
            maxSpeed /= 1.41f; // 1.41 is roughly sqrt of 2

        if (grounded)
            acceleration = 0.8f;
        else
            acceleration = 0.1f;

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f)
        {
            if (Input.GetAxis("Horizontal") > 0.01f)
            {
                if (relativeVelocity.x < maxSpeed)
                    rb.velocity = transform.TransformDirection(new Vector3(relativeVelocity.x + acceleration, relativeVelocity.y, relativeVelocity.z));
            }
            else
            {
                if (relativeVelocity.x > -maxSpeed)
                    rb.velocity = transform.TransformDirection(new Vector3(relativeVelocity.x - acceleration, relativeVelocity.y, relativeVelocity.z));
            }
        }
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f)
        {
            if (Input.GetAxis("Vertical") > 0.01f)
            {
                if (relativeVelocity.z < maxSpeed)
                    rb.velocity = transform.TransformDirection(new Vector3(relativeVelocity.x, relativeVelocity.y, relativeVelocity.z + acceleration));
            }
            else
            {
                if (relativeVelocity.z > -maxSpeed)
                    rb.velocity = transform.TransformDirection(new Vector3(relativeVelocity.x, relativeVelocity.y, relativeVelocity.z - acceleration));
            }
        }
    }*/

    private void CheckRotation()
    {
        CheckVerticalMouse();
        CheckHorizontalMouse();
        camObj.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        /*if(sprinting)
            camObj.transform.localPosition = new Vector3(0, 0, 0);
        else
            camObj.transform.localPosition = new Vector3(0, 0.5f, 0);*/
        rb.MoveRotation(Quaternion.Euler(horRotX, horRotY, zRotation));
    }

    public float GetVerticalRotation()
    {
        return verticalRotation;
    }

    public Vector3 GetRotationVector()
    {
        return new Vector3(horRotX, horRotY, zRotation);
    }

    private void CheckVerticalMouse()
    {
        if (Input.GetAxis("Mouse Y") < 0)
        {
            if (Input.GetAxis("Mouse Y") * VERTICAL_MOUSE_SENSITIVITY * frameNormalizer + verticalRotation < MAX_MOUSE_Y)
            {
                verticalRotation -= Input.GetAxis("Mouse Y") * VERTICAL_MOUSE_SENSITIVITY * frameNormalizer;
            }
            else
            {
                verticalRotation = MAX_MOUSE_Y;
            }
        }
        else
        {
            if (Input.GetAxis("Mouse Y") * VERTICAL_MOUSE_SENSITIVITY * frameNormalizer + verticalRotation > -MAX_MOUSE_Y)
            {
                verticalRotation -= Input.GetAxis("Mouse Y") * VERTICAL_MOUSE_SENSITIVITY * frameNormalizer;

            }
            else
            {
                verticalRotation = -MAX_MOUSE_Y;
            }
        }
    }


    float tempHorRotX, tempHorRotY;

    private void CheckHorizontalMouse()
    {
        if (Mathf.Abs(zRotation) > 90f)
            tempHorRotY = -Mathf.Abs(Mathf.Abs(zRotation) - 90f) / 90f; // value of 0 to -1
        else
            tempHorRotY = Mathf.Abs(Mathf.Abs(zRotation) - 90f) / 90f; // value of 0 to 1

        float tempZAdjust = zRotation - 90f;
        if (tempZAdjust <= -180f)
            tempZAdjust += 360f;

        if (Mathf.Abs(tempZAdjust) > 90f)
            tempHorRotX = Mathf.Abs(Mathf.Abs(tempZAdjust) - 90f) / 90f; // value of 0 to -1
        else
            tempHorRotX = -Mathf.Abs(Mathf.Abs(tempZAdjust) - 90f) / 90f; // value of 0 to 1

        horizontalRotation += Input.GetAxis("Mouse X") * HORIZONTAL_MOUSE_SENSITIVITY * frameNormalizer;

        horRotX = tempHorRotX * horizontalRotation;
        horRotY = tempHorRotY * horizontalRotation;

        if (horizontalRotation > 180f)
            horizontalRotation -= 360f;
        else if (horizontalRotation <= -180)
            horizontalRotation += 360f;
    }

    private void CheckAttack()
    {
        gunCooldown -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
            leftMouseDown = true;
        if (Input.GetMouseButtonUp(0))
            leftMouseDown = false;

        if (leftMouseDown && gunCooldown <= 0 && ammo > 0)
        {
            gunCooldown = GUN_COOLDOWN;
            FireGun();
        }
    }

    RaycastHit hit;
    EnemyScript storedES;

    private void FireGun()
    {
        leS.ShowLaser();
        acS.PlaySound("bzzzt");
        noisy = true;
        StartCoroutine("EndNoisy");
        Debug.DrawRay(transform.position, camObj.transform.forward, Color.red, 1000f);
        if (Physics.Raycast(camObj.transform.position, camObj.transform.forward, out hit, 1000f))
        {
            storedES = null;
            foreach (EnemyScript es in enemyS)
            {
                if (es.transform == hit.transform)
                {
                    storedES = es;
                    break;
                }
            }

            if (storedES != null)
            {
                storedES.Hurt(damage);
                bloodSparkEffectS.Spark(hit.point);
            }
            else
            {
                sparkEffectS.Spark(hit.point);
            }
        }
    }

    IEnumerator EndNoisy()
    {
        yield return new WaitForSeconds(0.02f);
        noisy = false;
    }

    private void CheckJump()
    {
        if ((grounded || doubleJump) && Input.GetKeyDown(KeyCode.Space))
        {
            if (!grounded) // if doublejumping
            {
                doubleJump = false;

                switch (orientation)
                {
                    case 0:
                        if (rb.velocity.y < 0)
                            rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
                        else
                            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + jumpSpeed, rb.velocity.z);
                        break;
                    case 1:
                        if (rb.velocity.x > 0)
                            rb.velocity = new Vector3(-jumpSpeed, rb.velocity.y, rb.velocity.z);
                        else
                            rb.velocity = new Vector3(rb.velocity.x - jumpSpeed, rb.velocity.y, rb.velocity.z);
                        break;
                    case 2:
                        if (rb.velocity.y > 0)
                            rb.velocity = new Vector3(rb.velocity.x, -jumpSpeed, rb.velocity.z);
                        else
                            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - jumpSpeed, rb.velocity.z);
                        break;
                    case 3:
                        if (rb.velocity.x < 0)
                            rb.velocity = new Vector3(jumpSpeed, rb.velocity.y, rb.velocity.z);
                        else
                            rb.velocity = new Vector3(rb.velocity.x + jumpSpeed, rb.velocity.y, rb.velocity.z);
                        break;
                }
            }
            else
            {
                switch (orientation)
                {
                    case 0:
                        rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
                        break;
                    case 1:
                        rb.velocity = new Vector3(-jumpSpeed, rb.velocity.y, rb.velocity.z);
                        break;
                    case 2:
                        rb.velocity = new Vector3(rb.velocity.x, -jumpSpeed, rb.velocity.z);
                        break;
                    case 3:
                        rb.velocity = new Vector3(jumpSpeed, rb.velocity.y, rb.velocity.z);
                        break;
                }
            }
        }
    }

    public void SetGrounded(bool b)
    {
        grounded = b;
        if (b)
        {
            changedOrientation = false;
            doubleJump = true;
        }
    }
    /*
    public void SetOrientation(int n)
    {
        if (n != orientation && !grounded && !changedOrientation)
        {
            changedOrientation = true;
            orientation = n;
        }
    }
    */

    public void AddOrientationObject(GravityGroundScript ggS)
    {
        bool found = false;
        foreach (GravityGroundScript obj in groundS)
            if (obj == ggS)
                found = true;
        if (!found)
            groundS.Add(ggS);
    }

    public void RemoveOrientationObject(GravityGroundScript ggS)
    {
        groundS.Remove(ggS);
    }

    public int GetOrientation()
    {
        return orientation;
    }

    public bool GetNoisy()
    {
        print("noisy calling");
        return noisy;
    }

    public void Hurt(float dam)
    {
        acS.PlaySound("hitcough");
        health -= dam;
        if (health <= 0)
            Death();
    }

    private void Death()
    {
        if (!dead)
        {
            acS.PlaySound("downslope");
            Cursor.lockState = CursorLockMode.None;
            dead = true;
            foreach (GameObject o in loseObjects)
                o.SetActive(true);
        }
    }

    private void Win()
    {
        if (!dead)
        {
            acS.PlaySound("clapping");
            Cursor.lockState = CursorLockMode.None;
            dead = true;
            foreach (GameObject o in winObjects)
                o.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WinHitbox"))
            Win();
        else if (other.CompareTag("Fire"))
            Hurt(10f);
    }

}
