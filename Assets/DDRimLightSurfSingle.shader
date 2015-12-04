Shader "Custom/DDRimLightSurfSingle" {
Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _Color ("Diffuse Material Color", Color) = (1,1,1,1)
      _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
      _BeAtkColor ("BeAtkColor ", Range(1,10)) = 1
      _Gray ("GrayValue ", Range(0,1)) = 0
      _ModelAlpha ("ModelAlpha ", Range(0,1)) = 1
      
      
      _AlphaTex ("AlphaTex ", 2D) = "white" {}
      
      _FlagTex ("FlagTex", 2D) = "white" {}
	  _FlagTexScale("FlagTexScale",Float) = 1
      
      _FlagTexPower ("RimPower ", Float) = 0
      _RimColor ("CubeColor ", Color) = (1,1,1,1)
      
    }
    
    
    SubShader {
      Tags { "RenderType" = "Opaque" }
	  Blend SrcAlpha OneMinusSrcAlpha
      Cull Off
      		
      CGPROGRAM
      #pragma surface surf Lambert alphatest:_Cutoff
      
      struct Input {
          fixed2 uv_MainTex;
          //fixed2 uv_FlagTex;
          fixed2 uv_AlphaTex;
          
          //fixed3 viewDir;
          fixed3 worldRefl;
          
      };
      
      sampler2D _MainTex;
      fixed4 _Color;
	  fixed _BeAtkColor;
	  fixed _Gray;
      fixed _ModelAlpha;
      fixed _FlagTexPower;
      
      sampler2D _AlphaTex;
      sampler2D _FlagTex;
      fixed _FlagTexScale;
      
      fixed4 _RimColor;
      
      void surf (Input IN, inout SurfaceOutput o) 
      {
      
		fixed4 tex = tex2D(_MainTex,  IN.uv_MainTex);
		fixed4 c = tex * _Color;
        fixed3 refColor = _Gray<0.5?fixed3(c.rgb):fixed3(c.g);
		o.Albedo = refColor*_BeAtkColor;
		o.Alpha = _ModelAlpha*c.a;
		
		fixed3 TempRefColor = refColor;
		
		if(_FlagTexPower>0.1)
		{
			fixed4 mAlphaTex = tex2D(_AlphaTex,  IN.uv_AlphaTex);
			if(mAlphaTex.a>0.1)
			{
				fixed rate = 4*_FlagTexScale;
				fixed3 viewNormal = normalize(IN.worldRefl);//normalize(IN.viewDir);//
				fixed nuVX=viewNormal.x/rate+_Time*5;
				fixed nuvY = viewNormal.y/rate;//IN.uv_FlagTex.y;
				fixed2 newUv = fixed2(nuVX,nuvY);
				fixed4 mflagTex = tex2D(_FlagTex,  newUv);
				TempRefColor = TempRefColor+(mflagTex.rgb*mflagTex.a*_RimColor*_FlagTexPower);
			}
		}
		o.Emission =  TempRefColor;
      }
      ENDCG
      
    }
    Fallback "Diffuse"
  }
