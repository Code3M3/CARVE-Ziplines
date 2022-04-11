using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FollowerHookAttachment : MonoBehaviour
{
    public GamemanagerPlayer.playerMovementState playerManager;

    public UnityEvent OnAttach;
    public UnityEvent OnDetach;

    public SplineFollower follower; //reference the one and only in the project
    public GameObject hookHoverMesh;
    
    public SplineComputer _computer; //change this to private eventually and change value to the computer detected by the raycast

    void OnDrawGizmosSelected()
    {
    }

    private void Update()
    {
        // Raycast to detect the spline and attachment point

        // Move HoverSphere to the attachment point

        // Assign follower to the spline
    }

    public void FollowerToTargetPoint(SplineComputer splineComputer)
    {
        // Set the follower to the correct spline computer !!IMPORTANT CODE HERE!!
        follower.spline = splineComputer;

        // Calculate spline follower position with projection
        SplineSample targetSplineSamp = follower.spline.Project(hookHoverMesh.transform.position);

        double percent = targetSplineSamp.percent;
        //Debug.Log("percentage along spline: " + percent);

        //Vector3 targetSplinePos = follower.EvaluatePosition(percent);
        follower.SetPercent(percent); //apply percent
    }

    public Vector3 CalculateFollowerSplineForwardVector()
    {
        SplineSample splineSampleOfFollower = follower.Evaluate(follower.GetPercent());

        return splineSampleOfFollower.forward;
    }

    private void FixedUpdate()
    {
    }

    public void ActivateZipline()
    {
        // set follower speed
        OnAttach.Invoke();
    }

    public void DeactivateZipline()
    {
        // set speed to 0, freeze in current pos
        OnDetach.Invoke();
    }
}
