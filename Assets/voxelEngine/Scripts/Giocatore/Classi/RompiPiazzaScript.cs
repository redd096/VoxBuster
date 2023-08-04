using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RompiPiazzaScript
{
    //per rompere tutti i blocchi (utile per la creative)
    public bool nessunBloccoIndistruttibile = false;

    //rateo di fuoco - il tempo tra un colpo e l'altro quando si tiene premuto per rompere un blocco
    public float rateoDiFuoco = 0.1f;

    //danno ai nemici e ai blocchi
    public float danno = 1;

    //per sapere se si ha rilasciato il tasto per piazzare i blocchi
    bool rilasciatoInputPiazza;

    //distanza a cui si arriva con il Raycast
    public float distanza = 20;


    //per sapere quanto tempo è passato dall'ultimo sparo (quando si tiene premuto il tasto per rompere un blocco)
    [HideInInspector]
    public float canFire;

    //per distruggere gli alberi
    public class blocchiDaRimuovere
    {
        public Chunk chunk;
        public Vector3Int blockIndex;
    }
    [HideInInspector]
    public List<blocchiDaRimuovere> lista_blocchiDaRimuovere = new List<blocchiDaRimuovere>();
    [HideInInspector]
    public int frameDaUltimoBloccoRimosso = 5;   //per rompere un blocco dell'albero ogni tot frame

    //rompi_ viene usato per tutto ciò che riguarda l'input con cui si rompono blocchi (anche in caso si colpisca un nemico, ad esempio per sparare)
    //piazza_ viene usato per tutto ciò che riguarda l'input con cui si piazzano i blocchi (anche in caso si colpisca un nemico, ad esempio con una skill)

    public virtual void UpdateFunction(Camera cam, bool InputRompi, bool InputPiazza)
    {
        RimuoviBlocchiLista();

        if (PuoiEseguire(InputRompi, InputPiazza) == false)
        {
            return;
        }

        EseguiInput(cam, InputRompi, InputPiazza);
    }

    public virtual bool PuoiEseguire(bool InputRompi, bool InputPiazza)
    {
        //se si prova a rompere un blocco e non si può ancora (in base al rateo di fuoco), allora non esegue la funzione
        //se si prova a piazzare un blocco, ma lo si è già piazzato e non si ha rilasciato il tasto (si tiene il tasto premuto), allora non esegue la funzione
        if (InputRompi && Time.time < canFire || InputPiazza && rilasciatoInputPiazza == false)
        {
            return false;
        }

        //se si preme il tasto per rompere i blocchi
        if (InputRompi)
        {
            //settiamo il momento in cui si potrà di nuovo rompere blocchi
            canFire = Time.time + rateoDiFuoco;
        }

        //se si preme il tasto per piazzare blocchi, allora rilasciatoInputPiazza == false (finché non si lascia il tasto, non si possono piazzare blocchi)
        //se non si tiene premuto il tasto per piazzare il blocco, allora rilasciatoInputPiazza == true (quando si premerà il bottone, si potrà piazzare il blocco)
        if (InputPiazza)
        {
            rilasciatoInputPiazza = false;
        }
        else
        {
            rilasciatoInputPiazza = true;
        }

        return true;
    }

    public virtual void EseguiInput(Camera cam, bool InputRompi, bool InputPiazza)
    {
        //rompe o piazza il blocco
        if (InputRompi)
        {
            ColpitoChunk(cam, true);
        }
        else if (InputPiazza)
        {

            ColpitoChunk(cam, false);
        }
    }

    //controlla d'aver colpito un chunk, quindi richiama le funzioni dei blocchi o dei nemici (che potrebbero non essere nemici, ma tutto ciò che non riguarda il chunk)
    //bRompi == true se si usa l'input per rompere i blocchi, bRompi == false se si usa l'input per piazzare i blocchi
    public void ColpitoChunk(Camera cam, bool bRompi)
    {
        RaycastHit hit;

        Ray vRay = new Ray();

        //se il mouse è bloccato al centro dello schermo, crea il Ray dal centro in avanti, altrimenti parte dalla posizione del mouse sullo schermo
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            vRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        }
        else
        {
            vRay = cam.ScreenPointToRay(Input.mousePosition);
        }

        //lancia il raycast fino alla distanza predefinita
        if (Physics.Raycast(vRay, out hit, distanza))
        {
            //controlla che il collider abbia il componente chunk
            Chunk chunk = hit.collider.GetComponent<Chunk>();
            if (chunk != null)
            {
                //chiama la funzione per rompere i blocchi o per piazzare i blocchi
                if (bRompi)
                    Rompi_Blocco(hit);
                else
                    Piazza_Blocco(hit);
            }
            else
            {
                //chiama le funzioni per quando non si prende un chunk, in base all'input che si usa (per rompere o per piazzare blocchi)
                if (bRompi)
                    Rompi_Nemico(hit);
                else
                    Piazza_Nemico(hit);
            }
        }
    }

    //la funzione se si cerca di rompere un blocco, e ciò che si è preso è realmente un chunk
    public virtual void Rompi_Blocco(RaycastHit hit)
    {
        Blocco blocco = ModificheGiocatore.OttieniBlocco(hit);
        if (blocco != null)
        {
            //otteniamo il chunk di appartenenza del blocco, il suo index,
            //e poi chiamiamo la funzione per danneggiarlo
            Chunk chunk = hit.collider.GetComponent<Chunk>();
            Vector3Int blockIndex = ModificheGiocatore.OttieniIndexBlocco(hit, chunk.chunkPosition, false);

            //si danneggia o distrugge il blocco
            DanneggiaBlocco(blocco, chunk, blockIndex, hit);
        }
    }

    //la funzione se si usa l'input per rompere i blocchi, ma ciò che si è preso non è un chunk
    public virtual void Rompi_Nemico(RaycastHit hit)
    {
        Debug.Log("Non è stato colpito un chunk, potrebbe essere un nemico: " + hit.transform.name);
    }

    //la funzione se si cerca di piazzare un blocco, e ciò che si è preso è realmente un chunk
    public virtual void Piazza_Blocco(RaycastHit hit)
    {
        if (puoi_piazzareBlocchi(hit))
        {
            ModificheGiocatore.SettaBlocco(hit, new BloccoVetro(), true);
        }
    }

    //la funzione se si usa l'input per piazzare i blocchi, ma ciò che si è preso non è un chunk
    public virtual void Piazza_Nemico(RaycastHit hit)
    {
        Debug.Log("Non è stato piazzato in un chunk, potrebbe aver toccato un nemico: " + hit.transform.name);
    }

    public bool puoi_piazzareBlocchi(RaycastHit hit)
    {
        //controlla che il punto in cui si vuole piazzare un blocco, sia occupato da un BloccoAria
        //se il blocco non esiste, allora non si può modificare il tipo di blocco, e se non è aria non si può piazzare un altro blocco
        Blocco b = ModificheGiocatore.OttieniBlocco(hit, true);

        if (b != null && b.GetType() == new BloccoAria().GetType())
            return true;

        return false;
    }

    public virtual void DanneggiaBlocco(Blocco blocco, Chunk chunk, Vector3Int blockIndex, RaycastHit hit)
    {
        //si controlla che non sia indistruttibile
        if (blocco.indistruttibile == false || nessunBloccoIndistruttibile)
        {
            //si fa il danno
            blocco.vitaBlocco -= danno;

            //si distrugge il blocco
            if (blocco.vitaBlocco <= 0)
            {
                //se si tratta del tronco di un albero, vanno distrutti tutti i blocchi sopra
                DistruggiAlbero(blocco, chunk, blockIndex);

                ModificheGiocatore.SettaBlocco(hit, new BloccoAria());
            }
        }
    }

    public virtual void DistruggiAlbero(Blocco blocco, Chunk chunk, Vector3Int blockIndex)
    {
        //se si tratta di un tronco (blocco di legno)
        if (blocco.GetType() == new BloccoLegno().GetType())
        {
            Vector3Int chunkPosition = chunk.chunkPosition;
            int posInizioFoglie = blockIndex.y + 6;
            int primoBloccoTronco = posInizioFoglie - 6;

            //spacca il tronco -> yt parte da 1, perché il primo blocco viene già distrutto nella funzione che richiama questa
            for (int yt = 1; yt < 6; yt++)
            {
                //otteniamo il tronco che vogliamo rompere (blockIndex.y + yt)
                Blocco bloccoDaRompere = chunk.mondo.OttieniBlocco(chunkPosition.x, chunkPosition.y, chunkPosition.z, blockIndex.x, blockIndex.y + yt, blockIndex.z);

                if (bloccoDaRompere.GetType() == new BloccoLegno().GetType())
                {
                    //se è di legno, lo aggiunge ai blocchi da rompere, 
                    AggiungiBlocchiLista(chunk, blockIndex.x, blockIndex.y + yt, blockIndex.z);
                }
                else if (bloccoDaRompere.GetType() == new BloccoFoglie().GetType() || bloccoDaRompere.GetType() == new BloccoFoglie1().GetType()
                    || bloccoDaRompere.GetType() == new BloccoFoglie2().GetType() || bloccoDaRompere.GetType() == new BloccoFoglie3().GetType())
                {
                    //se è un tipo di foglie, setta la posInizioFoglie e significa che si è spaccata una parte più alta del tronco, non la radice
                    posInizioFoglie = blockIndex.y + yt;
                    primoBloccoTronco = posInizioFoglie - 6;
                    break;
                }
            }

            //spacca le foglie
            for (int xi = -2; xi <= 2; xi++)
            {
                for (int yi = primoBloccoTronco + 4; yi <= primoBloccoTronco + 8; yi++)
                {
                    for (int zi = -2; zi <= 2; zi++)
                    {
                        //otteniamo la foglia che vogliamo rompere (blockIndex.x + xi, yi, blockIndex.z + zi)
                        Blocco bloccoDaRompere = chunk.mondo.OttieniBlocco(chunkPosition.x, chunkPosition.y, chunkPosition.z, blockIndex.x + xi, yi, blockIndex.z + zi);

                        //se è un qualche tipo di foglia, lo aggiunge ai blocchi da rompere
                        if (bloccoDaRompere.GetType() == new BloccoFoglie().GetType() || bloccoDaRompere.GetType() == new BloccoFoglie1().GetType()
                            || bloccoDaRompere.GetType() == new BloccoFoglie2().GetType() || bloccoDaRompere.GetType() == new BloccoFoglie3().GetType())
                        {
                            AggiungiBlocchiLista(chunk, blockIndex.x + xi, yi, blockIndex.z + zi);
                        }
                    }
                }
            }

            //elimina luce

            //la posizione della luce all'interno del tronco
            Vector3 posLuce = FunzioniMondo.OttieniPosBlocco(chunk.chunkPosition.x, chunk.chunkPosition.y, chunk.chunkPosition.z, blockIndex.x, primoBloccoTronco + 1, blockIndex.z);
            //si ottengono tutte le luci nel chunk
            Light[] luciInChunk = chunk.GetComponentsInChildren<Light>();
            foreach (Light l in luciInChunk)
            {
                //se si trova una luce che ha la stessa posizione di posLuce, allora la si distrugge
                if (l.transform.position == posLuce)
                {
                    GameObject.Destroy(l.gameObject);
                    break;
                }
            }
        }
    }

    public virtual void RimuoviBlocchiLista()
    {
        //se ci sono alberi da rompere, allora viene eliminato un blocco ogni tot frame
        while (lista_blocchiDaRimuovere.Count > 0)
        {
            //controlla che siano già passati i frame
            if (frameDaUltimoBloccoRimosso > 0)
            {
                frameDaUltimoBloccoRimosso--;
                break;
            }

            //ottieni un blocco random tra quelli che vanno rimossi
            blocchiDaRimuovere var = lista_blocchiDaRimuovere[Random.Range(0, lista_blocchiDaRimuovere.Count)];

            Chunk chunk = var.chunk;
            Vector3Int chunkPosition = chunk.chunkPosition;
            Vector3Int blockIndex = var.blockIndex;
            //trasforma il blocco in aria
            chunk.mondo.SettaBlocco(chunkPosition.x, chunkPosition.y, chunkPosition.z, blockIndex.x, blockIndex.y, blockIndex.z, new BloccoAria(), true);

            //rimuove il blocco dalla lista
            lista_blocchiDaRimuovere.Remove(var);

            //per rompere un blocco ogni tot frame
            frameDaUltimoBloccoRimosso = 2;
            break;
        }
    }

    public virtual void AggiungiBlocchiLista(Chunk chunk, int blockX, int blockY, int blockZ)
    {
        blocchiDaRimuovere bloccoDaModificare = new blocchiDaRimuovere();

        //anche se il blocco dovesse trovarsi in un chunk adiacente, 
        //nel momento in cui verrà chiamato chunk.mondo.SettaBlocco, verrà già trovato il chunk e il reale index del blocco
        bloccoDaModificare.chunk = chunk;
        bloccoDaModificare.blockIndex = new Vector3Int(blockX, blockY, blockZ);

        //aggiunge il blocco alla lista dei blocchi da rimuovere
        lista_blocchiDaRimuovere.Add(bloccoDaModificare);
    }
}