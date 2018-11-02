using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIcontroller : MonoBehaviour
{

    public NavMeshAgent agent;
    public List<double> dest;
    public List<Node> nodes;
    public List<Edge> edges;
    public List<int> exits;

    //vars for A*
    private List<int> extraExits;
    private List<int> searched;
    private List<int> visitedList;
    private Dictionary<int, List<double>> paths;
    private List<int> neighbors;

    void Start()
    {
        extraExits = new List<int>();
        searched = new List<int>();
        neighbors = new List<int>();
        visitedList = new List<int>();
    }

    void Update()
    {
        bool reachedDest = hasReachedDest();
        if (reachedDest)
        {
            if (exits.Contains((int)dest[0]))
            {
                Destroy(this);
                return;
            }
            //update visited list
            if (!visitedList.Contains((int)dest[0]))
                visitNode((int)dest[0]);
            //search if necessary
            if (dest.Count == 1)
                dest = repeatedA((int)dest[0]);
            dest.RemoveAt(0);
            //get destination node
            Node n = getNode((int)dest[0]);
            Vector3 v = new Vector3(n.x, 0, n.y);
            agent.SetDestination(v);
        }
    }

    public void SetGraph(Node start, List<Node> n, List<Edge> ed, List<int> ex)
    {
        dest = new List<double>();
        dest.Add(start.id);
        nodes = n;
        edges = ed;
        exits = ex;
    }

    private void addEdges(List<Edge> newEdges)
    {
        foreach(Edge e in newEdges)
        {
            if (!edges.Contains(e))
                edges.Add(e);
        }
    }

    private void visitNode(int n)
    {
        visitedList.Add((int)dest[0]);
        Node temp = getNode(n);
        foreach (Edge e in temp.edges)
        {
            if (!edges.Contains(e))
                edges.Add(e);
        }
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

    private bool hasReachedDest()
    {
        if (dest.Count == 0)
        {
            return false;
        }
        Node n = getNode((int)(dest[0]));
        if (Math.Abs((int)transform.position.x - n.x) < 1 && Math.Abs((int)transform.position.z - n.y) < 1)
        {
            return true;
        }
        return false;
    }

    private double heuristic(int cur)
    {
        double val = 100000;
        foreach (int n in exits)
        {
            if (val > getDistance(cur, n))
                val = getDistance(cur, n);
        }
        foreach(int n in extraExits)
        {
            if (val > getDistance(cur, n))
                val = getDistance(cur, n);
        }
        return val;
    }

    private double getDistance(int a, int b)
    {
        Node nodeA = getNode(a);
        Node nodeB = getNode(b);
        return Math.Sqrt(Math.Pow(nodeA.x - nodeB.x, 2) + Math.Pow(nodeA.y - nodeB.y, 2));
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
                    double h = heuristic(e);
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

    private List<double> repeatedA(int curNode)
    {
        //initialize hashmap, searched, neighbor list
        paths = new Dictionary<int, List<double>>();
        searched = new List<int>();
        neighbors = new List<int>();
        //insert curNode into hashmap and neighborlist, g(x) = 0
        List<double> l = new List<double>();
        l.Add(0);
        l.Add(heuristic(curNode));
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
        double curPathCost = 100000;
        foreach (KeyValuePair<int, List<double>> n in paths)
        {
            if (curPathCost > paths[n.Key][0] + paths[n.Key][1] && !(visitedList.Contains(n.Key)))
            {
                curPathCost = paths[n.Key][0] + paths[n.Key][1];
                l = paths[n.Key];
            }
        }
        //return lowest path cost
        return l.GetRange(1, l.Count - 1);
    }
}