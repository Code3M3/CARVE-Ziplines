using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FollowerHookAttachment : MonoBehaviour
{
    public GamemanagerPlayer.playerMovementState playerManager;

    public UnityEvent OnAttach;
    [SerializeField] float detectionSize = 0.44f;

    public SplineFollower follower; //reference the one and only in the project
    public GameObject hookHoverMesh;
    
    public SplineComputer _computer; //change this to private eventually and change value to the computer detected by the raycast

    bool _isAttached;
    PhysicsHand grabbingHand = null;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionSize);
    }

    private void Update()
    {
        // Raycast to detect the spline and attachment point

        // Move HoverSphere to the attachment point

        // Assign follower to the spline
    }

    public void FollowerToTargetPoint()
    {
        // Calculate spline follower position with projection
        SplineSample targetSplineSamp = follower.spline.Project(hookHoverMesh.transform.position);

        double percent = targetSplineSamp.percent;
        //Debug.Log("percentage along spline: " + percent);

        //Vector3 targetSplinePos = follower.EvaluatePosition(percent);
        follower.SetPercent(percent); //apply percent
    }

    private void FixedUpdate()
    {
        if (grabbingHand == null) return;

        float distanceToHand = Vector3.Distance(grabbingHand.gameObject.transform.position, transform.position);

        if (distanceToHand <= 5f)
        {
            if (!_isAttached && grabbingHand._attachActivated)
            {
                // invoke
                Debug.Log("attach");
                OnAttach.Invoke();
                _isAttached = true;

                playerManager = GamemanagerPlayer.playerMovementState.Zipline;
            }
        }
        else
        {
            if (grabbingHand == null) return;

            if (_isAttached && !grabbingHand._attachActivated)
            {
                _isAttached = false;

                playerManager = GamemanagerPlayer.playerMovementState.Grounded;
            }
        }
    }

    public void ActivateZipline()
    {
        OnAttach.Invoke();
    }

    private void OnTriggerEnter(Collider other) //check if hand is grabbing too
    {
        Debug.Log("collision detected");
        // check if collision object is a hand

        if (other.gameObject.GetComponent<PlayerHandTag>() == null) return;

        grabbingHand = other.GetComponentInParent<PhysicsHand>();
    }

    private void OnTriggerExit(Collider other)
    {
 
    }
}
