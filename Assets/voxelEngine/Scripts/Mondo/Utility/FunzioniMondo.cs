using UnityEngine;
using SimplexNoise; // per Noise.Generate in GetNoise()

public class FunzioniMondo
{
	/// <summary>
	/// Ottieni il noise da usare durante la generazione del Terreno o per le costruzioni
	/// </summary>
	public static float GetNoise(int staticSeed, float x, float y, float z, float scale, float max)
	{
		//Questa funziona usa le coordinate come campione.
		//Prende anche un valore per cui moltiplicherà le coordinate (scale).
		//Qui, un valore più piccolo campionerà un'area più piccola, dandoci un noise regolare (smooth), adatto per le montagne con lunghe distanze tra le vette e le valli,
		//mentre un valore più grande, renderà più frequenti dossi e dirupi.
		//Il parametro max, è il valore massimo che può essere restituito.
		//Ci verrà restituito sempre un valore int tra lo zero e il max.

		//vengono prima apportate delle modifiche in base al seed
		x += staticSeed;
		y += staticSeed;
		z += staticSeed;

		return Mathf.FloorToInt((Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f)) * Blocco.grandezzaBlocco;
	}    

	/// <summary>
	/// controlla che un valore sia in un determinato range
	/// </summary>
	public static bool InRange(float valore, float min, float max)
	{
		if(valore < min || valore > max)
			return false;

		return true;
	}

	/// <summary>
	/// controlla che un blocco sia nel range di un determinato chunk
	/// </summary>
	public static bool BlockInRange(int blockIndex, bool moltiplicare = false)
	{
		//se moltiplicare == true significa che abbiamo passato l'asse y
        //se moltiplicare == true allora si controlla grandezzaChunk * moltiplicatoreY
        //altrimenti si controlla grandezzaChunk
        int grandezza = moltiplicare ? Chunk.grandezzaChunk * Chunk.moltiplicatoreY : Chunk.grandezzaChunk;

        //controlla che non sia sotto lo 0, né oltre la grandezza in cui si è deciso di cercare
        //se va sotto lo 0 o oltre la grandezza, vuol dire che il blocco cercato non si trova in questo chunk
        if (blockIndex < 0 || blockIndex >= grandezza)
            return false;

        return true;
	}

    ///<summary>
    ///Controlla che il giocatore sia sempre nello stesso chunk
    ///</summary>
    public static bool PlayerInChunk(Vector3Int nuova_PosGiocatore, Vector3Int vecchia_posGiocatore)
    {
        //se la x, la y e la z della nuova posizione corrispondono a quelli della vecchia, ritorna true, altrimenti ritorna false
        if (nuova_PosGiocatore.x == vecchia_posGiocatore.x && nuova_PosGiocatore.y == vecchia_posGiocatore.y && nuova_PosGiocatore.z == vecchia_posGiocatore.z)
            return true;

        return false;
    }

	/// <summary>
	/// BugFix usato nello StartFunction di Mondo.cs -> se valoreConfronto non rientra nel range min/max, allora verrà restituito valoreFix e segnato un errore in console,
    /// altrimenti si restituisce semplicemente il valore iniziale (valoreDaModificare)
    /// restituisce object e usa come valoreDaModificare object, per poter restituire sia string che valori numerici
	/// </summary>
	public static object BugFixIniziale(object valoreDaModificare, float valoreConfronto, float min, float max, float valoreFix, string errore)
	{
		if (InRange(valoreConfronto, min, max) == false)
		{
			valoreDaModificare = valoreFix;
			Debug.Log(errore);
		}
		return valoreDaModificare;
	}

    /// <summary>
	/// Ottieni le coordinate del chunk
	/// </summary>
    public static Vector3Int OttieniChunkPlayer(Vector3 posPlayer)
    {
        Vector3Int pos = new Vector3Int();

        //Ottieni il chunk in cui si trova questo gameObject
        //È come la funzione OttieniPosChunk ma il blocco da considerare è sempre lo 0,0,0
        int grandezzaChunk = Mathf.FloorToInt(Chunk.grandezzaChunk * Blocco.grandezzaBlocco);
        int YgrandezzaChunk = Mathf.FloorToInt(Chunk.grandezzaChunk * Chunk.moltiplicatoreY * Blocco.grandezzaBlocco);

        pos.x = Mathf.FloorToInt(posPlayer.x / grandezzaChunk) * grandezzaChunk;
        pos.y = Mathf.FloorToInt(posPlayer.y / YgrandezzaChunk) * YgrandezzaChunk;
        pos.z = Mathf.FloorToInt(posPlayer.z / grandezzaChunk) * grandezzaChunk;

        return pos;
    }

