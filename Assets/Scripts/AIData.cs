using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class AIData : MonoBehaviour
{

    public List<double> dest;
    public List<Node> nodes;
    public List<Edge> edges;
    public List<int> exits;

    //internal vars
    private NavMeshAgent agent;
    private List<Edge> traversed;

    private bool canCommunicate;
    private int start;
    private bool isInit;
    private AgentManager mngr;
    private int social = 0;
    private float delayTime;

    private float distanceTraveled = 0;
    private Vector3 lastPosition;

    //vars for A*
    private List<int> extraExits;
    private List<int> searched;
    private List<int> visitedList;
    private Dictionary<int, List<double>> paths;
    private List<int> neighbors;

    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        gameObject.GetComponent<Renderer>().material.color = Color.green;
        traversed = new List<Edge>();

        lastPosition = transform.position;

        extraExits = new List<int>();
        searched = new List<int>();
        neighbors = new List<int>();
        visitedList = new List<int>();
    }

    public void check() {

        //keep track of distance traveled
        distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;
        //check if agent has reached destination
        bool reachedDest = hasReachedDest();
        //check if agent is able to move/not currently communicating
        if (agent.isStopped)
            agent.velocity = Vector3.zero;
        else {
            //if agent has reached destination, go to next location
            if (reachedDest) {
                //agent has reached exit
                if (exits.Contains((int)dest[0])) {
                    mngr.storeData(isInit, start, distanceTraveled);
                    Destroy(gameObject);
                    return;
                }
                //update visited list
                if (!visitedList.Contains((int)dest[0]))
                    visitNode((int)dest[0]);
                //get edge data for primacy/recency, pt. 1
                int a = (int)dest[0];
                //search if necessary
                if (dest.Count == 1)
                    dest = repeatedA((int)dest[0]);
                if (dest != null) {
                dest.RemoveAt(0);
                //get edge data for primacy/recency, pt. 2
                int b = (int)dest[0];
                traversed.Add(getEdge(a,b));
                //get destination node
                if (dest.Count > 0) {
                    Vector3 v = getNode((int)dest[0]).getLoc();
                    agent.SetDestination(v);
                }
            }
            }
        }
    }

    public void setInternalVars(int s, AgentManager m, float f, bool b) {
        canCommunicate = true;
        mngr = m;
        social = s;
        delayTime = f;
        isInit = b;
    }

    public void SetGraph(Node s, List<Node> n, List<Edge> ed, List<int> ex) {
        dest = new List<double>();
        start = s.id;
        dest.Add(start);
        nodes = n;
        edges = ed;
        exits = ex;
    }

    public List<Edge> getEdges() {
        return this.edges;
    }

    private void addEdges(List<Edge> newEdges) {
        foreach (Edge e in newEdges) {
            if (!edges.Contains(e))
                edges.Add(e);
        }
    }

    private void visitNode(int n) {
        visitedList.Add((int)dest[0]);
        Node temp = getNode(n);
        foreach (Edge e in temp.edges) {
            if (!edges.Contains(e))
                edges.Add(e);
        }
    }

    public Node getNode(int id) {
        foreach (Node n in nodes) {
            if (n.id == id)
                return n;
        }
        UnityEngine.Debug.Log("Node not found");
        return null;
    }

    public Edge getEdge(int a, int b) {
        foreach (Edge e in edges) {
            if ((e.n1 == a && e.n2 == b) || (e.n1 == b && e.n2 == a))
                return e;
        }
        return null;
    }

    private bool hasReachedDest() {
        if (dest == null || dest.Count == 0) {
            return false;
        }
        Vector3 destV = getNode((int)(dest[0])).getLoc();
        if(Vector3.Distance(destV, transform.position) < 5)
            return true;
        return false;
    }

    private double heuristic(int cur, List<int> le, bool thisNode) {
        double val = 100000;
        foreach (int n in le) {
            if (val > getDistance(cur, n))
                val = getDistance(cur, n);
        }
        if (thisNode)
            foreach (int n in extraExits) {
                if (val > getDistance(cur, n))
                    val = getDistance(cur, n);
            }
        return val;
    }

    private double getDistance(int a, int b)
    {
        Vector3 nodeA = getNode(a).getLoc();
        Vector3 nodeB = getNode(b).getLoc();
        return Vector3.Distance(nodeA, nodeB);
    }

    private List<double> expand(int curNode)
    {
        //if current node is an exit, return path
        foreach (int n in exits)
        {
            if (n == curNode)
                return paths[n].GetRange(1, paths[n].Count - 1);
        }
        //get all unchecked nodes connected to curNode
        List<int> edge = new List<int>();
        foreach (Edge e in edges)
        {
            if (e.n1 == curNode && !edge.Contains(e.n2))
                edge.Add(e.n2);
            if (e.n2 == curNode && !edge.Contains(e.n1))
                edge.Add(e.n1);
        }
        //insert into g(x), h(x), path into hashmap, selecting for minimum g(x)
        foreach (int e in edge)
        {
            if (!searched.Contains(e))
            {
                double g = paths[curNode][0] + getDistance(curNode, e);
                if (paths.ContainsKey(e))
                    paths[e][0] = Math.Min(g, paths[e][0]);
                else
                {
                    double h = heuristic(e, exits, true);
                    List<double> l = new List<double>(paths[curNode]);
                    l[0] = g;
                    l[1] = h;
                    l.Add(e);
                    paths.Add(e, l);
                }
                //add nodes to neighbor list
                if (!(neighbors.Contains(e)))
                    neighbors.Add(e);
            }
        }
        //add to searched list
        searched.Add(curNode);
        //return null, exit not found
        return null;
    }

    private int removeNext()
    {
        if (neighbors.Count == 0)
            return -1;
        //iterate through neighbor list, selecting for minimum g(x) + h(x)
        int n = neighbors[0];
        foreach (int x in neighbors)
        {
            if (paths[n][0] + paths[n][1] > paths[x][0] + paths[x][1])
                n = x;
        }
        //remove and return node with minimum g(x) + h(x)
        neighbors.Remove(n);
        return n;
    }

    private List<Edge> query(int curNode, List<int> otherExits)
    {
        //initialize hashmap, searched, neighbor list
        paths = new Dictionary<int, List<double>>();
        searched = new List<int>();
        neighbors = new List<int>();

        //insert curNode into hashmap and neighborlist, g(x) = 0
        List<double> l = new List<double>();
        l.Add(0);
        l.Add(heuristic(curNode, otherExits, false));
        paths.Add(curNode, l);

        neighbors.Add(curNode);
        //while neighborList.Count > 0
        while (neighbors.Count > 0)
        {
            l = expand(removeNext());
            if (l != null)
            {
                List<Edge> edgePath = new List<Edge>();
                int a = curNode;
                int b = -1;
                for (int i = 2; i < l.Count - 1; i++)
                {
                    b = (int)l[i];
                    edgePath.Add(getEdge(a, b));
                    a = b;
                }
                return rememberEdge(edgePath);
            }
            return new List<Edge>();
        }
        //if loop exited, exit not found, do not recommend path
        return null;
    }

    private List<double> repeatedA(int curNode)
    {
        //initialize hashmap, searched, neighbor list
        paths = new Dictionary<int, List<double>>();
        searched = new List<int>();
        neighbors = new List<int>();
        //insert curNode into hashmap and neighborlist, g(x) = 0
        List<double> l = new List<double>();
        l.Add(0);
        l.Add(heuristic(curNode, exits, true));
        paths.Add(curNode, l);

        neighbors.Add(curNode);
        //while neighborList.Count > 0
        while (neighbors.Count > 0)
        {
            l = expand(removeNext());
            if (l != null)
                return l;
        }
        //if loop exited, exit not found, iterate through hashmap for lowest path cost not visited yet
        double curPathCost = 100000000000;
        foreach (KeyValuePair<int, List<double>> n in paths)
        {
            if (curPathCost > paths[n.Key][0] + paths[n.Key][1] && !(visitedList.Contains(n.Key)))
            {
                curPathCost = paths[n.Key][0] + paths[n.Key][1];
                l = paths[n.Key];
            }
        }
        //return lowest path cost
        if(l == null)
            return l;
        return l.GetRange(1, l.Count - 1);
    }

    private void OnTriggerEnter(Collider collision) {
        AIData other = collision.gameObject.GetComponent<AIData>();
        if (!(other == null) && canCommunicate && dest != null) {
            System.Random rnd = new System.Random();
            //Agent communicating
            if (rnd.Next(0, 100) < social) {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                canCommunicate = false;
                //UnityEngine.Debug.Log("Communicating.");
                List<Edge> pathEdges = query((int)dest[0], other.exits);
                if (pathEdges != null)
                    addEdges(pathEdges);
                StartCoroutine(communicationDelay());
            }
                
        }
    }

    //delayed movement for communication
    IEnumerator communicationDelay() {    
        yield return new WaitForSeconds(delayTime);
        agent.isStopped = false;
        //UnityEngine.Debug.Log("Done communicating");
        gameObject.GetComponent<Renderer>().material.color = Color.green;
        StartCoroutine(waitBetweenCommunication());
    }

    IEnumerator waitBetweenCommunication() {
        yield return new WaitForSeconds(10);
        canCommunicate = true;
    }

    private List<Edge> rememberEdge(List<Edge> edgesToPass) {
        List<Edge> rememberedEdges = new List<Edge>();
        foreach (Edge e in edgesToPass) {
            System.Random rnd = new System.Random();
            //if edge has been visited
            if(traversed.Contains(e)) {
                UnityEngine.Debug.Log("An edge we know");
                double d = 0;
                for(int i = 0; i < traversed.Count; i++)
                    if(traversed[i] == e)
                        d = (i + 0.5) / traversed.Count;
                if(rnd.NextDouble() < getPrimacyRecency(d))
                    rememberedEdges.Add(e);
            }
            //else (edge has not been visited)
            else {
                if(rnd.NextDouble() < 0.5)
                    rememberedEdges.Add(e);
            }
        }
        if(rememberedEdges.Count < edgesToPass.Count)
            UnityEngine.Debug.Log("primacy and recency works");
        return rememberedEdges;
    }

    private double getPrimacyRecency(double x) {
        double y = ((1 / Math.Sqrt(2 * Math.PI)) * Math.Pow(Math.E, -0.5 * Math.Pow((x - 0.5), 2)));
        y *= -0.5;
        y += 1;
        UnityEngine.Debug.Log("value: " + y);
        return y;
    }
}