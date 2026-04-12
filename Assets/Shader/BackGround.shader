Shader "Unlit/BackGround"
{
    Properties
    {
        _ColorA("Color A", Color) = (0.12,0.19,0.39,1)
        _ColorB("Color B", Color) = (0.95,0.58,0.68,1)
        _ColorC("Accent Color", Color) = (0.20,0.88,0.70,1)
        _Speed("Animation Speed", Range(0,2)) = 0.20
        _Scale("Pattern Scale", Range(0.1,10)) = 1.4
        _PatternStrength("Pattern Strength", Range(0,1)) = 0.6
        _Contrast("Contrast", Range(0,3)) = 1.1
        _Saturation("Saturation", Range(0,2)) = 1.0
        _TimeOffset("Time Offset", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        LOD 100

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _ColorA;
            fixed4 _ColorB;
            fixed4 _ColorC;
            float _Speed;
            float _Scale;
            float _PatternStrength;
            float _Contrast;
            float _Saturation;
            float _TimeOffset;

            // 2D hash -> 2D
            float2 hash21(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453123);
            }

            // value noise (smooth interpolation)
            float noise21(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = dot(hash21(i + float2(0.0, 0.0)), f - float2(0.0, 0.0));
                float b = dot(hash21(i + float2(1.0, 0.0)), f - float2(1.0, 0.0));
                float c = dot(hash21(i + float2(0.0, 1.0)), f - float2(0.0, 1.0));
                float d = dot(hash21(i + float2(1.0, 1.0)), f - float2(1.0, 1.0));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // fractional Brownian motion
            float fbm(float2 p)
            {
                float f = 0.0;
                float amp = 0.5;
                for (int i = 0; i < 5; i++)
                {
                    f += amp * noise21(p);
                    p = p * 2.0 + 100.0;
                    amp *= 0.5;
                }
                return f;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // simple 3-color palette blending
            fixed3 palette(float t, fixed3 a, fixed3 b, fixed3 c)
            {
                fixed3 m = lerp(a, b, smoothstep(0.0, 0.6, t));
                m = lerp(m, c, smoothstep(0.4, 1.0, t));
                return m;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // center coordinate and maintain aspect ratio
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = (uv - 0.5) * float2(aspect, 1.0) * _Scale;

                float time = _Time.y * _Speed + _TimeOffset;

                // layered noise & patterns
                float n = fbm(p + time * 0.1);

                float r = length(p);
                float ang = atan2(p.y, p.x);

                float rings = sin(ang * 3.0 + time * 0.5 + fbm(p * 2.0) * 2.0) * 0.5 + 0.5;
                float vortex = sin(r * 3.0 - time * 0.3 + fbm(p * 0.7) * 2.0) * 0.5 + 0.5;

                float pattern = lerp(n, rings, _PatternStrength);
                pattern = lerp(pattern, vortex, 0.25);

                float drift = fbm(p * 0.5 + time * 0.07) * 0.5 + 0.25;
                pattern = saturate(pattern * (0.6 + 0.8 * drift));

                fixed3 col = palette(pattern, _ColorA.rgb, _ColorB.rgb, _ColorC.rgb);

                // contrast & saturation
                col = (col - 0.5) * _Contrast + 0.5;
                float lum = dot(col, fixed3(0.299, 0.587, 0.114));
                col = lerp(fixed3(lum, lum, lum), col, _Saturation);

                return fixed4(col, 1.0);
            }

            ENDCG
        }
    }
    FallBack Off
}
