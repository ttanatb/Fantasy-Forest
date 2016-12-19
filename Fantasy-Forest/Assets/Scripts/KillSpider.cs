using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillSpider : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Spider") {
            if (other)
            {
                if (other.gameObject.GetComponent<Follower>())
                    other.gameObject.GetComponent<Follower>().Die();
                else if (other.gameObject.GetComponent<Leader>())
                    other.gameObject.GetComponent<Leader>().Die();
            }
        }
    }


}
