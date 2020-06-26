using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CloudFine{
    /// <summary>
    /// Example of how Agent population can be modified.
    /// Click to anywhere to spawn a new Agent.
    /// Removed agents are hidden and cached using Kill() instead of destroyed.
    /// New agents are pulled from cache if possible instead of creating a new object.
    /// </summary>
    public class PopulationControlUIExample : MonoBehaviour
    {
        public FlockBox _flockBox;
        public Agent _agent;
        public int _initialPopulation = 100;
        public Text _populationText;


        private List<Agent> _spawnedAgents = new List<Agent>();
        private List<Agent> _cachedAgents = new List<Agent>();

        private void Start()
        {
            for(int i=0; i<_initialPopulation; i++)
            {
                AddAgent();
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                //check to see if the click was over UI
                if ((!EventSystem.current.IsPointerOverGameObject()) )
                {
                    ClickAddAgent(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                //check to see if the click was over UI
                if ((!EventSystem.current.IsPointerOverGameObject()))
                {
                    RemoveAgent();
                }
            }
        }

        private void RefreshPopulationCount()
        {
            if (_populationText)
            {
                _populationText.text = _spawnedAgents.Count.ToString();
            }
        }

        public void AddAgent()
        {
            Agent agent;
            if (_cachedAgents.Count > 0)
            {
                agent = _cachedAgents[0];
                _cachedAgents.RemoveAt(0);
            }
            else
            {
                agent = GameObject.Instantiate<Agent>(_agent);
            }
            _spawnedAgents.Add(agent);
            agent.Spawn(_flockBox);
            RefreshPopulationCount();
        }

        public void RemoveAgent()
        {
            if (_spawnedAgents.Count > 0)
            {
                Agent toDestroy = _spawnedAgents[0];
                _spawnedAgents.RemoveAt(0);
                //use kill to deactivate and cache the Agent instead of destroying
                toDestroy.Kill();
                _cachedAgents.Add(toDestroy);
                RefreshPopulationCount();
            }
        }

        private void ClickAddAgent(Vector3 position)
        {
            Agent agent = GameObject.Instantiate<Agent>(_agent);
            agent.Spawn(_flockBox, position);
            _spawnedAgents.Add(agent);
            Debug.Log("click spawn " + position, agent);
            RefreshPopulationCount();

        }
    }
}
