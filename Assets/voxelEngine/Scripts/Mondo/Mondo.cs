using UnityEngine;
using System.Collections.Generic; // per i dictionary

//script da aggiungere ad un manager_object

public class Mondo : MonoBehaviour
{
	//il nome del mondo e il seed in stringhe, create durante la creazione della mappa
    public string nomeMondo = "Mondo dei Voxel";
    public string seed = "Voxel Seed";

    //Definisce un dizionario, con le coordinate come chiave e il Chunk come valore
    //Possiamo utilizzarlo per memorizzare quanti chunk vogliamo e in qualsiasi posizione, e utilizzarli in base alle loro coordinate
    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    //da utilizzare in GeneraTerreno.cs per generare le costruzioni
    public Dictionary<Vector3, BloccoCostruzione> blocchiCostruzione = new Dictionary<Vector3, BloccoCostruzione>();

    public struct BloccoCostruzione
    {
        public Blocco blocco;
        public bool caricato;

        public BloccoCostruzione(Blocco blocco, bool caricato)
        {
            this.blocco = blocco;
            this.caricato = caricato;
        }
    }

    //il prefab dei chunk
    public GameObject chunkPrefab;

    //la funzione viene richiamata dal MenùIniziale, per settare le variabili come il nome del mondo, seed, grandezzaChunk, ecc...
    public void StartFunction(string temp_nomeMondo, string temp_seed, int temp_grandezzaBlocco, int temp_grandezzaChunk, int temp_moltiplicatoreY, GameObject temp_menùIniziale)
    {
        //ottieni i valori settati nel menù iniziale, poi distruggi il gameObject con script
        float grandezzaBlocco = 0;
        int grandezzaChunk = 0;
        int moltiplicatoreY = 0;
        nomeMondo = temp_nomeMondo;
        seed = temp_seed;

        //grandezzaBlocco
        {
            switch (temp_grandezzaBlocco)
            {
                case 0:
                    grandezzaBlocco = 0.25f;
                    break;
                case 1:
                    grandezzaBlocco = 0.5f;
                    break;
                case 2:
                    grandezzaBlocco = 1;
                    break;
            }

            Blocco.grandezzaBlocco = grandezzaBlocco;
        }
        //grandezzaChunk
        {
            switch (temp_grandezzaChunk)
            {
                case 0:
                    grandezzaChunk = 4;
                    break;
                case 1:
                    grandezzaChunk = 8;
                    break;
                case 2:
                    grandezzaChunk = 16;
                    break;
                case 3:
                    grandezzaChunk = 32;
                    break;
            }

            Chunk.grandezzaChunk = grandezzaChunk;
        }
        //moltiplicatoreY
        {
            switch (temp_moltiplicatoreY)
            {
                case 0:
                    moltiplicatoreY = 1;
                    break;
                case 1:
                    moltiplicatoreY = 2;
                    break;
                case 2:
                    moltiplicatoreY = 4;
                    break;
                case 3:
                    moltiplicatoreY = 8;
                    break;
            }

            Chunk.moltiplicatoreY = moltiplicatoreY;
        }

        Destroy(temp_menùIniziale);

        //In caso di problemi
        {
            //nomeMondo.Length viene già controllata alla creazione del mondo, nel menù iniziale
            //se il seed è lasciato vuoto o supera la lunghezza di 50 caratteri, allora viene impostato in base all'orario
            //gli altri valori vengono impostati per rientrare nei requisiti necessari agli script
            float time = (float)System.DateTime.Now.Hour + ((float)System.DateTime.Now.Minute /100);     //minuti diviso cento per averli dietro la virgola -> (float)ore.minuti
            string timeTicks = System.DateTime.Now.Ticks.ToString();    //ticks è un long, ed ha un valore troppo alto per essere usato come float
            float.TryParse(timeTicks.Substring(timeTicks.Length -7, 7), out time);  //quindi lo trasformo in string, uso solo le ultime 7 cifre e lo riporto in float
            seed = FunzioniMondo.BugFixIniziale(seed, seed.Length, 1, 50, time, "Reimpostato seed, da: "+ seed +" a: "+ time).ToString();
            Blocco.grandezzaBlocco = (float)FunzioniMondo.BugFixIniziale(Blocco.grandezzaBlocco, temp_grandezzaBlocco, 0, 2, 1, "Reimpostata grandezza blocchi, da: "+ grandezzaBlocco +" a: "+ 1);
            Chunk.grandezzaChunk = (int)FunzioniMondo.BugFixIniziale(Chunk.grandezzaChunk, temp_grandezzaChunk, 0, 3, 16, "Reimpostata grandezza chunk, da: "+ grandezzaChunk +" a: "+ 16);
            Chunk.moltiplicatoreY = (int)FunzioniMondo.BugFixIniziale(Chunk.moltiplicatoreY, temp_moltiplicatoreY, 0, 3, 1, "Reimpostato MoltiplicatoreY, da: "+ moltiplicatoreY +" a:"+ 1);
        }

        //setta il seed come numero da usare nella generazione del mondo, utilizzando la stringa precedentemente creata
        {
            int seedInt = seed.GetHashCode();

            int lunghezzaSeed = Mathf.Abs(seedInt).ToString().Length;
            int seedNumber = Mathf.FloorToInt(seedInt / Mathf.Pow(10, lunghezzaSeed - 4));

            GeneraTerreno.seed = seedNumber;
        }

        //print("grandezzaBlocco: " + Blocco.grandezzaBlocco);
        //print("grandezzaChunk: " + Chunk.grandezzaChunk);
        //print("moltiplicatoreY: " + Chunk.moltiplicatoreY);
        //print("seed: " + GeneraTerreno.seed);
        //print(System.DateTime.Now);
        //print(System.DateTime.Now.Hour);
        //print(System.DateTime.Now.Minute);
        //print(System.DateTime.Now.Ticks);
    }

