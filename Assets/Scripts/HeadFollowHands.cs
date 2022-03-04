using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadFollowHands : MonoBehaviour
{
    [SerializeField] private GameObject _leftHand;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3 (_leftHand.transform.position.x,
            _leftHand.transform.position.y - 1.0f,
            _leftHand.transform.position.z);
    }
}
