Shader "PetArena/Painted Stone Core"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _CoreColor ("Core", Color) = (0.3,0.88,0.5,1)
        _Pattern ("Element engraving", Float) = 1
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0; };
            sampler2D _MainTex;fixed4 _CoreColor;float _Pattern;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv);
                // Recolor only the painted green crystal, preserving its gold mount and shading.
                float core=saturate((c.g-max(c.r,c.b))*7);
                float light=dot(c.rgb,float3(.21,.71,.08));
                float2 p=i.uv*float2(23,7);
                float veinSignal;
                if(_Pattern<.5)veinSignal=abs(sin((p.x+p.y)*12));                    // metal facets
                else if(_Pattern<1.5)veinSignal=abs(sin(p.y*16+sin(p.x*8)*2));     // leaf veins
                else if(_Pattern<2.5)veinSignal=abs(sin(p.y*14+sin(p.x*10)));      // water ripples
                else if(_Pattern<3.5)veinSignal=abs(sin(p.x*13+sin(p.y*5)*3));     // fire wisps
                else veinSignal=min(abs(sin(p.x*11)),abs(sin(p.y*13)));           // mineral seams
                float engraving=(1-smoothstep(.02,.13,veinSignal))*.18;
                fixed3 crystal=_CoreColor.rgb*(light*1.5+.12)+engraving;
                c.rgb=lerp(c.rgb,crystal,core);
                return c*i.color;
            }
            ENDCG
        }
    }
}
