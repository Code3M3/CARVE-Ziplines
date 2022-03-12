using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FollowerHookAttachment : MonoBehaviour
{
    public UnityEvent OnAttach;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collision detected");
        // check if collision object is a hand
        if (other.gameObject.GetComponent<PlayerHandTag>() == null) return;

        // check if attachment is activated

        // invoke
        Debug.Log("attach");
        OnAttach.Invoke();
    }
}
