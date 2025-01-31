Shader "Custom/FullscreenOutline"
{
    Properties
    {
        _ColorThreshold("Color Threshold", Range(0.001, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "FullscreenOutlinePass"
            
            // Render States
            Cull Off
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Properties
            TEXTURE2D(_BlitTexture);       // Render buffer source texture
            SAMPLER(sampler_BlitTexture);
            float _ColorThreshold;

            // Vertex Shader
            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.pos = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Fragment Shader
            half4 frag(VertexOutput i) : SV_Target
            {
                // Use a fallback resolution for debugging
                #ifdef UNITY_SHADER_DEBUG
                float2 texelSize = float2(1.0 / 1920.0, 1.0 / 1080.0); // Example for 1080p resolution
                #else
                float2 texelSize = 1.0 / _ScreenParams.xy; // Normal behavior
                #endif

                // Sample current pixel
                float4 currentColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv);

                // Sample neighboring pixels
                float4 leftColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv + float2(-texelSize.x, 0));
                float4 rightColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv + float2(texelSize.x, 0));
                float4 upColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv + float2(0, texelSize.y));
                float4 downColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv + float2(0, -texelSize.y));

                // Calculate color differences ignoring the alpha channel
                float diffLeft = distance(currentColor.rgb, leftColor.rgb);
                float diffRight = distance(currentColor.rgb, rightColor.rgb);
                float diffUp = distance(currentColor.rgb, upColor.rgb);
                float diffDown = distance(currentColor.rgb, downColor.rgb);

                // Debug: Visualize differences
                // return float4(diffLeft, diffRight, diffUp, 1);

                // Check if any neighbor exceeds the threshold
                if (diffLeft > _ColorThreshold || diffRight > _ColorThreshold ||
                    diffUp > _ColorThreshold || diffDown > _ColorThreshold)
                {
                    return float4(0, 0, 0, 1); // Outline color (black)
                }

                // Return the original pixel color
                return currentColor;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/UniversalPipeline/FallbackError"
}