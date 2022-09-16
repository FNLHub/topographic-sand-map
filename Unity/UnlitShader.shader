Shader "Unlit/UnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        segments ("Height colors", 2D) = "white" {}
        lineWidth ("Line Width", Float) = 10
        lineCount ("Lines per full depth", Float) = 5
        contour ("Contour Intensity", Float) = 1
        blurLayers ("Blur Layers", Float) = 0
        rawDepth ("Raw depth",Float) = 0
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D segments;
            float lineWidth;
            float lineCount;
            float blurLayers;
            float contour;
            float rawDepth;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                float v = tex2D(_MainTex, i.uv).r;
                float dx = ddx(v);
                float dy = ddy(v);
                float dv = sqrt(abs(dx*dx)+abs(dy*dy));
                float clampedV = frac(v*lineCount);
                float lineCloseness = smoothstep(1,0,clampedV/dv/lineWidth);
                lineCloseness += smoothstep(1,0,(1-clampedV)/dv/lineWidth);
                lineCloseness = 1-(lineCloseness*contour);
                float2 sec = float2(lerp(v-clampedV/lineCount+0.5/lineCount,v,blurLayers),0);
                return lerp(tex2D(segments,sec),v,rawDepth)*lineCloseness;
            }
            ENDCG
        }
    }
}