    void Start()
    {
        //se il chunk non viene messo manualmente, lo si prende dalla cartella Resources/Prefabs
        if(chunkPrefab == null)
        {
            chunkPrefab = (GameObject)Resources.Load("Prefabs/Chunk", typeof(GameObject));
        }
    }
    


    // inizio funzioni	



	/// <summary>
	/// Crea o carica il chunk
	/// </summary>
    public void CreaChunk(Vector3Int chunkPos)
    {
        //se il chunk già esiste, non serve generarlo nuovamente (non dovrebbe succedere, almeno che non ci sia un errore)
        if(OttieniChunk(chunkPos) != null)
        {
            Debug.Log("Si sta tentando di creare un chunk già esistente.");
            return;
        }
        
        //instanzia il chunk a queste coordinate, usando il chunk prefab
        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(chunkPos.x, chunkPos.y, chunkPos.z), Quaternion.Euler(Vector3.zero)) as GameObject;

        //ottieni il componente "chunk" dell'oggetto
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();
        
        newChunk.transform.position = new Vector3(chunkPos.x, chunkPos.y, chunkPos.z);
        newChunk.transform.rotation = Quaternion.Euler(Vector3.zero);

        //assegna i suoi valori
        newChunk.chunkPosition = chunkPos;
        newChunk.mondo = this;

        //aggiungi al dizionario dei chunk, con la sua posizione come chiave
        chunks.Add(chunkPos, newChunk);
        
        //genera il terreno
        GeneraTerreno generaTerreno = new GeneraTerreno();
        newChunk = generaTerreno.GeneraChunk(newChunk);

        //si imposta che il chunk è da aggiornare, per renderizzarlo
        newChunk.daAggiornare = true;      

