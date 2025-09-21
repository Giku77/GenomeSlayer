// File: XRayRim_Occluded_URP.shader
Shader "Custom/XRayRim_Occluded_URP"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0.82, 1, 1)
        _RimPow       ("Rim Power", Range(0.5, 8)) = 2.2
        _Alpha        ("Alpha", Range(0, 1)) = 0.7
        _DepthBias    ("Depth Bias", Range(0, 0.05)) = 0.005
        _Thickness    ("Smooth Thickness", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags{
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back

        Pass
        {
            Name "XRayRim"

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma prefer_hlslcc gles

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _OutlineColor;
            float  _RimPow;
            float  _Alpha;
            float  _DepthBias;
            float  _Thickness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
                float4 screenPos  : TEXCOORD2;
                float3 posWS      : TEXCOORD3;   // ★ 월드좌표를 넘긴다
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.posWS     = posWS;                          // ★ 전달
                OUT.positionCS = TransformWorldToHClip(posWS);
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS  = GetWorldSpaceViewDir(posWS);
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            // 0~1이 아니라 부드럽게 0..1 마스크
            float occlusionMask(float2 uv, float fragEye)
            {
            #if defined(REQUIRES_SCENE_DEPTH_TEXTURE)
                float scene01 = SampleSceneDepth(uv);
                float sceneEye = LinearEyeDepth(scene01);
                float edge1 = sceneEye + _DepthBias;
                float edge2 = edge1  + max(_Thickness, 1e-5);
                return smoothstep(edge1, edge2, fragEye);
            #else
                return 0.0;
            #endif
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 N = SafeNormalize(IN.normalWS);
                float3 V = SafeNormalize(IN.viewDirWS);

                // 스크린 UV
                float2 uv = IN.screenPos.xy / IN.screenPos.w;

                // ★ 버텍스에서 넘긴 월드좌표를 뷰공간으로 변환해 EyeDepth 계산
                float3 posVS = TransformWorldToView(IN.posWS);
                float  fragEye = abs(posVS.z);

                // 가려짐 마스크
                float m = occlusionMask(uv, fragEye);

                // 림
                float ndotv = saturate(dot(N, V));
                float rim = pow(1.0 - ndotv, _RimPow);

                float3 baseCol = _OutlineColor.rgb;
                float3 bright  = baseCol * 1.2;
                float3 rimCol  = lerp(baseCol, bright, rim);

                float3 col = rimCol * m;
                float  a   = _Alpha * m;

                return float4(col, a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
