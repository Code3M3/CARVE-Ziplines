using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadFollowHands : MonoBehaviour
{
    [SerializeField] private GameObject _leftHand;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.rotation = _leftHand.transform.rotation;
    }
}
