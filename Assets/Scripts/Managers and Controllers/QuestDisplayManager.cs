﻿using System;
using Managers_and_Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;
using static QuestMapController;

public class QuestDisplayManager : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button sendButton;
    [SerializeField] private GameObject displayContent;
    [SerializeField] private GameObject simpleContent;
    [SerializeField] private GameObject[] stamps;

    public Quest flyerQuest;
    
    private void Start()
    {
        sendButton.onClick.AddListener(OnButtonClick);
        SetDisplaying(false);
    }

    private void OnButtonClick()
    {
        flyerQuest.StartQuest();
        GetComponent<HighlightOnHover>().mouseOver = false;
        foreach (var stamp in stamps)
        {
            sendButton.gameObject.SetActive(false);
            stamp.SetActive(true);
        }

        JukeboxController.Instance.PlayStamp();
    }

    public void SetQuest(Quest q)
    {
        flyerQuest = q;
        titleText.text = q.title;
        descriptionText.text = q.description;
        sendButton.gameObject.SetActive(true);
        foreach (var stamp in stamps)
        {
            stamp.SetActive(false);
        }
    }

    public void SetDisplaying(bool displaying)
    {
        displayContent.SetActive(displaying);
        simpleContent.SetActive(!displaying);
        if (displaying && flyerQuest)
        {
            bool enoughAdventurers = Manager.RemovableAdventurers > flyerQuest.adventurers;
            bool enoughMoney = Manager.Wealth >= flyerQuest.cost;
            
            statsText.text =
                (enoughAdventurers ? "" : "<color=#820000ff>") + 
                "Adventurers: " + flyerQuest.adventurers +
                (enoughAdventurers ? "" : "</color>") +
                (enoughMoney ? "" : "<color=#820000ff>") +
                "\nCost: " + flyerQuest.cost +
                (enoughMoney ? "" : "</color>") +
                "\nDuration: " + flyerQuest.turns + " turns";

                sendButton.interactable = enoughAdventurers && enoughMoney;
        }
    }
}
