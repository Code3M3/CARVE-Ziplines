using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineConnector : MonoBehaviour
{
    public GameObject[] objs;

    private LineRenderer line;

    private void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
    }

    private void Update()
    {
        for(int i = 0; i < objs.Length; i++){
            line.SetPosition(i, objs[i].transform.position);
        }
    }
}
