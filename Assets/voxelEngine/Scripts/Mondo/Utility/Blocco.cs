using UnityEngine;

[System.Serializable]
public class Blocco
{
    //utilizzare 1, 0.5f, 0.25f -> rispettivamente un metro, mezzo metro e un quarto
    public static float grandezzaBlocco = 1;

    //decide se il blocco è indistruttibile
    public bool indistruttibile = true;
    //la vita del blocco, prima di essere distrutto (se indistruttibile == false)
    public float vitaBlocco = 100;

    //la direzione viene usata per creare le facce, collisioni, ecc...
    public enum Direzione { nord, est, sud, ovest, sopra, sotto };
    //semi_solido è necessario per blocchi trasparenti come il vetro:
    //il vetro funziona come ogni blocco solido, ma se al suo fianco c'è un blocco realmente solido, come la pietra
    //allora la pietra dovrà disegnare la faccia anche se si trova contro il vetro (per renderla visibile)
    public enum Solidità { solido, semi_solido, non_solido};

    //da modificare in base a quante immagini ci sono in ogni riga (0.25f se ci sono 4 tile per ogni riga, 0.50f se ce ne sono 2)
    //0.25f = 1/4 -> 0.50f = 1/2 
    const float tileSize = 0.25f;

    //per salvare solo i blocchi modificati, invece dell'intero chunk
    public bool modificato = false;

    //costruttore base Blocco
    public Blocco() { }

    public virtual DatiMesh DatiBlocco(Chunk chunk, int x, int y, int z, DatiMesh datiMesh)
    {
        //creo le variabili da passare poi alle funzioni qua sotto
        float mezzoBlock = grandezzaBlocco / 2;
        float posX = x * grandezzaBlocco;
        float posY = y * grandezzaBlocco;
        float posZ = z * grandezzaBlocco;

        //controlla che la faccia dell'altro blocco adiacente sia non_solida,
        //oppure che quella di questo blocco sia solida e quella del blocco adiacente sia semi_solida.
        //Se la faccia è non_solida crea anche le collisioni,
        //se è semi_solida crea solo la mesh, perché si vede la faccia di questo blocco, ma non si entra in contatto con essa. 
        //Altrimenti non viene creata la faccia
        Solidità sopra = chunk.OttieniBlocco(x, y + 1, z).IsSolid(Direzione.sotto);
        if (sopra == Solidità.non_solido || (IsSolid(Direzione.sopra) == Solidità.solido && sopra == Solidità.semi_solido))
        {
            bool usaCollisioni = (sopra == Solidità.non_solido);
            datiMesh = DatiFacciaSopra(chunk, posX, posY, posZ, datiMesh, mezzoBlock, usaCollisioni);
        }

        Solidità sotto = chunk.OttieniBlocco(x, y - 1, z).IsSolid(Direzione.sopra);
        if (sotto == Solidità.non_solido || (IsSolid(Direzione.sotto) == Solidità.solido && sotto == Solidità.semi_solido))
        {
            bool usaCollisioni = (sotto == Solidità.non_solido);
            datiMesh = DatiFacciaSotto(chunk, posX, posY, posZ, datiMesh, mezzoBlock, usaCollisioni);
        }

        Solidità nord = chunk.OttieniBlocco(x, y, z + 1).IsSolid(Direzione.sud);
        if (nord == Solidità.non_solido || (IsSolid(Direzione.nord) == Solidità.solido && nord == Solidità.semi_solido))
        {
            bool usaCollisioni = (nord == Solidità.non_solido);
            datiMesh = DatiFacciaNord(chunk, posX, posY, posZ, datiMesh, mezzoBlock, usaCollisioni);
        }

        Solidità est = chunk.OttieniBlocco(x + 1, y, z).IsSolid(Direzione.ovest);
        if (est == Solidità.non_solido || (IsSolid(Direzione.est) == Solidità.solido && est == Solidità.semi_solido))
        {
            bool usaCollisioni = (est == Solidità.non_solido);
            datiMesh = DatiFacciaEst(chunk, posX, posY, posZ, datiMesh, mezzoBlock, usaCollisioni);
        }

        Solidità sud = chunk.OttieniBlocco(x, y, z - 1).IsSolid(Direzione.nord);
        if (sud == Solidità.non_solido || (IsSolid(Direzione.sud) == Solidità.solido && sud == Solidità.semi_solido))
        {
            bool usaCollisioni = (sud == Solidità.non_solido);
            datiMesh = DatiFacciaSud(chunk, posX, posY, posZ, datiMesh, mezzoBlock, usaCollisioni);
        }

        Solidità ovest = chunk.OttieniBlocco(x - 1, y, z).IsSolid(Direzione.est);
        if (ovest == Solidità.non_solido || (IsSolid(Direzione.ovest) == Solidità.solido && ovest == Solidità.semi_solido))
        {
            bool usaCollisioni = (ovest == Solidità.non_solido);
            datiMesh = DatiFacciaOvest(chunk, posX, posY, posZ, datiMesh, mezzoBlock, usaCollisioni);
        }

        return datiMesh;
    }

