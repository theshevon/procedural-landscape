// Shader Script to colour and light the vertices of mesh representing the ocean 
// surface for COMP30019 Project 01.
// The code used in the application of Phong's illumination model is attributed  
// to Alexandre Mutel (SharpDX), Jeremy Nicholson (UoM), Chris Ewin (UoM) & 
// Alex Zable (UoM).
//
// Written by Brendan Leung & Shevon Mendis, September 2018.

Shader "Unlit/WaterShader"
{
    Properties
    {
        // Light-related properties.
        _PointLightColor ("Point Light Color", Color) = (0, 0, 0)
        _PointLightPosition ("Point Light Position", Vector) = (0.0, 0.0, 0.0)
        
        // Water properties.
        _MainTex("Main Texture", 2D) = "white" {}
        _Transparency("Transparency", Range(0.0, 1)) = 0.75
        _Amplitude("Amplitude", Range(0,5)) = 0.1
        _Frequency("Frequency", Range(0,5)) = 1.28
        _Speed("Speed", Range(0,5)) = 1.5
        _Glossiness("Glossiness", Range(0,500)) = 200
        _BlendFactor("Blend Factor", Range(0,1)) = 0.5
    }
    
    SubShader
    {
        // Since water is transparent, ensure that it is rendered
        // after every other object.
        Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag 
            #include "UnityCG.cginc"
            
            // declarations
            uniform float3 _PointLightColor;
            uniform float3 _PointLightPosition;
            uniform sampler2D  _MainTex;
            float _BlendFactor;
            float _Transparency;
            float _Speed;
            float _Amplitude;
            float _Frequency;
            float _Glossiness;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldVertex : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };
            
            // Calculate and return the new position of the vertex.
            float4 getNewPosition(float4 vertex)
            {
                // The vertex needs to participate a sinusoidal waveform.
                vertex.y = sin(_Time.y * _Speed + vertex.x * _Frequency) * _Amplitude +
                cos(_Time.y * _Speed + vertex.x * _Frequency * 2) * _Amplitude;
                
                return vertex;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // Calculate the bitangent.
                float4 bitangent = (0,0,0,0);
                bitangent.xyz = cross(v.normal, v.tangent);
                
                // Offset the vertex to get wave animation.
                v.vertex = getNewPosition(v.vertex);

                // Convert vertex position into world coordinates.
                float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
                
                // Approximate a new tangent and bitangent by first offsetting the new vertex position 
                // by a fraction of the tangent and bitangent and then deducting the new vertex them from  
                // vectors obtained.
                float4 vertexAndTangent = getNewPosition(v.vertex + v.tangent * 0.01);
                float4 vertexAndBitangent = getNewPosition(v.vertex + bitangent * 0.01);
                float4 newTangent = vertexAndTangent - v.vertex; 
                float4 newBitangent = vertexAndBitangent - v.vertex;  
                
                // Calculate the new approximated normal.
                float4 newNormal = (0,0,0,0);
                newNormal.xyz = cross(newTangent, newBitangent);
                v.normal = newNormal;
                
                // Convert the normal into world coordinates.
                float3 worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), v.normal.xyz));
                
                // Transform vertex in world coordinates to camera coordinates.
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                  
                o.worldVertex = worldVertex;
                o.worldNormal = worldNormal;
                
                o.uv = v.uv;
               
                return o;
            }
            
            fixed4 frag(v2f v) : SV_Target
            {
                
                // Blend texture and vertex colours.
                v.color = (tex2D(_MainTex, v.uv) * _BlendFactor) + (v.color * (1.0f - _BlendFactor));
                
                float3 interpolatedNormal = normalize(v.worldNormal);
                                                                                
                // Calculate ambient RGB intensities.
                float Ka = 1;
                float3 amb = v.color.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * Ka;

                // Calculate diffuse RBG reflections, we save the results of L.N because we will use it again
                // (when calculating the reflected ray in our specular component).
                float fAtt = 1;
                float Kd = 1;
                float3 L = normalize(_PointLightPosition - v.worldVertex.xyz);
                float LdotN = dot(L, v.worldNormal.xyz);
                float3 dif = fAtt * _PointLightColor.rgb * Kd * v.color.rgb * saturate(LdotN);
                
                // Calculate specular reflections.
                float Ks = 1;
                float specN = _Glossiness; 
                float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);
                float3 R = normalize((2 * LdotN * interpolatedNormal) - L);
                float3 spe = fAtt * _PointLightColor.rgb * Ks * pow(saturate(dot(V, R)), specN);

                // Combine Phong's illumination model components.
                float4 returnColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
                returnColor.rgb = amb.rgb + dif.rgb + spe.rgb;
                returnColor.a = _Transparency;
                
                return returnColor;
            }
            
            ENDCG
        }
    }
}
