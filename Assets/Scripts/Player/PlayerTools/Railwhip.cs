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
    public float shootForceMultiplier = 20f;
    public float shootDrag = 0.5f;
    private bool canRelease;
    private ConfigurableJoint configJoint;

    Vector3 _previousPosition;

    private void Start()
    {
        controllerRB = controllerWithRB.GetComponent<Rigidbody>();
        controllerCenterOfMass = controllerRB.centerOfMass;

        railWhipSphereRB = railWhipSphere.GetComponent<Rigidbody>();

        configJoint = railWhipSphere.GetComponent<ConfigurableJoint>();

        shootButton.action.started += OnShootPressed;
        shootButton.action.canceled += OnShootCanceled;

        _previousPosition = controllerWithRB.transform.position;
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

        if (configJoint.connectedBody == null)
        {
            configJoint.connectedBody = controllerRB;

            configJoint.angularXMotion = ConfigurableJointMotion.Limited;
            configJoint.angularYMotion = ConfigurableJointMotion.Limited;
            configJoint.angularZMotion = ConfigurableJointMotion.Limited;

            float AB = 3.402823e+38f;
            float CD = .9f;
            float DE = 30f;
            configJoint.xDrive = new JointDrive()
            {
                maximumForce = AB,
                positionDamper = CD,
                positionSpring = DE
            };
            configJoint.yDrive = new JointDrive()
            {
                maximumForce = AB,
                positionDamper = CD,
                positionSpring = DE
            };
            configJoint.zDrive = new JointDrive()
            {
                maximumForce = AB,
                positionDamper = CD,
                positionSpring = DE
            };
        }

        if (!canRelease) canRelease = true;
    }

    void ReleaseSphere()
    {
        if (railWhipSphere == null) return;

        if (configJoint.connectedBody != null)
        {
            configJoint.connectedBody = null;

            configJoint.angularXMotion = ConfigurableJointMotion.Free;
            configJoint.angularYMotion = ConfigurableJointMotion.Free;
            configJoint.angularZMotion = ConfigurableJointMotion.Free;

            float AB = 0f;
            float CD = 0f;
            float DE = 0f;
            configJoint.xDrive = new JointDrive()
            {
                maximumForce = AB,
                positionDamper = CD,
                positionSpring = DE
            };
            configJoint.yDrive = new JointDrive()
            {
                maximumForce = AB,
                positionDamper = CD,
                positionSpring = DE
            };
            configJoint.zDrive = new JointDrive()
            {
                maximumForce = AB,
                positionDamper = CD,
                positionSpring = DE
            };
        }

        Vector3 angularVelocity = controllerRB.angularVelocity;
        Vector3 objectPos = railWhipSphere.transform.localPosition;
        Vector3 controllerVelocityCross = Vector3.Cross(angularVelocity, objectPos - controllerCenterOfMass);

        // add our angular velocity and center of mass calculations to controller linear velocity
        Vector3 linearVelocity = controllerRB.velocity;
        Vector3 fullTossVelocity = linearVelocity + controllerVelocityCross;

        //apply this to our railwhip sphere with added acceleration (depending on strength of toss)
        float drag = GetDrag();

        railWhipSphereRB.velocity = -fullTossVelocity * shootForceMultiplier * drag;
        railWhipSphereRB.angularVelocity = -angularVelocity *shootForceMultiplier * drag;

        //come back to this code!!
        //railWhipSphere = null; // if it hits something, null it!!!! otherwise, make it swing back after a certain amount of time
        //come back to this code!! decide what to do for respawning and spawning spheres

        AddVelocityHistory();
        ResetVelocityHistory();
    }

    float GetDrag()
    {
        Vector3 handVelocity = (controllerWithRB.transform.localPosition - _previousPosition) / Time.fixedDeltaTime; //prevpos is from prev frame
        float drag = 1 / handVelocity.magnitude + 0.01f; //add .01 bc we don't want it to ever be 0
        drag = drag > 1 ? 1 : drag; //if drag is greater than 1 set to 1 otherwise it's just drag
        drag = drag < 0.03f ? 0.03f : drag;

        _previousPosition = controllerWithRB.transform.position;
        return drag;
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
