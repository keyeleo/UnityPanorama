using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour {
    public bool disabled = false;

    private float lastUpdateShowTime = 0f;
    private readonly float updateTime = 1f;

    private int frames = 0;
    private float FPS = 0;
    private int verts = 0;
    private int tris = 0;

    private GUIStyle bb = new GUIStyle();

    void Start()
    {
        bb.normal.background = null;
        bb.normal.textColor = new Color(1.0f, 0.5f, 0.0f);
        bb.fontSize = 24;
        lastUpdateShowTime = Time.realtimeSinceStartup;
    }

    void MeshStats()
    {
        tris = 0;
        verts = 0;
        foreach (GameObject obj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            foreach (MeshFilter f in obj.GetComponentsInChildren<MeshFilter>())
            {
                tris += f.sharedMesh.triangles.Length / 3;
                verts += f.sharedMesh.vertexCount;
            }
        }
    }

    void OnGUI()
    {
        if (!disabled) {
            GUILayout.Label(tris.ToString("tris #,##0"), bb);
            GUILayout.Label(verts.ToString("verts #,##0"), bb);
            GUILayout.Label(FPS.ToString("FPS 0.0"), bb);
        }
    }

    void Update()
    {
        frames++;
        if (Time.realtimeSinceStartup - lastUpdateShowTime >= updateTime)
        {
            FPS = frames / (Time.realtimeSinceStartup - lastUpdateShowTime);
            frames = 0;
            lastUpdateShowTime = Time.realtimeSinceStartup;
            if (!disabled)
                MeshStats();
        }
    }
}