	/// <summary>
	/// Ottieni le coordinate del chunk
	/// </summary>
    public static Vector3Int OttieniPosChunk(int chunkX, int chunkY, int chunkZ, int blockX = 0, int blockY = 0, int blockZ = 0)
    {
        //crea la variabile per la grandezza di un singolo chunk (i blocchi potrebbero essere da 0.5f ad esempio, invece che da 1 metro)
        //ottiene un valore integer della posizione del blocco diviso il numero di blocchi presenti in un chunk (numeroBlocchi = Chunk.grandezzaChunk).
        //moltiplica il valore ottenuto per l'effettiva dimensione di un chunk.
        //somma la posizione del chunk di partenza (chunkX, chunkY, chunkZ)
        //e si ottiene la posizione del nuovo chunk
        Vector3Int pos = new Vector3Int();

        //si usa un float per la grandezzaChunk o non si potrebbe trovare i chunk precedenti con valori negativi dei blocchi
        //-1 * 16 = 0
        //-1 * (float)16 = -1
        float numeroBlocchi = Chunk.grandezzaChunk;
        int grandezzaChunk = Mathf.FloorToInt(numeroBlocchi * Blocco.grandezzaBlocco);
        int YgrandezzaChunk = Mathf.FloorToInt(numeroBlocchi * Chunk.moltiplicatoreY * Blocco.grandezzaBlocco);

        //con blocchi da 1 metro, senza moltiplicatoreY, sarebbe semplicemente così:
        //pos.x = chunkX + (Mathf.FloorToInt(blockX / Chunk.grandezzaChunk) * Chunk.grandezzaChunk);
        pos.x = chunkX + (Mathf.FloorToInt(blockX / numeroBlocchi) * grandezzaChunk);
        pos.y = chunkY + (Mathf.FloorToInt(blockY / (numeroBlocchi * Chunk.moltiplicatoreY)) * YgrandezzaChunk);
        pos.z = chunkZ + (Mathf.FloorToInt(blockZ / numeroBlocchi) * grandezzaChunk);
        
        //un esempio sull'asse x partendo dal chunk in posizione 16,16,16, con chunk da 16 e blocchi da 0.5 metri:
        //si deve settare il blocco -2,0,0, ovvero il blocco 14,0,0 del chunk 8,16,16 (il penultimo blocco -da 0 a 15- del chunk a sinistra)
        //si esegue -2 / 16 e si ottiene -0.125, prendendo poi l'integer che sarà -1
        //lo si moltiplica per la grandezza del chunk (16*0.5 = 8) e si ottiene -8
        //si somma la posizione iniziale 16
        //e si ottiene la posizione x del chunk (16 - 8 = 8), dato che cercavamo il chunk 8,16,16

        return pos;
    }

	/// <summary>
	/// Ottieni l'indice del blocco all'interno di un chunk
	/// </summary>
	public static Vector3Int ottieniIndexBlocco(int blockX, int blockY, int blockZ)
	{
        int grandezzaChunk = Chunk.grandezzaChunk;
		//se il blocco è un valore negativo, allora è grandezzaChunk + il numero del blocco, altrimenti il blocco [modulo] grandezzaChunk
        //esempio sull'asse x, con blockX = -2 e grandezzaChunk = 16
        //abbiamo trovato che il chunk è quello a sinistra, il blocco sarà il penultimo ("14" da 0 a 15) -> 16 + (-2) = 14
        //esempio sull'asse x, con blockX = 17 e grandezzaChunk = 16
        //abbiamo trovato che il chunk è quello a destra, il blocco sarà il secondo ("1" da 0 a 15) -> 17 % 16 = 1 (17/16 = 0 con resto 1)
        int newX = blockX < 0 ? grandezzaChunk + blockX : blockX % grandezzaChunk;
        int newY = blockY < 0 ? (grandezzaChunk * Chunk.moltiplicatoreY) + blockY : blockY % (grandezzaChunk * Chunk.moltiplicatoreY);
        int newZ = blockZ < 0 ? grandezzaChunk + blockZ : blockZ % grandezzaChunk;

		return new Vector3Int(newX, newY, newZ);
	}

