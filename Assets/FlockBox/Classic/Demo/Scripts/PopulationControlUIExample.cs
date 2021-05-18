using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if FLOCKBOX_DOTS
using Unity.Entities;
#endif

namespace CloudFine.FlockBox{
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
#if FLOCKBOX_DOTS

        private List<Entity> _spawnedEntities = new List<Entity>();
#endif
        private void Start()
        {
            AddAgent(_initialPopulation);
        }

        private void OnEnable()
        {
#if FLOCKBOX_DOTS

            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (Entity e in _spawnedEntities)
            {
                manager.SetEnabled(e, true);
            }
#endif
        }
        private void OnDisable()
        {
#if FLOCKBOX_DOTS

            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (Entity e in _spawnedEntities)
            {
                manager.SetEnabled(e, false);
            }
#endif
        }

        private void RefreshPopulationCount()
        {
            if (_populationText)
            {
#if FLOCKBOX_DOTS
                if (_flockBox.DOTSEnabled)
                {
                    _populationText.text = _spawnedEntities.Count.ToString();
                }
                else
#endif
                {
                    _populationText.text = _spawnedAgents.Count.ToString();
                }
            }
        }

        public void AddAgent(int toAdd)
        {
#if FLOCKBOX_DOTS
            if (_flockBox.DOTSEnabled)
            {
                _spawnedEntities.AddRange(_flockBox.InstantiateAgentEntitiesFromPrefab(_agent, toAdd));
            }
            else
#endif
            {
                for (int i = 0; i < toAdd; i++)
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
                    agent.Spawn(_flockBox, _flockBox.RandomPosition());
                }
            }
            RefreshPopulationCount();

        }



        public void RemoveAgent(int toRemove)
        {
#if FLOCKBOX_DOTS
            if (_flockBox.DOTSEnabled)
            {
                EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                Entity e;
                for(int i =0; i<toRemove; i++)
                {
                    if (_spawnedEntities.Count > 0)
                    {
                        e = _spawnedEntities[0];
                        _spawnedEntities.RemoveAt(0);
                        manager.DestroyEntity(e);
                    }
                }
            }
            else
#endif
            {
                for (int i = 0; i < toRemove; i++)
                {
                    if (_spawnedAgents.Count > 0)
                    {
                        Agent toDestroy = _spawnedAgents[0];
                        _spawnedAgents.RemoveAt(0);
                        //use kill to deactivate and cache the Agent instead of destroying
                        toDestroy.Kill();
                        _cachedAgents.Add(toDestroy);
                    }
                }
            }
            RefreshPopulationCount();

        }
    }
}
