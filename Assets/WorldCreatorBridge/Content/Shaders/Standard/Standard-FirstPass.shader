// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Nature/Terrain/WC Standard" {
    Properties {
        // used in fallback on old cards & base map        
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}        
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
        _ColorMap("ColorMap (RGB)", 2D) = "white" 
        _OffsetSize("Offset / Size", Vector) = (0,0,0,0)
    }

    SubShader {
        Tags {
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
        }

        CGPROGRAM
        #pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
        #pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.
        #pragma target 3.0
        // needs more than 8 texcoords
        #pragma exclude_renderers gles
        #include "UnityPBSLighting.cginc"

        #pragma multi_compile_local __ _NORMALMAP

        #define TERRAIN_STANDARD_SHADER
        #define TERRAIN_INSTANCED_PERPIXEL_NORMAL
        #define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
        #include "TerrainSplatmapCommon.cginc"

        half _Metallic0;
        half _Metallic1;
        half _Metallic2;
        half _Metallic3;

        half _Smoothness0;
        half _Smoothness1;
        half _Smoothness2;
        half _Smoothness3;

        sampler2D _ColorMap;
        float4 _OffsetSize;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            half4 splat_control;
            half weight = 1;
            fixed4 mixedDiffuse;
            float2 uv = IN.tc.xy * _OffsetSize.zw + _OffsetSize.xy;
            half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
            SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, o.Normal);
            o.Albedo = mixedDiffuse.rgb * tex2D(_ColorMap, uv).rgb;
            o.Alpha = weight;
            o.Smoothness = mixedDiffuse.a;
            o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
        }
        ENDCG

        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }

    Dependency "AddPassShader"    = "Hidden/TerrainEngine/Splatmap/WCStandard-AddPass"
    Dependency "BaseMapShader"    = "Hidden/TerrainEngine/Splatmap/WCStandard-Base"
    Dependency "BaseMapGenShader" = "Hidden/TerrainEngine/Splatmap/WCStandard-BaseGen"

    Fallback "Nature/Terrain/Diffuse"
}
