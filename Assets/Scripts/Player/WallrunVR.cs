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
    [SerializeField] private float wallRunGravity;
    [SerializeField] public float wallRunSlideForce;

    [SerializeField] private float _forceMultiplier = 500f;

    bool wallLeft = false;
    bool wallRight = false;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    Vector3 moveDirection;

    private Rigidbody rb;

    bool CanWallRun()
    {
        return !Physics.Raycast(detectionOrigin.transform.position, Vector3.down, minimumJumpHeight);
    }

    void CheckWallNearby()
    {
        var camLeftDir = -camera.transform.right;
        camLeftDir.y = 0;
        camLeftDir = camLeftDir.normalized;

        wallLeft = Physics.Raycast(detectionOrigin.transform.position, camLeftDir+(-transform.up), out leftWallHit, wallDistance);
        Debug.DrawRay(detectionOrigin.transform.position, camLeftDir + (-transform.up), Color.red);

        wallRight = Physics.Raycast(detectionOrigin.transform.position, -camLeftDir+(-transform.up), out rightWallHit, wallDistance);
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

        rb.AddForce(Vector3.up * wallRunGravity, ForceMode.Force);

        if (wallLeft)
        {
            Vector3 wallRunDirection = Vector3.ProjectOnPlane(moveDirection, leftWallHit.normal);
            rb.AddForce(wallRunDirection * wallRunSlideForce * _forceMultiplier, ForceMode.Force);
            rb.AddForce(transform.forward * wallRunSlideForce * _forceMultiplier, ForceMode.Force);
        }
        else if (wallRight)
        {
            Vector3 wallRunDirection = Vector3.ProjectOnPlane(moveDirection, rightWallHit.normal);
            rb.AddForce(wallRunDirection * wallRunSlideForce * _forceMultiplier, ForceMode.Force);
            rb.AddForce(transform.forward * wallRunSlideForce * _forceMultiplier, ForceMode.Force);
        }
    }

    void StopWallRun()
    {
        rb.AddForce(moveDirection.normalized, ForceMode.Acceleration); // just a little kick for the end

        rb.useGravity = true;
    }
}
