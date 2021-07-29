using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PanoScene : MonoBehaviour
{

	public static PanoScene Instance = null;

	public GameObject sceneObject;
	public GameObject panoObject;
	public GameObject spotObject;
	public Character character;
	public Transform spots;

	public TextAsset settings;
	public Texture[] cubeTextures;
	public Texture[] panoTextures;
	public float speed = 0.2f;
	public float sceneAlpha0 = 0.0f;
	public float sceneAlpha1 = 0.75f;

	Panorama panorama;
	Location[] locations;

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start()
	{
		//Debug.Log(settings.text);
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
			//angle
			location.angle = (float)jpoint["angle"];
			//viewpoint
			location.viewpoint = new Vector3();
			var jvp = (JObject)jpoint["viewpoint"];
			location.viewpoint.x = (float)jvp["x"];
			location.viewpoint.y = (float)jvp["z"]; //swap y and z
			location.viewpoint.z = (float)jvp["y"];
			//spot
			location.spot = new Vector3();
			jvp = (JObject)jpoint["spot"];
			location.spot.x = (float)jvp["x"];
			location.spot.y = (float)jvp["z"];
			location.spot.z = (float)jvp["y"];

			locations[i] = location;
			//Debug.Log(i + ": " + location.spot.x+ ", " + location.spot.y+ "," + location.spot.z+" - "+location.angle);

			//instantiate spots
			var spot = GameObject.Instantiate(spotObject, location.spot, Quaternion.identity);
			spot.transform.SetParent(spots);
			spot.GetComponentInChildren<Spot>().locationid = location.locationid;
		}


		if (character)
		{
			var pano = GameObject.Instantiate(panoObject, Vector3.zero, Quaternion.identity);
			pano.transform.SetParent(transform);
			panorama = pano.GetComponent<Panorama>();
		}

		preprocessSceneMaterial();
		StartCoroutine(MoveTo(locations[0].locationid, true));
	}

	public IEnumerator MoveTo(string locationid, bool teleport = false)
	{
		for (int i = 0; i < locations.Length; ++i)
		{
			Location location = locations[i];
			if (location.locationid == locationid)
			{
				float time = Vector3.Distance(location.viewpoint, transform.position) / speed * 0.1f;
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
						character.gameObject.transform.localPosition = location.viewpoint;
						panorama.gameObject.transform.localPosition = location.viewpoint;
						panorama.gameObject.transform.localEulerAngles = new Vector3(0, location.angle, 0);
						panorama.SetTextures(textures, texture);
					}
					else
					{
						iTween.MoveTo(character.gameObject, location.viewpoint, time);
						iTween.MoveTo(panorama.gameObject, location.viewpoint, time);
						panorama.gameObject.transform.localEulerAngles = new Vector3(0, location.angle, 0);
						iTween.FadeTo(sceneObject, iTween.Hash("alpha", sceneAlpha1, "time", time / 2, "easetype", iTween.EaseType.easeInExpo));
						iTween.FadeTo(panorama.gameObject, iTween.Hash("alpha", 0.0f, "time", time / 2, "easetype", iTween.EaseType.easeInExpo));

						yield return new WaitForSeconds(time / 2);
						panorama.SetTextures(textures, texture);
						iTween.FadeTo(sceneObject, iTween.Hash("alpha", sceneAlpha0, "time", time / 2, "easetype", iTween.EaseType.easeInExpo));
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

		sceneObject.GetComponent<Renderer>().GetMaterials(list);
		foreach (var mat in list)
		{
			Utility.SetMaterialTransparentMask(mat);
		}
		iTween.FadeTo(sceneObject, sceneAlpha0, 0.01f);
	}

	public Location FindAdjacent(Vector3 source, float threshold = 1f)
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
	}

	public class Location
	{
		public Vector3 viewpoint;
		public Vector3 spot;
		public float angle;
		public string locationid;
	}
}