        //carica il chunk, per caricare i blocchi che sono stati modificati. È già stato aggiunto ai chunk da aggiornare, quindi al primo update verrà renderizzato.
        SaveAndLoad.CaricaChunk(newChunk);
    }

    /// <summary>
	/// rimuove il chunk
	/// </summary>
    public void RimuoviChunk(int chunkX, int chunkY, int chunkZ)
    {
        Chunk chunk = null;

        if (chunks.TryGetValue(new Vector3Int(chunkX, chunkY, chunkZ), out chunk))
        {
            //viene distrutto il gameObject e rimosso dalla lista dei chunks
            Destroy(chunk.gameObject);
            chunks.Remove(new Vector3Int(chunkX, chunkY, chunkZ));
        }
    }	

	/// <summary>
	/// Ottieni un chunk dalle coordinate
	/// </summary>
	public Chunk OttieniChunk(Vector3Int posChunk)
	{
        //ottieni il chunk se è già stato instanziato, se no ritorna null
        Chunk chunk = null;

        chunks.TryGetValue(posChunk, out chunk);

        return chunk;
	}

	/// <summary>
	/// Ottieni un chunk, ricercandolo tramite le coordinate
	/// </summary>
    public Chunk OttieniChunk(int chunkX, int chunkY, int chunkZ, int blockX, int blockY, int blockZ)
    {
        //ottieni la posizione del chunk
        Vector3Int pos = FunzioniMondo.OttieniPosChunk(chunkX, chunkY, chunkZ, blockX, blockY, blockZ);

        //ottieni il chunk se è già stato instanziato, se no ritorna null
        return OttieniChunk(pos);
    }

    /// <summary>
    /// Ottieni blocco, ricercandolo tramite le coordinate
    /// </summary>
    public Blocco OttieniBlocco(int chunkX, int chunkY, int chunkZ, int blockX, int blockY, int blockZ)
    {
        //ottieni il chunk che contiene questo blocco
        Chunk chunk = OttieniChunk(chunkX, chunkY, chunkZ, blockX, blockY, blockZ);

        if (chunk != null)
        {
            //ottieni l'indice del blocco
            Vector3Int indexBlocco = FunzioniMondo.ottieniIndexBlocco(blockX, blockY, blockZ);

            //ottieni il blocco, chiamando OttieniBlocco dallo script chunk
            Blocco blocco = chunk.OttieniBlocco(indexBlocco.x, indexBlocco.y, indexBlocco.z);

            return blocco;
        }
        else
        {
            //il blocco d'aria forza tutti i blocchi attorno a mostrare le facce che gli si affacciano contro,
            //quindi è perfetto nel caso non si riuscisse a trovare un blocco
            return new BloccoAria();
        }

    }

    ///<summary>
    ///Setta blocco, ricercandolo tramite le coordinate
    ///E aggiorna il chunk, e nel caso, quelli adiacenti
    ///- modificaBlocco, da settare se è una modifica del giocatore e dev'essere salvata
    ///</summary>
    public void SettaBlocco(int chunkX, int chunkY, int chunkZ, int blockX, int blockY, int blockZ, Blocco blocco, bool modificaBlocco = false)
    {
        //ottieni il chunk che contiene questo blocco
        Chunk chunk = OttieniChunk(chunkX, chunkY, chunkZ, blockX, blockY, blockZ);

        if (chunk != null)
        {            
            //ottieni l'indice del blocco
            Vector3Int indexBlocco = FunzioniMondo.ottieniIndexBlocco(blockX, blockY, blockZ);

            int newX = indexBlocco.x;
            int newY = indexBlocco.y;
            int newZ = indexBlocco.z;

            //setta il blocco
            chunk.SettaBlocco(newX, newY, newZ, blocco);

            if (modificaBlocco)
            {
                //se si salvano solo i blocchi modificati, si comunica che questo blocco è stato modificato
                chunk.OttieniBlocco(newX, newY, newZ).modificato = true;

                //salva il chunk, in quanto è appena stato modificato
                SaveAndLoad.SalvaChunk(chunk);          
            }

            //si imposta che il chunk è da aggiornare, per renderizzarlo
            chunk.daAggiornare = true;

            //se si modificano i blocchi ai bordi del chunk, bisogna aggiornare anche i chunk adiacenti (controlla tutti i 6 lati)
            //si passa la posizione del blocco, il bordo che si controlla, la posizione del chunk
            //poi si passa la posizione del blocco adiacente, per trovare il chunk di cui fa parte, nel caso si stesse modificando realmente un bordo
            AggiornaSeUguale(newX, 0, chunkX, chunkY, chunkZ, blockX - 1, blockY, blockZ);
            AggiornaSeUguale(newX, Chunk.grandezzaChunk - 1, chunkX, chunkY, chunkZ, blockX + 1, blockY, blockZ);
            AggiornaSeUguale(newY, 0, chunkX, chunkY, chunkZ, blockX, blockY - 1, blockZ);
            AggiornaSeUguale(newY, (Chunk.grandezzaChunk * Chunk.moltiplicatoreY) - 1, chunkX, chunkY, chunkZ, blockX, blockY + 1, blockZ);
            AggiornaSeUguale(newZ, 0, chunkX, chunkY, chunkZ, blockX, blockY, blockZ - 1);
            AggiornaSeUguale(newZ, Chunk.grandezzaChunk - 1, chunkX, chunkY, chunkZ, blockX, blockY, blockZ + 1);
        }
    }

    private void AggiornaSeUguale(int valore1, int valore2, int chunkX, int chunkY, int chunkZ, int blockX, int blockY, int blockZ)
    {
        //valore1 corrisponde al blocco che si modifica
        //valore2 corrisponde al bordo (blocco 0 o grandezzaChunk -1)
        //se il valore1 e il valore2 sono uguali, significa che stiamo modificando un bordo, si aggiorna quindi il chunk adiacente

        if (valore1 == valore2)
        {
            //si è passato il valore del blocco adiacente, quindi si riesce ad ottenere il chunk di cui fa parte, per aggiornarlo
            Chunk chunk = OttieniChunk(chunkX, chunkY, chunkZ, blockX, blockY, blockZ);

            //se il chunk esiste, si imposta che è da aggiornare, per renderizzarlo
            if(chunk != null)
                chunk.daAggiornare = true;
        }
    }
}