using UnityEngine;

[System.Serializable]
public class BloccoFoglie : Blocco
{
    public BloccoFoglie()
        : base()
    {
        indistruttibile = false;
        vitaBlocco = 1;
    }

    //public override Color32 ColoreUnicoFaccia(Direzione direzione)
    //{
    //    //verde scuro
    //    Color32 coloreFaccia = new Color32(0, 100, 0, 200);
    //
    //    return coloreFaccia;
    //}

    public override Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = new Vector2Int();

        textureFaccia.x = 1;
        textureFaccia.y = 1;

        return textureFaccia;
    }
}
