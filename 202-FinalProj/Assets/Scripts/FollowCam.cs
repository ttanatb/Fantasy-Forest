using UnityEngine;
using System.Collections;

public class FollowCam : MonoBehaviour
{

    private GameObject target;

    float distance = 8.0f;
    float height = 1.8f;
    float heightDamping = 2.0f;
    float positionDamping = 1.0f;
    float rotationDamping = 0.8f;


    // Use this for initialization
    void Start()
    {
        target = GameObject.Find("Footman");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = Vector3.Lerp(transform.position, target.transform.position - target.transform.forward * distance, positionDamping * Time.deltaTime);
        pos.y = Mathf.Lerp(transform.position.y, target.transform.position.y + height, heightDamping * Time.deltaTime);
        transform.position = pos;
        transform.forward = Vector3.Lerp(transform.forward, target.transform.forward, Time.deltaTime * rotationDamping);
    }
}
