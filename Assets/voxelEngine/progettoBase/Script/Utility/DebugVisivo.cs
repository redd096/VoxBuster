using UnityEngine;

public class DebugVisivo : MonoBehaviour
{
    //per decidere cosa far fare
    public enum Opzioni { niente, linea, sfera };
    public Opzioni CoseDaFare = Opzioni.niente;

    private Color color;
    private Vector3 positionStart;
    private Vector3 positionEnd;
    private float radius;

    /* da inserire in uno script per il debug di uno SphereCast
     
    //Debug
    {
        DebugVisivo debug = transform.GetComponent<DebugVisivo>();
        if (debug == null)
            debug = transform.gameObject.AddComponent<DebugVisivo>();

        debug.SphereCast(transform.position, controller.radius, Vector3.down, hit, distanza);
    }

    */

    private DebugVisivo[] OttieniListaDebug(int lunghezzaLista)
    {
        //ottieni la lista dei DebugVisivo già presenti nel giocatore e rimuovi questo script dalla lista
        DebugVisivo[] DebugsArray = transform.GetComponents<DebugVisivo>();
        DebugVisivo[] debug = new DebugVisivo[lunghezzaLista];

        int index = 0;

        for (int i = 0; i < DebugsArray.Length; i++)
        {
            if (DebugsArray[i] == this)
            {
                continue;
            }

            debug[index] = DebugsArray[i];
            index++;
        }

        //se i DebugVisivo non sono sufficienti (o non ci sono proprio), crearne altri
        for (int i = index; i < lunghezzaLista; i++)
        {
            debug[i] = transform.gameObject.AddComponent<DebugVisivo>();
        }

        return debug;
    }

    void OnDrawGizmos()
    {
        switch(CoseDaFare)
        {
            case Opzioni.linea:
                DrawLine();
                break;
            case Opzioni.sfera:
                DrawSphere();
                break;
        }
    }

    // Funzioni utilizzate dallo script per disegnare

    private void DrawLine()
    {
        Gizmos.color = color;
        Gizmos.DrawLine(positionStart, positionEnd);
    }

    private void DrawSphere()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(positionStart, radius);
    }

    // Funzioni da richiamare

    public void Linea(Color colore, Vector3 partenza, Vector3 fine)
    {
        CoseDaFare = Opzioni.linea;

        color = colore;
        positionStart = partenza;
        positionEnd = fine;
    }

    public void Sfera (Color colore, Vector3 posizione, float raggio)
    {
        CoseDaFare = Opzioni.sfera;

        color = colore;
        positionStart = posizione;
        radius = raggio;
    }

    public void SphereCast (Vector3 partenza, float raggio, Vector3 direzione, RaycastHit hit, float distanza)
    {
        //ottieni 3 DebugVisivo
        DebugVisivo[] debug = OttieniListaDebug(3);

        //disegna sfera di partenza
        debug[0].Sfera(Color.white, partenza, raggio);

        //mostrare il punto colpito o quello massimo a cui si può arrivare
        Vector3 colpito = hit.point;
        if (hit.transform == null)
            colpito = partenza + (direzione * distanza);

        //disegna ray da un centro sfera ad un altro
        debug[1].Linea(Color.cyan, partenza, colpito);

        //se ha colpito, indietreggiare con la sfera finale (in quanto si colpisce col bordo e non col centro della sfera)
        if (hit.transform != null)
            colpito -= (direzione * raggio);

        //disegna sfera finale
        debug[2].Sfera(Color.red, colpito, raggio);
    }
}
