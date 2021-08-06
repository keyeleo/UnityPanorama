using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Collision FX : spawn Fx prefab when object collide with environmental objects.
    /// Depends on non-trigger collider
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollisionFx : MonoBehaviour
    {
        /// <summary>
        /// Collision event 
        /// </summary>
        public enum CollisionEvent
        {
            /// <summary>
            /// Spawn fx at collision enter
            /// </summary>
            Enter = 0,

            /// <summary>
            /// Spawn fx at collision stay
            /// </summary>
            Stay,

            /// <summary>
            /// Spawn fx at collision exit
            /// </summary>
            Exit,
        }

        /// <summary>
        /// Spawn condition type.
        /// </summary>
        public enum SpawnConditionType
        {
            EveryCollision = 0,

            /// <summary>
            /// Only spawn when collide with collider at target layer
            /// </summary>
            TargetLayer,

            /// <summary>
            /// Only spawns when collide with collider at target tag.
            /// </summary>
            TargetTag,
        }

        /// <summary>
        /// Config item : config collision FX spawning on collision event.
        /// </summary>
        [System.Serializable]
        public class CollisionFxEntryCfg
        {
            [Tooltip("The fx prefab, life time is 1.0 second. So it must be designed as one-time FX.")]
            public GameObject FxPrefab;

            [Tooltip("The trigger Fx spawning event")]
            public CollisionEvent TriggerEvent = CollisionEvent.Enter;

            [Tooltip("If true, spawns at collision contact point.\r\n And transform up axis points the contact normal ray.")]
            public bool DynamicSpawn = true;

            [Tooltip("If DynamicSpawn == false, the anchor point to spawn FX.")]
            public Transform SpawnAtAnchor;

            [Tooltip("Spawn interval - how often the FX is spawned."), Min(0)]
            public float Interval = 0.5f;

            /**** Spawn Condition ****/
            [Tooltip("Spawn condition type.")]
            public SpawnConditionType spawnConditionType = SpawnConditionType.EveryCollision;

            [Tooltip("Spawn when collides with target layer collider."), Min(0)]
            public int TargetLayer = 1;

            [Tooltip("Spawn when collides with target tag collider.")]
            public string TargetTag = string.Empty;
        }

        [SerializeField]
        List<CollisionFxEntryCfg> fxConfigs = new List<CollisionFxEntryCfg>();

        /// <summary>
        /// Gets the FX configs
        /// </summary>
        public IReadOnlyList<CollisionFxEntryCfg> CollisionFxConfigs
        {
            get
            {
                return fxConfigs;
            }
        }

        List<CollisionFxSpawn> spawners = new List<CollisionFxSpawn>();

        private void Start()
        {
            for (int i = fxConfigs.Count - 1; i >= 0; i--)
            {
                var cfg = fxConfigs[i];
                if(cfg.FxPrefab == null)
                {
                    fxConfigs.RemoveAt(i);
                    continue;
                }
                switch(cfg.TriggerEvent)
                {
                    case CollisionEvent.Enter:
                        gameObject.AddComponent<CollisionFxEnter>().cfg = cfg;
                        break;

                    case CollisionEvent.Stay:
                        gameObject.AddComponent<CollisionFxStay>().cfg = cfg;
                        break;

                    case CollisionEvent.Exit:
                        gameObject.AddComponent<CollisionFxExit>().cfg = cfg;
                        break;
                }
            }
        }

        #region Spawner SubClasses

        /// <summary>
        /// Sub class : spawns collision FX at enter
        /// </summary>
        abstract class CollisionFxSpawn : MonoBehaviour
        {
            public CollisionFxEntryCfg cfg;

            float prevSpawnTime = 0;

            public const float kFxLifetime = 1;

            protected void SpawnFx(ContactPoint contactPoint)
            {
                if (cfg != null && cfg.FxPrefab && (Time.time - prevSpawnTime >= cfg.Interval))
                {
                    GameObject fxInstance = Instantiate(cfg.FxPrefab,
                        !cfg.DynamicSpawn && cfg.SpawnAtAnchor != null ? cfg.SpawnAtAnchor.position : contactPoint.point,
                        !cfg.DynamicSpawn && cfg.SpawnAtAnchor != null ? cfg.SpawnAtAnchor.rotation : Quaternion.identity);
                    if (!fxInstance.activeSelf)
                    {
                        fxInstance.SetActive(true);
                    }
                    if (cfg.DynamicSpawn)
                    {
                        fxInstance.transform.up = contactPoint.normal;
                    }
                    Destroy(fxInstance, kFxLifetime);
                    prevSpawnTime = Time.time;
                }
            }

            protected bool CheckSpawnCondidition(GameObject contactGameObject)
            {
                if (cfg != null && contactGameObject != null)
                {
                    switch (cfg.spawnConditionType)
                    {
                        case SpawnConditionType.EveryCollision:
                            return true;
                        case SpawnConditionType.TargetTag:
                            return string.Compare(contactGameObject.tag, cfg.TargetTag) == 0;
                        case SpawnConditionType.TargetLayer:
                            return contactGameObject.layer == cfg.TargetLayer;
                        default: return false;
                    }
                }
                else
                    return false;
            }
        }

        class CollisionFxEnter : CollisionFxSpawn
        {
            private void OnCollisionEnter(Collision collision)
            {
                if (CheckSpawnCondidition(collision.gameObject) && collision.contactCount > 0)
                {
                    SpawnFx(collision.contacts[0]);
                }
            }
        }


        class CollisionFxStay : CollisionFxSpawn
        {
            private void OnCollisionStay(Collision collision)
            {
                if (CheckSpawnCondidition(collision.gameObject) && collision.contactCount > 0)
                {
                    SpawnFx(collision.contacts[0]);
                }
            }
        }

        class CollisionFxExit : CollisionFxSpawn
        {
            private void OnCollisionExit(Collision collision)
            {
                if (CheckSpawnCondidition(collision.gameObject) && collision.contactCount > 0)
                {
                    SpawnFx(collision.contacts[0]);
                }
            }
        }
        #endregion
    }

}