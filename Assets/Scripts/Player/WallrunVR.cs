using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunVR : MonoBehaviour
{
    [Header("Movement and Detecton")]
    [SerializeField] Transform camera;
    [Space]
    [SerializeField] GameObject detectionOrigin;
    [SerializeField] float wallDistance = 0.5f;
    [SerializeField] float minimumJumpHeight = 1.5f;

    [Header("Wall Running")]
    public LayerMask wallMask;
    [SerializeField] private float antiWallRunGravity;
    [SerializeField] public float wallRunSlideForce;
    [SerializeField] public float maxWallSpeed;
    public float wallRunAcceleration = 2f; //how quickly we build speed to run up walls

    [SerializeField] private float _forceMultiplier = 500f;

    bool wallLeft = false;
    bool wallRight = false;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    Vector3 moveDirection;

    private Rigidbody rb;

    bool CanWallRun()
    {
        // can only wall run when not grounded & when a wall is detected
        return !Physics.Raycast(detectionOrigin.transform.position, Vector3.down, minimumJumpHeight)
            && (wallRight || wallLeft);
    }

    void CheckWallNearby()
    {
        var camLeftDir = -camera.transform.right;
        camLeftDir.y = 0;
        camLeftDir = camLeftDir.normalized;

        wallLeft = Physics.Raycast(detectionOrigin.transform.position, camLeftDir+(-transform.up), out leftWallHit, wallDistance, ~wallMask);
        Debug.DrawRay(detectionOrigin.transform.position, camLeftDir + (-transform.up), Color.red);

        wallRight = Physics.Raycast(detectionOrigin.transform.position, -camLeftDir+(-transform.up), out rightWallHit, wallDistance, ~wallMask);
        Debug.DrawRay(detectionOrigin.transform.position, -camLeftDir + (-transform.up), Color.white);
    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        moveDirection = rb.velocity.normalized;

        CheckWallNearby();

        if (CanWallRun())
        {
            if (wallLeft)
            {
                StartWallRun();
                Debug.Log("wall run on left");
            }
            else if (wallRight)
            {
                StartWallRun();
                Debug.Log("wall run on right");
            }
            else
            {
                // no wall on left or right
                StopWallRun();
            }
        }
        else
        {
            // stop wall run when not high enough from ground to wall run
            StopWallRun();
        }
    }

    void StartWallRun()
    {
        rb.useGravity = false;

        if (rb.velocity.magnitude <= maxWallSpeed)
        {
            rb.AddForce(Vector3.up * antiWallRunGravity * _forceMultiplier * Time.fixedDeltaTime);

            if (wallLeft)
            {
                rb.AddForce(transform.forward * wallRunSlideForce * _forceMultiplier * Time.fixedDeltaTime);

                // make sure we stick to wall
                rb.AddForce(transform.right * _forceMultiplier * 10 * Time.fixedDeltaTime);

                Vector3 LerpVelocity = Vector3.Lerp(rb.velocity, transform.forward, wallRunAcceleration * Time.fixedDeltaTime);
                rb.velocity = LerpVelocity;
            }
            else if (wallRight)
            {
                rb.AddForce(transform.forward * wallRunSlideForce * _forceMultiplier * Time.fixedDeltaTime);

                // make sure we stick to wall
                rb.AddForce(-transform.right * _forceMultiplier * 10 * Time.fixedDeltaTime);

                Vector3 LerpVelocity = Vector3.Lerp(rb.velocity, transform.forward, wallRunAcceleration * Time.fixedDeltaTime);
                rb.velocity = LerpVelocity;
            }
        }
    }

    void StopWallRun()
    {
        rb.AddForce(moveDirection.normalized * _forceMultiplier * Time.fixedDeltaTime, ForceMode.Impulse); // just a little kick for the end

        rb.useGravity = true;
    }
}
