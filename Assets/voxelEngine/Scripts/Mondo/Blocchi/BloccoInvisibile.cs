using UnityEngine;

[System.Serializable]
public class BloccoInvisibile : Blocco
{
    public BloccoInvisibile()
        : base()
    {
    }

    //non ricordo perché l'avevo creato, ma se la solidità è non_solido è l'equivalente di un BloccoAria(), quindi inutile tenerlo.
    //praticamente aggiunge solo i vertici e i triangoli del mesh_collider e non della mesh, e non passa uv o colori.
    //può tornare utile come semi_solido (così gli altri blocchi mostrano la faccia ugualmente), 
    //ma è più comodo crearlo con un uv alpha 0, o un colore invisibile, lasciando creare la mesh normale.

    protected override DatiMesh DatiFacciaSopra(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco));

        datiMesh.AddColQuadTriangles();

        return datiMesh;
    }

    protected override DatiMesh DatiFacciaSotto(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco));

        datiMesh.AddColQuadTriangles();

        return datiMesh;
    }

    protected override DatiMesh DatiFacciaNord(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco));

        datiMesh.AddColQuadTriangles();

        return datiMesh;
    }

    protected override DatiMesh DatiFacciaEst(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco));

        datiMesh.AddColQuadTriangles();

        return datiMesh;
    }

    protected override DatiMesh DatiFacciaSud(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x + mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco));

        datiMesh.AddColQuadTriangles();

        return datiMesh;
    }

    protected override DatiMesh DatiFacciaOvest(Chunk chunk, float x, float y, float z, DatiMesh datiMesh, float mezzoBlocco, bool usaCollisioni)
    {
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z + mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z + mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y + mezzoBlocco, z - mezzoBlocco));
        datiMesh.AddColVertex(new Vector3(x - mezzoBlocco, y - mezzoBlocco, z - mezzoBlocco));

        datiMesh.AddColQuadTriangles();

        return datiMesh;
    }

    public override Solidità IsSolid(Direzione direzione)
    {
        return Solidità.non_solido;
    }
}
