Shader "Unlit/RuralPaperBackground"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.93, 0.90, 0.85, 1)
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.15
        _SpotStrength ("Spot Strength", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _BaseColor;
            float _NoiseStrength;
            float _SpotStrength;

            // --- Ruido SIMPLEX fake usando dot + fract ---
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                float2 u = f * f * (3 - 2 * f);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 4.0; // Zoom al ruido
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Fondo base
                float3 col = _BaseColor.rgb;

                // Ruido de grano fino
                float grain = noise(i.uv * 12.0);
                col -= grain * _NoiseStrength * 0.15;

                // Manchas suaves tipo papel rural
                float blot = noise(i.uv * 1.5);
                col -= blot * _SpotStrength * 0.20;

                // Pequeñas variaciones verticales tipo fibra
                float fiber = sin(i.uv.y * 30.0) * 0.02;
                col -= fiber;

                return float4(col, 1);
            }
            ENDCG
        }
    }
}