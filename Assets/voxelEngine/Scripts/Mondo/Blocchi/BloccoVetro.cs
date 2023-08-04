using UnityEngine;

[System.Serializable]
public class BloccoVetro : Blocco
{
    public BloccoVetro()
        : base()
    {
        indistruttibile = false;
        vitaBlocco = 1;
    }

    //public override Color32 ColoreUnicoFaccia(Direzione direzione)
    //{
    //    //rosso trasparente
    //    Color32 coloreFaccia = new Color32(20, 0, 0, 50);
    //
    //    return coloreFaccia;
    //}

    public override Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = new Vector2Int();

        textureFaccia.x = 2;
        textureFaccia.y = 1;

        return textureFaccia;
    }

    //semi_solido, così se ci sono altri blocchi semi_solidi o non_solidi, allora non disegnano la faccia
    //se però c'è un blocco solido adiacente, quest'ultimo disegna la faccia, per mostrarla attraverso questo blocco
    public override Solidità IsSolid(Direzione direzione)
    {
        return Solidità.semi_solido;
    }
}
