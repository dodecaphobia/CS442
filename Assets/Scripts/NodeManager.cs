using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour {
	public List<Node> nodes;
	private int[] numKnow = null;

	// Update is called once per frame
	void Update () {
		if(numKnow != null) {
			Array.Clear(numKnow, 0, numKnow.Length);
			List<AIData> foundAgents = new List<AIData>(FindObjectsOfType<AIData>());
			foreach (AIData a in foundAgents) {
				bool[] nodesKnown = new bool[numKnow.Length];
				foreach (Edge e in a.edges) {
					if(!nodesKnown[e.n1]) {
						nodesKnown[e.n1] = true;
						numKnow[e.n1]++;
					}
					if(!nodesKnown[e.n2]) {
						nodesKnown[e.n2] = true;
						numKnow[e.n2]++;
					}
				}
			}

			foreach (Node n in nodes) {
				n.changeColor(((float) numKnow[n.id]) / ((float) nodes.Count));
			}
		}
	}

	public void done(List<Node> n) {
		nodes = n;
		numKnow = new int[nodes.Count];
	}
}
