﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
/*
 * At the moments works with the place script to handle hover actions (because at the moment, it requires a right click to open up a help
 * This doesn't translate that well when it's scaled to more than just building information (having to right click everything, even knowing to right click)
 * As a result, making the helper object fade in after a certain amount of time of the cursor being hovered over an object
 * In addition, making enumerators for this script so it adapts to the need of each ui object as opposed to having to make a new script each time
 */ 

public enum UIType {building, threat, quest, destroy, money, adventurers, satisfaction, efficiency, spending };

public class Tooltip : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    //[SerializeField] private GameObject[] helperPrefab = new GameObject[5];
    public GameObject tooltipPrefab;
    public Vector3 offset;

    public float delay = 0.2f;
    public float fadeDuration = 0.3f;
    
    //building[0], threat[1], quest[2], destroy[3], money[4], sidebar[5]
    //public UIType uiType;
    private GameObject tooltipInstance;

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(CreateTooltip());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(tooltipInstance);
    }

    private IEnumerator CreateTooltip()
    {
        tooltipInstance = Instantiate(tooltipPrefab, transform, false);
        tooltipInstance.transform.localPosition = offset;
        CanvasGroup canvasGroup = tooltipInstance.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        yield return new WaitForSeconds(delay);

        float counter = 0f;
        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(0, 1, counter / fadeDuration);

            yield return null;
        }
    }

    /*private void UIGet(UIType ui)
    {
        switch (ui)
        {
            case UIType.building:
                InfoInstantiate(helperPrefab[0], new Vector3(0, 110, 0));
                if (helper) helper.GetComponent<BuildingHelper>().FillText(gameObject);
                break;
            case UIType.threat:
                InfoInstantiate(helperPrefab[1], new Vector3(0, -80, 0));
                break;
            case UIType.quest:
                InfoInstantiate(helperPrefab[2], new Vector3(0, 60, 0));
                break;
            case UIType.destroy:
                InfoInstantiate(helperPrefab[3], new Vector3(0, 80, 0));
                break;
            case UIType.money:
                InfoInstantiate(helperPrefab[4], new Vector3(50, 110, 0));
                break;
            case UIType.adventurers:
                InfoInstantiate(helperPrefab[5], new Vector3(-100, 0, 0));
                break;
            case UIType.satisfaction:
                InfoInstantiate(helperPrefab[6], new Vector3(-100, 0, 0));
                break;
            case UIType.efficiency:
                InfoInstantiate(helperPrefab[7], new Vector3(-100, 0, 0));
                break;
            case UIType.spending:
                InfoInstantiate(helperPrefab[8], new Vector3(-100, 0, 0));
                break;
        }
    }*/
}