using UnityEngine;
using System.Collections;
using System;

public class Follower : VehicleMovement
{

    Leader leader;

    protected override void Start()
    {
        base.Start();

    }

    protected override void CalcSteringForces()
    {
       // throw new NotImplementedException();
    }
}
