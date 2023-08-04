using UnityEngine;

public static class ModificaLayer
{
    // i layer funzionano in binario -> in questo modo si ottiene il valore binario di 8 (in cui in genere metto il layer "Giocatore")
    // LayerMask Layer = 1 << 8;

    // in questo modo si aggiunge il valore binario del layer chiamato "Ignore Raycast"
    // Layer |= 1 << LayerMask.NameToLayer("Ignore Raycast");

    // Normalmente il Raycast colpirebbe solo le cose con layer 8 o Ignore Raycast, 
    // con la tilde (~) fa il contrario -> colpisce tutto tranne ciò che ha quei layer
    // Layer = ~Layer;

    //oppure si può fare in una riga LayerMask Layer = ~( (1 << 8) | (1 << LayerMask.NameToLayer("Ignore Raycast")) );



    /// <summary>
    /// Un raycast lanciato con questo LayerMask, colpirà tutti i layer tranne quelli nell'array
    /// </summary>
    /// <param name="IndiceLayer"></param>
    /// <returns></returns>
    public static LayerMask LayerTuttoTranne (int[] IndiceLayer)
    {
        LayerMask Layer = new LayerMask();

        for (int i = 0; i < IndiceLayer.Length; i++)
        {
            Layer |= 1 << IndiceLayer[i];
        }

        Layer = ~Layer;

        return Layer;
    }

    /// <summary>
    /// Un raycast lanciato con questo LayerMask, colpirà tutti i layer tranne quelli nell'array
    /// </summary>
    /// <param name="NomeLayer"></param>
    /// <returns></returns>
    public static LayerMask LayerTuttoTranne (string[] NomeLayer)
    {
        int[] layers = LayerStringToInt(NomeLayer);

        return LayerTuttoTranne(layers);
    }

    /// <summary>
    /// Un raycast lanciato con questo LayerMask, colpirà solo i layer nell'array
    /// </summary>
    /// <param name="IndiceLayer"></param>
    /// <returns></returns>
    public static LayerMask LayerSolo (int[] IndiceLayer)
    {
        LayerMask Layer = new LayerMask();

        for (int i = 0; i < IndiceLayer.Length; i++)
        {
            Layer |= 1 << IndiceLayer[i];
        }

        return Layer;
    }

    /// <summary>
    /// Un raycast lanciato con questo LayerMask, colpirà solo i layer nell'array
    /// </summary>
    /// <param name="NomeLayer"></param>
    /// <returns></returns>
    public static LayerMask LayerSolo (string[] NomeLayer)
    {
        int[] layers = LayerStringToInt(NomeLayer);

        return LayerSolo(layers);
    }

    /// <summary>
    /// Vengono passati i layer tramite il loro nome, e ne viene restituito un array con il loro indice
    /// </summary>
    /// <param name="LayersName"></param>
    /// <returns></returns>
    private static int[] LayerStringToInt (string[] LayersName)
    {
        int[] LayersIndex = new int[LayersName.Length];

        for(int i = 0; i < LayersIndex.Length; i++)
        {
            int layer = LayerMask.NameToLayer(LayersName[i]);

            LayersIndex[i] = layer;
        }

        return LayersIndex;
    }
}
