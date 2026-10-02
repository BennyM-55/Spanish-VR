Shader "Bozo/Bozo_Toon"
{
    Properties
    {
        [MainTexture][NoScaleOffset]_Texture2D("Texture2D", 2D) = "white" {}
        [Normal][NoScaleOffset]_NormalMap("NormalMap", 2D) = "bump" {}
        [Normal][NoScaleOffset]_IDMap("IDMap", 2D) = "bump" {}
        [Header(Colors Options)]
        [Space(10)]

        [Toggle(_USECUSTOMTEXTURE)]_UseCustomTexture("useCustomTexture", Float) = 0
        _Color_1("Color_1", Color) = (1, 0, 0, 0)
        _Color_2("Color_2", Color) = (0, 1, 0, 0)
        _Color_3("Color_3", Color) = (0, 0, 1, 0)
        _Color_4("Color_4", Color) = (0.9727613, 1, 0, 0)
        _Color_5("Color_5", Color) = (0, 0.9845986, 1, 0)
        _Color_6("Color_6", Color) = (1, 0, 0.988061, 0)
        _Color_7("Color_7", Color) = (1, 1, 1, 0)
        _Color_8("Color_8", Color) = (0.5031446, 0.5031446, 0.5031446, 0)
        _Color_9("Color_9", Color) = (0, 0, 0, 0)

        [Header(Outline Options)]
        [Space(10)]
        [Toggle(_USEOUTLINE)]_UseOutline("useOutline", Float) = 1
        _OutlineColor("OutlineColor", Color) = (0, 0, 0, 0)
        _OutlineDistance("OutlineDistance", Float) = 2

        [Header(Lighting Options)]
        [Space(10)]
        [Toggle(_USENORMALMAP)]_UseNormalMap("UseNormalMap", Float) = 0
        _LightHeightDirectionStrength("LightHeightDirectionStrength", Range(0, 1)) = 1
        _Brightness("Brightness",  Range(0, 2)) = 1

        [Header(Rimlight Options)]
        [Space(10)]
        [Toggle(_USERIMLIGHT)]_UseRimlight("Use Rimlight", Float) = 0
        _BaseColor("BaseColor", Color) = (1, 1, 1, 0)
        _RimlightPower("Rimlight Power", Range(0, 10)) = 0.5
        _RimlightSharpness("Rimlight Sharpness", Range(0, 1)) = 0.5
        _RimlightColor("RimLightColor", Color) = (0, 0, 0, 0)
        [Toggle(_RIMLIGHTOVERLAY)]_UseRimlightOverlay("Use Rimlight Overlay", Float) = 1
        [Header(Shadow Options)]
        [Space(10)]
        _ShadowColor("ShadowColor", Color) = (0.8862745, 0.8862745, 0.8862745, 0)
        _ShadowEdgeColor("ShadowEdgeColor", Color) = (1, 0.9058824, 0.827451, 0)
        _ShadowEdge("ShadowEdge", Range(0.001, 1)) = 0.1
        _ShadowSharpness("ShadowSharpness", Range(0.001, 1)) = 0.1
        _RecieveShadows("RecieveShadows",  Range(0, 1)) = 1

        [Header(Ambiant Options)]
        [Space(10)]
        [ToggleUI]_UseAmbiant("UseAmbiant", Float) = 1
        _AmbiantStrength("AmbiantStrength", Range(0, 3)) = 0.5
        [Header(MultiLight Options)]
        [ToggleUI]_UseMultipleLights("UseMultipleLights", Float) = 0
        _MultipleLightSharpness("MultipleLightSharpness", Range(0, 1)) = 0
        _MultipleStrength("MultipleStrength", Float) = 1

        [Header(Decal Options)]
        [Space(10)]
        [NoScaleOffset]_DecalMap("DecalMap", 2D) = "black" {}
        _DecalUVSet("DecalUVSet", Range(0, 1)) = 0
        _DecalBlend("DecalBlend", Range(0, 1)) = 0
        _DecalScale("DecalScale", Vector) = (1, 1, 0, 0)
        _DecalColor_1("DecalColor_1", Color) = (0, 0, 0, 0)
        _DecalColor_2("DecalColor_2", Color) = (0, 0, 0, 0)
        _DecalColor_3("DecalColor_3", Color) = (0, 0, 0, 0)
        [Header(Pattern Options)]
        [Space(10)]
        [NoScaleOffset]_PatternMap("PatternMap", 2D) = "black" {}
        _PatternUVSet("PatternUVSet", Range(0, 1)) = 0
        _PatternBlend("PatternBlend", Range(0, 1)) = 0
        _PatternScale("PatternScale", Vector) = (1, 1, 0, 0)
        _PatternColor_1("PatternColor_1", Color) = (0, 0, 0, 0)
        _PatternColor_2("PatternColor_2", Color) = (0, 0, 0, 0)
        _PatternColor_3("PatternColor_3", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
            Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        Pass
        {
            Tags {"LightMode"="ForwardBase"}

            Stencil
            {
                Ref 221
                WriteMask 221
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _USECUSTOMTEXTURE
            #pragma multi_compile _ _USEAMBIANT
            #pragma multi_compile _ _USENORMALMAP
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            #pragma multi_compile _ _USERIMLIGHT
		    #pragma multi_compile _ _RIMLIGHTOVERLAY
            #pragma multi_compile_fog


            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float3 normalOS : NORMAL;
                float4 tangentOS  : TANGENT;
                float4 vertexColor : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 pos : SV_POSITION;
                float3 positionWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float3x3 tangentToWorld: TEXCOORD4;
                float4 vertexColor : COLOR;
                SHADOW_COORDS(7)
                UNITY_FOG_COORDS(8)
                float3 viewDirWS   : TEXCOORD9;
                
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _Texture2D;
            float4 _Texture2D_ST;

            //BaseColor
            float4 _Color_1;
            float4 _Color_2;
            float4 _Color_3;
            float4 _Color_4;
            float4 _Color_5;
            float4 _Color_6;
            float4 _Color_7;
            float4 _Color_8;
            float4 _Color_9;

            //decal
            sampler2D _DecalMap;
            float4 _DecalMap_ST;
            float  _DecalUVSet;
            float  _DecalBlend;
            float4 _DecalScale;
            float4 _DecalColor_1;
            float4 _DecalColor_2;
            float4 _DecalColor_3;

            //pattern
            sampler2D _PatternMap;
            float4 _PatternMap_ST;
            float  _PatternUVSet;
            float  _PatternBlend;
            float4 _PatternScale;
            float4 _PatternColor_1;
            float4 _PatternColor_2;
            float4 _PatternColor_3;

            //lighting
            sampler2D _NormalMap;
            float _Brightness;
            float3 _BaseColor;
            float _LightHeightDirectionStrength;
            float _ShadowSharpness;
            float _ShadowEdge;
            float4 _ShadowColor;
            float4 _ShadowEdgeColor;
            float _AmbiantStrength;
            float _RecieveShadows;
            float _RimlightPower;
            float _RimlightSharpness;
            float3 _RimlightColor;

            Varyings vert (Attributes v)
            {
                Varyings o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(Varyings, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.positionWS = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1)).xyz;
                o.normalWS = UnityObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _Texture2D);
                o.uv1 = TRANSFORM_TEX(v.uv1, _Texture2D);
                o.vertexColor = v.vertexColor;

                float3 tangentWS = UnityObjectToWorldDir(v.tangentOS.xyz);
                float3 bitangentWS = cross(o.normalWS, tangentWS) * v.tangentOS.w;
                o.tangentToWorld = float3x3(tangentWS, bitangentWS, o.normalWS);

                //output.screenPos = ComputeScreenPos(output.pos);
                TRANSFER_SHADOW(o); 
                UNITY_TRANSFER_FOG(o,o.pos);

                return o;
            }

            float4 ApplyPattern(float4 tex, float flatTexture, float mask, Varyings i)
            {
                float2 uv = lerp(i.uv, i.uv1, _PatternUVSet);
                float2 scaleduv = uv * _PatternScale + ((_PatternScale * -1) / 2) + 0.5;

                float4 pattern = tex2D(_PatternMap, scaleduv);

                float4 color1 = lerp(0, _PatternColor_1, pattern.x);
                float4 color2 = lerp(0, _PatternColor_2, pattern.y);
                float4 color3 = lerp(0, _PatternColor_3, pattern.z);
                
                float4 combine = color1 + color2 + color3;

                float steppedMask = step(0.01, mask);
                steppedMask = steppedMask  * i.vertexColor.x;
                steppedMask = steppedMask * pattern.a;
                steppedMask = lerp(pattern.a, steppedMask, _PatternBlend);
                float4 blend  = lerp(combine, flatTexture * combine, _PatternBlend);

                float4 final = lerp(tex, blend, pattern.w * steppedMask);
                return float4(final.rgb, tex.a + pattern.a);
            }

            float4 ApplyDecal(float4 tex, float flatTexture, float mask, Varyings i)
            {
                float2 uv = lerp(i.uv, i.uv1, _DecalUVSet);
                //float2 scaleduv = uv * _DecalScale;
                float2 scaleduv = uv * _DecalScale + ((_DecalScale * -1) / 2) + 0.5;

                float4 decal = tex2D(_DecalMap, scaleduv);

                float4 color1 = lerp(0, _DecalColor_1, decal.x);
                float4 color2 = lerp(0, _DecalColor_2, decal.y);
                float4 color3 = lerp(0, _DecalColor_3, decal.z);

                float4 combine = color1 + color2 + color3;

                float steppedMask = step(0.06, mask);

                float4 blend  = lerp(combine, flatTexture * combine, _DecalBlend);

                float4 final = lerp(tex, blend, decal.a);
                return float4(final.rgb, tex.a);
            }

            float4 CustomColors(float4 tex, float4 vertexColors)
            {
               float4 color1 = lerp(0, _Color_1, tex.x);
               float4 color2 = lerp(0, _Color_2, tex.y);
               float4 color3 = lerp(0, _Color_3, tex.z);
               float4 color4 = lerp(0, _Color_4, tex.x);
               float4 color5 = lerp(0, _Color_5, tex.y);
               float4 color6 = lerp(0, _Color_6, tex.z);
               float4 color7 = lerp(0, _Color_7, tex.x);
               float4 color8 = lerp(0, _Color_8, tex.y);
               float4 color9 = lerp(0, _Color_9, tex.z);

               float4 combine1 = color1 + color2 + color3;
               float4 combine2 = color4 + color5 + color6;
               float4 combine3 = color7 + color8 + color9;

               float4 layer1 = lerp(0, combine1, vertexColors.x);
               float4 layer2 = lerp(layer1, combine2, vertexColors.y);
               float4 layer3 = lerp(layer2, combine3, vertexColors.z);

               return float4(layer3.rgb, tex.a);
            }

            fixed4 frag (Varyings v) : SV_Target
            {
                half3 normalWS = normalize(v.normalWS);
                // sample the texture
                half4 map = tex2D(_Texture2D, v.uv);
                half4 albedo = map;

                #ifdef _USENORMALMAP
                    half3 normalTS = UnpackNormal(tex2D(_NormalMap, v.uv));
                    normalWS = normalize(mul(normalTS, v.tangentToWorld));
                #endif

                #ifdef _USECUSTOMTEXTURE
                half4 flat = map.x + map.y + map.z;
                albedo = CustomColors(map, v.vertexColor);
                albedo = ApplyPattern(albedo, flat, map.x, v);
                albedo = ApplyDecal(albedo, flat, map.x, v);
                #endif

                // Initialize lighting
                half3 diffuse = 0;
                half3 specular = 0;
                half3 ambient = 0;
                #ifdef _USEAMBIANT
                ambient = ShadeSH9(half4(normalWS, 1.0)) * _AmbiantStrength;
                ambient = ambient * albedo.rgb;
                #endif

                // Main Light Properties
                half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                half3 heightlessLightDir = half3(lightDir.x, 0, lightDir.z);
                float3 dir = lerp(heightlessLightDir, lightDir, _LightHeightDirectionStrength);
                half3 lightColor = _LightColor0.rgb;
                fixed shadowAtten = SHADOW_ATTENUATION(v);

                // Toon Lighting
                half NdotL = dot(normalWS, dir);

                float shadow = smoothstep(0, _ShadowSharpness, NdotL);
                float shadowEdge = smoothstep(0 - _ShadowEdge, _ShadowSharpness - _ShadowEdge, NdotL);

                float3 shadowColors = lerp(_ShadowColor, _ShadowEdgeColor, shadowEdge);
                float3 litColor = lerp(shadowColors, _BaseColor, shadow);
                float3 recievedShadows = lerp(_ShadowColor, litColor, shadowAtten);

                float3 finalLight = lerp(litColor, recievedShadows, _RecieveShadows);
                finalLight = finalLight * _Brightness;

                diffuse += finalLight;

                half3 finalColor = albedo.rgb * (diffuse * lightColor) + ambient;

                #if _USERIMLIGHT
                    
                    float3 viewDir = normalize(_WorldSpaceCameraPos - v.positionWS);
                    float fresnel = pow(1.0 - saturate(dot(v.normalWS, viewDir)), _RimlightPower);
                    fresnel = smoothstep(0, _RimlightSharpness, fresnel);
                    #if _RIMLIGHTOVERLAY
                        finalColor = lerp(finalColor, _RimlightColor, fresnel);
                    #elif !_RIMLIGHTOVERLAY
                        finalColor += fresnel * finalColor * _RimlightColor;
                    #endif
                #endif

                //half3 finalColor = albedo.rgb * (diffuse * lightColor) + ambient;
                // apply fog
                UNITY_APPLY_FOG(v.fogCoord, finalColor);
                clip(albedo.a - 0.5f);
                return half4(finalColor.rgb, 1);
            }
            ENDCG
        }

        // ForwardAdd pass (additional lights)
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }
            Blend One One // Additive blending for additional lights
            ZWrite Off // No need to write depth for additive pass

            Stencil
            {
                Ref 221
                WriteMask 221
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _USEMILTIPLELIGHTS
            #pragma multi_compile _ _USECUSTOMTEXTURE
            #pragma multi_compile_fwdadd


            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertexColor : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID

            };

            struct v2f
            {

                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(4,5)

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _Texture2D;
            float4 _Texture2D_ST;

            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            //BaseColor
            float4 _Color_1;
            float4 _Color_2;
            float4 _Color_3;
            float4 _Color_4;
            float4 _Color_5;
            float4 _Color_6;
            float4 _Color_7;
            float4 _Color_8;
            float4 _Color_9;

            //decal
            sampler2D _DecalMap;
            float4 _DecalMap_ST;
            float  _DecalUVSet;
            float  _DecalBlend;
            float4 _DecalScale;
            float4 _DecalColor_1;
            float4 _DecalColor_2;
            float4 _DecalColor_3;

            //pattern
            sampler2D _PatternMap;
            float4 _PatternMap_ST;
            float  _PatternUVSet;
            float  _PatternBlend;
            float4 _PatternScale;
            float4 _PatternColor_1;
            float4 _PatternColor_2;
            float4 _PatternColor_3;
            float _MultipleLightSharpness;
            float _MultipleStrength;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _Texture2D);
                o.uv1 = TRANSFORM_TEX(v.uv1, _Texture2D);
                o.vertexColor = v.vertexColor;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                TRANSFER_SHADOW(o);

                return o;
            }

            float4 ApplyPattern(float4 tex, float flatTexture, float mask, v2f i)
            {
                float2 uv = lerp(i.uv, i.uv1, _PatternUVSet);
                //float2 scaleduv = uv * _DecalScale;
                float2 scaleduv = uv * _PatternScale + ((_PatternScale * -1) / 2) + 0.5;

                float4 pattern = tex2D(_PatternMap, scaleduv);

                float4 color1 = lerp(0, _PatternColor_1, pattern.x);
                float4 color2 = lerp(0, _PatternColor_2, pattern.y);
                float4 color3 = lerp(0, _PatternColor_3, pattern.z);
                
                float4 combine = color1 + color2 + color3;

                float steppedMask = step(0.01, mask);
                steppedMask = steppedMask  * i.vertexColor.x;
                steppedMask = steppedMask * pattern.a;
                steppedMask = lerp(pattern.a, steppedMask, _PatternBlend);
                float4 blend  = lerp(combine, flatTexture * combine, _PatternBlend);

                float4 final = lerp(tex, blend, pattern.w * steppedMask);
                return float4(final.rgb, tex.a + pattern.a);
            }

            float4 ApplyDecal(float4 tex, float flatTexture, float mask, v2f i)
            {
                float2 uv = lerp(i.uv, i.uv1, _DecalUVSet);
                //float2 scaleduv = uv * _DecalScale;
                float2 scaleduv = uv * _DecalScale + ((_DecalScale * -1) / 2) + 0.5;

                float4 decal = tex2D(_DecalMap, scaleduv);

                float4 color1 = lerp(0, _DecalColor_1, decal.x);
                float4 color2 = lerp(0, _DecalColor_2, decal.y);
                float4 color3 = lerp(0, _DecalColor_3, decal.z);

                float4 combine = color1 + color2 + color3;

                float steppedMask = step(0.06, mask);

                float4 blend  = lerp(combine, flatTexture * combine, _DecalBlend);

                float4 final = lerp(tex, blend, decal.a);
                return float4(final.rgb, tex.a);
            }

            float4 CustomColors(float4 tex, float4 vertexColors)
            {
               float4 color1 = lerp(0, _Color_1, tex.x);
               float4 color2 = lerp(0, _Color_2, tex.y);
               float4 color3 = lerp(0, _Color_3, tex.z);
               float4 color4 = lerp(0, _Color_4, tex.x);
               float4 color5 = lerp(0, _Color_5, tex.y);
               float4 color6 = lerp(0, _Color_6, tex.z);
               float4 color7 = lerp(0, _Color_7, tex.x);
               float4 color8 = lerp(0, _Color_8, tex.y);
               float4 color9 = lerp(0, _Color_9, tex.z);

               float4 combine1 = color1 + color2 + color3;
               float4 combine2 = color4 + color5 + color6;
               float4 combine3 = color7 + color8 + color9;

               float4 layer1 = lerp(0, combine1, vertexColors.x);
               float4 layer2 = lerp(layer1, combine2, vertexColors.y);
               float4 layer3 = lerp(layer2, combine3, vertexColors.z);

               return float4(layer3.rgb, tex.a);
            }

            

            fixed4 frag (v2f v) : SV_Target
            {
                #ifdef _USEMILTIPLELIGHTS

                    half3 normal = normalize(v.worldNormal);
                    // sample the texture
                    half4 map = tex2D(_Texture2D, v.uv);
                    half4 albedo = map;

                    #ifdef _USECUSTOMTEXTURE
                    half4 flat = map.x + map.y + map.z;
                    albedo = CustomColors(map, v.vertexColor);
                    albedo = ApplyPattern(albedo, flat, map.x, v);
                    albedo = ApplyDecal(albedo, flat, map.x, v);
                    albedo = half4(albedo.rgb, 1);
                    #endif


                    float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                    float NdotL = dot(normal, lightDir);

                    // Optional smooth transition
                    // toon = smoothstep(_Threshold - _Smooth, _Threshold + _Smooth, NdotL);

                    float atten = LIGHT_ATTENUATION(v);
                    float3 diffuse = NdotL * atten;
                    diffuse = smoothstep(0, _MultipleLightSharpness, diffuse);
                    diffuse =  saturate(diffuse) * _LightColor0.rbg;
                    diffuse = diffuse * _MultipleStrength * albedo;

                    return fixed4(diffuse, 1);

                #endif

                    return fixed4(0,0,0,1);
            

            }
            ENDCG
        }
        //Outline Pass
        Pass
        {
            Cull Front
            ZWrite On

            Stencil
            {
                Ref 221
                WriteMask 221
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _USEOUTLINE
            #include "UnityCG.cginc"


            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR; 
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR; 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _OutlineDistance;
            float4 _OutlineColor;


            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_OUTPUT(Varyings, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 norm = normalize(IN.normalOS);
                float3 pos = IN.positionOS.xyz + IN.normalOS * (_OutlineDistance * 0.001) * IN.color.a;
                OUT.color = IN.color;
                OUT.positionHCS = UnityObjectToClipPos(pos);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                #ifdef _USEOUTLINE

                    float finalMask = 1;
                    finalMask = finalMask * IN.color.a;

                    clip(IN.color.a - 0.5);
                    return float4(_OutlineColor.rgb , IN.color.a);
                    return _OutlineColor;

                #else

                    discard;
                    //clip(1 - 0.5);
                    return float4(0,0,0,0);

                #endif
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode"="ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }


    }
}
