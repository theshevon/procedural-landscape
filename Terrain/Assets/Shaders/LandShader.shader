// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/LandShader"
{
    Properties
    {
        _PointLightColor ("Point Light Color", Color) = (0, 0, 0)
        _PointLightPosition ("Point Light Position", Vector) = (0.0, 0.0, 0.0)
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _BumpTex ("Bump Texture", 2D) = "bump" {}
        _Glossiness ("Glossiness", Range(0,1)) = 0.5
        //_Metallic ("Metallic", Range(0,1)) = 0.0
        _BlendFct ("Blend Factor", Float) = 0.5

    }
    SubShader
    {
        Pass
        {
            Tags {"LightMode" = "ForwardBase"}
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            uniform float3 _PointLightColor;
            uniform float3 _PointLightPosition;
            uniform sampler2D _MainTex;
            uniform float _BlendFct;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float4 color : COLOR;
                float2 uv_main : TEXCOORD0;
                float2 uv_bump : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv_main : TEXCOORD0;
                float2 uv_bump : TEXCOORD1;
                float4 worldVertex : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
            };
            
          
            v2f vert(appdata v)
            {
                v2f o;
                
                // Transform vertex in world coordinates to camera coordinates
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                
                o.uv_main = v.uv_main;
                o.uv_bump = v.uv_bump;
                
                
                // Convert Vertex position and corresponding normal into world coords.
                float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
                float3 worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), v.normal.xyz));
                
                o.worldVertex = worldVertex;
                o.worldNormal = worldNormal;
                
                
                
                //TRANSFER_SHADOW(o);
                
                return o;
            }
            
            fixed4 frag(v2f v) : SV_Target
            {
                float3 interpolatedNormal = normalize(v.worldNormal);
                
                if (v.pos.y < 0) {
                    v.color = (tex2D(_MainTex, v.uv_main) * _BlendFct) + (v.color * (1.0f - _BlendFct));
                }else{
                    v.color = tex2D(_MainTex, v.uv_main);
                }
                                
                // Calculate ambient RGB intensities
                float Ka = 1;
                float3 amb = v.color.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * Ka;

                // Calculate diffuse RBG reflections, we save the results of L.N because we will use it again
                // (when calculating the reflected ray in our specular component)
                float fAtt = 1;
                float Kd = 1;
                float3 L = normalize(_PointLightPosition - v.worldVertex.xyz);
                float LdotN = dot(L, v.worldNormal.xyz);
                float3 dif = fAtt * _PointLightColor.rgb * Kd * v.color.rgb * saturate(LdotN);
                
                // Calculate specular reflections
                float Ks = 1;
                float specN = 0.5; // Values>>1 give tighter highlights
                float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);
                float3 R = normalize((2 * LdotN * interpolatedNormal) - L);
                float3 spe = fAtt * _PointLightColor.rgb * Ks * pow(saturate(dot(V, R)), specN);

                // Combine Phong illumination model components
                float4 returnColor = tex2D(_MainTex, v.uv_main);
                returnColor.rgb += amb.rgb + dif.rgb + spe.rgb;
                returnColor.a = v.color.a;
                
                //fixed shadow = SHADOW_ATTENUATION(v);
                //returnColor.rgb *= v.color * shadow;
                
                return returnColor;
            }
            ENDCG
        }
        
        Pass
        {
            Tags {"LightMode" = "ShadowCaster"} // renders the depth of the terrain into the shadow
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_ShadowCaster
            #include "UnityCG.cginc"
            
            struct appdata{
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                V2F_SHADOW_CASTER;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o) //create shadow
                return o;
            }
            
            float4 frag(v2f v) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(v)
            }
            
            ENDCG
        }
    }
}
