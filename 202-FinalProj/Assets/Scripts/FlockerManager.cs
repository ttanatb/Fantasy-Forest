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

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        obstacles = FindObjectsOfType<Obstacle>();

        //positions the flockers
        flock = new Flocker[flockCount];
        for (int i = 0; i < flockCount; i++)
        {
            flock[i] = ((GameObject)Instantiate(agentPrefab, Vector3.one * i, Quaternion.identity)).GetComponent<Flocker>();
            flock[i].Initialize(this, minBounds, maxBounds, terrainData);
        }

        //random goal generation, REPLACE
        goals = GameObject.FindGameObjectsWithTag("Flower");


        //calculates variables
        CalcAverageAlignment();
        CalcAveragePos();
        currentGoal = goals[Random.Range(0,goals.Length)].transform.position;
        
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
        else if ((avgPosition - currentGoal).sqrMagnitude < 5)
        {
            timer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Calculates the average alignment
    /// </summary>
    private void CalcAverageAlignment()
    {
        //normalizes all the forwards

        avgAlignment = Vector3.zero;
        foreach (Flocker flocker in flock)
        {
            avgAlignment += flocker.transform.forward;
        }

        avgAlignment = avgAlignment.normalized;
    }

    /// <summary>
    /// Calculates the average position
    /// </summary>
    private void CalcAveragePos()
    {
        //averages all the position

        avgPosition = Vector3.zero;
        foreach (Flocker flocker in flock)
        {
            avgPosition += flocker.transform.position;
        }

        avgPosition /= flock.Length;
    }


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
}
