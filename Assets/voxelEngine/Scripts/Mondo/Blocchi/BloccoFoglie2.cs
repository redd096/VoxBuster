using UnityEngine;

[System.Serializable]
public class BloccoFoglie2 : BloccoFoglie
{
    public BloccoFoglie2()
        : base()
    {
    }

    public override Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = new Vector2Int();

        textureFaccia.x = 0;
        textureFaccia.y = 2;

        return textureFaccia;
    }
}
