// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ZeldaWater_V2" {
	Properties {
    	_MainTex ("Texture", 2D) = "white" { }
    	_Speed ("Speed", Range (0.001, 1.0)) = .2
        _SpeedX ("SpeedX", Range (0.001, 1.0)) = .3
        _SpeedY ("SpeedY", Range (0.001, 1.0)) = .3
        _Intensity ("Intensity", Range (1.0, 10.0)) = 3.0
		_Frequency ("Frequency", Range (1.0, 10.0)) = 4.0
		_Angle ("Angle", Range (1.0, 17.0)) = 7.0
		_Delta ("Delta", Range (1.0, 100.0)) = 20.0
		_Intence ("Intence", Range (10.0, 1000.0)) = 400.0
		_Emboss ("Emboss", Range (0.01, 1.0)) = .3
		_HeightScale ("HeightScale", Range (0.001, 3.0)) = 0.5
		_NoiseScale ("NoiseScale", Range (0.001, 2.0)) = 0.02
		_WaveSpeed ("WaveSpeed", Range (0.01, 10.0)) = 2.5
	}
	SubShader {
		Pass {		
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "noiseSimplex.cginc"
			
			sampler2D	_MainTex;
			float		_Speed;
			float	    _SpeedX;
			float	    _SpeedY;
			float	    _Intensity;
			float		_Frequency;
			float		_Angle;
			float		_Delta;
			float		_Intence;
			float		_Emboss;
			float		_HeightScale;
			float		_NoiseScale;
			float		_WaveSpeed;

			struct v2f {
			    float4  pos : SV_POSITION;
			    float2  uv : TEXCOORD0;
			};

			float4 _MainTex_ST;
			
			const int steps = 8;
			const float PI = 3.1415926535897932;
			
			float coli(float2 coord)
			  {
			    float delta_theta = 2.0 * PI / float(_Angle);
			    float col = 0.0;
			    float theta = 0.0;

			      float2 adjc = coord;
			      theta = delta_theta * 1.0;
			      adjc.x += cos(theta) * _Time.y * _Speed + _Time.y * _SpeedX;
			      adjc.y -= sin(theta) * _Time.y * _Speed - _Time.y * _SpeedY;
			      col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))* _Frequency)* _Intensity;
			      
			       adjc = coord;
			      theta = delta_theta * 2.0;
			      adjc.x += cos(theta) * _Time.y * _Speed + _Time.y * _SpeedX;
			      adjc.y -= sin(theta) * _Time.y * _Speed - _Time.y * _SpeedY;
			      col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))* _Frequency)* _Intensity;
			      
			       adjc = coord;
			      theta = delta_theta * 3.0;
			      adjc.x += cos(theta) * _Time.y * _Speed + _Time.y * _SpeedX;
			      adjc.y -= sin(theta) * _Time.y * _Speed - _Time.y * _SpeedY;
			      col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))* _Frequency)* _Intensity;
			      
			       adjc = coord;
			      theta = delta_theta * 4.0;
			      adjc.x += cos(theta) * _Time.y * _Speed + _Time.y * _SpeedX;
			      adjc.y -= sin(theta) * _Time.y * _Speed - _Time.y * _SpeedY;
			      col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))* _Frequency)* _Intensity;
			      
			       adjc = coord;
			      theta = delta_theta * 5.0;
			      adjc.x += cos(theta) * _Time.y * _Speed + _Time.y * _SpeedX;
			      adjc.y -= sin(theta) * _Time.y * _Speed - _Time.y * _SpeedY;
			      col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))* _Frequency)* _Intensity;
			      
			       adjc = coord;
			      theta = delta_theta * 6.0;
			      adjc.x += cos(theta) * _Time.y * _Speed + _Time.y * _SpeedX;
			      adjc.y -= sin(theta) * _Time.y * _Speed - _Time.y * _SpeedY;
			      col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))* _Frequency)* _Intensity;
			      
			       adjc = coord;
			      theta = delta_theta * 7.0;
			      adjc.x += cos(theta) * _Time.y * _Speed + _Time.y * _SpeedX;
			      adjc.y -= sin(theta) * _Time.y * _Speed - _Time.y * _SpeedY;
			      col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))* _Frequency)* _Intensity;

			    return cos(col);
			  }
			
			v2f vert (appdata_base v)
			{
				float ns = snoise(float3(v.vertex.x * _NoiseScale, v.vertex.z * _NoiseScale, _Time.x * _WaveSpeed));
				v.vertex.y += ns * _HeightScale;
			    v2f o;
			    o.pos = UnityObjectToClipPos (v.vertex);
			    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
			    return o;
			}

			half4 frag (v2f i) : COLOR
			{
				float2 p = i.uv , c1 = p, c2 = p;
				float cc1 = coli(c1);

				c2.x += 1000.0 / _Delta;
				float dx = _Emboss*(cc1-coli(c2)) / _Delta;

				c2.x = p.x;
				c2.y += 500.0 / _Delta;
				float dy = _Emboss * (cc1 - coli(c2))/ _Delta;

				c1.x += dx;
				c1.y = -(c1.y+dy);

				float alpha = 1.+dot(dx,dy)* _Intence;
				return tex2D(_MainTex, c1);
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
