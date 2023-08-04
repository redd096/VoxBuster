using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

//script da aggiungere ad un Empty GameObject, per creare il prefab da mettere in Resources/Prefabs, da usare poi tramite Mondo.cs

//dovrà essere aggiunto manualmente il materiale
//Sopra la funzione dei colori e dell'uv, viene spiegato il materiale da mettere.

public class Chunk : MonoBehaviour
{
    //utilizzare valori che diviso 2 e diviso 4 restituiscano sempre un integer (4, 8, 16, 32, ...)
    public static int grandezzaChunk = 16;
    //quanto dev'essere alto il chunk. Un 16x16x16 potrebbe diventare 16x32x16 con un moltiplicatoreY == 2
    public static int moltiplicatoreY = 1;

    //i blocchi che compongono il chunk
    public Blocco[,,] blocchi = new Blocco[grandezzaChunk, grandezzaChunk * moltiplicatoreY, grandezzaChunk];

    //per avere accesso allo script Mondo.cs per settare o ottenere blocchi di altri chunk
    public Mondo mondo;
    //le coordinate del chunk (x,y,z)
    public Vector3Int chunkPosition;

    //la mesh e il mesh collider del chunk
    MeshFilter mesh_filter;
    MeshCollider mesh_coll;
    
    //per sapere se il chunk va renderizzato (aggiornato o prima creazione)
    public bool daAggiornare = true;

    void Awake()
    {
        mesh_filter = gameObject.GetComponent<MeshFilter>();
        mesh_coll = gameObject.GetComponent<MeshCollider>();
    }
    
    void Update()
    {
        //aggiorna il chunk, per renderizzarlo
        if (daAggiornare)
        {
            daAggiornare = false;
            AggiornaChunk();
        }
    }

    //Invia le informazioni calcolate della mesh
    //ai componenti della mesh e della collisione
    private void RenderMesh(DatiMesh datiMesh)
    {
        //crea la mesh
        mesh_filter.mesh.Clear();
        mesh_filter.mesh.vertices = datiMesh.vertices.ToArray();
        mesh_filter.mesh.triangles = datiMesh.triangles.ToArray();

        //passa colori o uv della mesh
        //mesh_filter.mesh.colors32 = datiMesh.colors.ToArray();
        //mesh_filter.mesh.RecalculateNormals(); //va messa qua se si usano solo i colori e non l'uv
        mesh_filter.mesh.uv = datiMesh.uv.ToArray();
        mesh_filter.mesh.RecalculateNormals();

        //crea la mesh necessaria per il mesh collider
        mesh_coll.sharedMesh = null;
        Mesh mesh = new Mesh();
        mesh.vertices = datiMesh.colVertices.ToArray();
        mesh.triangles = datiMesh.colTriangles.ToArray();
        mesh.RecalculateNormals();
        //passa la mesh appena creata nella mesh collider
        mesh_coll.sharedMesh = mesh;        
    }


    //inizio funzioni, ma in genere conviene richiamare quelle di Mondo.cs
    
    ///<summary>
    ///ottieni il blocco del chunk, o richiama la funzione in Mondo.cs
    ///</summary>
    public Blocco OttieniBlocco(int blockX, int blockY, int blockZ)
    {
        //se si trova in questo chunk, ritorna il blocco
        //altrimenti chiama OttieniBlocco all'interno dello script mondo,
        //così che esso trovi il chunk a cui il blocco appartiene

        if (FunzioniMondo.BlockInRange(blockX) && FunzioniMondo.BlockInRange(blockY, true) && FunzioniMondo.BlockInRange(blockZ))
            return blocchi[blockX, blockY, blockZ];

        return mondo.OttieniBlocco(chunkPosition.x, chunkPosition.y, chunkPosition.z, blockX, blockY, blockZ);
    }

    ///<summary>
    ///setta il blocco del chunk, o richiama la funzione in Mondo.cs
    ///</summary>
    public void SettaBlocco(int blockX, int blockY, int blockZ, Blocco blocco)
    {
        //se si trova in questo chunk, setta il tipo di blocco (aria, erba...)
        //altrimenti chiama SettaBlocco all'interno dello script mondo,
        //così che esso trovi il chunk a cui il blocco appartiene

        if (FunzioniMondo.BlockInRange(blockX) && FunzioniMondo.BlockInRange(blockY, true) && FunzioniMondo.BlockInRange(blockZ))
        {
            blocchi[blockX, blockY, blockZ] = blocco;
        }
        else
        {
            mondo.SettaBlocco(chunkPosition.x, chunkPosition.y, chunkPosition.z, blockX, blockY, blockZ, blocco);
        }
    }

    ///<summary>
    ///Aggiorna il chunk, controllando i blocchi di cui è composto e aggiungendo o togliendo facce in base ai chunk adiacenti (magari creati successivamente)
    ///</summary>
    public void AggiornaChunk()
    {
        DatiMesh datiMesh = new DatiMesh();

        for (int x = 0; x < grandezzaChunk; x++)
        {
            for (int y = 0; y < grandezzaChunk * moltiplicatoreY; y++)
            {
                for (int z = 0; z < grandezzaChunk; z++)
                {
                    datiMesh = blocchi[x, y, z].DatiBlocco(this, x, y, z, datiMesh);
                }
            }
        }

        RenderMesh(datiMesh);
    }
}
