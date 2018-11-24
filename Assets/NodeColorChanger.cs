using System.Collections.Generic;
using UnityEngine;

public class NodeColorChanger : MonoBehaviour {

    public Color lerpedColor1 = Color.red;
    public Color lerpedColor2 = Color.green;
    
    // public GameObject sphereNode;
    private List<AIcontroller> foundAgents;
    private Node loc = null;

    public void setNode(Node n)
    {
        loc = n;
    }

	// Update is called once per frame
	void Update () {
        if (loc != null)
        {
            foundAgents = new List<AIcontroller>(FindObjectsOfType<AIcontroller>());
            int agentsWhoKnow = 0;

            foreach (AIcontroller agent in foundAgents)
            {
                foreach (Edge edge in agent.getEdges())
                {
                    if (loc.id == edge.n1 || loc.id == edge.n2)
                    {
                        agentsWhoKnow += 1;
                        break;
                    }
                }
            }


            if (foundAgents.Count != 0)
            {
                float percentGradient = (float)agentsWhoKnow / foundAgents.Count;
                Debug.Log("Percent gradient for node " + loc.id + ": " + percentGradient);
                MeshRenderer nodeRenderer = (MeshRenderer)gameObject.GetComponent("MeshRenderer");
                nodeRenderer.material.color = Color.Lerp(Color.red, Color.blue, percentGradient);
            }
        }
        // nodeRenderer.material.color = Color.Lerp(Color.red, Color.blue, percentGradient);
    }
}
