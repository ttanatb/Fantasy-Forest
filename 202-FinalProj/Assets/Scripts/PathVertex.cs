using UnityEngine;
using System.Collections;

public class PathVertex : MonoBehaviour
{
    private PathVertex nextVertex;
    private Vector3 segment;
    private Vector3 unitVector;
    private static int count;
    private int index;
    private float radius = 1f;
    private float radiusSqr;
    private float magnitude;

    public Vector3 Position { get { return transform.position; } }
    public Vector3 Segment { get { return segment; } }
    public Vector3 UnitVector { get { return unitVector; } }
    public float Radius { get { return radius; } }
    public float RadiusSqr { get { return radiusSqr; } }
    public float Magnitude { get { return magnitude; } }

    // Use this for initialization
    void Awake()
    {
        count++;
        index = count;

        gameObject.name = "Path " + index;
    }

    public void SetNextVertex(PathVertex nextPathVertex)
    {
        nextVertex = nextPathVertex;

        segment = nextVertex.Position - transform.position;
        segment.y = 0;
        transform.forward = segment.normalized;

        magnitude = segment.magnitude;
        unitVector = segment.normalized;

    }

    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
        radiusSqr = Mathf.Pow(radius, 2);
    }

    void Update()
    {
        //Debug.DrawLine(transform.position, transform.right * radius + transform.position);
    }
}
