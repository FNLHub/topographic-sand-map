/*
    KEYSHORTCUTS:
        N - cycle to next color scheme
        C - change contour line opacity
        B - change layer blur level
        R - Toggle view raw depth data
        Arrow keys - Move frame
        Q - Rotate counterclockwise
        E - Rotate clockwise
        = (+) - Zoom in
        -     - Zoom out










**/






using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class Map : MonoBehaviour
{
    static Color[][] cols = {
        new Color[10] {
            new Color(0.8f,0.1f,0.1f,1),//Red
            new Color(0.75f,0.3f,0.1f,1),//Red
            new Color(0.7f,0.5f,0.1f,1),//Orange
            new Color(0.65f,0.65f,0.1f,1),//Yellow
            new Color(0.1f,0.65f,0.1f,1),//Green
            new Color(0.1f,0.5f,0.5f,1),//Teal
            new Color(0.1f,0.1f,1.0f,1),//Blue
            new Color(0.2f,0.1f,0.8f,1),//Blue
            new Color(0.5f,0.1f,0.7f,1),//Purple
            new Color(0.7f,0.1f,0.3f,1),
        },
        new Color[10] {
            new Color(0.075f,0.075f,0.3f,1), //Ocean floor
            new Color(0.1f,0.1f,0.4f,1), // Deep ocean
            new Color(0.15f,0.15f,0.6f,1), //Ocean
            new Color(0.2f,0.2f,0.8f,1),//Shallow ocean
            new Color(0.9f,0.8f,0.6f,1), // Beach

            new Color(0.2f,0.5f,0.2f,1), //Grass
            new Color(0.2f,0.5f,0.2f,1), //Grass
            new Color(0.5f,0.5f,0.5f,1), // Above tree line
            new Color(0.8f,0.8f,0.8f,1),  // Partial snow
            new Color(1,1,1,1) // Full snow
        }
    };
    int curCols = 0;
    // Start is called before the first frame update
    Renderer _renderer;
    MaterialPropertyBlock propBlock;
    Texture2D levels;
    float useContour = 0.8f;
    float blurLayers = 0;
    bool rawDepth = false;
    public float baseHeight = 1000.0f;//Millimeters
    public float heightRange = 300.0f;//Millimeters
    void Start()
    {
        levels = new Texture2D(10, 1);
        levels.wrapMode = TextureWrapMode.Clamp;
        propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
        _renderer.GetPropertyBlock(propBlock);
        propBlock.SetTexture("segments", levels);
        _renderer.SetPropertyBlock(propBlock);
        levels.SetPixels(cols[0]);
        levels.Apply();
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("q")) {
            Vector3 rot=transform.localRotation.eulerAngles;
            rot.z += 10*Time.deltaTime;
            transform.localRotation = Quaternion.Euler(0,0,rot.z);
        }
        if(Input.GetKeyDown("e")) {
            Vector3 rot=transform.localRotation.eulerAngles;
            rot.z -= 10*Time.deltaTime;
            transform.localRotation = Quaternion.Euler(0,0,rot.z);

        }
        if(Input.GetKeyDown("=")) {
            Vector3 rot=transform.localScale;
            rot.x*=1.005f;
            rot.y*=1.005f;
            transform.localScale=rot;
        }
        if(Input.GetKeyDown("-")) {
            Vector3 rot=transform.localScale;
            rot.x/=1.005f;
            rot.y/=1.005f;
            transform.localScale=rot;

        }
        if (Input.GetKeyDown("n"))
        {
            curCols += 1;
            if (curCols == cols.Length) curCols = 0;
            levels.SetPixels(cols[curCols]);
            levels.Apply();
        }
        if (Input.GetKeyDown("c"))
        {
            useContour += 0.1f;
            if (useContour >= 1.0f) useContour -= 1.0f;
            _renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("contour", useContour);
            _renderer.SetPropertyBlock(propBlock);
        }
        if (Input.GetKeyDown("b"))
        {
            blurLayers += 0.1f;
            if (blurLayers >= 1.0f) blurLayers -= 1.0f;
            _renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("blurLayers", blurLayers);
            _renderer.SetPropertyBlock(propBlock);
        }
        if (Input.GetKeyDown("r"))
        {
            rawDepth = !rawDepth;
            _renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("rawDepth", rawDepth ? 1.0f : 0.0f);
            _renderer.SetPropertyBlock(propBlock);
        }
        Vector3 change = new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"),0)*0.1f*Time.deltaTime;
        transform.position = transform.position + change;
        //Load map
        Texture2D map = new Texture2D(512,424,TextureFormat.RGBAFloat,false);
        _renderer.GetPropertyBlock(propBlock);
        propBlock.SetTexture("_MainTex", map);
        _renderer.SetPropertyBlock(propBlock);
        byte[] bytes = File.ReadAllBytes("Assets/Unity/depthdata.bin");
        float[] f = new float[512*424];
        Buffer.BlockCopy(bytes,0,f,0,512*424*4);
        Color[] c = new Color[512*424];
        for(int i=0;i<f.Length;i++) {
            float col = (baseHeight-f[i])/heightRange;
            c[i] = new Color(col,col,col,1.0f);
        }
        map.SetPixels(c);
        map.Apply();
    }
}
