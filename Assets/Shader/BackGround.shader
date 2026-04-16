Shader "Unlit/BackGround"
{
    Properties
    {
        _ColorA("Color A (Base)", Color) = (0.12,0.19,0.39,1)
        _ColorB("Color B (Mid)", Color) = (0.95,0.58,0.68,1)
        _ColorC("Accent Color", Color) = (0.20,0.88,0.70,1)
        
        [Header(Animation and Pattern)]
        _Speed("Animation Speed", Range(0,2)) = 0.15
        _Scale("Pattern Scale", Range(0.1,10)) = 2.0
        _WarpStrength("Fluid Warp Strength", Range(0, 2)) = 0.5 // 흐물거리는 강도
        
        [Header(Color Settings)]
        _Contrast("Contrast", Range(0,3)) = 1.2
        _Saturation("Saturation", Range(0,2)) = 1.0
        _ColorBands("Color Banding (Steps)", Range(1, 20)) = 8.0 // 발라트로 특유의 색상 층
        
        [Header(Retro CRT Effects)]
        _ScanlineStrength("Scanline Strength", Range(0, 0.2)) = 0.05
        _VignetteStrength("Vignette Strength", Range(0, 2)) = 0.8
        
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
            float _WarpStrength;
            float _Contrast;
            float _Saturation;
            float _ColorBands;
            float _ScanlineStrength;
            float _VignetteStrength;
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
                for (int i = 0; i < 4; i++)
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
                o.uv.y = 1.0 - o.uv.y; // UV 방향 보정 (플랫폼 차이 방지)
                return o;
            }

            fixed3 palette(float t, fixed3 a, fixed3 b, fixed3 c)
            {
                // 부드러운 그라데이션이 아닌, 약간의 끊어짐(Banding)을 주어 카드가 넘어가는 듯한 질감 형성
                t = floor(t * _ColorBands) / _ColorBands; 
                
                fixed3 m = lerp(a, b, smoothstep(0.0, 0.5, t));
                m = lerp(m, c, smoothstep(0.4, 1.0, t));
                return m;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = (uv - 0.5) * float2(aspect, 1.0) * _Scale;
                
                float time = _Time.y * _Speed + _TimeOffset;

                // --- 1. Domain Warping (유체처럼 일그러지는 효과) ---
                float2 q = p;
                // 사인 코사인 파동으로 공간을 구부립니다.
                q.x += sin(p.y * 2.5 + time * 0.8) * _WarpStrength;
                q.y += cos(p.x * 2.5 + time * 0.6) * _WarpStrength;

                // 일그러진 공간(q)을 바탕으로 노이즈 생성
                float n = fbm(q * 1.5 - time * 0.3);

                // --- 2. Color Palette & Banding ---
                // 노이즈 값을 팔레트에 넣어 색상을 추출
                fixed3 col = palette(n, _ColorA.rgb, _ColorB.rgb, _ColorC.rgb);

                // --- 3. Contrast & Saturation ---
                col = (col - 0.5) * _Contrast + 0.5;
                float lum = dot(col, fixed3(0.299, 0.587, 0.114));
                col = lerp(fixed3(lum, lum, lum), col, _Saturation);

                // --- 4. Retro CRT Effects (발라트로 특유의 레트로 질감) ---
                // 화면 중심에서 멀어질수록 어두워지는 비네팅(Vignette)
                float vignette = length(uv - 0.5) * 1.5;
                col *= 1.0 - vignette * _VignetteStrength;

                // 미세한 가로줄 스캔라인 (해상도에 비례)
                float scanline = sin(uv.y * _ScreenParams.y * 1.5);
                col -= (scanline * 0.5 + 0.5) * _ScanlineStrength;

                return fixed4(saturate(col), 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}