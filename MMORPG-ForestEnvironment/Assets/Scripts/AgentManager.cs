using UnityEngine;

/// <summary>
/// Abstract class for scene managers
/// </summary>
public abstract class AgentManager : MonoBehaviour
{
    public GameObject agentPrefab;
    protected TerrainData terrainData;
    protected Vector3 minBounds;
    protected Vector3 maxBounds;

    // Use this for initialization
    protected virtual void Start()
    {
        terrainData = FindObjectOfType<Terrain>().terrainData;
        minBounds = Vector3.zero;
        maxBounds = terrainData.size;
    }
}
