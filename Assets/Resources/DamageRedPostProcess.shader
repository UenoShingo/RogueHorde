Shader "Hidden/RogueHorde/DamageRedPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 0, 0, 1)
        _Intensity ("Intensity", Range(0, 1)) = 0
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _Intensity;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float amount = saturate(_Intensity) * _Color.a;
                col.rgb = lerp(col.rgb, _Color.rgb, amount);
                return col;
            }
            ENDCG
        }
    }
}
