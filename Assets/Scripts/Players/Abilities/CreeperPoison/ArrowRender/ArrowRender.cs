using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ArrowRender : MonoBehaviour
{
    [SerializeField] private Material arrowMaterial;
    [SerializeField] private Material arrowMaterialTransparent;
    [SerializeField] private DecalProjector decalProjector;

    public void SetTransparentMaterial()
    {
        if (decalProjector != null) decalProjector.material = arrowMaterialTransparent;
    }

    public void SetDeafaultMaterail()
    {
        if (decalProjector != null) decalProjector.material = arrowMaterial;
    }
}
