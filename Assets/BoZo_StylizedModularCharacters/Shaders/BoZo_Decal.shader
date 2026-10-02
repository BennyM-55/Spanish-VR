Shader "Hidden/BoZo_Decal"
{
    Properties
    {
        [MainTexture]_DecalTex ("Decal Texture", 2D) = "transparent" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _EdgeSoftness ("Edge Softness", Range(0, 0.5)) = 0.25
        _BakeOffset ("Bake Z Offset", Range(-1, 1)) = 0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Overlay+100"
        }

        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            sampler2D _DecalTex;
            float4 _DecalTex_ST;

            float4x4 _DecalWorldToLocal;
            float _BakeOffset;
            float _EdgeSoftness;
            fixed4 _Color;

            v2f vert(appdata input)
            {
                v2f output;

                output.worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.worldNormal = UnityObjectToWorldNormal(input.normal);

                float2 uv = input.uv;
                uv.y = 1.0 - uv.y;

                // UV-space baking projection.
                output.pos = float4(uv * 2.0 - 1.0, _BakeOffset, 1.0);

                output.uv = input.uv;

                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float3 localPos = mul(_DecalWorldToLocal, float4(input.worldPos, 1.0)).xyz;

                float3 distFromCenter = abs(localPos);

                // Same as your original values.
                float3 edgeFade = 1.0 - smoothstep(0.75 - 0.3, 0.75, distFromCenter);
                float totalFade = edgeFade.x * edgeFade.y * edgeFade.z;

                float2 decalUV = localPos.xy + 0.5;
                fixed4 decalCol = tex2D(_DecalTex, decalUV);

                float3 projectorForward = -_DecalWorldToLocal[2].xyz;

                float alignment = dot(normalize(input.worldNormal), normalize(projectorForward));
                float alphaFade = saturate(alignment * 0.5);

                decalCol.a *= alphaFade;
                decalCol.a *= totalFade;

                clip(decalCol.a - 0.001);

                fixed4 finalCol = decalCol * _Color;
                return finalCol;
            }

            ENDCG
        }
    }

    FallBack Off
}