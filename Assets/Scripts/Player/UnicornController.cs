using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnicornController : MonoBehaviour
{
    [Header("main physics")]
    [SerializeField] public Rigidbody sphere;
    [SerializeField] public float gravity = 10f;

    [Header("steering")]
    // NOTE: make sure scales are all zeroed out (1,1,1) to prevent mesh deformation
    [SerializeField] float rotationDrag = 50f;

    Rigidbody _rb;

    // Start is called before the first frame update
    void Start()
    {
        // setup
        _rb = GetComponent<Rigidbody>();
        _rb.maxAngularVelocity = float.PositiveInfinity;
    }

    private void FixedUpdate()
    {
        // gravity
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        // steering drag
        if (_rb.angularVelocity.sqrMagnitude > 0.01f)
        {
            _rb.AddRelativeTorque(-_rb.angularVelocity * rotationDrag, ForceMode.Acceleration);
        }
    }

    public void RotateUnicorn(float rotationForce)
    {
        // steering
        _rb.AddRelativeTorque(Vector3.up * rotationForce, ForceMode.Acceleration);
    }
}
