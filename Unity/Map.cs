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
using System.Threading;
using System.IO;
using UnityEngine;
using Windows.Kinect;

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

    private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private ushort[] _Data;

    Renderer _renderer;
    MaterialPropertyBlock propBlock;
    Texture2D levels;
    Texture2D map;
    float useContour = 0.8f;
    float blurLayers = 0;
    int debugType = 0;
    public UnityEngine.Vector4 corner1 = new UnityEngine.Vector4(0.0f, 0.0f, 0.0f, 1.0f);
    public UnityEngine.Vector4 corner2 = new UnityEngine.Vector4(1.0f, 0.0f, 0.0f, 1.0f);
    public UnityEngine.Vector4 corner3 = new UnityEngine.Vector4(0.0f, 1.0f, 0.0f, 1.0f);
    public UnityEngine.Vector4 corner4 = new UnityEngine.Vector4(1.0f, 1.0f, 0.0f, 1.0f);
    int curCorner = 0;
    //Shift corners by a linear amount
    void editCorners(UnityEngine.Vector4 shift)
    {
        if (curCorner == 1 || curCorner == 0) corner1 += shift;
        if (curCorner == 2 || curCorner == 0) corner2 += shift;
        if (curCorner == 3 || curCorner == 0) corner3 += shift;
        if (curCorner == 4 || curCorner == 0) corner4 += shift;
        propBlock.SetVector("corner1", corner1);
        propBlock.SetVector("corner2", corner2);
        propBlock.SetVector("corner3", corner3);
        propBlock.SetVector("corner4", corner4);

    }
    void OnGUI()
    {
        if (Input.GetKey("h"))
        {
            float screenW = Screen.width;
            float screenH = Screen.height;
            GUI.skin.label.fontSize = (int)(screenH / 20);
            GUI.Label(new Rect(0.0f, 0.0f, screenW, screenH), @"
                H - show help
                C - change contour line opacity
                B - blur layers
                X - switch between alternative displays
                N - change layer design

                Corner manipulation:
                Shift - increase speed
                1234 - select corner
                ` - select all corners
                WASD - Move
                R - raise base
                F - lower base
                T - raise max
                F - lower max
            ");

        }

    }
    void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            _Sensor.Open();
            _Reader = _Sensor.DepthFrameSource.OpenReader();
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
            Debug.Log("camera found");
        }
        else
        {
            Debug.Log("Camera not found");
        }
        levels = new Texture2D(10, 1);
        levels.wrapMode = TextureWrapMode.Repeat;
        propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
        _renderer.GetPropertyBlock(propBlock);
        propBlock.SetTexture("segments", levels);
        _renderer.SetPropertyBlock(propBlock);
        levels.SetPixels(cols[0]);
        levels.Apply();

        map = new Texture2D(512, 424, TextureFormat.RGBAFloat, false);
        _renderer.GetPropertyBlock(propBlock);
        propBlock.SetTexture("_MainTex", map);
        _renderer.SetPropertyBlock(propBlock);
    }
    // Update is called once per frame
    void Update()
    {
        float speedFactor = Time.deltaTime;
        if (Input.GetKey("left shift")) speedFactor *= 10;
        propBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(propBlock);
        _renderer.GetPropertyBlock(propBlock);
        //Corner control keys
        if (Input.GetKeyDown("1")) curCorner = 1;
        if (Input.GetKeyDown("2")) curCorner = 2;
        if (Input.GetKeyDown("3")) curCorner = 3;
        if (Input.GetKeyDown("4")) curCorner = 4;
        //Backtick selects all corners
        if (Input.GetKeyDown("`")) curCorner = 0;
        //WASD and arrow keys translate the viewport
        if (Input.GetAxis("Horizontal") != 0.0f) editCorners(new UnityEngine.Vector4(Input.GetAxis("Horizontal") * -0.03f * speedFactor, 0.0f, 0.0f, 0.0f));
        if (Input.GetAxis("Vertical") != 0.0f) editCorners(new UnityEngine.Vector4(0.0f, Input.GetAxis("Vertical") * 0.03f * speedFactor, 0.0f, 0.0f));
        //R and F control the height of the base
        if (Input.GetKey("r")) editCorners(new UnityEngine.Vector4(0.0f, 0.0f, 0.04f * speedFactor, 0.0f));
        if (Input.GetKey("f")) editCorners(new UnityEngine.Vector4(0.0f, 0.0f, 0.04f * -speedFactor, 0.0f));
        //T and G control the height of the maximum
        if (Input.GetKey("t")) editCorners(new UnityEngine.Vector4(0.0f, 0.0f, 0.0f, 0.06f * speedFactor));
        if (Input.GetKey("g")) editCorners(new UnityEngine.Vector4(0.0f, 0.0f, 0.0f, 0.06f * -speedFactor));

        //Visual keys
        //N toggles between different color maps
        if (Input.GetKeyDown("n"))
        {
            curCols = (curCols + 1) % cols.Length;
            levels.SetPixels(cols[curCols]);
            levels.Apply();
        }
        //C cycles through different contour lines
        if (Input.GetKeyDown("c")) propBlock.SetFloat("contour", (useContour = (useContour + 0.1f) % 1.1f));
        //B cycles through different level blur amounts
        if (Input.GetKeyDown("b")) propBlock.SetFloat("blurLayers", (blurLayers = (blurLayers + 0.1f) % 1.1f));
        //X cycles through different debug display modes
        if (Input.GetKeyDown("x"))
        {
            debugType++;
            propBlock.SetFloat("rawDepth", (debugType % 3 == 1) ? 1.0f : 0.0f);
            propBlock.SetFloat("bigScaleZ", (debugType % 3 == 2) ? 5.0f : 1.0f);
        }
        _renderer.SetPropertyBlock(propBlock);
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_Data);

                var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
                Color[] colors = new Color[frameDesc.Width * frameDesc.Height];
                for (int i = 0; i < frameDesc.Width * frameDesc.Height; i++)
                {
                    float d = 1.0f-_Data[i]/500f;
                    colors[i] = new Color(d,d,d,1.0f);
                }
                map.SetPixels(colors);
                map.Apply();
                frame.Dispose();
                frame = null;
            }
        }
    }


    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
                _Sensor.Close();
            _Sensor = null;
        }
    }
}
