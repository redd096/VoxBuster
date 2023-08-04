using UnityEngine;

[System.Serializable]
public class BloccoLegno : Blocco
{
    public BloccoLegno()
        : base()
    {
        indistruttibile = false;
        vitaBlocco = 4;
    }

    //public override Color32 ColoreUnicoFaccia(Direzione direzione)
    //{
    //    //marrone
    //    Color32 coloreFaccia = new Color32(160, 82, 45, 255);
    //
    //    return coloreFaccia;
    //}

    public override Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = new Vector2Int();

        textureFaccia.x = 0;
        textureFaccia.y = 1;

        return textureFaccia;
    }
}
