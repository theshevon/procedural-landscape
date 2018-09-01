//UNITY_SHADER_NO_UPGRADE

Shader "Unlit/Water01Shader"
{

    Properties
    {
        _Transparency("Transparency", Range(0.0, 1)) = 0.5
        _Amplitude("Amplitude", Range(0,5)) = 0.82
        _Frequency("Frequency", Range(0,5)) = 2.73
        _Speed("Speed", Range(0,5)) = 2.24
    }

    SubShader
    {

        Tags {"Queue" = "Transparent" "RenderType"="Transparent" }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };


            float _Transparency;
            float _Speed;
            float _Amplitude;
            float _Distance;

            // Implementation of the vertex shader
            // Builds the object
            v2f vert(appdata v)
            {
                
                v.vertex.y += sin(_Time.y * _Speed + v.vertex.x * _Frequency) * _Amplitude +
                sin(_Time.y * _Speed + v.vertex.x * _Frequency*3) * _Amplitude;

                v2f o;
                o.vertex += v.vertex;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.color = v.color;
                return o;
            }
            
            // Implementation of the fragment shader
            // Colours in the object
            fixed4 frag(v2f v) : SV_Target
            {
                fixed4 col = v.color;
                col.a = _Transparency;
                return col;
            }
            ENDCG
        }
    }
}
