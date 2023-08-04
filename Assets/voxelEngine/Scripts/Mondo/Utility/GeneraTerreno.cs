using UnityEngine;

public class GeneraTerreno
{
    //il seed, viene passato da Mondo.cs quando genera il terreno
    public static int seed = 1;

    //Queste sono le variabili per 3 parti separate del terreno

    //il primo layer per la pietra. Ha un'altezza di partenza,
    //poi la scala di noise da aggiungere all'altezza base,
    //e l'altezza del noise appena citato.
    
    //0.05f per il noise crea delle vette con circa 25 blocchi di distanza tra di loro. Ottimo per avere un terreno meno piatto.
    //L'altezza del noise è 4, così la differenza massima tra la vetta e la valle è di 4, non molto.

    //Poi abbiamo le variabili per le montagne, con una frequenza molto più bassa e un'altezza maggiore.
    //L'altezza minima qui, serve ad indicare dove può essere piazzata la pietra più bassa.

    //Infine un layer per la terra, che andrà sopra le pietre.
    //l'altezza base, è la profondità minima al di sopra della roccia
    //il noise è più disordinato delle pietre, ma con picchi più bassi.


    //pietraMontagna_frequenza è l'equivalente di pietraMontagna_noise
    //pietraMontagna_altezza è l'equivalente di pietraMontagna_altezzaNoise
    
    float pietra_altezzaBase = -24;
    float pietra_noise = 0.01f;     //0.01f 0.5m    //0.05f - 1m/0.5m
    float pietra_altezzaNoise = 4;

    float pietraMontagna_altezza = 48;
    float pietraMontagna_frequenza = 0.008f;
    float pietraMontagna_minimaAltezza = -12;

    float terra_altezzaBase = 1;
    float terra_noise = 0.01f;      //0.1f - 0.5m   //0.2f - 0.5m   //0.04f - 1m
    float terra_altezzaNoise = 3;

    // le variabili per creare cave (spazio composto da BloccoAria) e alberi (costruzione da instanziare in un punto della mappa)

    float cava_frequenza = 0.025f;
    float cava_grandezza = 7;      //14f - 0.5m     //7f - 1m

    float albero_frequenza = 0.2f;  //0.4f - 0.5m   //0.2f - 1m
    float albero_densità = 3;       //1f - 0.5m      //3f - 1m

    public Chunk GeneraChunk(Chunk chunk)
    {
        //prende un chunk, setta i blocchi e lo restituisce una volta settato.
        //Questo viene fatto, facendo un ciclo per ogni colonna del chunk e gestendole individualmente
        //(x = 0, y = da 0 a grandezza chunk * moltiplicatore y, z = 0)
        //(x = 0, y = da 0 a grandezza chunk * moltiplicatore y, z = 1) e così via
        
        for (int x = 0; x < Chunk.grandezzaChunk ; x++)
        {
            for (int z = 0; z < Chunk.grandezzaChunk ; z++)
            {
                chunk = GeneraColonnaChunk(chunk, chunk.chunkPosition.x, chunk.chunkPosition.z, x, z);
            }
        }

        return chunk;
    }

    private Chunk GeneraColonnaChunk(Chunk chunk, int chunkX, int chunkZ, int blockX, int blockZ)
    {
        //Creiamo una variabile altezzaPietra, settata in base all'altezza base.
        //Aggiungiamo il noise della montagna,
        //alziamo ogni valore sotto al pietraMontagna_minimaAltezza, appunto al suo valore. 
        //Fatto questo, applichiamo il noise della pietra.

        float posX = chunkX + blockX * Blocco.grandezzaBlocco;
        float posZ = chunkZ + blockZ * Blocco.grandezzaBlocco;

        float altezzaPietra = pietra_altezzaBase;

        altezzaPietra += FunzioniMondo.GetNoise(seed, posX, 0, posZ, pietraMontagna_frequenza, pietraMontagna_altezza);

        if (altezzaPietra < pietraMontagna_minimaAltezza)
            altezzaPietra = pietraMontagna_minimaAltezza;

        altezzaPietra += FunzioniMondo.GetNoise(seed, posX, 0, posZ, pietra_noise, pietra_altezzaNoise);
        
        //Poi creiamo una variabile altezzaTerra, uguale a ciò che è diventata ora altezzaPietra più terra_altezzaBase,
        //e aggiungiamo il noise della terra in cima.

        float altezzaTerra = altezzaPietra + terra_altezzaBase;

        altezzaTerra += FunzioniMondo.GetNoise(seed, posX, 100, posZ, terra_noise, terra_altezzaNoise);

        //Infine eseguiamo il ciclo per tutta la colonna, aggiungendo il blocco desiderato.
        
        for (int blockY = 0; blockY < Chunk.grandezzaChunk * Chunk.moltiplicatoreY; blockY++)
        {
            float posY = chunk.chunkPosition.y + blockY * Blocco.grandezzaBlocco;

            //creiamo il noise per le cave
            float cavaChance = FunzioniMondo.GetNoise(seed, posX, posY, posZ, cava_frequenza, 100);

            //se ci troviamo al di sotto dell'altezzaPietra, ci va quindi un BloccoPietra,
            //se ci troviamo al di sotto dell'altezzaTerra, ci va un BloccoTerra,
            //o BloccoErba se ci si trova in cima (altezzaTerra)
            //I blocchi vengono piazzati, solo se cava_grandezza è minimore del noise creato in precedenza.
            //Se non viene piazzato nessun blocco, viene creato un BloccoAria

            if (posY <= altezzaPietra && cava_grandezza < cavaChance)
            {
                FunzioniMondo.SettaBloccoNuovoChunk(blockX, blockY, blockZ, new BloccoPietra(), chunk);
            }
            else if (posY <= altezzaTerra && cava_grandezza < cavaChance)
            {
                if(posY == altezzaTerra)
                    FunzioniMondo.SettaBloccoNuovoChunk(blockX, blockY, blockZ, new BloccoErba(), chunk);
                else
                    FunzioniMondo.SettaBloccoNuovoChunk(blockX, blockY, blockZ, new BloccoTerra(), chunk);

                //per creare gli alberi.
                //Viene creato il noise degli alberi quando ci troviamo dove si creano blocchi d'erba e terra (non cava, determinata altezza, ecc...)
                //Se ci troviamo sul blocco di erba, quindi in cima (altezzaTerra), e il noise è inferiore alla densità di alberi,
                //allora si crea l'albero
                float alberoChance = FunzioniMondo.GetNoise(seed, posX, 0, posZ, albero_frequenza, 100);

                if (posY == altezzaTerra && alberoChance < albero_densità)
                    Albero.Crea(blockX, blockY, blockZ, chunk, seed);
            }
            else
            {
                //setta BloccoAria ogni blocco del chunk che non fa parte dei layer creati, o se sta creando una cava
                FunzioniMondo.SettaBloccoNuovoChunk(blockX, blockY, blockZ, new BloccoAria(), chunk);
            }
        }

        return chunk;
    }
}

