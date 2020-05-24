﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Scenario")][System.Serializable]
public class Event : ScriptableObject
{
    public enum EventType
    {
        Flavour,
        Adventurers,
        Threat,
        Chaos,
        Endgame,
        Chain,
        Special,
        Advert
    }

    public string headline;
    [TextArea(3,8)] public string article;
    public Sprite image;
    
    public EventType type;
    public bool headliner; // If the event should be the main one, should be true for all events with choices

    public int
        minChaos,
        minThreat,
        minWealth,
        maxChaos,
        maxThreat,
        maxWealth;
    
    [Tooltip("These are the outcomes that automatically run on an event")]
    public List<Outcome> outcomes = new List<Outcome>();
    
    [Tooltip("Up to 4 choices, with their own outcomes")]
    public List<Choice> choices = new List<Choice>();
    
    public string Execute() // Run the event's and return the outcome's description
    {
        return Outcome.Execute(outcomes);
    }

    public string MakeChoice(int choice)
    {
        return Outcome.Execute(choices[choice].outcomes);
    }
    
}