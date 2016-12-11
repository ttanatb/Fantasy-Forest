using UnityEngine;
using System.Collections;
using System;

public class PathFollower : VehicleMovement
{
    private enum State { Following, Pursuing, Returning };

    private State state;
    Path path;

    private Animator animator;

    public float wanderingWeight = 1f;
    public float pathWeight = 1f;

    public float spiderRadius = 15f;
    private float spiderRadiusSqr;
    private GameObject[] spiders;
    private VehicleMovement chasingTarget;
    private Vector3 distance;

    Vector3 pathSeek;
    Vector3 returnPos;


    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        path = GameObject.Find("Scene Manager").GetComponent<Path>();
        maxForce = 3f;
        maxSpeed = 2f;
        posToCenter = Vector3.zero;
        returnPos = position;
        spiderRadiusSqr = Mathf.Pow(spiderRadius, 2);
        spiders = GameObject.FindGameObjectsWithTag("Spider");
        freezeY = true;
        animator = GetComponent<Animator>();
    }


    protected override void CalcSteringForces()
    {

        switch (state)
        {
            case State.Following:
                totalForce += Wander() * wanderingWeight;
                pathSeek = path.SteerToClosestPath(farNextPos);
                if (pathSeek != Vector3.zero)
                {
                    pathSeek.y = position.y;
                    totalForce += Seek(pathSeek) * pathWeight;
                }

                foreach (GameObject spider in spiders)
                {
                    if (spider)
                    {


                        distance = spider.transform.position - position;

                        if (Vector3.Dot(distance, transform.forward) < 0)
                            break;

                        distance.y = 0;
                        if (distance.sqrMagnitude < spiderRadiusSqr && distance.magnitude < spiderRadius)
                        {
                            state = State.Pursuing;
                            chasingTarget = spider.GetComponent<Leader>();
                            if (!chasingTarget)
                                chasingTarget = spider.GetComponent<Follower>();

                            returnPos = position;
                            print("ASDFASDF");
                        }
                    }
                }
                break;

            case State.Pursuing:
                if (chasingTarget)
                {
                    totalForce += Seek(chasingTarget.transform.position);

                    distance = chasingTarget.transform.position - position;
                    distance.y = 0;

                    if (distance.sqrMagnitude > spiderRadiusSqr && distance.magnitude > spiderRadius)
                    {
                        state = State.Returning;
                    }

                    else if (chasingTarget && distance.sqrMagnitude < 1.7f)
                        animator.SetTrigger("attack");

                    foreach(GameObject spider in spiders)
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
                    foreach (GameObject spider in spiders)
                    {
                        if (spider)
                        {
                            distance = spider.transform.position - position;
                            distance.y = 0;

                            if (Vector3.Dot(distance, transform.forward) < 0)
                                break;

                            if (distance.sqrMagnitude < spiderRadiusSqr && distance.magnitude < spiderRadius)
                            {
                                state = State.Pursuing;
                                chasingTarget = spider.GetComponent<Leader>();
                                if (!chasingTarget)
                                    chasingTarget = spider.GetComponent<Follower>();
                                return;
                            }
                        }
                    }
                    state = State.Returning;
                }
                break;

            case State.Returning:
                totalForce += Seek(returnPos);
                if ((returnPos - position).sqrMagnitude < 5)
                {
                    state = State.Following;
                }
                break;
        }



    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Spider")
        {
            Destroy(collision.gameObject);
            spiders = GameObject.FindGameObjectsWithTag("Spider");
        }
    }
}
