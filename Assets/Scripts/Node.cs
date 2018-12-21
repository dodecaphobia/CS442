using System.Collections.Generic;

public class Node
{
    static int curID = 0;
    public float x, y;
    public int id;
    public List<Edge> edges;

    public Node(float p1, float p2)
    {
        x = p1;
        y = p2;
        id = curID;
        curID += 1;
        edges = new List<Edge>();
    }
}
