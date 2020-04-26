using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CloudFine{
    public class DemoPopulationControl : MonoBehaviour
    {
        public FlockBox _flockBox;
        public Agent _agent;

        public Text _populationText;

        private List<Agent> _spawnedAgents = new List<Agent>();

        private void Start()
        {
            for(int i=0; i<100; i++)
            {
                AddAgent();
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if ((!EventSystem.current.IsPointerOverGameObject()) )
                {
                    Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    AddAgent(clickPos);
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
            Agent agent = GameObject.Instantiate<Agent>(_agent);
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
                GameObject.Destroy(toDestroy);
                RefreshPopulationCount();
            }
        }

        private void AddAgent(Vector3 position)
        {
            Agent agent = GameObject.Instantiate<Agent>(_agent);
            agent.Spawn(_flockBox, position);
            _spawnedAgents.Add(agent);
            Debug.Log("click spawn " + position, agent);
            RefreshPopulationCount();

        }
    }
}
