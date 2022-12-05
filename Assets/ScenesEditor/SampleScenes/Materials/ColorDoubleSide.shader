Shader "XH/ColorDoubleSide"
{
	Properties
	{
		_Color("Color", Color) = (1.0,1.0,1.0,1.0)
	}

	SubShader
	{
		Tags { "Queue" = "Transparent+10" }

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off		
			ZTest Always
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			fixed4 _Color;

			v2f vert(appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{

			    UNITY_SETUP_INSTANCE_ID(i);

				return _Color * i.color;
			}

			ENDCG
		}
	}
}
