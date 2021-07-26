using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PanoScene : MonoBehaviour {

	public static PanoScene Instance = null;

	public GameObject panoObject;
	public GameObject spotObject;
	public Character character;

	public TextAsset settings;
	public Texture[] cubeTextures;
	public Texture[] panoTextures;
	public float speed = 0.02f;

	Panorama panorama;
	Location[] locations;

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start()
	{
		Debug.Log(settings.text);
		var obj = JObject.Parse(settings.text);
		var jloc = (JObject)obj["locations"];
		var jpoints = (JArray)jloc["points"];
		locations = new Location[jpoints.Count];
		for(int i=0;i< jpoints.Count; ++i) {
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
			//Debug.Log(i + ": " + location.spot.x+ ", " + location.spot.y+ "," + location.spot.z);

			//instantiate spots
			var spot = GameObject.Instantiate(spotObject, location.spot, Quaternion.identity);
			spot.transform.SetParent(transform);
			spot.GetComponentInChildren<Spot>().locationid = location.locationid;
		}


		var jinfo = (JObject)obj["catalogue"];
		var title = (string)jinfo["title"];
		Debug.Log("load scene: " + title+",total spots "+locations.Length);

		if (character)
        {
            var pano = GameObject.Instantiate(panoObject, Vector3.zero, Quaternion.identity);
            //pano.transform.SetParent(character.transform);
			panorama=pano.GetComponent<Panorama>();
        }

		StartCoroutine(MoveTo(locations[0].locationid, true));
    }

	public IEnumerator MoveTo(string locationid, bool teleport=false)
    {
		for(int i = 0; i < locations.Length; ++i)
        {
			Location location = locations[i];
            if (location.locationid == locationid)
            {
				float time = Vector3.Distance(location.viewpoint, transform.position) / speed * 0.1f;
				if (time > 0.001f)
				{
					Texture[] textures = new Texture[6];
					foreach (Texture tex in cubeTextures)
					{
						//Debug.Log("textures: " + tex.name);
						if (tex.name.Contains(location.locationid))
						{
							string index = tex.name.Substring(tex.name.LastIndexOf('_') + 1);
							textures[int.Parse(index)] = tex;
						}
					}

					if (teleport)
                    {
						gameObject.transform.localPosition = location.viewpoint;
						panorama.gameObject.transform.localPosition = location.viewpoint;
						panorama.gameObject.transform.localEulerAngles = new Vector3(0, location.angle, 0); ;
						panorama.SetCubeTextures(textures);
					}
					else
                    {
						iTween.MoveTo(gameObject, location.viewpoint, time);
						iTween.MoveTo(panorama.gameObject, location.viewpoint, time);
						panorama.gameObject.transform.localEulerAngles = new Vector3(0, location.angle, 0); ;

						panorama.SetCubeTextures(textures);
						iTween.FadeTo(panorama.gameObject, 0.0f, time / 2);
                        yield return new WaitForSeconds(time / 2);
                        iTween.FadeTo(panorama.gameObject, 1.0f, time / 2);
                    }
                }

				break;
			}
		}
		yield return null;
	}

	public bool FindAdjacent(Location target, Vector3 source, float threshold=1f)
    {
		float distance = float.PositiveInfinity;
		int index = -1;
		for(int i = 0; i < locations.Length; ++i)
        {
			float d = Vector3.Distance(locations[i].spot, source);
			if (d < distance)
            {
				distance = d;
				index = i;
			}
		}
        if (index != -1)
        {
			target = locations[index];
			return true;
        }else
			return false;
    }

	// Update is called once per frame
	void Update () {
		
	}

	public class Location
	{
		public Vector3 viewpoint;
		public Vector3 spot;
		public float angle;
		public string locationid;
	}
}
