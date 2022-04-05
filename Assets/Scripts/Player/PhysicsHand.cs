using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PhysicsHand : MonoBehaviour
{
    [Header("PID")]
    [SerializeField] float frequency = 50f;
    [SerializeField] float damping = 1f;
    [SerializeField] float rotFrequency = 100f;
    [SerializeField] float rotDamping = 0.9f;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] public ActionBasedController target; //target is the controller
    
    [Space]
    
    [Header("Springs")]
    [SerializeField] float climbForce = 1000f;
    [SerializeField] float climbDrag = 500f;

    [Space]

    [Header("Grabbing")]
    [SerializeField] private Transform grabber;
    [SerializeField] LayerMask grabbableLayer;
    [SerializeField] float distance = 5f;

    [Space]

    [Header("Zipline")]
    public bool _attachActivated;

    Vector3 _previousPosition;
    Rigidbody _rigidbody;
    bool _isColliding;
    private Collision _collision;

    [HideInInspector] public bool _isAttemptingGrab;
    GameObject _heldObject;
    Transform _grabPoint;
    private FixedJoint _joint1, _joint2;

    void Start()
    {
        //Initialization
        transform.position = target.transform.position;
        transform.rotation = target.transform.rotation;

        //Inputs Setup
        target.selectAction.action.started += Grab;
        target.selectAction.action.canceled += Release;

        //Setup
        _rigidbody = GetComponent<Rigidbody>();

        _rigidbody.maxAngularVelocity = float.PositiveInfinity;
         
        _previousPosition = transform.position;
    }

    private void OnDestroy()
    {
        target.selectAction.action.started -= Grab;
        target.selectAction.action.canceled -= Release;
    }

    void FixedUpdate()
    {
        PIDMovement();
        PIDRotation(); 
        // if (_isColliding) HookesLaw(); // make this if iscolliding or isattached
        
        DistanceCheck(); 
    }

    private void DistanceCheck()
    {
        if (Math.Abs(Vector3.Distance(target.transform.position, transform.position)) > distance)
        {
            transform.position = target.transform.position;
            transform.rotation = target.transform.rotation;
        }
    }

    void PIDMovement()
    {
        float kp = (6f * frequency) * (6f * frequency) * 0.25f;
        float kd = 4.5f * frequency * damping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        Vector3 force = (target.transform.position - transform.position) * ksg + (playerRigidbody.velocity - _rigidbody.velocity) * kdg;
        
        _rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    void PIDRotation()
    {
        float kp = (6f * rotFrequency) * (6f * rotFrequency) * 0.25f;
        float kd = 4.5f * rotFrequency * rotDamping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        Quaternion q = target.transform.rotation * Quaternion.Inverse(transform.rotation);
        if (q.w < 0)
        {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }
        q.ToAngleAxis(out float angle, out Vector3 axis);
        axis.Normalize();
        axis *= Mathf.Deg2Rad;
        Vector3 torque = ksg * axis * angle + -_rigidbody.angularVelocity * kdg;

        _rigidbody.AddTorque(torque, ForceMode.Acceleration);
    }

    void HookesLaw()
    {
        Vector3 displacementFromResting = transform.position - target.transform.position;
        Vector3 force = displacementFromResting * climbForce;

        float drag = GetDrag();

        playerRigidbody.AddForce(force, ForceMode.Acceleration);
        playerRigidbody.AddForce(drag * -playerRigidbody.velocity * climbDrag, ForceMode.Acceleration);
    }

    float GetDrag()
    {
        Vector3 handVelocity = (target.transform.localPosition - _previousPosition) / Time.fixedDeltaTime; //prevpos is from prev frame
        float drag = 1 / handVelocity.magnitude + 0.01f; //add .01 bc we don't want it to ever be 0
        drag = drag > 1 ? 1 : drag; //if drag is greater than 1 set to 1 otherwise it's just drag
        drag = drag < 0.03f ? 0.03f : drag;

        _previousPosition = transform.position;
        return drag;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isColliding = true;
        _collision = collision;
    }

    private void OnCollisionExit(Collision collision)
    {
        _isColliding = false;
        _collision = null;
    }

    private void Grab(InputAction.CallbackContext context)
    {
        _isAttemptingGrab = true;
        StartCoroutine(TryGrab());
    }

    IEnumerator TryGrab()
    {
        while(_isAttemptingGrab)
        {
            if (_collision != null && _collision.gameObject.TryGetComponent(out Rigidbody rb))
            {
                FixedJoint joint = _rigidbody.gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = rb;
                joint.breakForce = float.PositiveInfinity;
                joint.breakTorque = float.PositiveInfinity;
                joint.enableCollision = false;

                _attachActivated = true;

                _isAttemptingGrab = false;
            }
            yield return null;
        }
    }

    private void Release(InputAction.CallbackContext context)
    {
        _isAttemptingGrab = false;

        FixedJoint joint = GetComponent<FixedJoint>();
        if (joint != null)
        {
            _attachActivated = false;

            Destroy(joint);
        }
    }
}
