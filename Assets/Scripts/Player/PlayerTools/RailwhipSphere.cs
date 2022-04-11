using Dreamteck.Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RailwhipSphere : MonoBehaviour
{
    [Header("Spline Connection")]
    public LayerMask splineLayer;
    public GameObject splineFollower; 
    [SerializeField] FollowerHookAttachment hookAttachment;
    
    [Header("Physics")]
    Rigidbody railWhipSphereRB;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] float launchPower = 15f;
    [SerializeField] float attachPower = 150f;
    [SerializeField] float playerSpeedLimit = 100f;
    [SerializeField] PhysicsHand domHand;
    [SerializeField] Railwhip railwhip;

    [Header("Inputs")]
    public InputActionReference grabButton; // this is not actually grabbing, it's just the name of the mechanic in this game specifically

    [Header("Player Rotation on Zipline")]
    [SerializeField] public XROriginPointer rotateToFollowerScript;

    private bool updateTargetPos;
    private Vector3 targetPos;

    private bool isAttemptingGrab;
    private Collision splineCollision;

    private float startingScale;

    private float speed;
    private float savedMagnitude;
    Vector3 previousPosition;

    private bool isZiplining;

    private void Start()
    {
        railWhipSphereRB = GetComponent<Rigidbody>();

        // listen for controller events
        grabButton.action.started += OnGrabPressed;
        grabButton.action.canceled += OnGrabCanceled;

        previousPosition = playerRigidbody.transform.position;

        isAttemptingGrab = false;

        updateTargetPos = false; // trip cancelled so stop movement
        splineCollision = null; // we've canceled the trip to our collision target, so we need to void it

        startingScale = transform.localScale.x;
    }

    private void OnGrabCanceled(InputAction.CallbackContext obj)
    {
        isAttemptingGrab = false;

        updateTargetPos = false; // trip cancelled so stop movement
        splineCollision = null; // we've canceled the trip to our collision target, so we need to void it

        if (isZiplining)
        {
            // save mag so we can apply when player exits the zipline (and account for acceleration and add that on)
            savedMagnitude = Vector3.Magnitude(playerRigidbody.velocity);

            isZiplining = false;
            hookAttachment.DeactivateZipline();

            playerRigidbody.velocity = playerRigidbody.velocity.normalized * savedMagnitude;

            railWhipSphereRB.velocity = playerRigidbody.velocity;

            Invoke(nameof(TeleportSphere), 0.1f);
        }
    }

    void TeleportSphere()
    {
        railWhipSphereRB.constraints = RigidbodyConstraints.None;
        // return to close position and change velocity to follow player
        railWhipSphereRB.gameObject.transform.position = domHand.transform.position;

        railwhip.ResetJoint();
    }

    private void OnGrabPressed(InputAction.CallbackContext obj)
    {
        isAttemptingGrab = true;

        if (splineCollision != null)
        {
            targetPos = splineCollision.GetContact(0).point;
        }

            StartCoroutine(TryGrab());
    }

    IEnumerator TryGrab()
    {
        while (isAttemptingGrab)
        {
            if (splineCollision != null)
            {
                updateTargetPos = true; // this will activate the physics to travel

                isAttemptingGrab = false;
            }
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision) // eventually change this to spherecast or segmented raycast
    {
        Debug.Log("Railwhip Sphere colliding with:" + collision.gameObject);

        if (IsInLayerMask(collision.gameObject, splineLayer))
        {
            railWhipSphereRB.constraints = RigidbodyConstraints.FreezeAll; //to prevent triggering exit collider

            splineCollision = collision; // populate the collision variable to check in the grab trigger

            targetPos = collision.GetContact(0).point - (domHand.transform.position - playerRigidbody.transform.position);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log(collision.gameObject + " exited spline");
    }

    private void FixedUpdate()
    {
        // the further away from the player, the larger the scale
        float distanceScale = startingScale + Vector3.Distance(transform.position, domHand.transform.position)/10;
        Vector3 newScale = new Vector3(distanceScale, distanceScale, distanceScale);
        transform.localScale = newScale;

        if (updateTargetPos)
        {
            FollowTargetPos(launchPower);

            Debug.Log("distance to target: " + Vector3.Distance(targetPos, playerRigidbody.transform.position));
            if (Vector3.Distance(targetPos, playerRigidbody.transform.position) < 0.5f)
            {
                // set the follower to the correct place, only if spline marked object has an actual spline component
                if (splineCollision.gameObject.TryGetComponent(out SplineComputer targetSplineComputer))
                {
                    hookAttachment.FollowerToTargetPoint(targetSplineComputer);

                    // if we continue holding grab, zipline behavior (add zipline forces)
                    hookAttachment.ActivateZipline();

                    isZiplining = true;
                }

                updateTargetPos = false;
                splineCollision = null; // we've reached our spline collision target, so we need to void it

                railWhipSphereRB.constraints = RigidbodyConstraints.None; // free sphere from spline

                // otherwise if not a rail just continue as normal
            }
        }

        if (isZiplining)
        {
            targetPos = splineFollower.transform.position - (domHand.transform.position - playerRigidbody.transform.position);

            // change rotation to match spline direction !!!MAKE THIS PHYSICS BASED!!!
            Vector3 LerpDir = Vector3.Slerp(playerRigidbody.gameObject.transform.forward, hookAttachment.CalculateFollowerSplineForwardVector(), Time.fixedDeltaTime * 3);
            playerRigidbody.gameObject.transform.forward = LerpDir;

            // activate hookeslaw when attached to zipline !!important!!
            domHand.HookesLaw();

            FollowTargetPos(attachPower);
        }

    }

    void FollowTargetPos(float power)
    {
        Vector3 dirTowardTarget = targetPos - playerRigidbody.transform.position;
        playerRigidbody.AddForce(dirTowardTarget * power, ForceMode.Impulse);
    }

    float GetDrag()
    {
        Vector3 playerVelocity = (playerRigidbody.transform.position - previousPosition) / Time.fixedDeltaTime; //prevpos is from prev frame
        float drag = 1 / playerVelocity.magnitude + 0.01f; //add .01 bc we don't want it to ever be 0
        drag = drag > 1 ? 1 : drag; //if drag is greater than 1 set to 1 otherwise it's just drag
        drag = drag < 0.03f ? 0.03f : drag;

        previousPosition = playerRigidbody.transform.position;
        return drag;
    }

    bool CheckPlayerSpeedLimit()
    {
        // calc current player speed
        speed = Vector3.Magnitude(playerRigidbody.velocity);

        // return true if we are over speed limit
        return (speed > playerSpeedLimit);
    }

    public bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << obj.layer)) > 0);
    }
}
