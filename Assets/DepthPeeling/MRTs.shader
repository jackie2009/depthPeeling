Shader "Custom/MRTs"
{
	Properties
	{
		_Color("Color", color) = (1, 0, 0, 1)
			_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader
	{
	
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
		LOD 100
     cull off
        
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			  #pragma multi_compile_fwdbase
			#include "UnityCG.cginc"
#include "Lighting.cginc"
 #include "autolight.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{				
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal: NORMAL;
					float3 color : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				 SHADOW_COORDS(3)  
			};

			struct fout 
			{
				float4 rt0 : SV_Target0;
				float4 rt1 : SV_Target1;
			 			
			};
			
			float4 _Color;
        sampler2D _MainTex;
     uniform   sampler2D DepthRendered;
      uniform int DepthRenderedIndex;
			// *********
       
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normal = v.normal;
			 	 // 环境光
					fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
 
					// 将法线从对象空间转换到世界空间,
					// 由于法线是一个三维矢量，因此我们只需要截取_World2Object的前三行前三列即可
					fixed3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld,v.normal));
					
					// 得到光方向
					fixed3 WorldSpaceLightDir = normalize(_WorldSpaceLightPos0.xyz);
					
					// 漫反射计算,saturate函数是Cg提供的一种函数，它的作用是可以把参数截取到[0, 1]的范围内。
					fixed3 diffuse = _LightColor0.rgb  *saturate(dot(worldNormal,WorldSpaceLightDir));
					o.color = ambient + diffuse;
 
                o.screenPos = ComputeScreenPos(o.pos);
                TRANSFER_SHADOW(o) 
				return o;
			}
			
			fout frag(v2f i)
			{
				float depth = i.pos.z / i.pos.w;
		 fixed shadow = SHADOW_ATTENUATION(i);
                  half4 col=tex2D(_MainTex,i.uv)*_Color*shadow;
         col.rgb *=i.color;
                  clip(col.a-0.001);
                  float renderdDepth=tex2D(DepthRendered,i.screenPos.xy/i.screenPos.w).r;
                  if(DepthRenderedIndex>0&&depth>=renderdDepth-0.000001) discard;
			 
				fout o;
		 
				o.rt0=depth;
				o.rt1=col;
	 
				
				return o;
			}
			ENDCG
		}
	}
}