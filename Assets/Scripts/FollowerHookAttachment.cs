using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FollowerHookAttachment : MonoBehaviour
{
    public UnityEvent OnAttach;
    [SerializeField] float detectionSize = 0.44f;

    bool _isAttached;
    PhysicsHand grabbingHand = null;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionSize);
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
            }
        }
        else
        {
            if (grabbingHand == null) return;

            if (_isAttached && !grabbingHand._attachActivated)
            {
                _isAttached = false;
            }
        }
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
