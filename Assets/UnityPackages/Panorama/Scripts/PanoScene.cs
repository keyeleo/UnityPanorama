using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Panoramas
{
	public class PanoScene : MonoBehaviour
	{
		public static PanoScene Instance = null;

		//prefab to instantiate
		public GameObject panoPrefab;
		public GameObject spotPrefab;
		//the work camera/view will follow with panorama
		public GameObject workCamera;
		//assets to build scene and panorama
		public TextAsset settings;
		public Texture[] cubeTextures;
		public Texture[] panoTextures;
		//camera speed
		public float speed = 0.2f;
		//panorama fade alpha
		public float sceneAlpha0 = 0.0f;
		public float sceneAlpha1 = 0.75f;

		Panorama panorama;
		Location[] locations;
		GameObject cursorObject;

		void Awake()
		{
			Instance = this;
			gameObject.AddComponent<MeshCollider>();

			//Debug.Log(settings.text);
			GameObject spots = new GameObject("Spots");
			spots.transform.SetParent(transform.parent);

			var q1 = Quaternion.Euler(0, 90, 0);
			var obj = JObject.Parse(settings.text);
			var jloc = (JObject)obj["locations"];
			var jpoints = (JArray)jloc["points"];
			locations = new Location[jpoints.Count];
			for (int i = 0; i < jpoints.Count; ++i)
			{
				Location location = new Location();
				var jpoint = (JObject)jpoints[i];
				//locationid
				location.locationid = (string)jpoint["locationid"];
                //viewpoint
                location.viewpoint = new Vector3();
				var jobj = (JObject)jpoint["viewpoint"];
				location.viewpoint.x = (float)jobj["x"];
				location.viewpoint.y = (float)jobj["z"]; //swap y and z
				location.viewpoint.z = (float)jobj["y"];
				//rotation
				jobj = (JObject)jpoint["rotation"];
				location.rotation = new Quaternion(
					-(float)jobj["x"],
					-(float)jobj["z"],
					(float)jobj["y"],
					(float)jobj["w"])*q1;
				//spot
				location.spot = new Vector3();
				jobj = (JObject)jpoint["spot"];
				location.spot.x = (float)jobj["x"];
				location.spot.y = (float)jobj["z"];
				location.spot.z = (float)jobj["y"];

				locations[i] = location;
				//Debug.Log(i + ": " + location.spot.x+ ", " + location.spot.y+ "," + location.spot.z+" - "+location.angle);

				//instantiate spots
				var spot = GameObject.Instantiate(spotPrefab, location.spot, Quaternion.identity);
				spot.transform.SetParent(spots.transform);
				spot.name = location.locationid;
			}


			var pano = GameObject.Instantiate(panoPrefab, Vector3.zero, Quaternion.identity);
			pano.transform.SetParent(transform.parent);
			panorama = pano.GetComponent<Panorama>();

            preprocessSceneMaterial();

			//max time 3s
			float size=Mathf.Max(gameObject.GetComponent<Renderer>().bounds.size.x, gameObject.GetComponent<Renderer>().bounds.size.y);
			speed = size / 3f;
		}

		// Use this for initialization
		void Start()
		{
			StartCoroutine(MoveTo(locations[0].locationid, true));
		}

		public void SetCursor(Vector3 position, Vector3 eular)
		{
			if (!cursorObject)
			{
				cursorObject = GameObject.Instantiate(spotPrefab, position, Quaternion.identity);
				cursorObject.transform.SetParent(transform.parent);
			}
			else
			{
				cursorObject.transform.position = position;
			}
		}

		[System.Obsolete]
		public void OnPointerClick(PointerEventData data)
		{
			StartCoroutine(PanoScene.Instance.MoveTo(data.worldPosition));
		}

		public void OnPointerHover(PointerEventData data)
		{
			//PanoScene.Instance.SetCursor(data.worldPosition, data.worldNormal);
		}

		public IEnumerator MoveTo(Vector3 position, bool teleport = false)
		{
			yield return MoveTo(FindAdjacent(position).locationid);
		}

		public IEnumerator MoveTo(string locationid, bool teleport = false)
		{
			for (int i = 0; i < locations.Length; ++i)
			{
				Location location = locations[i];
				if (location.locationid == locationid)
				{
					float time = Mathf.Min(0.6f, Vector3.Distance(location.viewpoint, transform.position) / speed);
					if (time > 0.001f)
					{
						Texture[] textures = new Texture[6];
						Texture texture = null;
						foreach (Texture tex in cubeTextures)
						{
							if (tex.name.Contains(location.locationid))
							{
								string index = tex.name.Substring(tex.name.LastIndexOf('_') + 1);
								textures[int.Parse(index)] = tex;
							}
						}
						foreach (Texture tex in panoTextures)
						{
							if (tex.name == location.locationid)
							{
								texture = tex;
								break;
							}
						}

						//move camera and panorama
						if (teleport)
						{
							workCamera.transform.localPosition = location.viewpoint;
							panorama.gameObject.transform.localPosition = location.viewpoint;
							panorama.gameObject.transform.localRotation = location.rotation;
							panorama.SetTextures(textures, texture);
						}
						else
						{
							iTween.MoveTo(workCamera, location.viewpoint, time);
							iTween.MoveTo(panorama.gameObject, location.viewpoint, time);
							iTween.FadeTo(gameObject, iTween.Hash("alpha", sceneAlpha1, "time", time / 2, "easetype", iTween.EaseType.easeInExpo));
							iTween.FadeTo(panorama.gameObject, iTween.Hash("alpha", 0.0f, "time", time / 2, "easetype", iTween.EaseType.easeInExpo));

							yield return new WaitForSeconds(time / 2);
							panorama.gameObject.transform.localRotation = location.rotation;
							panorama.SetTextures(textures, texture);
							iTween.FadeTo(gameObject, iTween.Hash("alpha", sceneAlpha0, "time", time / 2, "easetype", iTween.EaseType.easeInExpo));
							iTween.FadeTo(panorama.gameObject, iTween.Hash("alpha", 1.0f, "time", time / 2, "easetype", iTween.EaseType.easeOutExpo));
						}
					}

					break;
				}
			}
			yield return null;
		}

		void preprocessSceneMaterial()
		{
			var list = new List<Material>();
			foreach (var renderer in panorama.gameObject.GetComponentsInChildren<Renderer>())
			{
				renderer.GetMaterials(list);
				foreach (var mat in list)
				{
					SetMaterialBackground(mat);
				}
			}

			gameObject.GetComponent<Renderer>().GetMaterials(list);
			foreach (var mat in list)
			{
				mat.shader = Shader.Find("Particles/Standard Unlit");
				RenderingUtility.SetMaterialTransparentMask(mat);
			}
			iTween.FadeTo(gameObject, sceneAlpha0, 0.01f);
		}

		Location FindAdjacent(Vector3 source, float threshold = 1f)
		{
			float distance = float.PositiveInfinity;
			int index = -1;
			for (int i = 0; i < locations.Length; ++i)
			{
				float d = Vector3.Distance(locations[i].spot, source);
				if (d < distance)
				{
					distance = d;
					index = i;
				}
			}
			return index == -1 ? null : locations[index];
		}

		// Update is called once per frame
		void Update()
		{

		}

		void SetMaterialBackground(Material material)
		{
			material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Background;
			material.SetFloat("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            material.SetInt("_ZWrite", 1);
            material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        public class Location
		{
			public Vector3 viewpoint;
			public Vector3 spot;
			public Quaternion rotation;
			public string locationid;
		}
	}
}