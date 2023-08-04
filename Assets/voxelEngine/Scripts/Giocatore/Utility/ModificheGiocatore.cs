using UnityEngine;

public static class ModificheGiocatore
{
	//le funzioni da richiamare per modificare il mondo dall'esterno 
	//(modifiche giocatore, invece che modifiche tramite costruzioni o creazione del mondo)


	//per avere la posizione come Vector3Int, invece che come Vector3
    public static Vector3Int OttieniIndexBlocco(Vector3 pos)
    {
        Vector3Int posBlocco = new Vector3Int(
            Mathf.RoundToInt(pos.x),
            Mathf.RoundToInt(pos.y),
            Mathf.RoundToInt(pos.z)
            );

        return posBlocco;
    }

	//Ottieni l'indice del blocco colpito
    public static Vector3Int OttieniIndexBlocco(RaycastHit hit, Vector3Int chunkPos, bool adiacente = false)
    {
        //"adiacente" decide se bisogna ottenere il blocco che si ha colpito o quello adiacente alla faccia che si ha colpito
        //ottenere il blocco adiacente, serve quando bisogna piazzare un blocco.
		//se adiacente == false invece si ottiene la posizione del blocco che si colpisce, per poterlo, ad esempio, distruggerlo

		//il vecchio metodo, che sembra essere rimpiazzato da un pos += (hit.normal * (grandezzaBlocco /2)); -> con un meno davanti se non è adiacente
        //Vector3 pos = new Vector3(
        //     MoveWithinBlock(hit.point.x, hit.normal.x, adiacente),
        //     MoveWithinBlock(hit.point.y, hit.normal.y, adiacente),
        //     MoveWithinBlock(hit.point.z, hit.normal.z, adiacente)
        //     );
        
        float grandezzaBlocco = Blocco.grandezzaBlocco;

        Vector3 pos = hit.point;

        //per piazzare il blocco, invece di romperlo, non bisogna mettere il meno davanti a (grandezzaBlocco / 2)
        if (adiacente)
            pos += (hit.normal * (grandezzaBlocco / 2));
        else
            pos += (hit.normal * -(grandezzaBlocco / 2));

        //abbiamo la posizione del chunk + quella del blocco, sottraiamo quella del chunk
        pos.x -= chunkPos.x;
        pos.y -= chunkPos.y;
        pos.z -= chunkPos.z;

        //ora dividiamo per la grandezza del blocco, per avere l'indice del blocco
        //esempio: il blocco (1,0,0), se si usa blocchi da 0,5m sarebbe il terzo del chunk (0, 0.5f, 1.0f, ...)
        //1 / 0.5f = 2
        pos /= grandezzaBlocco;

        return OttieniIndexBlocco(pos);
    }

    //static float MoveWithinBlock(float pos, float norm, bool adiacente = false)
    //{
        //When we raycast onto a cube block the axis of the face the raycast hits will be 0.5, exactly half way between two blocks. 
        //To solve that we use MoveWithinBlock.
        //MoveWithinBlock gets called with x, y or z and the haycastHit's normal's x y or z.
        //If the block is halfway between two blocks it will have a decimal of 0.5 
        //so rounding it and subtracting the the rounded value from the original value which leaves us with the decimals.
        //Then if we find that it's 0.5 we can use the normal to move it 
        //(if you're unfamiliar with normals they are vectors point in the way a triangle the facing, 
        //the normal included with the raycastHit is the normal of the triangle hit by the raycast). 
        //We can use the normal to move the point outwards or inwards, 
        //if we're getting the adjacent block add half the normal to the position pushing it outwards. 
        //Only add half because the whole thing could equal up to 1 which would push the position too far back. 
        //If we're not looking for the adjacent block subtract the same amount moving it further into the block we're pointing at.
        //Once we call MoveWithinBlock on every component of the position with the corresponding normal value,
        //we know that the position is within the block we want,
        //so we can call GetBlockPos with the vector3 and it will round it to a block position that we can return.

    //    float mezzoBlocco = Blocco.grandezzaBlocco / 2;
    //    
    //    if (pos - (int)pos == mezzoBlocco || pos - (int)pos == -mezzoBlocco)
    //    {
    //        if (adiacente)
    //        {
    //            pos += (norm * mezzoBlocco);
    //        }
    //        else
    //        {
    //            pos += (norm * -mezzoBlocco);
    //        }
    //    }
    //
    //    return (float)pos;
    //}

	///<summary>
	///Setta il blocco colpito
	///</summary>
    public static bool SettaBlocco(RaycastHit hit, Blocco blocco, bool adiacente = false)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();

        //se non ha un componente "chunk", ritorniamo false, perché ciò che abbiamo colpito non è un chunk
        if (chunk == null)
            return false;

        //Altrimenti, otteniamo l'indice del blocco e chiamiamo SettaBlocco, nello script Mondo.cs
        //Sarà possibile richiamare questa funzione da ovunque, e controllare se ha avuto successo dal valore di ritorno

        Vector3Int blockIndex = OttieniIndexBlocco(hit, chunk.chunkPosition, adiacente);

        chunk.mondo.SettaBlocco(chunk.chunkPosition.x, chunk.chunkPosition.y, chunk.chunkPosition.z, blockIndex.x, blockIndex.y, blockIndex.z, blocco, true);

        return true;
    }

	///<summary>
	///Ottieni il blocco colpito
	///</summary>
    public static Blocco OttieniBlocco(RaycastHit hit, bool adiacente = false)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();

        //se non ha un componente "chunk", ritorniamo false, perché ciò che abbiamo colpito non è un chunk
        if (chunk == null)
            return null;

        //Altrimenti, otteniamo l'indice del blocco e chiamiamo OttieniBlocco, nello script Mondo.cs
        //Sarà possibile richiamare questa funzione da ovunque, e controllare se ha avuto successo dal valore di ritorno
        Vector3Int blockIndex = OttieniIndexBlocco(hit, chunk.chunkPosition, adiacente);

        Blocco blocco = chunk.mondo.OttieniBlocco(chunk.chunkPosition.x, chunk.chunkPosition.y, chunk.chunkPosition.z, blockIndex.x, blockIndex.y, blockIndex.z);

        return blocco;
    }

}