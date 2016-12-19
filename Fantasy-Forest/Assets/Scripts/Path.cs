using UnityEngine;
using System.Collections;

/// <summary>
/// The set of vertices that make up a path, manages path followers
/// </summary>
public class Path : MonoBehaviour
{
    //variables
    public PathVertex[] vertices;
    private Vector3 avgAlignment;

    // Use this for initialization
    void Awake()
    {
        //sets up the vertices
        for(int i = 0; i < vertices.Length - 1; i++)
        {
            vertices[i].SetNextVertex(vertices[i+1]);
        }

        vertices[vertices.Length - 1].SetNextVertex(vertices[0]);
    }

    /*
    // Update is called once per frame
    void Update()
    {
        //testing
        foreach (PathVertex v in vertices)
            Debug.DrawLine(v.Position, v.Segment + v.Position);
    }
    */

    /// <summary>
    /// Steers path follower to closest point in the path
    /// </summary>
    public Vector3 SteerToClosestPath(Vector3 futurePosition)
    {
        //max distance
        float dist = float.MaxValue;
        Vector3 posToSeek = Vector3.zero;

        foreach (PathVertex vertex in vertices)
        {
                //Debug.DrawLine(vertex.Position, vertex.Position + vertex.Segment);

            //gets the hypothenuse
            Vector3 hypothenuse = futurePosition - vertex.Position;
            hypothenuse.y = 0;

                //if (vertex.gameObject.name == "Path 1")
                //{

                //    Debug.DrawLine(vertex.Position, vertex.Position + vertex.Segment);
                //    Debug.DrawLine(vertex.Position, vertex.Position + hypothenuse);
                //    print(vertex.gameObject.name + ": " + hypothenuse + " " + vertex.Segment);
                //    print(vertex.gameObject.name + ": " + vertex.Position + " " + vertex.NextPosition);
                //}

            //projects it
            float projection = Vector3.Dot(vertex.Segment, hypothenuse) / vertex.Magnitude;

                //if (vertex.gameObject.name == "Path 1")
                //    print(vertex.gameObject.name + ": " + projection + " " + vertex.Magnitude);

            //check if behind or beyond a vertex (has a little buffer)
            if (projection > vertex.Magnitude + 1 || projection < -1)
                continue;

                 //Debug.DrawLine(vertex.Position, hypothenuse + vertex.Position);

            //draws distance to projected point
            Vector3 distVect = vertex.UnitVector * projection + vertex.Position - futurePosition;
            distVect.y = 0;

                //Debug.DrawLine(vertex.Position, vertex.UnitVector * projection + vertex.Position);
                //Debug.DrawLine(vertex.UnitVector * projection + vertex.Position, futurePosition);

            //checks if inside radius of path
            if (distVect.sqrMagnitude < vertex.RadiusSqr)
                continue;

            //calculates real magnitude
            float distMag = distVect.magnitude;
            
            //checks if inside radius, less than threshold and less than min distance
            if (distMag > vertex.Radius && distMag < 20 && distMag < dist)
            {
                //updates variables accordingly
                dist = distMag;
                posToSeek = vertex.UnitVector * projection + vertex.Position;
            }
        }

        //if (posToSeek != Vector3.zero)
        //    Debug.DrawLine(futurePosition, posToSeek);

        return posToSeek;
    }
}
