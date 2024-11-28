Shader "Unlit/LaserShader"
{
    Properties
    {
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;

        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float4 pos : POSITION;
        float2 uv : TEXTCOORD0;
        float4 color : COLOR;

        UNITY_VERTEX_OUTPUT_STEREO
    };

    float4 _Color;
    sampler2D _MainTex;

    ENDCG

    SubShader
    {
        Pass
        {
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
            Blend SrcAlpha One //OneMinusSrcAlpha
            Cull Back
            ZWrite On

            CGPROGRAM
            #pragma vertex vert2
            #pragma fragment frag2
           
            v2f vert2(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = _Color;
                return o;
            }

            half4 frag2(v2f i) : COLOR
            {
                float4 color = _Color * tex2D(_MainTex, i.uv);
                return half4(color);
            }

            ENDCG
        }
    }
}
