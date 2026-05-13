Shader "Sprites/Tilemap Dissolve"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale", Range(1, 80)) = 24
        _EdgeWidth ("Edge Width", Range(0.001, 0.35)) = 0.08
        [HDR] _EdgeColor ("Edge Color", Color) = (0.0, 1.0, 1.8, 1)
        _EdgeIntensity ("Edge Intensity", Range(0, 4)) = 1.35
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Dissolve;
            float _NoiseScale;
            float _EdgeWidth;
            fixed4 _EdgeColor;
            float _EdgeIntensity;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.texcoord;
                output.color = input.color * _Color;

                #ifdef PIXELSNAP_ON
                output.vertex = UnityPixelSnap(output.vertex);
                #endif

                return output;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float Fbm(float2 uv)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    value += ValueNoise(uv) * amplitude;
                    uv = uv * 2.03 + 17.17;
                    amplitude *= 0.5;
                }
                return value;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float noise = Fbm(input.uv * _NoiseScale);
                float dissolve = saturate(_Dissolve);
                float visible = smoothstep(dissolve - _EdgeWidth, dissolve + _EdgeWidth, noise);
                float edge = smoothstep(dissolve - _EdgeWidth, dissolve, noise) - smoothstep(dissolve, dissolve + _EdgeWidth, noise);

                fixed4 color = tex2D(_MainTex, input.uv) * input.color;
                color.rgb += _EdgeColor.rgb * edge * _EdgeIntensity;
                color.a *= visible;
                return color;
            }
            ENDCG
        }
    }
}
