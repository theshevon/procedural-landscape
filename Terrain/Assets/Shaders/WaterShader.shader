// Shader script to colour and light the vertices of mesh representing water 
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
        // light properties
        _PointLightColor ("Point Light Color", Color) = (0, 0, 0)
        _PointLightPosition ("Point Light Position", Vector) = (0.0, 0.0, 0.0)
        
        // water properties
        _MainTex("Main Texture", 2D) = "white" {}
        _Transparency("Transparency", Range(0.0, 1)) = 0.5
        _Amplitude("Amplitude", Range(0,5)) = 0.82
        _Frequency("Frequency", Range(0,5)) = 2.73
        _Speed("Speed", Range(0,5)) = 2.24
        _Glossiness("Glossiness", Range(0,500)) = 250
        _BlendFactor("Blend Factor", Range(0,1)) = 0.75
    }
    
    SubShader
    {
        
        // since water will be transparent, ensure that it is rendered
        // after every other object
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
            uniform float _BlendFactor;
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
            
            // calculate and return the new position of the vertex
            float4 getNewPosition(float4 vertex)
            {
                // update the vertex position
                vertex.y = sin(_Time.y * _Speed + vertex.x * _Frequency) * _Amplitude +
                cos(_Time.y * _Speed + vertex.x * _Frequency * 2) * _Amplitude;
                
                return vertex;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // calculate the bitangent (vector perpendicular to normal and tangent)
                float4 bitangent = (0,0,0,0);
                bitangent.xyz = cross(v.normal, v.tangent);
                
                // offset the vertex to get wave animation
                v.vertex = getNewPosition(v.vertex);

                // convert Vertex position and corresponding normal into world coordinates
                float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
    
                float4 vertexAndTangent = getNewPosition(v.vertex + v.tangent * 0.01);
                float4 vertexAndBitangent = getNewPosition(v.vertex + bitangent * 0.01);
                
                // get the new (approximated) tangent and bitangent
                float4 newTangent = (vertexAndTangent - v.vertex); 
                float4 newBitangent = (vertexAndBitangent - v.vertex);  
                
                // calculate the new (approximated) normal 
                float4 newNormal = (0,0,0,0);
                newNormal.xyz = cross(newTangent, newBitangent);
                v.normal = newNormal;
                
                // convert nomarl into world coordinates
                float3 worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), v.normal.xyz));
                
                // transform vertex in world coordinates to camera coordinates
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                  
                o.worldVertex = worldVertex;
                o.worldNormal = worldNormal;
                
                o.uv = v.uv;
               
                return o;
            }
            
            fixed4 frag(v2f v) : SV_Target
            {
                
                // blend texture and vertex colours
                v.color = (tex2D(_MainTex, v.uv) * _BlendFactor) + (v.color * (1.0f - _BlendFactor));
                
                float3 interpolatedNormal = normalize(v.worldNormal);
                                                                                
                // Calculate ambient RGB intensities
                float Ka = 1;
                float3 amb = v.color.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * Ka;

                // calculate diffuse RBG reflections, we save the results of L.N because we will use it again
                // (when calculating the reflected ray in our specular component)
                float fAtt = 1;
                float Kd = 1;
                float3 L = normalize(_PointLightPosition - v.worldVertex.xyz);
                float LdotN = dot(L, v.worldNormal.xyz);
                float3 dif = fAtt * _PointLightColor.rgb * Kd * v.color.rgb * saturate(LdotN);
                
                // calculate specular reflections
                float Ks = 1;
                float specN = _Glossiness; 
                float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);
                float3 R = normalize((2 * LdotN * interpolatedNormal) - L);
                float3 spe = fAtt * _PointLightColor.rgb * Ks * pow(saturate(dot(V, R)), specN);

                // combine Phong illumination model components
                float4 returnColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
                returnColor.rgb = amb.rgb + dif.rgb + spe.rgb;
                returnColor.a = _Transparency;
                
                return returnColor;
            }
            
            ENDCG
        }
    }
}
