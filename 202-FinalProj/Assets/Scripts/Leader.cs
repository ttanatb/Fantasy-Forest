using UnityEngine;
using System.Collections;

public class Leader : VehicleMovement
{
    enum State { Wander, Pause, Seek, Flee };

    public float seekingWeight = 1;
    public float wanderingWeight = 1;
    public float obstacleWeight = 1;
    public float boundaryWeight = 1;

    private Vector3 followingPos;
    private TerrainData terrainData;

    public float minPauseTimer = 2;
    public float maxPauseTimer = 4;

    private float timer;

    public float minTimeToPause = 6;
    public float maxTimeToPause = 10;

    private State state = State.Wander;
    private Flocker[] butterflies;
    private Obstacle[] trees;
    private Follower[] followers;

    private Animator animator;

    public bool Seeking
    {
        get
        {
            if (state == State.Seek) return true;
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

    protected override void Start()
    {
        base.Start();
        SetBounds(Vector3.zero, FindObjectOfType<Terrain>().terrainData.size);
        maxForce = 4f;
        maxSpeed = 2f;
        radius = 1.4f;
        timer = Random.Range(minTimeToPause, maxTimeToPause);
        animator = GetComponent<Animator>();

        butterflies = FindObjectsOfType<Flocker>();
        trees = FindObjectsOfType<Obstacle>();
        terrainData = FindObjectOfType<Terrain>().terrainData;
        followers = FindObjectsOfType<Follower>();
    }

    protected override void CalcSteringForces()
    {
        switch (state)
        {
            case State.Wander:

                totalForce += Wander() * wanderingWeight;
                foreach(Obstacle obs in trees)
                {
                    totalForce += AvoidObstacle(obs) * obstacleWeight;
                }

                totalForce += SteerInBounds(minBounds, maxBounds) * boundaryWeight;
                UpdatePosBehind();

                if (timer < 0)
                {
                    state = State.Pause;
                    timer = Random.Range(minPauseTimer, maxPauseTimer);
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    animator.SetBool("Walk", false);
                }
                break;

            case State.Pause:
                if (timer < 0)
                {
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    state = State.Wander;
                    timer = Random.Range(minTimeToPause, maxTimeToPause);
                    animator.SetBool("Walk", true);
                }
                break;
            case State.Seek:
                break;
            case State.Flee:
                break;
            default: break;
        }

        timer -= Time.deltaTime;
        totalForce = Vector3.ClampMagnitude(totalForce, maxForce);
    }

    private void CheckIfCloseToButterfly()
    {
        //check for butterflies
    }

    private void UpdatePosBehind()
    {
        followingPos = position - transform.forward * 2f;
        followingPos.y = terrainData.GetHeight((int)(followingPos.x * terrainData.heightmapResolution / terrainData.size.x),
            (int)(followingPos.z * terrainData.heightmapResolution / terrainData.size.z));
    }

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
}