	/// <summary>
	/// Ottieni le coordinate del blocco
	/// </summary>
	public static Vector3 OttieniPosBlocco(int chunkX, int chunkY, int chunkZ, int blockX, int blockY, int blockZ)
    {
        //nel caso venga data la posizione di un blocco presente in un altro chunk
        Vector3 indexBlocco = ottieniIndexBlocco(blockX, blockY, blockZ);

        //ora serve trovare le coordinate del blocco all'interno del mondo, quindi bisogna moltiplicarlo per la grandezza dei blocchi (che potrebbe essere 0.5 invece di 1)
        indexBlocco.x *= Blocco.grandezzaBlocco;
        indexBlocco.y *= Blocco.grandezzaBlocco;
        indexBlocco.z *= Blocco.grandezzaBlocco;

        //ottieni la posizione del blocco all'interno del mondo, sommando la posizione del chunk e la coordinata del blocco
        return new Vector3(chunkX + indexBlocco.x, chunkY + indexBlocco.y, chunkZ + indexBlocco.z);
	}

	

    ///<summary>
    ///genera il blocco del chunk, se il chunk non esiste ancora o quel blocco è ancora null. 
    ///Salva in Mondo.cs i blocchi che andranno poi messi quando si creerà il chunk
    ///- rimpiazzaBlocchi serve per costringere alla sovrascrittura del blocco, anche se già generato
    ///</summary>
    public static void SettaBloccoNuovoChunk(int blockX, int blockY, int blockZ, Blocco blocco, Chunk chunk, bool rimpiazzaBlocchi = false)
    {
        //ottieni la posizione del chunk (utile se si cerca di settare un blocco di un altro chunk, come quando si creano le costruzioni)
        Vector3Int posChunk = OttieniPosChunk(chunk.chunkPosition.x, chunk.chunkPosition.y, chunk.chunkPosition.z, blockX, blockY, blockZ);

        //ottieni il chunk in cui dovrebbe comparire il blocco
        Chunk realChunk = chunk.mondo.OttieniChunk(posChunk);

        //ottieni posizione del Blocco
        Vector3 posizioneBlocco = OttieniPosBlocco(posChunk.x, posChunk.y, posChunk.z, blockX, blockY, blockZ);

        if (realChunk == null)
        {
            //se il chunk non è ancora stato creato, il blocco viene inserito in un dictionary "blocchiCostruzione" di Mondo.cs
            //così quando creerà il nuovo chunk, sovrascriverà il blocco che si sarebbe creato, con quello all'interno del dictionary (per costruzioni)

            //prima si controlla che non ci sia già il blocco all'interno del dictionary
            Mondo.BloccoCostruzione bloccoCostruzione;
            if (chunk.mondo.blocchiCostruzione.TryGetValue(posizioneBlocco, out bloccoCostruzione))
            {
                //se il blocco è presente nel dictionary, se caricato == true, significato che è stato caricato da un file di salvataggio, quindi non viene sovrascritto
                if (bloccoCostruzione.caricato)
                {
                    return;
                }
            }

            //se il blocco non si trova nel dictionary o non è stato caricato da un file di salvataggio
            //prima il remove e poi l'add per sovrascrivere, nel caso si sia già deciso che tipo di blocco dovesse essere (si rimuove il vecchio tipo di blocco)
            //questo significa che una costruzione creata in precedenza, potrebbe venire sovrascritta da una costruzione nelle vicinanze
            chunk.mondo.blocchiCostruzione.Remove(posizioneBlocco);
            chunk.mondo.blocchiCostruzione.Add(posizioneBlocco, new Mondo.BloccoCostruzione(blocco, false));
        }
        else
        {
            //se deve rimpiazzare i blocchi o il blocco del chunk è uguale a null
            //controlla che non ci sia già il blocco salvato nel dictionary di Mondo.cs (quello che si genera se realChunk == null o viene caricato da file)
            //se c'è, inserisce quello, altrimenti quello che si è generato
            if (rimpiazzaBlocchi || chunk.OttieniBlocco(blockX, blockY, blockZ) == null)
            {
                Mondo.BloccoCostruzione realBlock;

                if (chunk.mondo.blocchiCostruzione.TryGetValue(posizioneBlocco, out realBlock))
                {
                    chunk.SettaBlocco(blockX, blockY, blockZ, realBlock.blocco);

                    //se non è un blocco caricato da file, può essere rimosso dal dictionary, per limitare le dimensioni
                    if (realBlock.caricato == false)
                        chunk.mondo.blocchiCostruzione.Remove(posizioneBlocco);
                }
                else
                {
                    chunk.SettaBlocco(blockX, blockY, blockZ, blocco);
                }
            }
        }
    }
}
