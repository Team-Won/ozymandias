﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceTerrain : MonoBehaviour
{
    public Map map;
    private RaycastHit hit;
    public float distance = 1.5f;
    private LayerMask lm;
    public GameObject terrainBuilding;
    public int numberRadius = 1;
    
    private bool isPlaced = false;
    public bool isAesthetic = false;

    private void Awake()
    {
        if (!isAesthetic)
        {
            if (!map)
            {
                map = GameObject.FindObjectOfType<Map>();
            }
            lm = LayerMask.GetMask("Surface", "Terrain");
            PlaceTerrain pt = terrainBuilding.GetComponent<PlaceTerrain>();
            if (pt)
            {
                pt.enabled = false;
            }
        }
        else
        {
            this.enabled = false;
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //if (Vector3.Distance(transform.position, map.transform.position) <= map.transform.lossyScale.x)
        //{
        //    Place();
        //}
          
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAesthetic)
        {
            if (!isPlaced)
            {
                if (Vector3.Distance(transform.position, map.transform.position) <= map.transform.lossyScale.x)
                {
                    Place();
                }

            }
        }
        
    }

    public void Place()
    {
        if (!isPlaced)
        {
            isPlaced = true;
            if (map)
            {
                if (GetSurfaceHit())
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Surface"))
                    {
                        map.CreateBuilding(terrainBuilding, hit.point);

                        Destroy(gameObject);
                    }

                }
            }
        }
        
        
    }

    public bool GetSurfaceHit()
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, distance, lm);
    }
}
