using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    static int curID = 0;
    public float x, y;
    public int id;
    public List<Edge> edges;

    void Start() {
        x = transform.position.x;
        y = transform.position.z;
        id = curID;
        Debug.Log("Node generated: " + id);
        curID += 1;
        edges = new List<Edge>();
    }

    public Vector3 getLoc() {
        return transform.position;
    }

    public void changeColor (float percentGradient) {        
        MeshRenderer nodeRenderer = (MeshRenderer)gameObject.GetComponent("MeshRenderer");
        if(percentGradient < 0.5f) {
            nodeRenderer.material.color = Color.Lerp(Color.blue, Color.yellow, percentGradient * 2.0f);
        }
        else
        {
            nodeRenderer.material.color = Color.Lerp(Color.yellow, Color.red, (percentGradient - 0.5f) * 2);        
        }
    }
}
