using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CaricaChunk
{
    public bool attivo = true;

    //distanza a cui generare chunk -> unità di misura: i chunk stessi 
    //è quindi da considerare moltiplicata per Chunk.grandezzaChunk * Blocco.grandezzaBlocco (perché potrebbero essere da 16 come da 32, e con blocchi da 1 come da 0.5f)
    //sia la distanzaChunk (asse x ed asse z), sia la distanzaY, sono come moltiplicate per 2, perché vengono usate sia in positivo che negativo
	//nel senso che carica distanzaChunk davanti al giocatore, distanzaChunk dietro, a destra e a sinistra. E carica distanzaY sia sopra il giocatore che sotto
    //con distanzaChunk = 10, viene caricato un numero x = 19 e z = 19 (in quanto il primo chunk sarà quello del giocatore) -> 19*19 = 361 chunk
    //con distanzaY = 4, verranno create colonne di 7 chunk (3 sopra, 3 sotto e 1 in cui è presente il giocatore)           -> 361*7 = 2527 chunk
    public int distanzaChunk = 30;
    public int distanzaY = 4;
    //distanza a cui rimuovere i chunk (unità di misura: sempre i chunk) - viene controllato solo sull'asse x e z, e rimuove tutta la colonna (asse y)
    public int distanzaRimuovereChunk = 60;

    //numero di chunk da caricare e aggiornare ad ogni frame
    public int chunkDaCaricare = 1;
    public int chunkDaAggiornare = 1;

    //per avere accesso allo script Mondo.cs per generare i chunk
    [HideInInspector]
    public Mondo mondo;
    
    //buildList crea i chunk (setta i blocchi, instanzia il prefab del chunk e lo renderizza)
    //la updateList serve per renderizzare nuovamente i chunk, nel caso si sia creato un nuovo chunk adiacente e quindi alcune facce non dovrebbero più essere visibili
    List<Vector3Int> buildList = new List<Vector3Int>();
    List<Vector3Int> updateList = new List<Vector3Int>();

	//per rimuovere chunk solo ogni tot frames
    private int timer = 0;

    //per controllare che il giocatore non si sia spostato in un altro chunk
    private Vector3Int posGiocatore;

    public void UpdateFunction(Transform transform)
    {
        if (attivo == false)
            return;

		//se è passato un tot numero di frame, rimuoverà i chunk distanti e non farà nient'altro per questo frame
        if (RimuoviChunks(transform))
            return;

        //TrovaChunksDaCaricare, troverà tutti i chunk che dovranno essere caricati.
        //CaricaEAggiornaChunks, invece li genererà un po' per volta, aggiungendone ad ogni frame un numero predefinito.
        
        //Se il giocatore si troverà sempre nello stesso chunk, questi verranno caricati normalmente,
        //se invece si sposterà, verranno caricati solo quelli per questo frame, poi verranno azzerate la buildList e la updateList,
        //che verranno ricaricate nel frame successivo (in quanto azzerate), seguendo la stessa logica:
        //ovvero, se ci si sposta ancora, ne verrà caricato solo il numero predefinito per quel frame, poi verranno azzerate nuovamente le liste.

        //IL PROBLEMA DI QUESTO SCRIPT
        //È CHE QUANDO IL GIOCATORE SI SPOSTA E SI AZZERA LA updateList, QUESTO FARà Sì CHE ALCUNI CHUNK NON VENGANO MAI AGGIORNATI, MANTENENDO FACCE RENDERIZZATE ANCHE SE NON VISIBILI.
        //La soluzione sarebbe non cancellare l'updateList, ma non si sa quanti elementi potrebbe finire per avere, dato che aggiorna i chunk solo quando la buildList è pari a 0.

        bool giocatoreStessaPosizione = GiocatoreStessaPos(transform);

        TrovaChunksDaCaricare();
        CaricaEAggiornaChunks(giocatoreStessaPosizione);
    }

    public void ChunkCaricati()
    {
        if(updateList.Count == 0)
        {
            Debug.Log("I chunk sono stati completamente caricati");
        }
    }

    private bool GiocatoreStessaPos(Transform transform)
    {
        //ottieni le coordinate del chunk in cui si trova il giocatore
        Vector3Int nuovaPosGiocatore = FunzioniMondo.OttieniChunkPlayer(transform.position);
        
        //controlla se il giocatore si trova in un altro chunk (e nel caso, ne aggiorna la posizione).
        //Se si trova sempre nello stesso, viene restituito true.
        //Altrimenti, viene restituito false, quindi verranno caricati solo i chunk di questo frame, poi azzerate le liste, che verranno ricreate nel frame successivo.
        if(FunzioniMondo.PlayerInChunk(nuovaPosGiocatore, posGiocatore) == false)
        {
            posGiocatore = nuovaPosGiocatore;
            return false;
        }

        return true;
    }

    private void TrovaChunksDaCaricare()
    {    
        //Se non ci sono già altri chunk da generare (o se sono state azzerate in seguito a uno spostamento del giocatore)
        if (buildList.Count == 0 && updateList.Count == 0)
        {
            //Controlla nei chunk vicini
            for (int x = 0; x < distanzaChunk; x++)
            {
                //           5
                //         5 4 5
                //       5 4 3 4 5
                //     5 4 3 2 3 4 5
                //   5 4 3 2 1 2 3 4 5
                // 5 4 3 2 1 0 1 2 3 4 5
                //   5 4 3 2 1 2 3 4 5
                //     5 4 3 2 3 4 5
                //       5 4 3 4 5
                //         5 4 5
                //           5

                //esempio: x è arrivato ad essere == a 2
                //xi = 2, z = 0 ->      carica (2,0), z == 0, (-2,0), z == 0  ->    xi-- = 1
                //xi = 1, z = 1 ->      carica (1,1), (1,-1), (-1,1), (-1,-1) ->    xi-- = 0
                //xi = 0, z = 2 ->      carica (0,2), (0,-2), x == 0, x == 0
                
                int xi = x;

                for (int zi = 0; zi <= x; zi++)
                {
                    SelezionaChunkDaCaricare(xi, zi, posGiocatore.x, posGiocatore.y, posGiocatore.z);

                    if(zi != 0)
                        SelezionaChunkDaCaricare(xi, -zi, posGiocatore.x, posGiocatore.y, posGiocatore.z);

                    if (xi != 0)
                        SelezionaChunkDaCaricare(-xi, zi, posGiocatore.x, posGiocatore.y, posGiocatore.z);

                    if (xi != 0 && zi != 0)
                        SelezionaChunkDaCaricare(-xi, -zi, posGiocatore.x, posGiocatore.y, posGiocatore.z);

                    xi--;
                }
            }
        }
    }

    void SelezionaChunkDaCaricare(int x, int z, int xGiocatore, int yGiocatore, int zGiocatore)
    {
        int grandezzaChunk = Mathf.FloorToInt(Chunk.grandezzaChunk * Blocco.grandezzaBlocco);
        int YgrandezzaChunk = Mathf.FloorToInt(Chunk.grandezzaChunk * Chunk.moltiplicatoreY * Blocco.grandezzaBlocco);

        //ad esempio: x = 0, z = 1, giocatore allo (16,16,16) e serve una colonna distanzaY == 4, con grandezzaChunk = 16 e grandezzaBlocco = 0.5f (grandezzaChunk = 16 * 0.5f = 8)
        // newChunkPos = (0*8+16, 0*8+16, 1*8+16)  -> sarà quindi il chunk davanti al giocatore (16,16,24)
        // newChunkPos = (16, 1*8+16, 24)          -> il chunk sopra al precedente (16,24,24)   -> menoNewChunkPos (16, -1*8+16, 24)    -> il chunk sotto al primo (16, 8, 24)
        // newChunkPos = (16, 2*8+16, 24)          -> il chunk sopra ancora (16,32,24)          -> menoNewChunkPos (16, -2*8+16, 24)    -> il chunk sotto ancora (16, 0, 24)
        // newChunkPos = (16, 3*8+16, 24)          -> il chunk sopra ancora (16,40,24)          -> menoNewChunkPos (16, -3*8+16, 24)    -> il chunk sotto ancora (16, -8, 24)

        //si carica tutta la colonna di chunk nella posizione (x,z)
        for (int y = 0; y < distanzaY; y++)
        {

            //xGiocatore, yGiocatore, zGiocatore, sono la posizione del chunk in cui si trova il giocatore, quindi quello di partenza che si potrebbe definire lo (0,0,0)
            //x * grandezzaChunk, per passare a quello a destra in base al valore di x (x == 1, sarà il chunk a destra, x == 2, sarà il secondo chunk a destra, ecc...)
            Vector3Int newChunkPos = new Vector3Int(
                x * grandezzaChunk + xGiocatore,
                y * YgrandezzaChunk + yGiocatore,
                z * grandezzaChunk + zGiocatore);

            //si ottiene il chunk, e se non esiste, lo si aggiunge alla buildList per generarlo
			Chunk chunk = mondo.OttieniChunk(newChunkPos);
            
            if(chunk == null)
                buildList.Add(newChunkPos);

            //se y è diverso da 0, si carica anche il chunk alla stessa (x,z) ma y in negativo, quindi al di sotto del player
            if (y != 0)
            {
                //si ottiene la posizione negativa -y, per avere anche i chunk sotto al giocatore
                Vector3Int menoNewChunkPos = new Vector3Int (
                    newChunkPos.x, 
                    -y * YgrandezzaChunk + yGiocatore, 
                    newChunkPos.z);

                //e se non esiste, si aggiunge anche questo chunk alla buildList
                Chunk menoChunk = mondo.OttieniChunk(menoNewChunkPos);

                if (menoChunk == null)
                    buildList.Add(menoNewChunkPos);
            }
        }
    }

    void CaricaEAggiornaChunks(bool giocatoreStessaPosizione)
    {
        //se la buildList non è vuota, genera un numero predefinito di chunk
        for (int i = 0; i < chunkDaCaricare; i++)
        {
            //se finiscono i chunk da caricare, ferma il ciclo for
            if(buildList.Count <= 0)
                break;

            GeneraChunk(buildList[0]);
        }

        //se ha finito di generare i chunk, aggiorna quelli già creati, per eliminare le facce non visibili
        if(buildList.Count == 0)
        {
            //se l'updateList non è vuota, aggiorna un numero predefinito di chunk
            for(int i = 0; i < chunkDaAggiornare; i++)
            {
                //se finiscono i chunk da aggiornare, ferma il ciclo for
                if(updateList.Count <= 0)
                    break;

                //Aggiorna i chunk.
                //Se viene restituito false, vuol dire che il chunk da aggiornare non esiste o è già presente in lista,
                //quindi si usa i-- per aggiornare un altro chunk al suo posto
                if(AggiornaChunk(updateList[0]) == false)
                {
                    if(updateList.Count > 0)
                        i--;
                }
            }
        }

        //se il giocatore si trova in un altro chunk, una volta generati i chunk di questo frame, azzera le liste
        if(giocatoreStessaPosizione == false)
        {            
            AzzeraListe();
        }
    }

    void GeneraChunk(Vector3Int chunkPos)
    {
        //rimuove il chunk dalla buildList, in quanto sta per essere generato
        buildList.Remove(chunkPos);

        //crea il chunk -> setta i blocchi, lo instanzia e al primo update verrà renderizzato (aggiornaChunk())
        mondo.CreaChunk(chunkPos);

        //dovranno essere aggiornati i chunk adiacenti ====================================================================
        int grandezzaChunk = Mathf.FloorToInt(Chunk.grandezzaChunk * Blocco.grandezzaBlocco);
        int YgrandezzaChunk = Mathf.FloorToInt(Chunk.grandezzaChunk * Chunk.moltiplicatoreY * Blocco.grandezzaBlocco);

        //si trova la posizione dei chunk adiacenti
        Vector3Int chunkSopra = new Vector3Int(chunkPos.x, chunkPos.y + YgrandezzaChunk, chunkPos.z);
        Vector3Int chunkSotto = new Vector3Int(chunkPos.x, chunkPos.y - YgrandezzaChunk, chunkPos.z);
        Vector3Int chunkNord = new Vector3Int(chunkPos.x, chunkPos.y, chunkPos.z + grandezzaChunk);
        Vector3Int chunkEst = new Vector3Int(chunkPos.x + grandezzaChunk, chunkPos.y, chunkPos.z);
        Vector3Int chunkSud = new Vector3Int(chunkPos.x, chunkPos.y, chunkPos.z - grandezzaChunk);
        Vector3Int chunkOvest = new Vector3Int(chunkPos.x - grandezzaChunk, chunkPos.y, chunkPos.z);

        //se non si trovano già in lista, e il chunk è già stato creato, li si aggiunge
        AggiungiChunkDaAggiornare(chunkSopra);
        AggiungiChunkDaAggiornare(chunkSotto);
        AggiungiChunkDaAggiornare(chunkNord);
        AggiungiChunkDaAggiornare(chunkEst);
        AggiungiChunkDaAggiornare(chunkSud);
        AggiungiChunkDaAggiornare(chunkOvest);
        //===============================================================================================
    }

    private void AggiungiChunkDaAggiornare(Vector3Int chunkPos)
    {
        //controlla che il chunk non sia già in lista, e che sia già stato creato
        //e nel caso, l'aggiunge all'updateList
        if(updateList.Contains(chunkPos) == false && mondo.OttieniChunk(chunkPos) != null)
        {
            updateList.Add(chunkPos);
        }
    }

    bool AggiornaChunk(Vector3Int chunkPos)
    {
        //rimuovi il chunk dalla updateList, in quanto sta per essere aggiornato, o è inesistente
        updateList.Remove(chunkPos);

        //Ottiene il chunk nella posizione data
        Chunk chunk = mondo.OttieniChunk(chunkPos);

        //il chunk potrebbe non esistere, perché magari rimosso per la troppa distanza
        if (chunk != null)
        {
            //se il chunk esiste, si imposta che è da aggiornare, per renderizzarlo
            chunk.daAggiornare = true;

            return true;            
        }

        return false;
    }

    bool RimuoviChunks(Transform transform)
    {
        //ogni 10 frame controlla tutti i chunk caricati (quelli presenti in mondo.chunks)
        //e rimuove quelli che si trovano troppo lontani

        if (timer == 10)
        {
            List<Vector3Int> chunksDaRimuovere = new List<Vector3Int>();
            //non viene controllato l'asse y, così da rimuovere completamente le colonne di chunk, solo se distanti sull'asse X o Z
            Vector3 posizioneGiocatore = new Vector3(transform.position.x, 0, transform.position.z);

            foreach (var chunk in mondo.chunks)
            {
                //passando il chunk, con valore y pari a 0, viene cancellata tutta la colonna, solo quando sarà distante sull'asse X o Z
                float distanza = Vector3.Distance(new Vector3(chunk.Value.chunkPosition.x, 0, chunk.Value.chunkPosition.z), posizioneGiocatore);

                int grandezzaChunk = Mathf.FloorToInt(Chunk.grandezzaChunk * Blocco.grandezzaBlocco);
                int distanzaCuiRimuovere = distanzaRimuovereChunk * grandezzaChunk;

				//aggiunge alla lista di chunk da rimuovere, tutti quelli troppo distanti
                if (distanza > distanzaCuiRimuovere)
                    chunksDaRimuovere.Add(chunk.Key);
            }

			//rimuove i chunk distanti
            foreach (var chunk in chunksDaRimuovere)
                mondo.RimuoviChunk(chunk.x, chunk.y, chunk.z);

            timer = 0;

            return true;
        }

		//se non rimuove nessun chunk, aggiunge che è passato un frame e ritorna false
        timer++;

        return false;
    }

    public void AzzeraListe()
    {
        buildList.Clear();
        updateList.Clear();
    }
}