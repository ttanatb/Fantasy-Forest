using UnityEngine;
using System.Collections;

public class Path : MonoBehaviour
{
    public PathVertex[] vertices;

    // Use this for initialization
    void Awake()
    {
        for(int i = 0; i < vertices.Length - 1; i++)
        {
            vertices[i].SetNextVertex(vertices[i+1]);
        }

        vertices[vertices.Length - 1].SetNextVertex(vertices[0]);
    }

    // Update is called once per frame
    void Update()
    {
        //foreach (PathVertex v in vertices)
        //Debug.DrawLine(v.Position, v.Segment + v.Position);
    }

    public Vector3 SteerToClosestPath(Vector3 futurePosition)
    {
        float dist = float.MaxValue;
        Vector3 posToSeek = Vector3.zero;

        foreach (PathVertex vertex in vertices)
        {
            //Debug.DrawLine(vertex.Position, vertex.Position + vertex.Segment);

            Vector3 hypothenuse = futurePosition - vertex.Position;
            hypothenuse.y = 0;

            //if (vertex.gameObject.name == "Path 1")
            //{

            //    Debug.DrawLine(vertex.Position, vertex.Position + vertex.Segment);
            //    Debug.DrawLine(vertex.Position, vertex.Position + hypothenuse);
            //    print(vertex.gameObject.name + ": " + hypothenuse + " " + vertex.Segment);
            //    print(vertex.gameObject.name + ": " + vertex.Position + " " + vertex.NextPosition);
            //}

            float projection = Vector3.Dot(vertex.Segment, hypothenuse) / vertex.Magnitude;

            //if (vertex.gameObject.name == "Path 1")
            //    print(vertex.gameObject.name + ": " + projection + " " + vertex.Magnitude);

            if (projection > vertex.Magnitude || projection < 0)
                continue;
            //Debug.DrawLine(vertex.Position, hypothenuse + vertex.Position);

            Vector3 distVect = vertex.UnitVector * projection + vertex.Position - futurePosition;
            distVect.y = 0;

            //Debug.DrawLine(vertex.Position, vertex.UnitVector * projection + vertex.Position);
            //Debug.DrawLine(vertex.UnitVector * projection + vertex.Position, futurePosition);


            if (distVect.sqrMagnitude < vertex.RadiusSqr)
                continue;

            float distMag = distVect.magnitude;
            if (distMag > vertex.Radius && distMag < 20 && distMag < dist)
            {
                
                dist = distMag;
                posToSeek = vertex.UnitVector * projection + vertex.Position;
            }
        }

        if (posToSeek != Vector3.zero)
            Debug.DrawLine(futurePosition, posToSeek);

        return posToSeek;
    }
}
