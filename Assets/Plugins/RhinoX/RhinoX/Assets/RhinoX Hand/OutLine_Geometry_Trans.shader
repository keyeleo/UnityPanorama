Shader "Unlit/OutLine_Geometry_Trans"
{
	Properties
	{
		[HDR]_Color("Color",color) = (1,1,1,1)
		_Pow("Pow",range(0,128)) = 3
		_MainTex("MainTex",2D) = "black"{}
    }
    SubShader
    {
        Tags { "queue"="Geometry+1" }
        LOD 100
		Pass
        {
			blend one one
			zwrite on
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
                return 0;
            }
            ENDCG
        }
        Pass
        {
			blend srcalpha oneminussrcalpha
			zwrite on
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal:NORMAL;
				fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half dot:TEXCOORD1;
				half3 v:TEXCOORD2;
				half3 n:NORMAL;
            };

			fixed4 _Color;

			fixed _Pow;
			sampler2D _MainTex;
			float4 _MainTex_ST;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.v =normalize(WorldSpaceViewDir(v.vertex));
				o.n =UnityObjectToWorldNormal(v.normal);

				//o.dot = 1-saturate(dot(normalize(WorldSpaceViewDir(v.vertex)), UnityObjectToWorldNormal(v.n)));
				//o.dot = pow(o.dot, _Pow)*v.color.r*v.color.r;
				//o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				half3 v = normalize(i.v);
				half3 n = normalize(i.n);
				half d = 1-saturate(dot(i.v,i.n));
				d = pow(d, _Pow);
				fixed4 c = d*_Color;
				//c.a *= i.color.a;
				//c = saturate(c);
                return c;
            }
            ENDCG
        }
    }
}
