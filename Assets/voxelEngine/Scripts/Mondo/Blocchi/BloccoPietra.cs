using UnityEngine;

[System.Serializable]
public class BloccoPietra : Blocco
{
    public BloccoPietra()
        : base()
    {
    }

    //public override Color32 ColoreUnicoFaccia(Direzione direzione)
    //{
    //    Color32 coloreFaccia = Color.grey;
    //
    //    return coloreFaccia;
    //}

    public override Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = new Vector2Int();

        textureFaccia.x = 3;
        textureFaccia.y = 0;

        return textureFaccia;
    }
}
