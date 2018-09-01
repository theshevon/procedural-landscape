Shader "Unlit/LandShader"
{
    Properties
    {
        _PointLightColor ("Point Light Color", Color) = (0, 0, 0)
        _PointLightPosition ("Point Light Position", Vector) = (0.0, 0.0, 0.0)
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _BumpTex ("Bump Texture", 2D) = "bump" {}

    }
    SubShader
    {
        Pass
        {
            Tags {"RenderType" = "Opaque" "LightMode" = "ForwardBase"}
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            uniform float3 _PointLightColor;
            uniform float3 _PointLightPosition;
            uniform sampler2D _MainTex;
            uniform sampler2D _BumpTex;            

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
                float3 normal : NORMAL;
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
                                
                return o;
            }
            
            fixed4 frag(v2f v) : SV_Target
            {
                float3 interpolatedNormal = normalize(v.worldNormal);
                
                v.color = tex2D(_MainTex, v.uv_main);
                
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
                //float specN = _Glossiness; // Values>>1 give tighter highlights
                float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);
                float3 R = normalize((2 * LdotN * interpolatedNormal) - L);
                
                //float3 spe = fAtt * _PointLightColor.rgb * Ks * pow(saturate(dot(V, R)), specN);

                // Combine Phong illumination model components
                float4 returnColor = (0.0f, 0.0f, 0.0f, 0.0f);
                returnColor.rgb += amb.rgb + dif.rgb ; //+ spe.rgb;
                returnColor.a = v.color.a;

                return returnColor;
            }
            ENDCG
        }
    }
}
