Shader "Custom/DrawOutsideDoF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" }
        LOD 200

        Pass
        {
            ZWrite Off // Disable depth writes
            Blend SrcAlpha OneMinusSrcAlpha // Standard alpha blending
            Cull Off // Render both sides (optional)

            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Preserve alpha for smooth edges
                return tex;
            }
            ENDCG
        }
    }
}
