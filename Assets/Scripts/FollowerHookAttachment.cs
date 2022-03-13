using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FollowerHookAttachment : MonoBehaviour
{
    public UnityEvent OnAttach;
    bool _isAttached;
    private void OnTriggerEnter(Collider other) //check if hand is grabbing too!!!! ADD THIS!!!!!!
    {
        Debug.Log("collision detected");
        // check if collision object is a hand
        if (other.gameObject.GetComponent<PlayerHandTag>() == null) return;

        // check if attachment is activated
        if (!_isAttached)
        {
            // invoke
            Debug.Log("attach");
            OnAttach.Invoke();
            _isAttached = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(_isAttached)
        {
            //_isAttached = false;
        }
    }
}
