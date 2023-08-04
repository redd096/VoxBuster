using UnityEngine;

[System.Serializable]
public class BloccoFoglie3 : BloccoFoglie
{
    public BloccoFoglie3()
        : base()
    {
    }

    public override Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = new Vector2Int();

        textureFaccia.x = 1;
        textureFaccia.y = 2;

        return textureFaccia;
    }
}
