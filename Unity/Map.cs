using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Threading;
using System.IO;
using UnityEngine;
using Windows.Kinect;

class ThemeLayer
{
    public Color[] tex;
    public Color col;
    public ThemeLayer(Color mainCol, string file = "white.png", Color? fileTint = null)
    {
        Color tint = fileTint ?? new Color(1f, 1f, 1f, 1f);
        col = mainCol * tint;
        if (file == "white.png" && tint.a == 1f) tint = col;
        tex = new Color[1024 * 1024];
        Texture2D tex2d = new Texture2D(2, 2);
        tex2d.LoadImage(File.ReadAllBytes("Assets/topographic-sand-map/Textures/" + file));
        tex = tex2d.GetPixels();
        if (tint.r != 1 || tint.g != 1 || tint.b != 1 || tint.a != 1) for (int i = 0; i < tex.Length; i++)
            {
                tex[i] *= tint;
            }
    }
    static public Tuple<Texture2D, Texture3D> createTheme(ThemeLayer[] layers)
    {
        Texture3D tex3d = new Texture3D(1024, 1024, layers.Length, TextureFormat.RGBA32, 1);
        List<Color> cols = new List<Color>();
        for (int i = 0; i < layers.Length; i++)
        {
            cols.AddRange(layers[i].tex);
        }
        tex3d.SetPixels(cols.ToArray());
        tex3d.wrapModeU = TextureWrapMode.Repeat;
        tex3d.wrapModeV = TextureWrapMode.Repeat;
        tex3d.wrapModeW = TextureWrapMode.Clamp;

        Texture2D tex2d = new Texture2D(1, layers.Length);
        cols = new List<Color>();
        for (int i = 0; i < layers.Length; i++)
        {
            cols.Add(layers[i].col);
        }
        tex2d.SetPixels(cols.ToArray());
        tex2d.wrapMode = TextureWrapMode.Clamp;
        return new Tuple<Texture2D, Texture3D>(tex2d, tex3d);
    }
    //THEMES
    public static ThemeLayer[][] themes = {
        new ThemeLayer[] {
            new ThemeLayer(new Color(0.23f,0.38f,1.0f,1f),"water.png",new Color(0.6f,0.6f,0.6f,1f)),//Deep Ocean
            new ThemeLayer(new Color(0.23f,0.38f,1.0f,1f),"water.png",new Color(0.8f,0.8f,0.8f,1f)),//Ocean
            new ThemeLayer(new Color(0.23f,0.38f,1.0f,1f),"water.png",new Color(1f,1f,1f,1f)),//Shallow Ocean
            new ThemeLayer(new Color(0.85f,0.75f,0.57f,1f),"sand.png",new Color(1f,1f,1f,1)),// Beach
            new ThemeLayer(new Color(0.85f,0.75f,0.57f,1f),"sand.png",new Color(1f,1f,1f,1)),// Beach
            new ThemeLayer(new Color(0.38f,0.51f,0.22f,1f),"grass.png",new Color(1f,1f,1f,1)),// Grass
            new ThemeLayer(new Color(0.38f,0.51f,0.22f,1f),"grass.png",new Color(1f,1f,1f,1)),// Grass
            new ThemeLayer(new Color(0.45f,0.45f,0.45f,1f),"rock.png",new Color(1f,1f,1f,1)),// Rock
            new ThemeLayer(new Color(0.45f,0.45f,0.45f,1f),"rock.png",new Color(1f,1f,1f,1)),// Rock
            new ThemeLayer(new Color(0.9f,0.9f,0.9f,1f),"snow.png",new Color(0.8f,0.8f,0.8f,1)),//Light snow
            new ThemeLayer(new Color(0.9f,0.9f,0.9f,1f),"snow.png",new Color(0.8f,0.8f,0.8f,1)),//Light snow
            new ThemeLayer(new Color(0.9f,0.9f,0.9f,1f),"snow.png",new Color(1f,1f,1f,1)),//Snow
        },
        new ThemeLayer[] {
            new ThemeLayer(new Color(0.8f,0.1f,0.1f,1)),//Red
            new ThemeLayer(new Color(0.75f,0.3f,0.1f,1)),//Red
            new ThemeLayer(new Color(0.7f,0.5f,0.1f,1)),//Orange
            new ThemeLayer(new Color(0.65f,0.65f,0.1f,1)),//Yellow
            new ThemeLayer(new Color(0.1f,0.65f,0.1f,1)),//Green
            new ThemeLayer(new Color(0.1f,0.5f,0.5f,1)),//Teal
            new ThemeLayer(new Color(0.1f,0.1f,1.0f,1)),//Blue
            new ThemeLayer(new Color(0.2f,0.1f,0.8f,1)),//Blue
            new ThemeLayer(new Color(0.5f,0.1f,0.7f,1)),//Purple
        }
    };
};

