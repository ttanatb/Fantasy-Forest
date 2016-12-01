using UnityEngine;
using System.Collections;
using System;

public class Leader : VehicleMovement {

    public float seekingWeight = 1;
    public float wanderingWeight = 1;
    public float obstacleWeight = 1;
    public float boundaryWeight = 1;

    private bool seeking = false;

    public bool Seeking
    {
        get { return seeking; }
    }

    protected override void Start()
    {
        base.Start();
        maxForce = 4f;
        maxSpeed = 2f;
    }

    protected override void CalcSteringForces()
    {
        CheckIfCloseToButterfly();
        if (seeking)
        {
            //seek closest butterfly
        }
        else totalForce += Wander() * wanderingWeight;
        //obstacle
        //boundary
        totalForce = Vector3.ClampMagnitude(totalForce, maxForce);
    }

    private void CheckIfCloseToButterfly()
    {
        //check for butterflies
    }
}
