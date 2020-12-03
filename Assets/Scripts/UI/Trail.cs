﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Trail : MonoBehaviour
    {
        public Transform[] waypoints = new Transform[3];
        private float percentsPerSecond = 0.7f; 
        private float currentPathPercent = 0.0f;
        private GameObject target;
    
        public ParticleSystem particles;
    
        public void SetTarget(Metric metric)
        {
            // Set start
            waypoints[0].position = Input.mousePosition;
            // Set end
            target = FindStatBar(metric);
            if (!target) {
                Destroy(gameObject);
                return;
            }
            waypoints[2] = target.transform;

            // Set midpoint for curve. Bend vertical and then horizontal
            Vector3 difference = waypoints[2].position - waypoints[0].position;
            waypoints[1].position = new Vector3(
                waypoints[0].position.x + difference.x * 1 / 3, 
                waypoints[0].position.y + difference.y * 2 / 3,
                0);
        
            // Change particle color based on target
            var particlesMain = particles.main;
            particlesMain.startColor = waypoints[2].Find("Mask").Find("Fill").GetComponent<Image>().color;
        }

        void Update()
        {
            if (currentPathPercent < 1)
            {
                currentPathPercent += percentsPerSecond * Time.deltaTime;
                if (currentPathPercent > 1) currentPathPercent = 1;
                iTween.PutOnPath(particles.gameObject, waypoints, currentPathPercent);
                percentsPerSecond += 0.035f;
            }
            else
            {
                particles.transform.position = waypoints[2].position;
                StartCoroutine(Decay());
            }
        }

        IEnumerator Decay()
        {
            yield return new WaitForSeconds(2f);
            Destroy(gameObject);
        }
    
        public GameObject FindStatBar(Metric metric)
        {
            switch (metric)
            {
                case Metric.Food: return GameObject.Find("Food Bar");
                case Metric.Luxuries: return GameObject.Find("Luxury Bar");
                case Metric.Entertainment: return GameObject.Find("Entertainment Bar");
                case Metric.Equipment: return GameObject.Find("Equipment Bar");
                case Metric.Magic: return GameObject.Find("Magic Bar");
                case Metric.Weaponry: return GameObject.Find("Weaponry Bar");
                //case Metric.Defense: return GameObject.Find("Threat Bar");
                default: return null;
            }
        }
    }
}
