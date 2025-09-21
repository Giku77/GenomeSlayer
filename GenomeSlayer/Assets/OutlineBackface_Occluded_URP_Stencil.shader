Shader "Custom/OutlineBackface_Occluded_URP_Stencil"
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

        // ▼ Pass 0: 화면에 '채우지 않고' 앞면 영역만 스텐실에 기록
        Pass
        {
            Name "MaskFront"
            Cull Back               // 앞면만 남김
            ZTest LEqual
            ZWrite Off
            ColorMask 0

            Stencil {
                Ref 1
                Comp Always
                Pass Replace        // 그려지는 곳을 스텐실=1로 표시
            }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS:POSITION; };
            struct Varyings   { float4 positionCS:SV_POSITION; };

            Varyings vert(Attributes IN){
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float4 frag(Varyings IN):SV_Target { return 0; } // 실제로는 아무 것도 안 그림
            ENDHLSL
        }

        // ▼ Pass 1: 뒷면 팽창 + 스텐실 바깥(=외곽)에서만 그리기 + 가려짐 마스크(m)
        Pass
        {
            Name "OutlineBackface"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front               // 백면만 그림

            Stencil {
                Ref 1
                Comp NotEqual        // 스텐실이 1이 '아닌'(외곽) 곳만 통과
            }

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

            // 씬 깊이 → EyeDepth(버전 호환)
            float occlusionMask(float2 uv, float fragEye)
            {
                float sceneRaw = SampleSceneDepth(uv);
                float scene01  = Linear01Depth(sceneRaw, _ZBufferParams);
                float sceneEye = lerp(_ProjectionParams.y, _ProjectionParams.z, scene01);

                float e1 = sceneEye + _DepthBias;
                float e2 = e1 + max(_Thickness, 1e-5);
                return smoothstep(e1, e2, fragEye); // 0..1
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 nWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 pWS = TransformObjectToWorld(IN.positionOS.xyz) + nWS * _OutlineWidth;

                OUT.posWS     = pWS;
                OUT.positionCS = TransformWorldToHClip(pWS);
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv    = IN.screenPos.xy / IN.screenPos.w;
                float3 posVS = TransformWorldToView(IN.posWS);
                float  fragEye = abs(posVS.z);

                float m = occlusionMask(uv, fragEye);   // 가려졌을 때만 0→1
                float3 col = _OutlineColor.rgb * m;
                float  a   = _Alpha * m;
                return float4(col, a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
