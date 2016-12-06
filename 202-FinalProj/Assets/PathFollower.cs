using UnityEngine;
using System.Collections;
using System;

public class PathFollower : VehicleMovement
{

    Path path;

    public float wanderingWeight = 1f;
    public float pathWeight = 1f;

    Vector3 pathSeek;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        path = GameObject.Find("Scene Manager").GetComponent<Path>();
        maxForce = 3f;
        maxSpeed = 2f;
        posToCenter = Vector3.zero;
    }


    protected override void CalcSteringForces()
    {
        totalForce += Wander() * wanderingWeight;
        pathSeek = path.SteerToClosestPath(farNextPos);
        if (pathSeek != Vector3.zero)
        {
            pathSeek.y = position.y;
            totalForce += Seek(pathSeek) * pathWeight;
            //Debug.DrawLine(pathSeek, nextPos);
        }

        Debug.DrawLine(position,farNextPos);
    }
}
