using System.Collections.Generic;
using UnityEngine;

public class NodeColorChanger : MonoBehaviour {

    public Color lerpedColor1 = Color.red;
    public Color lerpedColor2 = Color.green;
    
    // public GameObject sphereNode;
    private List<AIcontroller> foundAgents;
    private Node loc = null;
    // private static int maxAgentsWhoKnow = 0;

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

            /*if(agentsWhoKnow < maxAgentsWhoKnow)
            {
                agentsWhoKnow = maxAgentsWhoKnow;
            }
            else
            {
                maxAgentsWhoKnow = agentsWhoKnow;
            }*/


            if (foundAgents.Count != 0)
            {
                // float percentGradient = (float)agentsWhoKnow / foundAgents.Count;
                float percentGradient = (float)agentsWhoKnow / 160.0f;
                Debug.Log("Percent gradient for node " + loc.id + ": " + percentGradient);
                MeshRenderer nodeRenderer = (MeshRenderer)gameObject.GetComponent("MeshRenderer");

                if(percentGradient < 0.5f)
                {
                    nodeRenderer.material.color = Color.Lerp(Color.blue, Color.yellow, percentGradient * 2.0f);
                }
                else
                {
                    nodeRenderer.material.color = Color.Lerp(Color.yellow, Color.red, (percentGradient - 0.5f) * 2);
                }
            }
        }
        // nodeRenderer.material.color = Color.Lerp(Color.red, Color.blue, percentGradient);
    }
}
