Shader "Custom/OutlineBackface_Occluded_URP"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0.82, 1, 1)
        _OutlineWidth ("Outline Width (world)", Range(0, 0.02)) = 0.005
        _Alpha        ("Alpha", Range(0,1)) = 0.8
        _DepthBias    ("Depth Bias", Range(0, 1)) = 0.005
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
        Cull Front    // 백면만 그림(앞면을 cull)

        Pass
        {
            Name "OutlineBackface"

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _CAMERA_DEPTH_TEXTURE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _OutlineColor;
            float  _OutlineWidth;
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
                float4 screenPos  : TEXCOORD0;
                float3 posWS      : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // float occlusionMask(float2 uv, float fragEye)
            // {
            // #if defined(REQUIRES_SCENE_DEPTH_TEXTURE)
            //     float scene01 = SampleSceneDepth(uv);
            //     float sceneEye = LinearEyeDepth(scene01);
            //     float e1 = sceneEye + _DepthBias;
            //     float e2 = e1 + max(_Thickness, 1e-5);
            //     return smoothstep(e1, e2, fragEye);
            // #else
            //     return 0.0;
            // #endif
            // }
            float occlusionMask(float2 uv, float fragEye)
            {
                // 깊이 텍스처에서 raw depth(0..1) 샘플
                float sceneRaw = SampleSceneDepth(uv);

                // 0..1 선형 깊이 (버전별로 1개 or 2개 인자 오버로드가 있으니 2인자 버전으로 강제)
                float scene01 = Linear01Depth(sceneRaw, _ZBufferParams);

                // Eye 공간 깊이(near~far 선형 보간)
                // _ProjectionParams.y = Near, _ProjectionParams.z = Far
                float sceneEye = lerp(_ProjectionParams.y, _ProjectionParams.z, scene01);

                // 깜빡임 완충 (bias/thickness)
                float e1 = sceneEye + _DepthBias;
                float e2 = e1 + max(_Thickness, 1e-5);

                return smoothstep(e1, e2, fragEye);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // 노멀 방향으로 팽창(월드 기준)
                float3 nWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 pWS = TransformObjectToWorld(IN.positionOS.xyz) + nWS * _OutlineWidth;

                OUT.posWS = pWS;
                OUT.positionCS = TransformWorldToHClip(pWS);
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.screenPos.xy / IN.screenPos.w;
                float3 posVS = TransformWorldToView(IN.posWS);
                float fragEye = abs(posVS.z);

                float m = occlusionMask(uv, fragEye);
                //float m = 1.0;

                float3 col = _OutlineColor.rgb * m;
                float  a   = _Alpha * m;
                return float4(col, a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}