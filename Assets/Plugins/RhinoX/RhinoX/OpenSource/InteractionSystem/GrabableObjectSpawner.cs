using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Grabable prefab object spawner script : spawn grabable object at specific anchor.
    /// </summary>
    public class GrabableObjectSpawner : MonoBehaviour
    {
        /// <summary>
        /// If true, spawns foods at the anchors when the script OnEnable().
        /// </summary>
        public bool SpawnOnEnable = true;

        /// <summary>
        /// Auto respawn : if true, auto respawn a new instance at the anchor , when the old grabable instance is grabbed by player hand.
        /// </summary>
        public bool AutoRespawn = true;

        /// <summary>
        /// Spawn delay : after how many seconds to spawn a new food instance ?
        /// </summary>
        public float SpawnDelay;

        /// <summary>
        /// The grabable game object prefab.
        /// </summary>
        public Grabable prefab;

        public List<Transform> anchors = new List<Transform>();

        /// <summary>
        /// Key = spawn at anchor , Value = spawned instance.
        /// </summary>
        Dictionary<Transform, Grabable> SpawnInstancesMap = new Dictionary<Transform, Grabable>();

        private void OnEnable()
        {
            if (SpawnOnEnable)
            {
                Spawn();
            }
        }

        /// <summary>
        /// Spawn grabable prefabs at the anchor points.
        /// </summary>
        public void Spawn()
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                SpawnAtAnchor(anchors[i], this.prefab);
            }
        }

        private void SpawnAtAnchor(Transform anchor, Grabable _prefab)
        {
            if (SpawnInstancesMap.ContainsKey(anchor) && SpawnInstancesMap[anchor] != null)
            {
                return;//already spawns at the anchor.
            }
            GameObject instance = Instantiate<GameObject>(_prefab.gameObject, anchor.position, anchor.rotation);
            if (SpawnInstancesMap.ContainsKey(anchor))
            {
                SpawnInstancesMap[anchor] = instance.GetComponent<Grabable>();
            }
            else
            {
                SpawnInstancesMap.Add(anchor, instance.GetComponent<Grabable>());
            }
            instance.GetComponent<Grabable>().OnGrabBegin += GrabableObjectSpawner_OnGrabBegin;
            if (!instance.activeSelf)
                instance.SetActive(true);
        }

        /// <summary>
        /// Detach a grabable from anchor.
        /// </summary>
        /// <param name="grabable"></param>
        private void OnGrabableDetach(Grabable grabable)
        {
            grabable.OnGrabBegin -= GrabableObjectSpawner_OnGrabBegin;
            Transform anchor = null;//the anchor of the grabable
            foreach (var item in this.SpawnInstancesMap)
            {
                if (item.Value == grabable)
                {
                    anchor = item.Key;
                }
            }
            if (anchor != null)
            {
                SpawnInstancesMap[anchor] = null;//clear the value, but don't remove key
            }

            //if auto respawn == true, respawn grabable instance at the anchor.
            if (AutoRespawn)
            {
                StartCoroutine(SpawnInDelayTime(anchor, this.prefab, this.SpawnDelay));
            }
        }

        IEnumerator SpawnInDelayTime(Transform anchor, Grabable _prefab, float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnAtAnchor(anchor, _prefab);
        }

        /// <summary>
        /// Event : on grab begin.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="grabable"></param>
        private void GrabableObjectSpawner_OnGrabBegin(PlayerHand hand, Grabable grabable)
        {
            if (hand != null)
            {
                OnGrabableDetach(grabable);
            }
        }
    }
}