using UnityEngine;

[System.Serializable]
public class BloccoErba : Blocco
{
    public BloccoErba()
        : base()
    {
        indistruttibile = false;
        vitaBlocco = 2;
    }    

    public override DatiMesh DatiBlocco(Chunk chunk, int x, int y, int z, DatiMesh datiMesh)
    {
        //crea le facce normalmente
        base.DatiBlocco(chunk, x, y, z, datiMesh);
        
        //questa funzione viene chiamata ogni volta che si aggiorna il chunk.
        //se quando si aggiorna si nota che il blocco sopra è solido, diventa un blocco di terra
        //se il blocco è non solido rimane d'erba, perché si potrebbe aver piazzato un blocco di vetro

        Solidità sopra = chunk.OttieniBlocco(x, y + 1, z).IsSolid(Direzione.sotto);
        if (sopra == Solidità.solido)
        {
            chunk.mondo.SettaBlocco(chunk.chunkPosition.x, chunk.chunkPosition.y, chunk.chunkPosition.z, x, y, z, new BloccoTerra(), true);
        }

        return datiMesh;
    }

    //public override Color32 ColoreUnicoFaccia(Direzione direzione)
    //{
    //    Color32 coloreFaccia = new Color32();
    //
    //    switch (direzione)
    //    {
    //        case Direzione.sopra:
    //    
    //            coloreFaccia = Color.white;
    //    
    //            return coloreFaccia;
    //    }
    //
    //    //marrone
    //    coloreFaccia = new Color32(139, 69, 19, 255);
    //
    //    return coloreFaccia;
    //}

    //faccia solo erba "sopra"
    //faccia solo terra "sotto"
    //faccia di terra con un po' di erba sopra, nei "lati"
    public override Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = new Vector2Int();

        switch (direzione)
        {
            case Direzione.sopra:

                textureFaccia.x = 0;
                textureFaccia.y = 0;

                return textureFaccia;

            case Direzione.sotto:

                textureFaccia.x = 2;
                textureFaccia.y = 0;

                return textureFaccia;
        }

        textureFaccia.x = 1;
        textureFaccia.y = 0;

        return textureFaccia;
    }
}

