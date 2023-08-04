using UnityEngine;

[System.Serializable]
public class BloccoAria : Blocco
{
    public BloccoAria()
        : base()
    {
    }

    //elimina facce e solidità
    public override DatiMesh DatiBlocco(Chunk chunk, int x, int y, int z, DatiMesh datiMesh)
    {
        return datiMesh;
    }

    public override Solidità IsSolid(Direzione direzione)
    {
        return Solidità.non_solido;
    }
}