public class Map : MonoBehaviour
{
    void Start()
    {
        SetupKinect();
        SetupShader();
        NextTheme();
        SetupPrettify();
        renderer.SetPropertyBlock(propBlock);
    }
    void Update()
    {
        propBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propBlock);
        KeyInput();
        ReadFrame();
        renderer.SetPropertyBlock(propBlock);
    }

    int curTheme = -1;
    public Texture3D themeTextures;
    Texture2D themeColors;
    void NextTheme()
    {
        curTheme = (curTheme + 1) % ThemeLayer.themes.Length;
        (themeColors, themeTextures) = ThemeLayer.createTheme(ThemeLayer.themes[curTheme]);
        propBlock.SetTexture("theme", themeTextures);
        propBlock.SetTexture("themeCol", themeColors);
        themeColors.Apply();
        themeTextures.Apply();
        propBlock.SetFloat("layerCount", ThemeLayer.themes[curTheme].Length);
    }

    //PRETTIFY
    int prettify_taa_kernel;
    int prettify_blur_kernel;
    int prettify_copy_kernel;
    RenderTexture prettifyInput;
    RenderTexture prettifyOld;
    RenderTexture prettifyOutput;
    public ComputeShader computeShader;
    RenderTexture create_rt(string name)
    {
        RenderTexture output = new RenderTexture(512, 424, sizeof(float) * 4, RenderTextureFormat.ARGBFloat);
        output.enableRandomWrite = true;
        output.Create();
        return output;
    }
    void SetupPrettify()
    {
        //Prettify compute shader
        prettify_taa_kernel = computeShader.FindKernel("time_interpolate");
        prettify_blur_kernel = computeShader.FindKernel("blur");
        prettify_copy_kernel = computeShader.FindKernel("copy_result");
        prettifyInput = create_rt("Input");
        prettifyOld = create_rt("Old");
        prettifyOutput = create_rt("Result");
        computeShader.SetTexture(prettify_taa_kernel, "Input", prettifyInput);
        computeShader.SetTexture(prettify_taa_kernel, "Old", prettifyOld);
        computeShader.SetTexture(prettify_blur_kernel, "Input", prettifyInput);
        computeShader.SetTexture(prettify_blur_kernel, "Result", prettifyOutput);
        computeShader.SetTexture(prettify_copy_kernel, "Input", prettifyInput);
        computeShader.SetTexture(prettify_copy_kernel, "Result", prettifyOutput);
    }


    //SHADER
    new Renderer renderer;
    MaterialPropertyBlock propBlock;
    Texture2D map;
    Texture2D finalTex;
    float useContour = 0.8f;
    float blurLayers = 0;
    float useTex = 1.0f;
    void SetupShader()
    {
        Cursor.visible = false;
        propBlock = new MaterialPropertyBlock();
        renderer = GetComponent<Renderer>();
        renderer.GetPropertyBlock(propBlock);
        map = new Texture2D(512, 424, TextureFormat.RGBAFloat, false);
        finalTex = new Texture2D(512, 424, TextureFormat.RGBAFloat, false);
        //finalTex.filterMode = FilterMode.Point;
        propBlock.SetTexture("tex", finalTex);
        LoadPreset();
        editCorners(new UnityEngine.Vector4(0.0f, 0.0f, 0.0f, 0.0f));
    }
    void ProcessFrame()
    {
        var size = useKinect ? sensor.DepthFrameSource.FrameDescription.LengthInPixels : 512 * 424;
        Color[] colors = new Color[size];
        for (int i = 0; i < size; i++)
            colors[i] = new Color(frameData[i], frameData[i], frameData[i], 1.0f);
        map.SetPixels(colors);
        map.Apply();
        Graphics.Blit(map, prettifyInput);
        //Time blurring
        computeShader.Dispatch(prettify_taa_kernel, 512 / 8, 424 / 8, 1);
        //Position blurring
        computeShader.Dispatch(prettify_blur_kernel, 512 / 8, 424 / 8, 1);
        //Just copy input to output (for disabling filters)
        //computeShader.Dispatch(prettify_copy_kernel, 512/8, 424/8, 1);
        RenderTexture.active = prettifyOutput;
        finalTex.ReadPixels(new Rect(0, 0, 512, 424), 0, 0);
        finalTex.Apply();
    }

    //ALIGNMENT
    int curCorner = 0;
    bool showHelp = false;
    public UnityEngine.Vector4[] corner = new UnityEngine.Vector4[] {
        new UnityEngine.Vector4(0.165f, 0.0078f, 600f, 150f),
        new UnityEngine.Vector4(1.0168f, 0.004f, 600f, 140f),
        new UnityEngine.Vector4(0.1556f, 0.41f, 500f, 100f),
        new UnityEngine.Vector4(1.0f, 0.49f, 500f, 100f)
    };
    void KeyInput()
    {
        //Help dialogue
        if (Input.GetKeyDown("h")) showHelp = !showHelp;
        //Corner control keys
        for (int i = 1; i <= 4; i++) if (Input.GetKeyDown(i.ToString())) curCorner = i;
        //Backtick selects all corners
        if (Input.GetKeyDown("`")) curCorner = 0;
        //WASD and arrow keys translate the viewport
        UnityEngine.Vector4 cornerChange =
            new UnityEngine.Vector4(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0, 0) * 0.03f;
        if (Input.GetKey("r")) cornerChange.z -= 20f;
        if (Input.GetKey("f")) cornerChange.z += 20f;
        if (Input.GetKey("t")) cornerChange.w -= 20f;
        if (Input.GetKey("g")) cornerChange.w += 20f;
        if (Input.GetKey("left shift")) cornerChange *= 10;
        editCorners(cornerChange * Time.deltaTime);

        //Visual keys
        if (Input.GetKeyDown("n")) NextTheme();
        if (Input.GetKeyDown("c")) propBlock.SetFloat("contour", (useContour = (useContour + 0.1f) % 1.1f));
        if (Input.GetKeyDown("b")) propBlock.SetFloat("blurLayers", (blurLayers = (blurLayers + 0.1f) % 1.1f));
        if (Input.GetKeyDown("x")) propBlock.SetFloat("useThemeTex", (useTex = (useTex + 0.1f) % 1.1f));

        if (Input.GetKey("left ctrl"))
        {
            if (Input.GetKeyDown("s")) SavePreset();
            if (Input.GetKeyDown("l")) LoadPreset();
            if (Input.GetKeyDown("q")) Application.Quit();
        }
    }
    //Shift corners by a linear amount
    void editCorners(UnityEngine.Vector4 shift)
    {
        for (int i = 0; i < 4; i++) if (curCorner == i + 1 || curCorner == 0) corner[i] += shift;
        propBlock.SetVector("corner1", corner[0]);
        propBlock.SetVector("corner2", corner[1]);
        propBlock.SetVector("corner3", corner[2]);
        propBlock.SetVector("corner4", corner[3]);
    }
    void SavePreset()
    {
        string presetDest = Application.persistentDataPath + "/save.dat";
        FileStream file;
        if (File.Exists(presetDest)) file = File.OpenWrite(presetDest);
        else file = File.Create(presetDest);
        new BinaryFormatter().Serialize(file, new float[]{
            corner[0].x,corner[0].y,corner[0].z,corner[0].w,
            corner[1].x,corner[1].y,corner[1].z,corner[1].w,
            corner[2].x,corner[2].y,corner[2].z,corner[2].w,
            corner[3].x,corner[3].y,corner[3].z,corner[3].w,
            useContour,blurLayers,useTex
        });
        file.Close();
        Debug.Log("Saved to: " + Application.persistentDataPath);
    }
    void LoadPreset()
    {
        string presetDest = Application.persistentDataPath + "/save.dat";
        FileStream file;
        if (File.Exists(presetDest)) file = File.OpenRead(presetDest);
        else { Debug.LogError("File not found"); return; }
        List<float> dat = new List<float>((float[])(new BinaryFormatter().Deserialize(file)));
        if (dat.Count <= 19) dat.AddRange(new[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f });
        corner[0] = new UnityEngine.Vector4(dat[0], dat[1], dat[2], dat[3]);
        corner[1] = new UnityEngine.Vector4(dat[4], dat[5], dat[6], dat[7]);
        corner[2] = new UnityEngine.Vector4(dat[8], dat[9], dat[10], dat[11]);
        corner[3] = new UnityEngine.Vector4(dat[12], dat[13], dat[14], dat[15]);
        useContour = dat[16];
        blurLayers = dat[17];
        useTex = dat[18];
        propBlock.SetFloat("contour", (useContour = dat[16]));
        propBlock.SetFloat("blurLayers", (blurLayers = dat[17]));
        propBlock.SetFloat("useThemeTex", (useTex = dat[18]));
        file.Close();
    }

    //KINECT INTERFACE
    private KinectSensor sensor;
    private DepthFrameReader reader;
    public ushort[] frameData;
    private bool useKinect = true;
    void SetupKinect()
    {
        sensor = KinectSensor.GetDefault();
        if (sensor == null) goto noCamera;
        sensor.Open();
        reader = sensor.DepthFrameSource.OpenReader();
        if (!sensor.IsAvailable) goto noCamera;
        frameData = new ushort[sensor.DepthFrameSource.FrameDescription.LengthInPixels];
        return;

    noCamera:
        Debug.LogError("Camera not found");
        useKinect = false;
        frameData = new ushort[512 * 424];
        return;
    }
    void ReadFrame()
    {
        //Read "fake frame" if Kinect is not connected
        if (useKinect == false)
        {
            if (frameData[0] == 0)
            {
                Color[] tex = new Color[512 * 424];
                Texture2D tex2d = new Texture2D(2, 2);
                tex2d.LoadImage(File.ReadAllBytes("Assets/topographic-sand-map/Textures/default_map.png"));
                tex = tex2d.GetPixels();
                for (int i = 0; i < 512 * 424; i++)
                {
                    frameData[i] = (ushort)(700 - (ushort)(tex[i].r * 200));
                }
            }
            if (Time.frameCount % 3 == 0) ProcessFrame();
        }
        if (reader == null) return;
        var frame = reader.AcquireLatestFrame();
        if (frame == null) return;
        frame.CopyFrameDataToArray(frameData);
        ProcessFrame();
        frame.Dispose();
    }
    void OnApplicationQuit()
    {
        if (reader != null) reader.Dispose();
        if (sensor != null && sensor.IsOpen) sensor.Close();
    }

    //GUI
    void OnGUI()
    {
        if (!showHelp) return;
        GUI.skin.label.fontSize = (int)(Screen.height / 26);
        GUI.Label(new Rect(100.0f, 0.0f, Screen.width - 100.0f, Screen.height), @"
            H - show help
            C - change contour line opacity
            B - blur layers
            N - change layer design

            Corner manipulation:
            Shift - increase speed
            1234 - select corner
            ` - select all corners
            WASD - Move
            R - raise base
            F - lower base
            T - raise max
            G - lower max
            ctrl + s - Save alignment
            ctrl + l - Load alignment
            ctrl + q - Quit
        ");
        float em = (Screen.height/13)+5f;
        GUI.skin.label.fontSize = (int)(em-5);
        GUI.Label(new Rect(50f, 50f, em, em), "2");
        GUI.Label(new Rect(Screen.width - 100f, 50f, em, em), "1");
        GUI.Label(new Rect(50f, Screen.height - 100f, em, em), "4");
        GUI.Label(new Rect(Screen.width - 100f, Screen.height - 100f, em, em), "3");
    }
}
