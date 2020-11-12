Shader "Tutorial/022_stencil_buffer/write"{
	//show values to edit in inspector
	Properties{
		[IntRange] _StencilRef("Stencil Reference Value", Range(0,255)) = 0

		[PerRendererData]  _MainTex("Sprite Texture", 2D) = "white" { }
	_Color("Tint", Color) = (1.000000,1.000000,1.000000,1.000000)
	_StencilComp("Stencil Comparison", Float) = 8.000000
	_Stencil("Stencil ID", Float) = 0.000000
	_StencilOp("Stencil Operation", Float) = 0.000000
	_StencilWriteMask("Stencil Write Mask", Float) = 255.000000
	_StencilReadMask("Stencil Read Mask", Float) = 255.000000
	_ColorMask("Color Mask", Float) = 15.000000
	[Toggle(UNITY_UI_ALPHACLIP)]  _UseUIAlphaClip("Use Alpha Clip", Float) = 0.000000
	}

		SubShader{
		//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry-1"}

		//stencil operation
		Stencil{
			Ref[_StencilRef]
			Comp Always
			Pass Replace
		}

		Pass{
		//don't draw color or depth
		Blend Zero One
		ZWrite Off

		CGPROGRAM
		#include "UnityCG.cginc"

		#pragma vertex vert
		#pragma fragment frag

		struct appdata {
			float4 vertex : POSITION;
		};

		struct v2f {
			float4 position : SV_POSITION;
		};

		v2f vert(appdata v) {
			v2f o;
			//calculate the position in clip space to render the object
			o.position = UnityObjectToClipPos(v.vertex);
			return o;
		}

		fixed4 frag(v2f i) : SV_TARGET{
			return 0;
		}

		ENDCG
	}
	}
}