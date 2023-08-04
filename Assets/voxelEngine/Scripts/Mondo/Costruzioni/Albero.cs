using UnityEngine;

public static class Albero
{

    static float albero_noise = 0.1f;
    static float albero_densità = 5;

    public static void Crea(int x, int y, int z, Chunk chunk, int seed)
    {
        //ottiene il noise
        float noise = FunzioniMondo.GetNoise(seed, x, y, z, albero_noise, 20);

        //crea le variabili per sapere quando creare un albero e quando un altro
        float density0 = albero_densità / 2;
        float density1 = density0 + (density0 / 3);
        float density2 = density1 + (density0 / 3);
        
        //se si vuole creare alberi completamente diversi
        //
        //if (noise < density0)
        //    CreaAlbero0(x, y, z, chunk);
        //else if (noise >= density0 && noise < density1)
        //    CreaAlbero1(x, y, z, chunk);


        //se si vuole modificare solo il colore delle foglie       

        int coloreFoglie = 0;

        if (noise < density0)
            coloreFoglie = 0;
        else if (noise >= density0 && noise < density1)
            coloreFoglie = 1;
        else if (noise >= density1 && noise < density2)
            coloreFoglie = 2;
        else
            coloreFoglie = 3;

        CreaAlbero(x, y, z, chunk, coloreFoglie);
    }

    static void CreaAlbero(int x, int y, int z, Chunk chunk, int coloreFoglie)
    {
        //per far diventare il blocco d'erba un blocco di terra
        FunzioniMondo.SettaBloccoNuovoChunk(x, y, z, new BloccoTerra(), chunk, true);

        //per creare l'albero sopra il blocco d'erba
        y += 1;

        //crea foglie
        for (int xi = -2; xi <= 2; xi++)
        {
            for (int yi = 4; yi <= 8; yi++)
            {
                for (int zi = -2; zi <= 2; zi++)
                {
                    Blocco foglie = new BloccoFoglie();

                    switch(coloreFoglie)
                    {
                        case 1:
                            foglie = new BloccoFoglie1();
                            break;
                        case 2:
                            foglie = new BloccoFoglie2();
                            break;
                        case 3:
                            foglie = new BloccoFoglie3();
                            break;
                    }

                    FunzioniMondo.SettaBloccoNuovoChunk(x + xi, y + yi, z + zi, foglie, chunk, true);
                }
            }
        }

        //crea il tronco
        for (int yt = 0; yt < 6; yt++)
        {
            FunzioniMondo.SettaBloccoNuovoChunk(x, y + yt, z, new BloccoLegno(), chunk, true);
        }

        //crea luce
        GameObject go = new GameObject();
        Light luce = go.AddComponent<Light>();
        luce.type = LightType.Point;
        luce.range = 30;
        luce.intensity = 5;
        if(coloreFoglie == 0)
            luce.color = new Color32(0, 255, 0, 255);
        else if (coloreFoglie == 1)
            luce.color = new Color32(255, 168, 0, 255);
        else if (coloreFoglie == 2)
            luce.color = new Color32(0, 255, 223, 255);
        else
            luce.color = new Color32(255, 100, 255, 255);

        Vector3 pos = new Vector3(
            chunk.chunkPosition.x + (x * Blocco.grandezzaBlocco),
            chunk.chunkPosition.y + (y * Blocco.grandezzaBlocco) + Blocco.grandezzaBlocco,
            chunk.chunkPosition.z + (z * Blocco.grandezzaBlocco));

        go.transform.position = pos;

        go.transform.SetParent(chunk.transform);
    }
    
    //static void CreaAlbero1(int x, int y, int z, Chunk chunk)
    //{
        //per creare l'albero sopra il blocco d'erba
    //    y += 1;
    //
        //crea foglie
    //    int maxFoglie = 4;
    //
    //    for (int yi = 4; yi <= 8; yi++)
    //    {
    //        for (int xi = -maxFoglie; xi <= maxFoglie; xi++)
    //        {
    //            for (int zi = -maxFoglie; zi <= maxFoglie; zi++)
    //            {
    //                GeneraTerreno.SettaBlocco(x + xi, y + yi, z + zi, new BloccoFoglie(), chunk, true);
    //            }
    //        }
    //        maxFoglie -= 1;
    //    }
    //
        //crea il tronco
    //    for (int yt = 0; yt < 8; yt++)
    //    {
    //        GeneraTerreno.SettaBlocco(x, y + yt, z, new BloccoLegno(), chunk, true);
    //    }
    //}
}