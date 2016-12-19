using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the flocking agents
/// </summary>
public class FlockerManager : AgentManager
{
    //the flock
    public int flockCount;
    private Flocker[] flock;
    private GameObject[] goals;
    private Obstacle[] obstacles;
    private GameObject[] spiders;

    //flocking variables
    private Vector3 avgAlignment;
    private Vector3 avgPosition;
    private Vector3 currentGoal;

    private float timer = 3;

    //for debugging
    public Material debugMaterial;

    //properties
    public Vector3 AveragePosition
    {
        get { return avgPosition; }
    }

    public Vector3 AverageAlignment
    {
        get { return avgAlignment; }
    }

    public Flocker[] Flockers
    {
        get { return flock; }
    }

    public Vector3 CurrentGoal
    {
        get { return currentGoal; }
    }

    public Obstacle[] Obstacles
    {
        get { return obstacles; }
    }

    public GameObject[] Spiders
    {
        get { return spiders; }
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        obstacles = FindObjectsOfType<Obstacle>();
        spiders = GameObject.FindGameObjectsWithTag("Spider");
        goals = GameObject.FindGameObjectsWithTag("Flower");
        currentGoal = goals[Random.Range(0, goals.Length)].transform.position;

        //positions the flockers
        flock = new Flocker[flockCount];
        for (int i = 0; i < flockCount; i++)
        {
            flock[i] = (Instantiate(agentPrefab, new Vector3(Random.Range(0, maxBounds.x), i,Random.Range(0, maxBounds.z)), Quaternion.identity)).GetComponent<Flocker>();
            flock[i].Initialize(this, minBounds, maxBounds, terrainData);
        }

        //calculates variables
        CalcAverageAlignment();
        CalcAveragePos();
    }

    // Update is called once per frame
    void Update()
    {
        CalcAverageAlignment();
        CalcAveragePos();

        if (timer < 0)
        {
            currentGoal = goals[Random.Range(0, goals.Length)].transform.position;
            timer = 3f;
        }

        Vector3 dist = avgPosition - currentGoal;
        dist.y = 0;
        if (dist.sqrMagnitude < 5)
        {
            timer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Calculates the average alignment
    /// </summary>
    private void CalcAverageAlignment()
    {
        avgAlignment = Vector3.zero;
        foreach (Flocker flocker in flock)
        {
            if (flocker.Flocking)
                avgAlignment += flocker.transform.forward;
        }

        avgAlignment = avgAlignment.normalized;
    }

    /// <summary>
    /// Calculates the average position
    /// </summary>
    private void CalcAveragePos()
    {
        int count = 0;
        avgPosition = Vector3.zero;
        foreach (Flocker flocker in flock)
        {
            if (flocker.Flocking)
            {
                count++;
                avgPosition += flocker.transform.position;
            }
        }
        if (count > 0)
            avgPosition /= flock.Length;
    }

    /*
    /// <summary>
    /// Draws the average position, average alignment, and goal
    /// </summary>
    void OnRenderObject()
    {
        //draws the alignment
        debugMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Vertex(avgPosition);
        GL.Vertex(avgPosition + avgAlignment * 2);
        GL.End();

        //draws a circle around position
        GL.Begin(GL.LINES);
        for (int i = 0; i < 100; ++i)
        {
            float a = i / (float)100;
            float angle = a * Mathf.PI * 2;
            GL.Vertex(avgPosition);
            GL.Vertex(avgPosition + new Vector3(Mathf.Cos(angle) * 0.2f, 0, Mathf.Sin(angle) * 0.2f));
        }
        GL.End();
    }
    */
}
