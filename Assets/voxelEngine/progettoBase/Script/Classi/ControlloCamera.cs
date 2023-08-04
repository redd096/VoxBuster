using UnityEngine;

[System.Serializable]
public class ControlloCamera
{
    //settaggio variabili, sensibilità e limiti della camera
    public float sensibilitàX = 15F;
    public float sensibilitàY = 15F;

    public float minimoX = -360F;
    public float massimoX = 360F;

    public float minimoY = -90F;
    public float massimoY = 90F;

    //rotazioneX e rotazioneY servono per settare la camera
    [HideInInspector]
    public float rotazioneX = 0F;

    [HideInInspector]
    public float rotazioneY = 0F;

    //le variabili necessarie per usare la camera in terza persona
    //per giocare in terza persona, bisogna impostare la camera come la si vuole, poi bisogna chiamare AvviaInTerzaPersona()
    //ad esempio, crea la scena col personaggio e la camera dietro di lui, e nello Start() chiamare AvviaInTerzaPersona()
    [System.Serializable]
    public class ControlloCameraTerzaPersona
    {
        //per sapere se si gioca in prima o in terza persona
        [HideInInspector]
        public bool usaTerzaPersona = false;

        //la velocità con cui la camera si sposta, quando c'è un oggetto tra lei e il giocatore
        public float speedLerp = 5;    

        //le variabili che vengono settate all'inizio, in base a come si è posizionata la camera nella Scene
        [HideInInspector]
        public float distanzaOriginale;
        [HideInInspector]
        public Vector3 posizioneLocaleCamera;

        //la variabile che viene usata per muovere la camera
        [HideInInspector]
        public float distanza;
    }
    public ControlloCameraTerzaPersona controlloTerzaPersona = new ControlloCameraTerzaPersona();

    public virtual void AvviaInTerzaPersona(Transform transform, Transform camTransform)
	{
        //setta le variabili necessarie per giocare in terza persona
        controlloTerzaPersona.usaTerzaPersona = true;

		controlloTerzaPersona.distanzaOriginale = -camTransform.localPosition.z;
		controlloTerzaPersona.distanza = controlloTerzaPersona.distanzaOriginale;
		controlloTerzaPersona.posizioneLocaleCamera = camTransform.localPosition;
	}

    public virtual void UpdateFunction(Transform transform, Transform camTransform, float mouseX, float mouseY)
    {
        //aggiungi lo spostamento alla camera
        rotazioneX += mouseX * sensibilitàX;
        rotazioneY += mouseY * sensibilitàY;

        //limita lo spostamento in base ai settaggi
        rotazioneX = ClampAngolo(rotazioneX, minimoX, massimoX);
        rotazioneY = ClampAngolo(rotazioneY, minimoY, massimoY);

        //rotazione effettiva
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, rotazioneX, 0);
        camTransform.localEulerAngles = new Vector3(-rotazioneY, camTransform.localEulerAngles.y, 0);

        //per far muovere la camera, nel caso si stia giocando in terza persona
        if(controlloTerzaPersona.usaTerzaPersona)
        {
            //controlla che non ci sia nulla tra la camera e il giocatore
            {
                LayerMask layerMask = ModificaLayer.LayerTuttoTranne(new string[] { "Giocatore"});
                RaycastHit hit;

                //se c'è, si avvicina, altrimenti torna alla posizione originale (lerp)
                if(Physics.Linecast(transform.position, camTransform.position, out hit, layerMask))
                {
                    controlloTerzaPersona.distanza = Mathf.Lerp(
                        controlloTerzaPersona.distanza, 
                        (hit.point - transform.position).magnitude, 
                        controlloTerzaPersona.speedLerp * Time.deltaTime);
                }
                else
                {
                    controlloTerzaPersona.distanza = Mathf.Lerp(
                        controlloTerzaPersona.distanza, 
                        controlloTerzaPersona.distanzaOriginale, 
                        controlloTerzaPersona.speedLerp * Time.deltaTime);
                }
            }

            //con questa si guarda sempre il centro del giocatore (tipo LookAt) e ci si muove intorno ad esso
            Quaternion rotation = Quaternion.Euler(-rotazioneY, rotazioneX, 0);
            camTransform.position = transform.position - rotation * (Vector3.forward * controlloTerzaPersona.distanza);
            
            //ora aggiungo la posizione iniziale, per avere il mirino dove dico io, invece che al centro del giocatore
            camTransform.localPosition = new Vector3(
                controlloTerzaPersona.posizioneLocaleCamera.x, 
                camTransform.localPosition.y + controlloTerzaPersona.posizioneLocaleCamera.y, 
                camTransform.localPosition.z);
        }
    }

    public virtual void ModificaRotazione(float rotX, float rotY)
    {
        //I valori vengono passati da 0 a 360, ma potrebbero essere necessari valori negativi -> al posto di 270, può servire -90
        rotX = NegativeAngolo(rotX, minimoX, massimoX);
        rotY = NegativeAngolo(rotY, minimoY, massimoY);

        //questo esegue un cambio immediato -> da aggiungere anche la possibilità di ruotarsi più smooth, ad esempio quando ci si ruota alle proprie spalle
        rotazioneX = rotX;
        rotazioneY = -rotY;
    }

    public static float ClampAngolo(float angolo, float min, float max)
    {
        // non può andare sotto i -360
        if (angolo < -360F)
            angolo += 360F;

        //non può andare sopra i 360
        if (angolo > 360F)
            angolo -= 360F;

        return Mathf.Clamp(angolo, min, max);
    }

    public static float NegativeAngolo(float angolo, float min, float max)
    {
        //se è maggiore di max, allora sottrae 360 per avere il valore in negativo
        if (angolo > max)
            angolo -= 360;

        return Mathf.Clamp(angolo, min, max); ;
    }
}
