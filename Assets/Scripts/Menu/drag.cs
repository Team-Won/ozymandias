﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //external access
    public GameManager gameManager;
    public Map map;
    public GameObject building;

    //card features
    private Image image;
    private Text text;

    //drag/drop requirements
    public Transform parent = null;
    public Transform parentPlaceholder = null;
    private GameObject buildingInstantiated;
    private GameObject placeHolder = null;

    //mouse tracking
    private Vector2 mousePos;
    private Vector3 worldPoint;
    private RaycastHit hit;

    private void Start()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<Text>();
    }

    private void Update()
    {
        //If we have something instantiated on the cursor, allow for rotation
        if (buildingInstantiated)
        {
            if (Input.GetMouseButtonDown(1))
            {
                buildingInstantiated.transform.Rotate(0, 30, 0);
            }
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        ////////////////////////////////Placeholder initialisation////////////////////////////
        placeHolder = new GameObject();
        placeHolder.transform.SetParent(transform.parent);
        LayoutElement le = placeHolder.AddComponent<LayoutElement>();
        le.preferredWidth = GetComponent<LayoutElement>().preferredWidth;
        le.preferredHeight = GetComponent<LayoutElement>().preferredHeight;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;
        placeHolder.transform.SetSiblingIndex(transform.GetSiblingIndex());
        //By making the transform a child of the canvas, it is not longer locked to a board
        parent = transform.parent;
        parentPlaceholder = parent;
        transform.SetParent(transform.parent.parent);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        /////////////////////////////////////////////////////////////////////////////////////
    }

    public void OnDrag(PointerEventData eventData)
    {
        //////////////////////////////////////////////////////Intantiate object on board/////////////////////////////////////////////////////////
        //If card is outside panel,
        if (!eventData.pointerEnter)
        {
            text.enabled = false;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            Physics.Raycast(ray, out hit);
            if (!buildingInstantiated)
            {
                if (hit.collider)
                {
                    buildingInstantiated = Instantiate(building, hit.point, transform.rotation);
                    image.enabled = false;
                }
            }
            else
            {
                buildingInstantiated.transform.position = hit.point;
            }
        }
        //If card is in panel
        else if (eventData.pointerEnter)                                                                                                                                {
            text.enabled = true;
            image.enabled = true                                                                                                                                      ;
            Destroy(buildingInstantiated)                                                                                                                                ;                                                                                                                                                                  }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////Placeholders////////////////////
        transform.position = eventData.position;
        int newSiblingIndex = parentPlaceholder.childCount;
        ///////////////////////////////////////////////////

        ///////////////////////////////////////////////////////Card Panel Position//////////////////////////////////////////////////////////////
        //The default position for a card is the right most of the list. Otherwise if position is more left than a card then put it there.
        for (int i = 0; i < parentPlaceholder.childCount; i++)
        {
            if (transform.position.x < parentPlaceholder.GetChild(i).position.x)
            {
                newSiblingIndex = i;

                if (placeHolder.transform.GetSiblingIndex() < newSiblingIndex)
                {
                    newSiblingIndex--;
                }
                break;
            }
        }
        placeHolder.transform.SetSiblingIndex(newSiblingIndex);
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ////////////////////////////////drag/drop settings//////////////////////////////////////
        transform.SetParent(parent);
        transform.SetSiblingIndex(placeHolder.transform.GetSiblingIndex());
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        Destroy(placeHolder);

        map.Occupy(thing, thingInstantiated.transform.position);
        
        Destroy(thingInstantiated);

        if (gameManager.CurrentWealth >= building.GetComponent<Building>().baseCost)
        {
            gameManager.Build(building.GetComponent<Building>());
            map.Occupy(building, buildingInstantiated.transform.position);
        }
        if (!eventData.pointerEnter)
        {
            text.enabled = true;
            image.enabled = true;
        }
        Destroy(placeHolder);
        Destroy(buildingInstantiated);
    }
}
