Shader "Unlit/UnlitShader"
{
    Properties
    {
        tex ("Texture", 2D) = "white" {}
        theme ("Theme texture", 3D) = "white" {}
        themeCol ("Theme colors", 2D) = "white" {}
        lineWidth ("Line Width", Float) = 10
        contour ("Contour Intensity", Float) = 1
        useThemeTex ("Theme Texture Intensity",Float) = 1
        blurLayers ("Blur Layers", Float) = 0
        layerCount ("Layer Count",Float) = 1

        corner1("Corner 1",Vector) = (0,0,0,1)
        corner2("Corner 2",Vector) = (1,0,0,1)
        corner3("Corner 3",Vector) = (0,1,0,1)
        corner4("Corner 4",Vector) = (1,1,0,1)

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D tex;

            sampler3D theme;
            sampler2D themeCol;
            float layerCount;

            float lineWidth;
            float blurLayers;
            float contour;
            float useThemeTex;

            float4 corner1;
            float4 corner2;
            float4 corner3;
            float4 corner4;

            v2f vert (appdata v) {
                v2f o;
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                //transform position based on the four corners
                float4 pos = lerp(lerp(corner2,corner1,i.uv.x),lerp(corner4,corner3,i.uv.x),i.uv.y);
                //Gaussian blur the sampling
                float v = tex2D(tex, pos.xy).r;
                v = (v-pos.z)/(pos.w-pos.z);
                //Find derivative to calculate closeness to contour line
                float dx = ddx(v);
                float dy = ddy(v);
                float dv = sqrt(abs(dx*dx)+abs(dy*dy));

                v *= layerCount;

                float fracv = frac(v);
                float lineCloseness = lerp(1.0,smoothstep(0,1,min(fracv,1-fracv)/dv/lineWidth),contour);
                //Get position on color map
                float sec = lerp((v-frac(v))/layerCount+0.05f,v/layerCount+0.05f,blurLayers);
                float4 color = lerp(tex2D(themeCol,float2(0,sec)),tex3D(theme,float3(pos.x*512/424,pos.y,sec)),useThemeTex);
                return color*lineCloseness;
            }
            ENDCG
        }
    }
}
