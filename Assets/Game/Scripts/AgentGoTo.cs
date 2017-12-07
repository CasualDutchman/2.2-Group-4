using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentGoTo : MonoBehaviour {

    public Transform target;

    NavMeshAgent agent;

	void Start () {
        agent = GetComponent<NavMeshAgent>();
        
	}
	
	void FixedUpdate () {
        //if(!agent.hasPath)
            agent.SetDestination(target.position);
    }
}
