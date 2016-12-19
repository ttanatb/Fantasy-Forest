using UnityEngine;

/// <summary>
/// Helper class for obstacles
/// </summary>
public class Obstacle : MonoBehaviour
{
    private float radius;
    private Vector3 position;

    public float Radius
    {
        get { return radius; }
    }

    public Vector3 Pos
    {
        get { return position; }
    }

    // Use this for initialization
    void Start()
    {
        position = GetComponent<Transform>().TransformPoint(GetComponent<CapsuleCollider>().center);
        radius = GetComponent<CapsuleCollider>().radius + .4f;
    }

    //private void Update()
    //{
    //    for (int i = 0; i < 100; i++)
    //    {
    //        float a = i / (float)100;
    //        float angle = a * Mathf.PI * 2;
    //        Debug.DrawLine(Pos, Pos + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
    //    }
    //}
}
