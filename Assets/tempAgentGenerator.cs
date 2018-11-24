using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class tempAgentGenerator : MonoBehaviour
{

    public int displacement;
    public string nodeFile;
    public string edgeFile;
    public int numAgents;
    public GameObject agent;
    private List<Node> spawn;
    private List<Node> nodes;
    private List<Edge> edges;
    private List<int> exits;

    void Start()
    {
        //preload graph nodes and exits
        StreamReader reader = File.OpenText(nodeFile);
        string line;
        spawn = new List<Node>();
        nodes = new List<Node>();
        edges = new List<Edge>();
        exits = new List<int>();
        while ((line = reader.ReadLine()) != null)
        {
            string[] items = line.Split(',');
            nodes.Add(new Node(float.Parse(items[0]), float.Parse(items[1])));
            if (int.Parse(items[2]) == 1)
                exits.Add(((Node)(nodes[nodes.Count - 1])).id);
            else if (int.Parse(items[2]) == 2)
                spawn.Add(nodes[nodes.Count - 1]);
        }
        //determine edges
        reader = File.OpenText(edgeFile);
        while ((line = reader.ReadLine()) != null)
        {
            string[] items = line.Split(',');
            Edge e = new Edge(int.Parse(items[0]) - displacement, int.Parse(items[1]) - displacement);
            edges.Add(e);
            Node n1 = getNode(e.n1);
            n1.edges.Add(e);
            Node n2 = getNode(e.n2);
            n2.edges.Add(e);
        }
        //generate agents using AIcontroller which have limited knowledge of the graph
        StartCoroutine(generateAgent());
    }

    private Node getNode(int id)
    {
        foreach (Node n in nodes)
        {
            if (n.id == id)
                return n;
        }
        return null;
    }

    private List<Edge> getEdges(Node n)
    {
        if (n == spawn[0])
        {
            return edges;
        }
        return new List<Edge>();
    }

    IEnumerator generateAgent()
    {
        List<int> otherExits = new List<int>();
        otherExits.Add(5);
        for (int i = 0; i < numAgents; i++)
        {
            foreach (Node n in spawn)
            {
                GameObject temp = Instantiate(agent, new Vector3(n.x, 1, n.y), Quaternion.identity);
                AIcontroller ai = temp.GetComponent<AIcontroller>();
                //Debug.Log("About to set graph.");
                List<Edge> e = getEdges(n);
                if (n == spawn[0])
                    ai.SetGraph(n, nodes, e, otherExits);
                else
                    ai.SetGraph(n, nodes, e, exits);
            }
            yield return new WaitForSeconds(1);
        }
    }
}
