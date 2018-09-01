// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/PhongShaderWater"
{
    Properties
    {
        _PointLightColor ("Point Light Color", Color) = (0, 0, 0)
        _PointLightPosition ("Point Light Position", Vector) = (0.0, 0.0, 0.0)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float3 _PointLightColor;
            uniform float3 _PointLightPosition;

            struct vertIn
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float4 color : COLOR;
            };

            struct vertOut
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 worldVertex : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };
            
            vertOut vert(vertIn v)
            {
                vertOut o;

                // Convert Vertex position and corresponding normal into world coords.
                float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
                float3 worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), v.normal.xyz));
                
                // Transform vertex in world coordinates to camera coordinates
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                
                o.worldVertex = worldVertex;
                o.worldNormal = worldNormal;

                return o;
            }
            
            fixed4 frag(vertOut v) : SV_Target
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
                float specN = 50; // Values>>1 give tighter highlights
                float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);
                float3 R = normalize((2 * LdotN * interpolatedNormal) - L);
                float3 spe = fAtt * _PointLightColor.rgb * Ks * pow(saturate(dot(V, R)), specN);

                // Combine Phong illumination model components
                float4 returnColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
                returnColor.rgb = amb.rgb + dif.rgb + spe.rgb;
                returnColor.a = v.color.a;
                
                return returnColor;
            }
            ENDCG
        }
    }
}
