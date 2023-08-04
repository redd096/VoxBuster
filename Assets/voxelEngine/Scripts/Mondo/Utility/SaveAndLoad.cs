using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using System.Collections.Generic;

[System.Serializable]
public class BlocchiSalvati
{
    //si trova in una classe Serializable, così da poter essere salvato.
    //blocchiModificati è il dictionary dei blocchi che sono stati, appunto modificati, all'interno del chunk.
    //usa come key la posizione del blocco all'interno del chunk, e come value il tipo di blocco
    public Dictionary<posizioneBlocco, Blocco> blocchiModificati = new Dictionary<posizioneBlocco, Blocco>();

    public BlocchiSalvati(Chunk chunk)
    {
        //controlla tutti i blocchi del chunk
        //e mette nella lista dei blocchi da salvare, tutti quelli che sono stati modificati

        for (int x = 0; x < Chunk.grandezzaChunk; x++)
        {
            for (int y = 0; y < Chunk.grandezzaChunk * Chunk.moltiplicatoreY; y++)
            {
                for (int z = 0; z < Chunk.grandezzaChunk; z++)
                {
                    if (!chunk.blocchi[x, y, z].modificato)
                        continue;

                    posizioneBlocco posBlocco = new posizioneBlocco(x, y, z);
                    blocchiModificati.Add(posBlocco, chunk.blocchi[x, y, z]);
                }
            }
        }
    }

}

//Vector3Int non è serializable, quindi creo una struct che possa essere usata al suo posto e possa essere salvata (serializable)
[System.Serializable]
public struct posizioneBlocco
{
    public int x;
    public int y;
    public int z;

    public posizioneBlocco(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}


public static class SaveAndLoad
{
	//la lista Blocchi dello script Chunk.cs deve essere public
	//ogni blocco dev'essere System.Serializable, per poterlo salvare come file binario (comprese le variabili private)

    //la cartella in cui verranno poi salvati tutti i mondi
    static string nomeCartella = "Salvataggi";

    ///<summary>
    ///riporta alla cartella dei salvataggi,
    ///e se non esiste, la crea.
    ///</summary>
    public static string CartellaDeiSalvataggi(string nomeMondo)
    {
        // percorso_del_progetto/Assets/nomeCartella
        string cartellaDeiSalvataggi = Application.dataPath + "/" + nomeCartella;

        //se il mondo ha un nome, cerca la sottocartella con quel nome
        if(nomeMondo != "")
            cartellaDeiSalvataggi += "/" + nomeMondo + "/";

        //se esiste la cartella percorso_del_progetto/Assets/nomeCartella/nomeMondo
        //allora ritorna il suo percorso, altrimenti la crea e ritorna comunque il percorso.
        if (!Directory.Exists(cartellaDeiSalvataggi))
        {
            Directory.CreateDirectory(cartellaDeiSalvataggi);
        }

        return cartellaDeiSalvataggi;
    }

    //restituisce il nome del file che verrà salvato, ovvero le coordinate del chunk x,y,z.bin
    private static string NomeFile(Vector3Int posizioneChunk)
    {
        string nomeFile = posizioneChunk.x + "," + posizioneChunk.y + "," + posizioneChunk.z + ".bin";
        return nomeFile;
    }

    ///<summary>
    ///salva il chunk all'interno della cartella dei salvataggi
    ///</summary>
    public static void SalvaChunk(Chunk chunk)
    {
        //se non ci sono blocchi modificati, non salvare
        BlocchiSalvati blocchiSalvati = new BlocchiSalvati(chunk);
        if (blocchiSalvati.blocchiModificati.Count == 0)
            return;

        //se il chunk ha dei blocchi modificati, ottiene la cartella in cui dovrà essere salvato e il nome del file da salvare.
        string saveFile = CartellaDeiSalvataggi(chunk.mondo.nomeMondo);
        saveFile += NomeFile(chunk.chunkPosition);

        //apre un FileStream di percorso_del_progetto/Assets/nomeCartella/nomeMondo/x,y,z.bin
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);

        //salva il file, con contenuto tutti i blocchi che sono stati modificati
        formatter.Serialize(stream, blocchiSalvati);            

        //chiude il FileStream
        stream.Close();
    }

    ///<summary>
    ///carica il chunk che si trova all'interno della cartella dei salvataggi
    ///</summary>
    public static bool CaricaChunk(Chunk chunk)
    {
        //ottiene la cartella in cui è stato salvato il chunk e il nome del file da caricare.
        string saveFile = CartellaDeiSalvataggi(chunk.mondo.nomeMondo);
        saveFile += NomeFile(chunk.chunkPosition);

        //se il file non esiste restituisce false
        if (!File.Exists(saveFile))
            return false;

        //apre un FileStream di percorso_del_progetto/Assets/nomeCartella/nomeMondo/x,y,z.bin
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);

