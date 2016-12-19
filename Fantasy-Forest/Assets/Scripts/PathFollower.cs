using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Path follower, footmen
/// </summary>
public class PathFollower : VehicleMovement
{
    //variables
    private enum State { Following, Pursuing, Returning };

    private State state;
    Path path;

    private Animator animator;

    public float wanderingWeight = 1f;
    public float obstacleWeight = 4f;
    public float pathWeight = 1f;

    public float spiderSearchRadius = 10f;
    private float spiderRadiusSqr;
    private GameObject[] spiders;
    private Obstacle[] trees;
    private VehicleMovement chasingTarget;
    private Vector3 distance;

    Vector3 pathSeek;
    Vector3 returnPos;


    // Use this for initialization
    protected override void Start()
    {
        path = GameObject.Find("Scene Manager").GetComponent<Path>();
        radius = 0.5f;
        maxForce = 3f;
        maxSpeed = 1.5f;
        savedMaxSpeed = maxSpeed;
        spiderRadiusSqr = Mathf.Pow(spiderSearchRadius, 2);
        spiders = GameObject.FindGameObjectsWithTag("Spider");
        trees = FindObjectsOfType<Obstacle>();
        freezeY = true;
        animator = GetComponent<Animator>();

        base.Start();
        returnPos = position;
        posToCenter = Vector3.zero;
    }

    /// <summary>
    /// Method to check if there are nearby spiders
    /// </summary>
    private void CheckNearbySpider()
    {
        foreach (GameObject spider in spiders)
        {
            //checks if spider exists
            if (spider)
            {
                distance = spider.transform.position - position;

                //checks if in front
                if (Vector3.Dot(distance, transform.forward) < 0)
                    break;

                distance.y = 0;

                //checks dist
                if (distance.sqrMagnitude < spiderRadiusSqr && distance.magnitude < spiderSearchRadius)
                {
                    state = State.Pursuing;
                    maxSpeed *= 2;
                    chasingTarget = spider.GetComponent<Leader>();
                    if (!chasingTarget)
                        chasingTarget = spider.GetComponent<Follower>();

                    returnPos = position;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Calculates how to steer
    /// </summary>
    protected override void CalcSteringForces()
    {
        //print(state);
        //based on each state
        switch (state)
        {
            case State.Following:
                //sets animation
                animator.SetBool("walk", true);

                //wander and seeks path
                totalForce += Wander() * wanderingWeight;

                pathSeek = path.SteerToClosestPath(farNextPos);
                if (pathSeek != Vector3.zero)
                {
                    pathSeek.y = position.y;
                    totalForce += Seek(pathSeek) * pathWeight;
                }

                //check spider
                CheckNearbySpider();
                break;

            case State.Pursuing:
                //checks if chasing target exists
                if (chasingTarget)
                {
                    //sets animation
                    animator.SetBool("walk", false);

                    //seeks target
                    totalForce += Seek(chasingTarget.transform.position);

                    //checks distacnce
                    distance = chasingTarget.transform.position - position;
                    distance.y = 0;

                    if (distance.sqrMagnitude > spiderRadiusSqr && distance.magnitude > spiderSearchRadius)
                    {
                        state = State.Returning;
                        maxSpeed /= 2f;
                        break;
                    }

                    //attacks if close enough
                    else if (chasingTarget && distance.sqrMagnitude < 1.7f)
                        animator.SetTrigger("attack");

                    //checks for closer spider
                    foreach (GameObject spider in spiders)
                    {
                        if (spider)
                        {
                            Vector3 comparedDist = spider.transform.position - position;
                            comparedDist.y = 0;

                            if (Vector3.Dot(distance, transform.forward) < 0)
                                break;

                            if (comparedDist.sqrMagnitude < distance.sqrMagnitude && comparedDist.magnitude < distance.magnitude)
                            {
                                chasingTarget = spider.GetComponent<Leader>();
                                if (!chasingTarget)
                                    chasingTarget = spider.GetComponent<Follower>();
                            }
                        }
                    }
                }
                else
                {
                    //returns to last point on the path
                    state = State.Returning;
                    maxSpeed /= 2f;
                    break;
                }
                break;

            case State.Returning:
                //sets animation
                animator.SetBool("walk", true);

                //seeks last point on path
                totalForce += Seek(returnPos);

                //avoids tree
                foreach (Obstacle obs in trees)
                {
                    totalForce += AvoidObstacle(obs) * obstacleWeight;
                }

                // checks if close enough to path
                if ((returnPos - position).sqrMagnitude < 5)
                {
                    state = State.Following;
                    break;
                }
                else CheckNearbySpider();
                break;
        }
    }
}
