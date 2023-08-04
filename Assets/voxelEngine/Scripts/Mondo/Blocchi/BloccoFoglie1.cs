using UnityEngine;

[System.Serializable]
public class BloccoFoglie1 : BloccoFoglie
{
    public BloccoFoglie1()
        : base()
    {
    }

    public override Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = new Vector2Int();

        textureFaccia.x = 3;
        textureFaccia.y = 1;

        return textureFaccia;
    }
}