        //controlla in BlocchiSalvati, per sapere quali blocchi sono stati modificati
        //e li passa nella lista dei blocchi del chunk, che poi si aggiornerà per renderizzarli
        BlocchiSalvati salvati = (BlocchiSalvati)formatter.Deserialize(stream);
        foreach (var blocco in salvati.blocchiModificati)
        {
            //usa come key la posizione del blocco e come value il tipo di blocco, quindi 
            //chunk.blocchi[keyX,keyY,keyZ] = value
            chunk.blocchi[blocco.Key.x, blocco.Key.y, blocco.Key.z] = blocco.Value;

            //aggiunge i blocchi caricati alla lista dei blocchiCostruzione, all'interno di Mondo.cs, così che non vengano sovrascritti quando si generano delle costruzioni
            Vector3 posizioneBlocco = FunzioniMondo.OttieniPosBlocco(chunk.chunkPosition.x, chunk.chunkPosition.y, chunk.chunkPosition.z, blocco.Key.x, blocco.Key.y, blocco.Key.z);
            Mondo.BloccoCostruzione bloccoCaricato = new Mondo.BloccoCostruzione(blocco.Value, true);
            chunk.mondo.blocchiCostruzione.Add(posizioneBlocco, bloccoCaricato);
        }

        //chiude il FileStream
        stream.Close();

        //restituisce true, perché è stato caricato il file
        return true;
    }

    ///<summary>
    ///salva i valoriMondo, ovvero i valori che vengono passati alla creazione del mondo
    ///-erroreNomeEsistente, serve per decidere se ricevere un errore nel caso il nome sia già stato usato, oppure creare comunque il mondo con un numero in fondo
    ///</summary>
    public static string SalvaMondo(string nomeMondo, ValoriMondo valoriMondo, bool erroreNomeEsistente)
    {
        //ottiene la cartella in cui dovrà essere salvato e il nome del file da salvare.
        string saveFile = CartellaDeiSalvataggi(nomeMondo);
        saveFile += "ValoriMondo.bin";

        //se il file esiste, allora significa che esiste già un mondo con lo stesso nome.
        if (File.Exists(saveFile))
        {
            //se erroreNomeEsistente == true, allora si restituisce l'errore in cui si dice che il nome è già in uso
            if(erroreNomeEsistente)
                return "Nome del mondo già esistente";

            //altrimenti viene creata una nuova cartella, con lo stesso nome ma in fondo un numero crescente.
            int numeroMondo = 1;

            //si prosegue finché esistono cartelle con lo stesso nome percorso_del_progetto/Assets/nomeCartella/nomeMondo1/ValoriMondo.bin
            //percorso_del_progetto/Assets/nomeCartella/nomeMondo2/ValoriMondo.bin, nomeMondo3, nomeMondo4, ecc...
            while (File.Exists(CartellaDeiSalvataggi(nomeMondo + numeroMondo) + "ValoriMondo.bin"))
            {
                numeroMondo++;
            }

            //quando si trova un nome che non è già stato usato, si modifica nomeMondo per avere davanti il numero ottenuto
            nomeMondo += numeroMondo;

            //crea la nuova cartella
            saveFile = CartellaDeiSalvataggi(nomeMondo);
            saveFile += "ValoriMondo.bin";
        }

        //apre un FileStream di percorso_del_progetto/Assets/nomeCartella/nomeMondo/ValoriMondo.bin
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);

        //salva i dati del mondo
        formatter.Serialize(stream, valoriMondo);

        //chiude il FileStream
        stream.Close();

        //restituisce il nome del mondo, nel caso sia stato modificato (perché già esistente)
        return nomeMondo;
    }

    ///<summary>
    ///carica i valoriMondo che si trovano all'interno della cartella dei salvataggi
    ///</summary>
    public static ValoriMondo CaricaMondo(string nomeMondoDaCaricare)
    {
        //ottiene la cartella in cui è stato salvato e il nome del file da caricare.
        string saveFile = CartellaDeiSalvataggi(nomeMondoDaCaricare);
        saveFile += "ValoriMondo.bin";

        //se non esiste il file, allora non carica e restituisce null
        //(non cancella la cartella creata, perché il giocatore potrebbe aver solo eliminato il file e non i chunk salvati al suo interno)
        if (!File.Exists(saveFile))
            return null;

        //apre un FileStream di percorso_del_progetto/Assets/nomeCartella/nomeMondo/ValoriMondo.bin
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);

        //ottiene i dati dal file caricato
        ValoriMondo datiCaricati = (ValoriMondo)formatter.Deserialize(stream);

        //chiude il FileStream
        stream.Close();

        //restituisce i dati caricati
        return datiCaricati;
    }
}
