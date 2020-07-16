#ifndef M8_DEPTHNORMALS_INCLUDED
#define M8_DEPTHNORMALS_INCLUDED

//from UnityCG.cginc
inline float3 DecodeViewNormalStereo(float4 enc4)
{
    float kScale = 1.7777;
    float3 nn = enc4.xyz * float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
    float g = 2.0 / dot(nn.xyz, nn.xyz);
    float3 n;
    n.xy = g * nn.xy;
    n.z = g - 1;
    return n;
}

// Z buffer depth to linear 0-1 depth
// Handles orthographic projection correctly
inline float Linear01Depth(float z)
{
    float isOrtho = unity_OrthoParams.w;
    float isPers = 1.0 - unity_OrthoParams.w;
    z *= _ZBufferParams.x;
    return (1.0 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
}

inline float3 SampleNormal(TEXTURE2D_PARAM(tex, sampler_tex), float2 uv)
{
    float4 raw = SAMPLE_TEXTURE2D(tex, sampler_tex, uv);
    return DecodeViewNormalStereo(raw);
}

inline float SampleDepth(TEXTURE2D_PARAM(tex, sampler_tex), float2 uv)
{
    float d;
#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
    d = SAMPLE_TEXTURE2D_ARRAY(tex, sampler_tex, uv, unity_StereoEyeIndex).r;
#else
    d = SAMPLE_DEPTH_TEXTURE(tex, sampler_tex, uv);
#endif
    return Linear01Depth(d);
}

#endif