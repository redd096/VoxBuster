using UnityEngine;

[System.Serializable]
public class BloccoTerra : Blocco
{
    public BloccoTerra()
        : base()
    {
        indistruttibile = false;
        vitaBlocco = 2;
    }

    int tempoPerCrescitaErba = 0;

    public override DatiMesh DatiBlocco(Chunk chunk, int x, int y, int z, DatiMesh datiMesh)
    {
        //crea le facce normalmente
        base.DatiBlocco(chunk, x, y, z, datiMesh);

        //questa funzione viene chiamata ogni volta che si aggiorna il chunk.
        //se quando si aggiorna si nota che il blocco sopra è diventato d'aria, si imposta il tempoPerCrescitaErba
        //se ad un successivo aggiornamento, è passato il tempoPerCrescitaErba, allora diventa un blocco d'erba
        //se invece ad un aggiornamento successivo, il blocco sopra diventa diverso dal blocco d'aria, allora si azzera il tempo

        //si potrebbe utilizzare un Invoke -> chiama la funzione per trasformarlo in blocco d'erba
        //e nella stessa funzione, si controlla se il blocco sopra è d'aria, altrimenti si riazzera il tempo
        Blocco bloccoSopra = chunk.OttieniBlocco(x, y + 1, z);
        if (bloccoSopra.GetType() == new BloccoAria().GetType())
        {
            if (tempoPerCrescitaErba == 0)
                tempoPerCrescitaErba = Mathf.FloorToInt(Time.time) + 60;
            else if (Time.time > tempoPerCrescitaErba)
            {
                chunk.mondo.SettaBlocco(chunk.chunkPosition.x, chunk.chunkPosition.y, chunk.chunkPosition.z, x, y, z, new BloccoErba(), true);
            }
        }
        else if (tempoPerCrescitaErba != 0)
            tempoPerCrescitaErba = 0;

        return datiMesh;
    }

    //public override Color32 ColoreUnicoFaccia(Direzione direzione)
    //{
    //    //marrone
    //    Color32 coloreFaccia = new Color32(139, 69, 19, 255);
    //
    //    return coloreFaccia;
    //}

    public override Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = new Vector2Int();

        textureFaccia.x = 2;
        textureFaccia.y = 0;

        return textureFaccia;
    }
}
