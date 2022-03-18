using UnityEngine;

public class MoveCamera : MonoBehaviour
{

    public Transform player;

    void FixedUpdate()
    {
        transform.position = player.transform.position;
    }
}