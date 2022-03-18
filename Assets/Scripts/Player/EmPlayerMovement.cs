using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//GREATLY REFERENCING THIS TUT SERIES: https://www.youtube.com/watch?v=TokAIw3IWos
public class EmPlayerMovement : MonoBehaviour
{
    [SerializeField] float playerHeight = 0.7f;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float movementMultiplier = 10f;
    [SerializeField] float airMultiplier = 0.4f; //less than 1 to reduce
    
    [Header("Drag")]
    public float groundDrag = 6f;
    public float airDrag = 2f;

    float horizontalMovement;
    float verticalMovement;

    [Header ("Ground Detection")]
    public LayerMask groundMask;
    [SerializeField] GameObject playerGroundingFoot;
    [SerializeField] float footSize = 0.4f;
    [Space]
    [SerializeField] private bool isGrounded;

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

    Rigidbody rb;

    RaycastHit slopeHit;
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up) //as long as the world is rightside up, this means it's not the ground see FPS controller tut #3
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(playerGroundingFoot.transform.position, footSize);
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(playerGroundingFoot.transform.position, footSize, groundMask);

        MyInput();
        ControlDrag();

        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private void ControlDrag()
    {
        if(isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    private void MyInput()
    {
        moveDirection = rb.velocity;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }
}
