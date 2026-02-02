Shader "Custom/Gradient"
{
    Properties
    {
        _ColorTop ("Top Color", Color) = (0.2, 0.6, 1, 1)
        _ColorBottom ("Bottom Color", Color) = (0.05, 0.1, 0.3, 1)
        _Angle ("Angle", Range(0, 360)) = 0
        _Midpoint ("Midpoint", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0.01, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            fixed4 _ColorTop;
            fixed4 _ColorBottom;
            float _Angle;
            float _Midpoint;
            float _Smoothness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float rad = _Angle * 3.14159265 / 180.0;
                float2 dir = float2(cos(rad), sin(rad));

                float t = dot(i.uv - 0.5, dir) + 0.5;

                float halfSmooth = _Smoothness * 0.5;
                t = smoothstep(_Midpoint - halfSmooth, _Midpoint + halfSmooth, t);

                fixed4 col = lerp(_ColorBottom, _ColorTop, t);

                // Vertex color — нужен для SpriteRenderer
                col *= i.color;

                return col;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
