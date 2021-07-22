using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PanoScene : MonoBehaviour {

	public TextAsset settings;

	Location[] locations;

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
			location.angle = (float)jpoint["angle"];

			location.viewpoint = new Vector3();
			var jvp = (JObject)jpoint["viewpoint"];
			location.viewpoint.x = (float)jvp["x"];
			location.viewpoint.y = (float)jvp["z"];
			location.viewpoint.z = (float)jvp["y"];

			location.spot = new Vector3();
			jvp = (JObject)jpoint["spot"];
			location.spot.x = (float)jvp["x"];
			location.spot.y = (float)jvp["z"];
			location.spot.z = (float)jvp["y"];

			locations[i] = location;
			Debug.Log(i + ": " + location.viewpoint);
		}


		var jinfo = (JObject)obj["catalogue"];
		var title = (string)jinfo["title"];
		Debug.Log("load scene: " + title+",total spots "+locations.Length);
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
	}
}
