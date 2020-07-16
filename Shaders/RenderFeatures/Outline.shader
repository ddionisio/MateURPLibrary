Shader "Hidden/M8/Render Features/Outline"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "../Library/DepthNormals.hlsl"

            #pragma shader_feature_local USE_DEPTH
            #pragma shader_feature_local USE_DEPTH_CAMERA_THRESHOLD

            #pragma shader_feature_local USE_NORMALS

            #pragma shader_feature_local USE_FADE
            #pragma shader_feature_local USE_FADE_EXP

            #pragma shader_feature_local USE_DISTORTION

            uniform half _Thickness;
            uniform half4 _EdgeColor;

#ifdef USE_DEPTH
            uniform half _DepthThresholdMin, _DepthThresholdMax;
#endif

#ifdef USE_NORMALS
            uniform half _NormalThresholdMin, _NormalThresholdMax;
#endif

#if USE_DEPTH_CAMERA_THRESHOLD
            uniform float _DepthNormalThreshold;
            uniform float _DepthNormalThresholdScale;

            float4x4 _ClipToView;
#endif

#if USE_FADE
            uniform float _FadeDistance;
            uniform float _FadeExponentialDensity;
#endif

#if USE_DISTORTION
            uniform float _DistortionStrength;

            TEXTURE2D(_DistortionTexture); SAMPLER(sampler_DistortionTexture);
            float4 _DistortionTexture_ST;
#endif
                        
            TEXTURE2D(_CameraColorTexture); SAMPLER(sampler_CameraColorTexture);
            float4 _CameraColorTexture_TexelSize;

            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

            TEXTURE2D(_CameraDepthNormalsTexture); SAMPLER(sampler_CameraDepthNormalsTexture);

            struct Attributes
            {
                float4 position : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;

#if USE_DEPTH_CAMERA_THRESHOLD
                float3 viewSpaceDir : TEXCOORD1;
#endif

                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.position.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;

#if USE_DEPTH_CAMERA_THRESHOLD
                output.viewSpaceDir = mul(_ClipToView, output.vertex).xyz;
#endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv;

                float4 original = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uv);
                                
                float offset_positive = +ceil(_Thickness * 0.5);
                float offset_negative = -floor(_Thickness * 0.5);
                float2 texel_size = 1.0 / float2(_CameraColorTexture_TexelSize.z, _CameraColorTexture_TexelSize.w);

                float left = texel_size.x * offset_negative;
                float right = texel_size.x * offset_positive;
                float top = texel_size.y * offset_negative;
                float bottom = texel_size.y * offset_positive;

#if USE_DISTORTION
                float2 distort = SAMPLE_TEXTURE2D(_DistortionTexture, sampler_DistortionTexture, TRANSFORM_TEX(uv, _DistortionTexture)).rg;
                distort = ((distort / 0.5) - 1);

                uv += distort * _DistortionStrength;
#endif

                float2 uv0 = uv + float2(left, top);
                float2 uv1 = uv + float2(right, bottom);
                float2 uv2 = uv + float2(right, top);
                float2 uv3 = uv + float2(left, bottom);

#ifdef USE_NORMALS
                float3 n0 = SampleNormal(TEXTURE2D_ARGS(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture), uv0);
                float3 n1 = SampleNormal(TEXTURE2D_ARGS(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture), uv1);
                float3 n2 = SampleNormal(TEXTURE2D_ARGS(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture), uv2);
                float3 n3 = SampleNormal(TEXTURE2D_ARGS(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture), uv3);

                float3 nd1 = n1 - n0;
                float3 nd2 = n3 - n2;
                float nEdge = sqrt(dot(nd1, nd1) + dot(nd2, nd2));
                nEdge = smoothstep(_NormalThresholdMin, _NormalThresholdMax, nEdge);
#else
                float nEdge = 0;
#endif

#ifdef USE_DEPTH
                float d0 = SampleDepth(TEXTURE2D_ARGS(_CameraDepthTexture, sampler_CameraDepthTexture), uv0);
                float d1 = SampleDepth(TEXTURE2D_ARGS(_CameraDepthTexture, sampler_CameraDepthTexture), uv1);
                float d2 = SampleDepth(TEXTURE2D_ARGS(_CameraDepthTexture, sampler_CameraDepthTexture), uv2);
                float d3 = SampleDepth(TEXTURE2D_ARGS(_CameraDepthTexture, sampler_CameraDepthTexture), uv3);

    #ifdef USE_DEPTH_CAMERA_THRESHOLD
        #ifndef USE_NORMALS
                float3 n0 = SampleNormal(TEXTURE2D_ARGS(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture), uv0);
        #endif

                float3 viewNormal = n0 * 2 - 1;
                float NdotV = 1 - dot(viewNormal, -input.viewSpaceDir);

                float normalThreshold01 = saturate((NdotV - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
                float normalThreshold = normalThreshold01 * _DepthNormalThresholdScale + 1;

                float depthThreshold = _DepthThresholdMax * d0 * normalThreshold;
    #else
                float depthThreshold = _DepthThresholdMax;
    #endif

                float dEdge = length(float2(d1 - d0, d3 - d2));
                dEdge = smoothstep(_DepthThresholdMin, depthThreshold, dEdge);
#else
                float dEdge = 0;
#endif

                half edge = max(dEdge, nEdge);

#if USE_FADE
                float d = SampleDepth(TEXTURE2D_ARGS(_CameraDepthTexture, sampler_CameraDepthTexture), input.uv);
                d = clamp(d * (_ProjectionParams.z / _FadeDistance), 0, 1);

    #if USE_FADE_EXP
                d = 1.0 / exp((1 - d) * _FadeExponentialDensity);
    #endif

                half edgeAlpha = _EdgeColor.a * (1.0 - d);
#else
                half edgeAlpha = _EdgeColor.a;
#endif

                half4 output;
                output.rgb = lerp(original.rgb, _EdgeColor.rgb, edge * edgeAlpha);
                output.a = original.a;

                return output;
            }

            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}
