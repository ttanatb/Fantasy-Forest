using UnityEngine;

/// <summary>
/// Leader, spider queens
/// </summary>
public class Leader : VehicleMovement
{
    //variables
    enum State { Wander, Pause, Flee };

    public float seekingWeight = 1;
    public float wanderingWeight = 1;
    public float obstacleWeight = 1;
    public float boundaryWeight = 1;
    public float fleeingWeight = 1;

    private Vector3 followingPos;
    private TerrainData terrainData;

    public float minPauseTimer = 2;
    public float maxPauseTimer = 4;

    private float timer;

    public float minTimeToPause = 6;
    public float maxTimeToPause = 10;

    private State state = State.Wander;
    private Obstacle[] trees;
    private Follower[] followers;
    private GameObject[] humans;

    public float fleeRadius;
    private float fleeRadiusSqr;

    private Animator animator;
    private float savedAnimSpeed;
    RigidbodyConstraints constraints;

    //properties
    public bool Fleeing
    {
        get
        {
            if (state == State.Flee) return true;
            else return false;
        }
    }

    public Vector3 FollowingPos
    {
        get { return followingPos; }
    }

    public Follower[] Followers
    {
        get { return followers; }
    }

    public Vector3 Position
    {
        get { return position - posToCenter; }
    }

    //initialization
    protected override void Start()
    {
        SetBounds(Vector3.zero, FindObjectOfType<Terrain>().terrainData.size);
        maxForce = 5f;
        maxSpeed = 2.75f;
        radius = 1.4f;
        timer = Random.Range(minTimeToPause, maxTimeToPause);
        animator = GetComponent<Animator>();
        savedAnimSpeed = 1.5f;
        animator.speed = savedAnimSpeed;

        trees = FindObjectsOfType<Obstacle>();
        terrainData = GameObject.Find("Terrain").GetComponent<Terrain>().terrainData;
        SetBounds(Vector3.zero, terrainData.size);
        followers = FindObjectsOfType<Follower>();
        humans = GameObject.FindGameObjectsWithTag("Human");
        fleeRadiusSqr = Mathf.Pow(fleeRadius, 2);
        constraints = GetComponent<Rigidbody>().constraints;

        base.Start();
    }

    protected override void CalcSteringForces()
    {
        //locks pos
        position.y = 0;
        animator.speed = maxSpeed * 0.7f;


        switch (state)
        {
            //wandering
            case State.Wander:
                totalForce += Wander() * wanderingWeight;

                //timer to pause
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    state = State.Pause;
                    timer = Random.Range(minPauseTimer, maxPauseTimer);
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    animator.SetBool("Walk", false);
                }

                CheckIfCloseToHuman();
                goto default;

            //pausing
            case State.Pause:
                
                //timer to walk again
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    GetComponent<Rigidbody>().constraints = constraints;
                    state = State.Wander;
                    timer = Random.Range(minTimeToPause, maxTimeToPause);
                    animator.SetBool("Walk", true);
                }

                CheckIfCloseToHuman();
                break;

            //increased speed to flee from humans
            case State.Flee:
                //calculates closest human to flee from
                int index = -1;
                float closestDist = float.MaxValue;
                for (int i = 0; i < humans.Length; i++)
                {
                    Vector3 dist = humans[i].transform.position - nextPos;
                    dist.y = 0;

                    if (dist.sqrMagnitude < fleeRadiusSqr * 2.25f  && dist.sqrMagnitude < Mathf.Pow(closestDist, 2))
                    {
                        float distMag = dist.magnitude;
                        if (distMag < fleeRadius * 1.5f && distMag < closestDist)
                        {
                            closestDist = distMag;
                            index = i;
                        }
                    }
                }
                if (index != -1)
                    totalForce += Flee(humans[index].transform.position) * fleeingWeight;
                else
                {
                    //return back to wandering
                    maxSpeed = savedMaxSpeed;
                    animator.speed = savedAnimSpeed;
                    state = State.Wander;
                }
                goto default;

            default:
                //avoids trees
                foreach (Obstacle obs in trees)
                {
                    totalForce += AvoidObstacle(obs) * obstacleWeight;
                }

                //steers towards the center
                totalForce += SteerInwards() * boundaryWeight;
                UpdatePosBehind();

                break;
        }
    }

    /// <summary>
    /// Checks if there are humans nearby
    /// </summary>
    private void CheckIfCloseToHuman()
    {
        foreach (GameObject footmen in humans)
        {
            Vector3 dist = footmen.transform.position - nextPos;
            dist.y = 0;
            if (dist.sqrMagnitude < fleeRadiusSqr && dist.magnitude < fleeRadius)
            {
                state = State.Flee;
                maxSpeed = savedMaxSpeed * 1.5f;
                animator.speed = savedAnimSpeed * 1.5f;
                return;
            }
        }
    }

    /// <summary>
    /// Updates the pos behind leader
    /// </summary>
    private void UpdatePosBehind()
    {
        followingPos = Vector3.Lerp(followingPos, position - transform.forward * 2f, Time.deltaTime * 5);
        //followingPos.y = terrainData.GetHeight((int)(followingPos.x * terrainData.heightmapResolution / terrainData.size.x),
        //    (int)(followingPos.z * terrainData.heightmapResolution / terrainData.size.z));
        followingPos.y = 0;
    }

    public void Die()
    {
        animator.SetTrigger("Die");
        //GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Destroy(GetComponent<Collider>());
        Destroy(this);
    }

    /*
    protected override void OnRenderObject()
    {
        base.OnRenderObject();
        GL.Begin(GL.LINES);
        for (int i = 0; i < 100; i++)
        {
            float a = i / (float)100;
            float angle = a * Mathf.PI * 2;
            GL.Vertex(followingPos);
            GL.Vertex(followingPos + new Vector3(Mathf.Cos(angle) * 1, 0, Mathf.Sin(angle) * 1));
        }
        GL.End();
    }
    */
}
