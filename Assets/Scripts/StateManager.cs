﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A managemanet class to trigger the scipts and connects the objects in Unity.
/// </summary>
public class StateManager : MonoBehaviour {

    public GrahamScan grahanScan;
    public Graph graph;
    public GameObject pointPrefab;
    public float offset = 2f;
    public Transform instancePos;
    public List<Vector2> selectedPoints;
    public List<GameObject> pinList;
    public List<Node> selectedNodes;
    public Canvas options;
    public Text pathText;
    Dijkstras djk;
    GameObject go;
    int pinNumber = 0;
    bool calculatingShortestPath = false;
    void Start () {

        djk = GetComponent<Dijkstras>();
        options.GetComponent<GraphicRaycaster>().enabled = false;

	}
	void Update () {

        RaycastHit hit;
        //Calculating shortest path is bool to control the current mode of the user
        if(calculatingShortestPath)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    GameObject hitted = hit.transform.gameObject;
                    if(hitted.tag == "Node") //Selecting a pin (node) from the scene
                    {
                        Debug.Log("Selected " + hitted.name);
                        selectedNodes.Add(hitted.GetComponent<Node>());
                        hitted.GetComponent<Node>().turnBlue();
                        if (selectedNodes.Count == 2) //When you get two nodes start the dijkstras algorithm
                        {
                            startDijkstras();
                        }
                    }
                }
            }
        }
        else //In this mode we spawn a node from every click of the user.
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {                
                instancePos.position = hit.point + hit.normal * offset;
                if (Input.GetMouseButtonDown(0))
                {
                    GameObject hitted = hit.transform.gameObject;
                    if (hitted.tag == "Map") //
                    {
                        go = Instantiate(pointPrefab, instancePos.position, Quaternion.identity) as GameObject;
                        selectedPoints.Add(new Vector2(instancePos.position.x, instancePos.position.z));
                        go.name = "pin" + pinNumber.ToString();
                        go.GetComponent<Node>().uiName.text = "P" + pinNumber.ToString();
                        pinNumber++;
                        pinList.Add(go);
                        djk.nodesList.Add(go.GetComponent<Node>());
                    }
                    if (pinNumber == 4)
                        options.GetComponent<GraphicRaycaster>().enabled = true;
                }
             }
        }
    }
    /// <summary>
    /// Clear the current graph and start the grahan scan algorithm
    /// </summary>
    public void startGrahanScan()
    {
        clearGraph();
        options.transform.Find("Toggle").GetComponent<Toggle>().isOn = false;
        graph.clearInfo();
        grahanScan.triggerGrahamScan(selectedPoints, pinList);
        calculatingShortestPath = false;
    }
    public void autoGrahanPath()
    {
        grahanScan.triggerGrahamScan(selectedPoints, pinList);
        calculatingShortestPath = true;
        graph.buildGraph(selectedPoints, pinList);
    }

    public void clearGraph()
    {
        StartCoroutine(ClearForReal());
      
    }
    void softClean()
    {
        foreach (GameObject n in pinList)
        {
            n.GetComponent<Node>().turnRed();
            n.GetComponent<Node>().nodeRadius.radius = 0.5f;
        }
    }
    IEnumerator ClearForReal()
    {
        yield return new WaitForSeconds(0.7f);
        GameObject[] cleanables = GameObject.FindGameObjectsWithTag("connection");
        for (var i = 0; i < cleanables.Length; i++)
        {
            Destroy(cleanables[i]);
        }
        cleanables = GameObject.FindGameObjectsWithTag("distanceInformer");
        for (var i = 0; i < cleanables.Length; i++)
        {
            Destroy(cleanables[i]);
        }
        foreach (GameObject n in pinList)
        {
            n.GetComponent<Node>().nodeRadius.radius = 0.5f;
            n.GetComponent<Node>().turnRed();
        }
    }
    /// <summary>
    /// Connect all the nodes to create a graph
    /// </summary>
    public void createGraph()
    {
        softClean();
        graph.buildGraph(selectedPoints, pinList);
    }
    /// <summary>
    /// Trigger the dijkstras algorithm from the two selected nodes.
    /// </summary>
    /// 
    public void autoPath(Node start, Node end)
    {

        Debug.Log("Recebido start " + start);
        Debug.Log("Recebido end " + end);

        //selectedNodes.Add(start);
        // selectedNodes.Add(end);

        string output =
      djk.GetShortestPath(start, end).ToString();
        pathText.text = output;
      //  selectedNodes.Clear();

    }
    void startDijkstras()
    {
        string output =
        djk.GetShortestPath(selectedNodes[0],selectedNodes[1]).ToString();
        pathText.text = output;
        selectedNodes.Clear();
    }
    public void togglecalculatingShortestPath()
    {
        calculatingShortestPath = !calculatingShortestPath;
    }
}
