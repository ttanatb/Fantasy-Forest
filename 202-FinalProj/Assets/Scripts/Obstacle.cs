using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour
{
    private float radius;
    private Vector3 position;

    public float Radius
    {
        get { return radius; }
    }

    public Vector3 Pos
    {
        get { return position; }
    }

    // Use this for initialization
    void Start()
    {
        position = GetComponent<Transform>().TransformPoint(GetComponent<CapsuleCollider>().center);
        radius = GetComponent<CapsuleCollider>().radius;
    }
}
