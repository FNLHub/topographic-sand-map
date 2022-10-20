Shader "Unlit/UnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        segments ("Height colors", 2D) = "white" {}
        lineWidth ("Line Width", Float) = 10
        contour ("Contour Intensity", Float) = 1
        blurLayers ("Blur Layers", Float) = 0
        rawDepth ("Raw depth",Float) = 0
        bigScaleZ ("BigScaleZ",Float) = 1

        heightRange("Height Range",float) = 100
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D segments;
            float lineWidth;
            float blurLayers;
            float contour;
            float rawDepth;
            float bigScaleZ;
            float gaussSize;

            float heightRange;
            float4 corner1;
            float4 corner2;
            float4 corner3;
            float4 corner4;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                //transform position based on the four corners
                float4 pos = lerp(lerp(corner2,corner1,i.uv.x),lerp(corner4,corner3,i.uv.x),i.uv.y);
                //Gaussian blur the sampling
                float v = tex2D(_MainTex, pos.xy).r;
                v = (v-pos.z)/(pos.w-pos.z)*10;
                //Find derivative to calculate closeness to contour line
                float dx = ddx(v);
                float dy = ddy(v);
                float dv = sqrt(abs(dx*dx)+abs(dy*dy));
                float clampedV = frac(v);
                float lineCloseness = smoothstep(1,0,clampedV/dv/lineWidth);
                lineCloseness += smoothstep(1,0,(1-clampedV)/dv/lineWidth);
                lineCloseness = 1-(lineCloseness*contour);
                //Get position on color map
                float sec = lerp((v-clampedV)/10+0.05f,v/10+0.05f,blurLayers)*bigScaleZ;
                return lerp(tex2D(segments,float2(sec,0)),v,rawDepth)*lineCloseness;
            }
            ENDCG
        }
    }
}
