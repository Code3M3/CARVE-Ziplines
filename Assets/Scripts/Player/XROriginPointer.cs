using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XROriginPointer : MonoBehaviour
{
    [SerializeField] public GameObject splineFollower; //one spline follower per player
    [SerializeField] public GameObject XROriginRotator;

    float smooth = 100f;

    Vector3 _previousPosition;

    // Start is called before the first frame update
    void Start()
    {
        _previousPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetPostition = new Vector3(splineFollower.transform.position.x,
                                       this.transform.position.y,
                                       splineFollower.transform.position.z);

        //XROriginRotator.transform.LookAt(targetPostition - transform.localPosition);

        var lookRot = Quaternion.LookRotation((targetPostition - transform.position) + XROriginRotator.transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * smooth);
    }
}
