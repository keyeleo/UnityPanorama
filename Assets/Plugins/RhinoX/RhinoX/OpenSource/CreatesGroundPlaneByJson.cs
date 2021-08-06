using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace Ximmerse.RhinoX
{
    [System.Serializable]
    public struct GroundPlanePlacementData
    {
        public GroundPlanePlacementItem[] items;
    }

    [System.Serializable]
    public class GroundPlanePlacementItem
    {
        public int beacon_id;

        public int group_id;

        public Vector3 position;

        public Vector3 rotation;

        public int coord_system_flag; // 0 = left hand (unity), 1 = right hand (openXR)

        public float confidence_thresh = 0.85f;

        public float max_distance_thresh = 3;

        public float min_distance_thresh = 0.2f;

        public float drift_recenter_angle_threshold = 28;

        public float drift_recenter_distance_threshold = 0.75f;
    }

    /// <summary>
    /// 读取指定路径的json配置文件，自动创建 Ground Plane。
    /// 配置文件举例:
    /// {
    ///     "items": [
    ///         {
    ///             "beacon_id": 1,
    ///             "group_id": 0,
    ///             "position": {
    ///                 "x": 0.0,
    ///                 "y": 0.0,
    ///                 "z": 0.0
    ///             },
    ///             "rotation": {
    ///                 "x": 0.0,
    ///                 "y": 0.0,
    ///                 "z": 0.0
    ///             },
    ///             "coord_system_flag": 0,
    ///             "confidence_thresh": 0.8500000238418579,
    ///             "max_distance_thresh": 3.0,
    ///             "min_distance_thresh": 0.20000000298023225,
    ///             "drift_recenter_angle_threshold": 28.0,
    ///             "drift_recenter_distance_threshold": 0.75
    ///         },
    ///         {
    ///             "beacon_id": 2,
    ///             "group_id": 1,
    ///             "position": {
    ///                 "x": 1.0,
    ///                 "y": 1.0,
    ///                 "z": 1.0
    ///             },
    ///             "rotation": {
    ///                 "x": 0.0,
    ///                 "y": 90.0,
    ///                 "z": 0.0
    ///             },
    ///             "coord_system_flag": 0,
    ///             "confidence_thresh": 0.8500000238418579,
    ///             "max_distance_thresh": 1.100000023841858,
    ///             "min_distance_thresh": 0.5,
    ///             "drift_recenter_angle_threshold": 7.5,
    ///             "drift_recenter_distance_threshold": 0.75
    ///         }
    ///     ]
    /// }

    /// </summary>
    public class CreatesGroundPlaneByJson : MonoBehaviour
    {

        public string JsonFilePath = "/sdcard/GroundPlaneConfig.txt";

        public bool autoCreates = true;

        public IEnumerator Start()
        {
            while (!RhinoXSystem.IsInitialized)
                yield return null;

            if (autoCreates)
            {
                CreateGroundPlanesFromConfig();
            }
        }

        [ContextMenu("Create ground plane from json config")]
        public void CreateGroundPlanesFromConfig()
        {
            try
            {
                if (!File.Exists(JsonFilePath))
                {
                    return;
                }
                var txt = File.ReadAllText(JsonFilePath);
                GroundPlanePlacementData placementData = JsonUtility.FromJson<GroundPlanePlacementData>(txt);
                for (int i = 0; i < placementData.items.Length; i++)
                {
                    GroundPlanePlacementItem groundPlaneItem = placementData.items[i];
                    var gpGO = new GameObject("GroundPlane - " + groundPlaneItem.beacon_id);
                    gpGO.AddComponent<TrackableIdentity>().TrackableID = groundPlaneItem.beacon_id;
                    var gp = gpGO.AddComponent<GroundPlane>();
                    gp.GroupID = groundPlaneItem.group_id;
                    if (RhinoXGlobalSetting.groundPlaneAlgorithm == GroundPlaneAlgorithm.Fusion)
                    {
                        gp.MinErrorHeadDiffAngle = groundPlaneItem.drift_recenter_angle_threshold;
                        gp.MinErrorHeadDistance = groundPlaneItem.drift_recenter_distance_threshold;

                        gp.MaxTrackedDistance = groundPlaneItem.max_distance_thresh;
                        gp.MinTrackedDistance = groundPlaneItem.min_distance_thresh;
                        gp.MinTrackedConfidence = groundPlaneItem.confidence_thresh;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [ContextMenu("Test convert to json string")]
        public void TestToJson()
        {
            GroundPlanePlacementData data = new GroundPlanePlacementData();
            data.items = new GroundPlanePlacementItem[]
            {
                new GroundPlanePlacementItem()
                {
                     beacon_id = 1, position = Vector3.zero, rotation = Vector3.zero,
                },

                new GroundPlanePlacementItem()
                {
                    beacon_id = 2, position = Vector3.one, rotation = new  Vector3(0,90,0),
                    confidence_thresh = 0.85f, coord_system_flag = 0, drift_recenter_angle_threshold = 7.5f,
                    drift_recenter_distance_threshold = 0.75f,
                    group_id = 1,
                    max_distance_thresh= 1.1f,
                    min_distance_thresh = 0.5f,
                }
            };

            Debug.Log(JsonUtility.ToJson(data, true));
        }
    }
}