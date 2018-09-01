
Shader "Unlit/WaterShader"
{
    Properties
    {
        // light properties
        _PointLightColor ("Point Light Color", Color) = (0, 0, 0)
        _PointLightPosition ("Point Light Position", Vector) = (0.0, 0.0, 0.0)
        
        // water properties
        _Transparency("Transparency", Range(0.0, 1)) = 0.5
        _Amplitude("Amplitude", Range(0,5)) = 0.82
        _Frequency("Frequency", Range(0,5)) = 2.73
        _Speed("Speed", Range(0,5)) = 2.24
        _Glossiness("Glossiness", Range(0,100)) = 50
    }
    SubShader
    {
        
        Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
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
            float _Transparency;
            float _Speed;
            float _Amplitude;
            float _Frequency;
            float _Glossiness;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float4 worldVertex : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // offset the vertex to get wave animation
                v.vertex.y += sin(_Time.y * _Speed + v.vertex.x * _Frequency) * _Amplitude +
                cos(_Time.y * _Speed + v.vertex.x * _Frequency*2) * _Amplitude;

                // Convert Vertex position and corresponding normal into world coords.
                float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
                float3 worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), v.normal.xyz));
                
                // Transform vertex in world coordinates to camera coordinates
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                
                o.worldVertex = worldVertex;
                o.worldNormal = worldNormal;
                
                TRANSFER_SHADOW(o);

                return o;
            }
            
            fixed4 frag(v2f v) : SV_Target
            {
                float3 interpolatedNormal = normalize(v.worldNormal);
                
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
                float specN = _Glossiness; // Values>>1 give tighter highlights
                float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);
                float3 R = normalize((2 * LdotN * interpolatedNormal) - L);
                float3 spe = fAtt * _PointLightColor.rgb * Ks * pow(saturate(dot(V, R)), specN);

                // Combine Phong illumination model components
                float4 returnColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
                returnColor.rgb = amb.rgb + dif.rgb + spe.rgb;
                fixed shadow = SHADOW_ATTENUATION(v);
                returnColor.rgb *= shadow;
                returnColor.a = _Transparency;
                
                return returnColor;
            }
            ENDCG
        }
        
        Pass
        {
            Tags {"LightMode" = "ShadowCaster"} // renders the depth of the waves into the shadow
            
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
