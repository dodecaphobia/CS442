using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public NodeManager nm;

    public int setSocial = 50;
    public int setInitialKnowledge = 50;
    public float setDelayTime = 100;

    public bool initializeDistance = true;
    public string minDistFile;
    
    public string dataFile;
    public int displacement;
    public string nodeFile;
    public string edgeFile;
    public int numAgents;
    public GameObject agent;
    public GameObject nodeObject;
    
    private List<Node> spawn;
    private List<Node> nodes;
    private List<Edge> edges;
    private List<int> exits;
    private List<float> minDists;

    void Start() {

        string path = "Assets\\AgentData\\" + dataFile;
        if (!File.Exists(path))
            File.Delete(path);
        StreamWriter dataReader = File.CreateText(path);
        dataReader.Close();

        //preload graph nodes and exits
        path = "Assets\\MapData\\" + nodeFile;
        StreamReader reader = File.OpenText(path);
        string line;
        spawn = new List<Node>();
        nodes = new List<Node>();
        edges = new List<Edge>();
        exits = new List<int>();
        List<Node> tempExits = new List<Node>();
        while ((line = reader.ReadLine()) != null) {
            string[] items = line.Split(',');
            GameObject temp = Instantiate(nodeObject, new Vector3(int.Parse(items[0]), 0, int.Parse(items[1])), Quaternion.identity);
            nodes.Add(temp.GetComponent<Node>());
            if (int.Parse(items[2]) == 1) {
                tempExits.Add(nodes[nodes.Count - 1]);
                exits.Add(((Node)(nodes[nodes.Count - 1])).id);
            }
            else if (int.Parse(items[2]) == 2)
                spawn.Add(nodes[nodes.Count - 1]);
        }

        nm.done(nodes);

        //generate agents using AIcontroller which have limited knowledge of the graph
        StartCoroutine("loadEdges", tempExits);
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

    private List<Edge> getEdges()
    {
        List<Edge> temp = new List<Edge>();
        System.Random rnd = new System.Random();
        foreach (Edge e in edges)
        {
            if (rnd.Next(0, 100) < setInitialKnowledge)
                temp.Add(e);
        }
        return temp;
    }

    public void storeData (bool isInit, int start, float f) {
        string path;
        if (isInit)
            path = "Assets\\AgentData\\" + minDistFile;
        else {
            path = "Assets\\AgentData\\" + dataFile;
            float denom = -1;
            for(int i = 0; i < spawn.Count; i++) {
                Node n = spawn[i];
                if(start == n.id)
                    denom = minDists[i];
            }
            f /= denom;
        }
        File.AppendAllText(path, (start + "," + f + Environment.NewLine));
    }

    void Update() {
        List<AIData> agents = new List<AIData>(FindObjectsOfType<AIData>());
        foreach(AIData agent in agents) {
            agent.check();
        }
    }

    IEnumerator loadEdges(List<Node> tempExits) {
        yield return new WaitForSeconds(1);
        //determine edges
        string path = "Assets\\MapData\\" + edgeFile;
        StreamReader reader = File.OpenText(path);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] items = line.Split(',');
            Edge e = new Edge(int.Parse(items[0]) - displacement, int.Parse(items[1]) - displacement);
            edges.Add(e);
            //Debug.Log("Checking for node #" + e.n1);
            Node n1 = getNode(e.n1);
            n1.edges.Add(e);
            //Debug.Log("Checking for node #" + e.n2);
            Node n2 = getNode(e.n2);
            n2.edges.Add(e);
        }

        minDists = new List<float>();
        if (initializeDistance) {
            path = "Assets\\AgentData\\" + minDistFile;
            if (!File.Exists(path))
                File.Delete(path);
            StreamWriter dataReader = File.CreateText(path);
            dataReader.Close();
            foreach (Node n in spawn) {
                Vector3 v1 = new Vector3(n.x, 1, n.y);
                float remainingDistance = 10000000f;
                foreach(Node e in tempExits) {
                    Vector3 v2 = new Vector3(e.x, 0, e.y);
                    UnityEngine.AI.NavMeshPath navPath = new UnityEngine.AI.NavMeshPath();
                    UnityEngine.AI.NavMesh.CalculatePath(v1,v2,UnityEngine.AI.NavMesh.AllAreas,navPath);
                    float tempDist = 0f;
                    for(int i = 1; i < navPath.corners.Length; i++) {
                        v2 = navPath.corners[i];
                        tempDist += Vector3.Distance(v1, v2);
                        v1 = v2;
                    }
                    if(tempDist < remainingDistance)
                        remainingDistance = tempDist;
                }
                minDists.Add(remainingDistance);
                storeData(true, n.id, remainingDistance);
            }
        }
        else {
            path = "Assets\\AgentData\\" + minDistFile;
            reader = File.OpenText(path);
            while ((line = reader.ReadLine()) != null) {
                string[] items = line.Split(',');
                minDists.Add(float.Parse(items[1]));
            }

        }
        StartCoroutine("generateAgent");
    }

    IEnumerator generateAgent() {
        for (int i = 0; i < numAgents; i++)
        {
            foreach (Node n in spawn)
            {
                //Debug.Log("Generating agent " + i + " at node " + n.id);
                GameObject temp = Instantiate(agent, new Vector3(n.x, 1, n.y), Quaternion.identity);
                AIData ai = temp.GetComponent<AIData>();
                ai.setInternalVars(setSocial, this, setDelayTime, false);
                List<Edge> e = getEdges();
                ai.SetGraph(n, nodes, e, exits);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
}