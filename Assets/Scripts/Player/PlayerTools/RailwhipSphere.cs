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

    public GameObject splineFollower; //instantiate this eventually
    [SerializeField] FollowerHookAttachment hookAttachment;
    
    [Header("Physics")]
    Rigidbody railWhipSphereRB;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] float launchPower = 15f;
    [SerializeField] float playerSpeedLimit = 100f;
    [SerializeField] PhysicsHand domHand;

    [Header("Inputs")]
    public InputActionReference grabButton; // this is not actually grabbing, it's just the name of the mechanic in this game specifically

    [Header("Player Rotation on Zipline")]
    [SerializeField] public XROriginPointer rotateToFollowerScript;

    private bool updateTargetPos;
    private Vector3 targetPos;

    private bool isAttemptingGrab;
    private Collision splineCollision;

    private float speed;
    Vector3 previousPosition;

    private bool isZiplining;

    private void Start()
    {
        railWhipSphereRB = GetComponent<Rigidbody>();

        // listen for controller events
        grabButton.action.started += OnGrabPressed;
        grabButton.action.canceled += OnGrabCanceled;

        previousPosition = playerRigidbody.transform.position;
    }

    private void OnGrabCanceled(InputAction.CallbackContext obj)
    {
        isAttemptingGrab = false;

        updateTargetPos = false; // trip cancelled so stop movement
        splineCollision = null; // we've canceled the trip to our collision target, so we need to void it

        isZiplining = false;
        hookAttachment.DeactivateZipline();
    }

    private void OnGrabPressed(InputAction.CallbackContext obj)
    {
        isAttemptingGrab = true;
        targetPos = gameObject.transform.position;

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

            targetPos = collision.GetContact(0).point; 
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // this is mostly just to prevent edge case errors
        if (IsInLayerMask(collision.gameObject, splineLayer))
            splineCollision = null;

        Debug.Log(collision.gameObject + " exited spline");
    }

    private void FixedUpdate()
    {
        playerRigidbody.gameObject.transform.forward = hookAttachment.CalculateFollowerSplineForwardVector();

        if (updateTargetPos)
        {
            FollowTargetPos();

            Debug.Log("distance to target: " + Vector3.Distance(targetPos, playerRigidbody.transform.position));
            if (Vector3.Distance(targetPos, playerRigidbody.transform.position) < 0.5f)
            {
                // set the follower to the correct place, only if spline marked object has an actual spline component
                if (splineCollision.gameObject.TryGetComponent(out SplineComputer targetSplineComputer))
                {
                    hookAttachment.FollowerToTargetPoint(targetSplineComputer);
                }

                railWhipSphereRB.constraints = RigidbodyConstraints.None; // free sphere from spline

                // if we continue holding grab, zipline behavior (add zipline forces)
                hookAttachment.ActivateZipline();

                isZiplining = true;

                updateTargetPos = false;
                splineCollision = null; // we've reached our spline collision target, so we need to void it

                // otherwise just continue as normal
            }
        }

        if (isZiplining)
        {
            targetPos = splineFollower.transform.position;

            FollowTargetPos();
        }

    }

    void FollowTargetPos()
    {
        Vector3 dirTowardTarget = targetPos - playerRigidbody.transform.position;
        playerRigidbody.AddForce(dirTowardTarget * launchPower, ForceMode.Impulse);
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