    protected virtual DatiMesh DatiFacciaSopra(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        //passa i vertici necessari per creare la mesh della faccia
        //se usaCollisioni == true, aggiunge anche la collisione
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco), usaCollisioni);

        //crea i triangoli per la creazione della mesh
        //se usaCollisioni == true, aggiunge anche la collisione
        datiMesh.AddQuadTriangles(usaCollisioni);

        //aggiunge uv e colori alla mesh
        datiMesh.uv.AddRange(UVsFaccia(Direzione.sopra));
        //datiMesh.colors.AddRange(ColoriFaccia(Direzione.sopra));

        return datiMesh;
    }

    protected virtual DatiMesh DatiFacciaSotto(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco), usaCollisioni);

        datiMesh.AddQuadTriangles(usaCollisioni);

        datiMesh.uv.AddRange(UVsFaccia(Direzione.sotto));
        //datiMesh.colors.AddRange(ColoriFaccia(Direzione.sotto));

        return datiMesh;
    }

    protected virtual DatiMesh DatiFacciaNord(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco), usaCollisioni);

        datiMesh.AddQuadTriangles(usaCollisioni);

        datiMesh.uv.AddRange(UVsFaccia(Direzione.nord));
        //datiMesh.colors.AddRange(ColoriFaccia(Direzione.nord));

        return datiMesh;
    }

    protected virtual DatiMesh DatiFacciaEst(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco), usaCollisioni);

        datiMesh.AddQuadTriangles(usaCollisioni);

        datiMesh.uv.AddRange(UVsFaccia(Direzione.est));
        //datiMesh.colors.AddRange(ColoriFaccia(Direzione.est));

        return datiMesh;
    }

    protected virtual DatiMesh DatiFacciaSud(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco), usaCollisioni);

        datiMesh.AddQuadTriangles(usaCollisioni);

        datiMesh.uv.AddRange(UVsFaccia(Direzione.sud));
        //datiMesh.colors.AddRange(ColoriFaccia(Direzione.sud));

        return datiMesh;
    }

    protected virtual DatiMesh DatiFacciaOvest(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco), usaCollisioni);
        datiMesh.AddVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco), usaCollisioni);

        datiMesh.AddQuadTriangles(usaCollisioni);

        datiMesh.uv.AddRange(UVsFaccia(Direzione.ovest));
        //datiMesh.colors.AddRange(ColoriFaccia(Direzione.ovest));

        return datiMesh;
    }

    //mostra la solidità della faccia del Blocco
    public virtual Solidità IsSolid(Direzione direzione)
    {
        return Solidità.solido;
    }


    //per utilizzare i colori
    //impostare lo standard vertex shader, dal progetto UnityVC
    //scaricabile da http://forum.unity3d.com/threads/standard-shader-with-vertex-colors.316529/
    //e trascinare il materiale sul chunkPrefab, o creare un materiale con quello shader

    //colorare i vertici di una faccia del blocco
    public virtual Color32[] ColoriFaccia(Direzione direzione)
    {
        Color32[] coloriVerticiFaccia = new Color32[4];
    
        Color32 newColor = ColoreUnicoFaccia(direzione);
    
        coloriVerticiFaccia[0] = newColor;
        coloriVerticiFaccia[1] = newColor;
        coloriVerticiFaccia[2] = newColor;
        coloriVerticiFaccia[3] = newColor;
    
        return coloriVerticiFaccia;
    }

    //restituisce un solo colore, per colorare tutti i vertici di una faccia con un solo colore
    public virtual Color32 ColoreUnicoFaccia(Direzione direzione)
    {
        Color32 coloreFaccia = Color.white;
    
        return coloreFaccia;
    }

    // per utilizzare l'UV
    // importare un'immagine divisa in 4x4 (altrimenti cambiare la variabile tileSize -> 0.25 = 1/4)
    // impostarla come "advanced" e uncheckare tutte le opzioni fino a wrap mode
    // settarla su "repeat" e settare il filter mode su "point"
    // settare la grandezza massima in base a quella dell'immagine importata
    // trascinare l'immagine sul prefab del chunk, così da usarla come materiale
    //
    // per poter utilizzarne la trasparenza, ad esempio per vedere attraverso i buchi tra le foglie
    // bisogna cambiare lo shader utilizzato (quello sul materiale che comparirà una volta trascinata la texture sul chunkPrefab)
    // da diffuse a legacy/transparent/cutout/diffuse

    //assegnare la texture ai vertici di una faccia del blocco
    public virtual Vector2[] UVsFaccia(Direzione direzione)
    {
        Vector2[] UVs = new Vector2[4];

        Vector2Int posTile = PosizioneTexture(direzione);
        
        UVs[0] = new Vector2(tileSize * posTile.x, tileSize * posTile.y);
        UVs[1] = new Vector2(tileSize * posTile.x, tileSize * posTile.y + tileSize);
        UVs[2] = new Vector2(tileSize * posTile.x + tileSize, tileSize * posTile.y + tileSize);
        UVs[3] = new Vector2(tileSize * posTile.x + tileSize, tileSize * posTile.y);

        return UVs;
    }

    //restituisce la posizione da cui prendere la texture
    public virtual Vector2Int PosizioneTexture(Direzione direzione)
    {
        Vector2Int textureFaccia = Vector2Int.zero;

        textureFaccia.x = 0;
        textureFaccia.y = 0;

        return textureFaccia;
    }
}
