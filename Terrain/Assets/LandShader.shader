﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/LandShader"
{
	/*Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}*/
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		//LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				//float2 uv : TEXCOORD0;
                float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
                //float2 uv : TEXCOORD0;
                float4 color : COLOR;
			};

			//sampler2D _MainTex;
			//float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}