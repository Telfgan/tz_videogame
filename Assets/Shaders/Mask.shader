Shader "MyShaders/Mask"
{
    Properties
    {
        [IntRange] _StencilRef("StencilRef", Range(0, 255)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+1" } 
        ColorMask 0
        ZWrite Off

        Stencil
        {
            Ref [_StencilRef]
            Pass Replace
        }

        Pass
        {

        }
    }      
}
