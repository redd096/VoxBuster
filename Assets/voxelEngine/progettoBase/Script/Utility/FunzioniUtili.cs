using System.Collections.Generic;
using UnityEngine;

public static class FunzioniUtili
{
    /// <summary>
    /// Ottieni una lista di script, escludendo "thisScript".
    /// Per avere, ad esempio, la lista script di un gameObject, rimuovendo lo script stesso da cui si richiama la funzione.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="scriptArray"></param>
    /// <param name="thisScript"></param>
    /// <returns></returns>
    public static T[] OttieniScripts<T>(object[] scriptArray, object thisScript = null)
    {
        T[] scripts = new T[scriptArray.Length];

        if(thisScript != null)
        {
            //la lunghezza sarà -1, perché viene tolto thisScript
            scripts = new T[scriptArray.Length -1];
        }

        //necessario per non prendere in considerazione thisScript
        int index = 0;  

        for (int i = 0; i < scriptArray.Length; i++)
        {
            //salta lo script in questione
            if (thisScript != null && scriptArray[i] == thisScript)
            {
                continue;
            }

            scripts[index] = (T)scriptArray[i];
            index++;
        }

        return scripts;
    }

    /// <summary>
    /// Crea GUIStyle con colore solido.
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static GUIStyle CreaGUIStyle(Color32 color)
    {
        //crea una texture di colore solido
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        //utilizza la texture come background del GUIStyle
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.normal.background = texture;

        return guiStyle;
    }

    /// <summary>
    /// Si ottiene l'ultimo elemento della lista passata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ListaValori"></param>
    /// <returns></returns>
    public static T OttieniUltimoDellaLista<T>(List<T> ListaValori)
    {
        if(ListaValori.Count == 0)
        {
            Debug.LogError("La lista è vuota");
        }

        T ultimoValore = ListaValori[ListaValori.Count - 1];

        return ultimoValore;
    }
}
