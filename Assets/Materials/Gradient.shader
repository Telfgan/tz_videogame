Shader "Custom/GradientShader"
{
    Properties
    {
        _Color1 ("Цвет 1 (низ)", Color) = (1, 0, 0, 1)  // Красный
        _Color2 ("Цвет 2 (верх)", Color) = (0, 0, 1, 1)  // Синий
        _GradientScale ("Масштаб градиента", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float height : TEXCOORD0;  // Относительная высота вершины
            };

            fixed4 _Color1;
            fixed4 _Color2;
            float _GradientScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Нормализуем высоту объекта в диапазоне [0, 1]
                float minY = unity_ObjectToWorld._m13;  // Минимальная Y-координата объекта
                float maxY = minY + _GradientScale;     // Максимальная Y-координата
                o.height = saturate((v.vertex.y - minY) / (maxY - minY));
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Лерпим между двумя цветами
                fixed4 col = lerp(_Color1, _Color2, i.height);
                return col;
            }
            ENDCG
        }
    }
}