using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// referencing https://karllewisdesign.com/how-to-improve-throwing-physics-in-vr/

public class Railwhip : MonoBehaviour
{
    [SerializeField] public GameObject railWhipSphere;
    private Rigidbody railWhipSphereRB;

    [SerializeField] public GameObject controllerWithRB;
    private Rigidbody controllerRB;
    private Vector3 controllerCenterOfMass;

    public Vector3[] velocityFrames;
    public Vector3[] angularVelocityFrames;
    private int currentVelocityFrameStep = 0;

    public InputActionReference shootButton;

    private bool canRelease;

    Vector3 _previousPosition;

    private void Start()
    {
        controllerRB = controllerWithRB.GetComponent<Rigidbody>();
        controllerCenterOfMass = controllerRB.centerOfMass;

        railWhipSphereRB = railWhipSphere.GetComponent<Rigidbody>();

        shootButton.action.started += OnShootPressed;
        shootButton.action.canceled += OnShootCanceled;

        _previousPosition = transform.position;
    }

    private void OnShootCanceled(InputAction.CallbackContext obj)
    {
        if (canRelease)
        {
            canRelease = false;
            ReleaseSphere();
        }
    }

    private void OnShootPressed(InputAction.CallbackContext obj)
    {
        Debug.Log("shoot");

        if (!canRelease) canRelease = true;
    }

    void ReleaseSphere()
    {
        if (railWhipSphere == null) return;
     
        ConfigurableJoint joint = railWhipSphere.GetComponent<ConfigurableJoint>();
        
        if (joint != null)
        {
            Destroy(joint);
        }

        Vector3 angularVelocity = controllerRB.angularVelocity;
        Vector3 objectPos = railWhipSphere.transform.localPosition;
        Vector3 controllerVelocityCross = Vector3.Cross(angularVelocity, objectPos - controllerCenterOfMass);

        // add our angular velocity and center of mass calculations to controller linear velocity
        Vector3 linearVelocity = controllerRB.velocity;
        Vector3 fullTossVelocity = linearVelocity + controllerVelocityCross;

        //apply this to our railwhip sphere
        railWhipSphereRB.velocity = fullTossVelocity;
        railWhipSphereRB.angularVelocity = angularVelocity;

        //come back to this code!!
        railWhipSphere = null; // if it hits something, null it!!!! otherwise, make it swing back after a certain amount of time
        //come back to this code!!

        AddVelocityHistory();
        ResetVelocityHistory();
    }

    private void FixedUpdate()
    {
        VelocityUpdate();
    }

    private void VelocityUpdate()
    {
        if (velocityFrames != null)
        {
            currentVelocityFrameStep++;

            if (currentVelocityFrameStep >= velocityFrames.Length)
            {
                currentVelocityFrameStep = 0;
            }

            // set velocity at current frame step to equal the crrent velocity and angular velocity of the object's rigidbody
            velocityFrames[currentVelocityFrameStep] = railWhipSphereRB.velocity;
            angularVelocityFrames[currentVelocityFrameStep] = railWhipSphereRB.angularVelocity;
        }
    }

    void AddVelocityHistory()
    {
        if (velocityFrames != null)
        {
            Vector3 velocityAverage = GetVectorAverage(velocityFrames);
            if (velocityAverage != null)
            {
                // if average isn't 0, apply to rigidbody
                railWhipSphereRB.velocity = velocityAverage;
            }
            Vector3 angularVelocityAverage = GetVectorAverage(angularVelocityFrames);
            if (angularVelocityAverage != null)
            {
                // if average isn't 0, apply to rigidbody
                railWhipSphereRB.angularVelocity = angularVelocityAverage;
            }
        }
    }

    Vector3 GetVectorAverage(Vector3[] vectors)
    {
        float
            x = 0f,
            y = 0f,
            z = 0f;

        int numVectors = 0;

        for (int i = 0; i < vectors.Length; i++)
        {
            if (vectors[i] != null)
            {
                x += vectors[i].x;
                y += vectors[i].y;
                z += vectors[i].z;

                numVectors++;
            }
        }

        if(numVectors > 0)
        {
            Vector3 average = new Vector3(x / numVectors, y / numVectors, z / numVectors);
            return average;
        }

        return Vector3.one;

    }

    void ResetVelocityHistory()
    {
        currentVelocityFrameStep = 0;

        if(velocityFrames != null && velocityFrames.Length > 0)
        {
            velocityFrames = new Vector3[velocityFrames.Length];
            angularVelocityFrames = new Vector3[velocityFrames.Length];
        }
    }
}
