using UnityEngine;
using System.Collections;

/// <summary>
/// The helper class for the path
/// </summary>
public class PathVertex : MonoBehaviour
{
    //variables
    private PathVertex nextVertex;
    private Vector3 segment;
    private Vector3 unitVector;
    private static int count;
    private int index;
    private float radius = 1f;
    private float radiusSqr;
    private float magnitude;

    //properties
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

    //for initialization
    public void SetNextVertex(PathVertex nextPathVertex)
    {
        nextVertex = nextPathVertex;

        segment = nextVertex.Position - transform.position;
        segment.y = 0;
        transform.forward = segment.normalized;

        magnitude = segment.magnitude;
        unitVector = segment.normalized;
    }

    // more initialization
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
        radiusSqr = Mathf.Pow(radius, 2);
    }
}
