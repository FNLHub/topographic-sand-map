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
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
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


            //WATER TEXTURE loosely based off of https://www.shadertoy.com/view/Mt2SzR
            float random(float x) { return frac(sin(x) * 10000.); }
            float noise(float2 p) { return random(p.x + p.y * 10000.); }
            float2 sw(float2 p) { return float2(floor(p.x), floor(p.y)); }
            float2 se(float2 p) { return float2(ceil(p.x), floor(p.y)); }
            float2 nw(float2 p) { return float2(floor(p.x), ceil(p.y)); }
            float2 ne(float2 p) { return float2(ceil(p.x), ceil(p.y)); }
            float smoothNoise(float2 p) {
                float2 interp = smoothstep(0., 1., frac(p));
                float s = lerp(noise(sw(p)), noise(se(p)), interp.x);
                float n = lerp(noise(nw(p)), noise(ne(p)), interp.x);
                return lerp(s, n, interp.y);
            }
            float fractalNoise(float2 p) {
                return (smoothNoise(p)*4+smoothNoise(p*2.)*2+smoothNoise(p*4))/7;
            }
            float water(float2 p) {
                float t=_Time*1.7;
                return 0.7+fractalNoise(p + float2(fractalNoise(p+t), fractalNoise(p-t)))*2.0;
            }



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

            float texSize = 3.0;

            v2f vert (appdata v) {
                v2f o;
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                float aspect = _ScreenParams.y/_ScreenParams.x;
                float2 stablePosition = float2(i.uv.x*aspect,i.uv.y);
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
                float4 color = lerp(tex2D(themeCol,float2(0,sec)),tex3D(theme,float3(stablePosition*texSize,sec)),useThemeTex);
                //Blend with contour line
                color = float4(color.rgb*lineCloseness,lerp(color.a,1,1-lineCloseness));
                //Blend with water
                color = float4(color.rgb*lerp(water(stablePosition*20.0)*float3(0.25,0.5,1),float3(1,1,1),color.a),1.);

                return color;
            }
            ENDCG
        }
    }
}
