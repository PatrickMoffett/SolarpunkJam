Shader "Custom/TilemapOutline" {
  Properties {
    _MainTex       ("Texture",           2D)   = "white" {}
    _OutlineColor  ("Outline Color",     Color) = (0,0,0,1)
    _Thickness     ("Outline Thickness", Float) = 1
  }
  SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" }
    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      sampler2D _MainTex;
      float4   _MainTex_TexelSize; // x=1/width, y=1/height
      float4   _OutlineColor;
      float    _Thickness;

      struct appdata {
        float4 vertex : POSITION;
        float2 uv     : TEXCOORD0;
        float4 color  : COLOR;
      };

      struct v2f {
        float4 pos : SV_POSITION;
        float2 uv  : TEXCOORD0;
        float4 col : COLOR;
      };

      v2f vert(appdata v) {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv  = v.uv;
        o.col = v.color;
        return o;
      }

    fixed4 frag(v2f i) : SV_Target {
        fixed4 baseCol = tex2D(_MainTex, i.uv) * i.col;
        if (baseCol.a < 0.5) discard;

        // 1px outline offset
        float2 ofs1 = _Thickness * _MainTex_TexelSize.xy;
        bool outX1 = tex2D(_MainTex, i.uv + float2(ofs1.x,0)).a < 0.5
                    || tex2D(_MainTex, i.uv - float2(ofs1.x,0)).a < 0.5;
        bool outY1 = tex2D(_MainTex, i.uv + float2(0,ofs1.y)).a < 0.5
                    || tex2D(_MainTex, i.uv - float2(0,ofs1.y)).a < 0.5;

        // 2px outline offset
        float2 ofs2 = ofs1 * 2;
        bool outX2 = tex2D(_MainTex, i.uv + float2(ofs2.x,0)).a < 0.5
                    || tex2D(_MainTex, i.uv - float2(ofs2.x,0)).a < 0.5;
        bool outY2 = tex2D(_MainTex, i.uv + float2(0,ofs2.y)).a < 0.5
                    || tex2D(_MainTex, i.uv - float2(0,ofs2.y)).a < 0.5;

        //  Draw 1px whenever there’s an immediate alpha-gap...
        bool draw1 = outX1 || outY1;
        // …and draw a second “far” pixel only where there *wasn’t* a 1px gap
        bool draw2 = !draw1 && (outX2 || outY2);

        return (draw1 || draw2)
            ? _OutlineColor
            : baseCol;
    }
      ENDCG
    }
  }
}
