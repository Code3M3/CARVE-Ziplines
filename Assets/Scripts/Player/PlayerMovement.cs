using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]

// using this tutorial https://www.youtube.com/watch?v=ViQzKZvYdgE as a base
public class PlayerMovement : MonoBehaviour
{
    [Header("values")]
    [SerializeField] float moveForce = 2f;
    [SerializeField] float dragForce = 1f;
    [SerializeField] float steerForce = 2f;
    [SerializeField] float minForce;
    [SerializeField] float minTimeBetweenStrokes;

    public Vector3[] velocityFrames;
    public Vector3[] angularVelocityFrames;

    [Header("player references")]
    [SerializeField] InputActionReference leftControllerMoveRef;
    [SerializeField] InputActionReference leftControllerVelocity;
    [SerializeField] InputActionReference leftControllerPosition;
    [SerializeField] InputActionReference leftControllerRotationRef;

    [SerializeField] InputActionReference leftControllerAngularVelocity;
    [SerializeField] InputActionReference rightControllerMoveRef;
    [SerializeField] InputActionReference rightControllerVelocity;
    [SerializeField] Transform forwardRef;

    [Header("player rotation")]
    [SerializeField] UnicornController unicornController;

    Rigidbody _rb;
    float _cooldownTimer;

    Vector3 _steerStartPos; 

    private int currentVelocityFrameStep = 0;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        VelocityUpdate();

        _cooldownTimer += Time.fixedDeltaTime;
        if (_cooldownTimer > minTimeBetweenStrokes
            && leftControllerMoveRef.action.IsPressed())
        {
            AddVelocityHistory();
        }

        if (leftControllerRotationRef.action.IsPressed())
        {
            Vector3 angularVelocityAverage = GetVectorAverage(angularVelocityFrames);
            Vector3 velocityAverage = GetVectorAverage(velocityFrames);
            if (angularVelocityAverage != null)
            {
                Vector3 localAngVelocity = angularVelocityAverage; // total angular velocity
                int torqueDir;

                if (velocityAverage.x < 0)
                {
                    // right hand stuff
                    torqueDir = 1;
                }
                else
                {
                    //left hand stuff
                    torqueDir = -1;
                }
                // if average isn't 0, set new rotation in the unicorn controller
                Vector3 worldAngVelocity = forwardRef.TransformDirection(localAngVelocity);
                float angularMagnitude = worldAngVelocity.magnitude;
                float rotationForce = angularMagnitude * 25 * torqueDir;

                unicornController.RotateUnicorn(rotationForce);
            }
        }

        if (_rb.velocity.sqrMagnitude > 0.01f)
        {
            _rb.AddForce(-_rb.velocity * dragForce, ForceMode.Acceleration);
        }
    }

    void AddVelocityHistory()
    {
        Vector3 velocityAverage = GetVectorAverage(velocityFrames);
        if (velocityFrames != null)
        {
            if (velocityAverage != null)
            {

                var leftHandVelocity = velocityAverage;
                Vector3 localVelocity = leftHandVelocity; // total velocity
                localVelocity *= -1; // reverse direction

                if (localVelocity.sqrMagnitude > minForce * minForce)
                {
                    Vector3 worldVelocity = forwardRef.TransformDirection(localVelocity);
                    _rb.AddForce(worldVelocity * moveForce, ForceMode.Acceleration);

                    _cooldownTimer = 0f;
                }
            }
        }
        Vector3 angularVelocityAverage = GetVectorAverage(angularVelocityFrames);
        if (angularVelocityAverage != null)
        {
            Vector3 localAngVelocity = angularVelocityAverage; // total angular velocity
            int torqueDir;
            
            if (velocityAverage.x < 0)
                {
                    // right hand stuff
                    torqueDir = 1;
                }
            else
                {
                    //left hand stuff
                    torqueDir = -1;
                }
            // if average isn't 0, set new rotation in the unicorn controller
            Vector3 worldAngVelocity = forwardRef.TransformDirection(localAngVelocity);
            float angularMagnitude = worldAngVelocity.magnitude;
            float rotationForce = angularMagnitude * steerForce * torqueDir;

            unicornController.RotateUnicorn(rotationForce);

            Debug.Log("angular: "+angularVelocityAverage);
        }
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
            velocityFrames[currentVelocityFrameStep] = leftControllerVelocity.action.ReadValue<Vector3>();
            angularVelocityFrames[currentVelocityFrameStep] = leftControllerAngularVelocity.action.ReadValue<Vector3>();
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

        if (numVectors > 0)
        {
            Vector3 average = new Vector3(x / numVectors, y / numVectors, z / numVectors);
            return average;
        }

        return Vector3.one;

    }

    void ResetVelocityHistory()
    {
        currentVelocityFrameStep = 0;

        if (velocityFrames != null && velocityFrames.Length > 0)
        {
            velocityFrames = new Vector3[velocityFrames.Length];
            angularVelocityFrames = new Vector3[velocityFrames.Length];
        }
    }
}
