using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// attach to each physics hand game object
public class HandJet : MonoBehaviour
{
    public InputActionReference jetButton;
    [SerializeField] Rigidbody _playerRigidbody;
    [SerializeField] public float jetForce = 20f;

    private bool _jetsActive;

    // Start is called before the first frame update
    void Start()
    {
        jetButton.action.started += jetButtonPressed;
        jetButton.action.canceled += jetButtonCancelled;
    }

    private void jetButtonCancelled(InputAction.CallbackContext obj)
    {
        _jetsActive = false;
    }

    private void jetButtonPressed(InputAction.CallbackContext obj)
    {
        _jetsActive = true;
    }

    void FixedUpdate()
    {
        if (_jetsActive) 
            _playerRigidbody.AddForce(gameObject.transform.forward * jetForce * Time.deltaTime, ForceMode.Impulse);
    }
}
