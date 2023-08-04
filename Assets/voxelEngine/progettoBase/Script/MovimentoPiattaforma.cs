using UnityEngine;
using System.Collections;

public class MovimentoPiattaforma : MonoBehaviour
{
	public bool localDirection = true;
    public Vector3 direzione = new Vector3(0, 0, 1);
    public float velocità = 10;  //in metri al secondo

    public bool loop = false;

    public float attesaLoop = 1;

    public float distanzaLoop = 10; //in metri

    private float distanzaPercorsa = 0;

    private void Start()
    {
        direzione.Normalize();
    }

	void Update ()
    {
        //se non è impostata alcuna direzione (può essere perché è in attesa del loop), non deve muoversi
        if(direzione == Vector3.zero)
        {
            return;
        }


        if (localDirection)
        {
            //la traslazione avviene in base alla rotazione dell'oggetto:
            //Un vector3(0, 0, 1), farà muovere l'oggetto in avanti in base alla rotazione (l'equivalente di transform.forward), che potrebbe non corrispondere all'asse Z
            transform.Translate(direzione * velocità * Time.deltaTime);
        }
        else
        {
            //in questo caso avviene in base alla coordinate del mondo, e non alla rotazione dell'oggetto
            transform.position = Vector3.MoveTowards(transform.position, transform.position + direzione, velocità * Time.deltaTime);
        }

        //se deve fare un loop, viene controllata la distanza percorsa
        //quando raggiunge quella impostata (distanzaLoop), torna indietro
        if(loop)
        {
            distanzaPercorsa += velocità * Time.deltaTime;
            if(distanzaPercorsa  >= distanzaLoop)
            {
                distanzaPercorsa = 0;

                //attesa prima di partire in senso inverso
                StartCoroutine(attendereLoop(-direzione));
            }
        }
	}

    IEnumerator attendereLoop(Vector3 direzione_nuova)
    {
        //rimuove la direzione, finché non finisce l'attesa del loop
        direzione = Vector3.zero;

        yield return new WaitForSeconds(attesaLoop);

        direzione = direzione_nuova;
    }
}
