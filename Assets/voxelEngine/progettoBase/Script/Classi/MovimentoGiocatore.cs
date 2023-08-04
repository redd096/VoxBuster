using UnityEngine;

[System.Serializable]
public class MovimentoGiocatore
{
    public float velocità = 6;
    public float altezzaSalto = 8;
    public float gravità = 20;
    public float maxVelocitàCaduta = 100;

    [System.Serializable]
    public class AutomaticJump
    {
        public bool isSaltoAutomatico = true;
        public float altezzaSaltoAutomatico = 6;

        //per sistemare il salto automatico
        [HideInInspector]
        public Vector3 offset = Vector3.zero;

        [HideInInspector]
        public bool staSaltando = false;
    }
    public AutomaticJump saltoAutomatico = new AutomaticJump();

    private Vector3 moveDirection = Vector3.zero;

    //per muoversi su una piattaforma/terreno in movimento
    private Transform terreno;
    private Vector3 ultimaPosizioneTerreno;

    public virtual void UpdateFunction(CharacterController controller, Transform transform, float inputHorizontal, float inputVertical, bool inputSalto)
    {
        //se controller.isGrounded è vero, allora isGrounded, altrimenti controlla se lo sphere cast di SuTerreno() ha toccato qualcosa
        RaycastHit hitGround = SuTerreno(controller, transform);
        bool isGrounded = Giocatore_suTerreno(controller, transform, hitGround);
        //===============================================

        //modifiche agli assi X e Z
        {
            moveDirection.x = inputHorizontal * velocità;
            moveDirection.z = inputVertical * velocità;
        }

        //modifiche all'asse Y
        {
            // solo se realmente a terra
            Function_aTerra(controller);

            // può non star toccando terra di qualche centimetro - utile su pendenze, mentre ci si sta muovendo in discesa
            Function_Grounded(isGrounded, inputSalto);

            // se c'è qualcosa davanti, ed è abilitato il salto automatico, allora viene saltato
            SaltoAutomatico(transform, isGrounded, inputVertical);
        }

        // per avere lo spostamento nella giusta direzione, basandosi sulla rotazione
        moveDirection = transform.TransformDirection(moveDirection);

        // per spostarsi se il terreno è in movimento
        TerrenoInMovimento(hitGround);

        // per aggiungere la gravità
        AggiuntaGravità();

        // l'effettivo movimento
        controller.Move(moveDirection * Time.deltaTime);
    }

    public virtual void Function_aTerra(CharacterController controller)
    {
        if (controller.isGrounded == false)
        {
            return;
        }

        moveDirection.y = 0;
        saltoAutomatico.staSaltando = false;
    }

    public virtual void Function_Grounded(bool isGrounded, bool inputSalto)
    {
        if (isGrounded == false)
        {
            return;
        }

        if (inputSalto)
        {
            moveDirection.y = altezzaSalto;
            saltoAutomatico.staSaltando = false;
        }
    }

    public void AggiuntaGravità()
    {
        //si aggiunge la gravità, e si imposta un limite massimo di velocità in caduta (Mathf.Max perché si usano valori al negativo)
        moveDirection.y -= gravità * Time.deltaTime;
        moveDirection.y = Mathf.Max(moveDirection.y, -maxVelocitàCaduta);

    }

    public void TerrenoInMovimento(RaycastHit hit)
    {
        bool seguirePiattaforma = false;

        //se si ha colpito qualcosa -> se si è su un nuovo terreno, allora CambiaTerreno(), altrimenti limitarsi a seguire la piattaforma
        if (hit.transform != null)
        {
            if (terreno == null || hit.transform != terreno)
                CambiaTerreno(hit.transform);
            else
                seguirePiattaforma = true;
        }
        else
            terreno = null;

        //se seguirePiattaforma e la posizione è diversa da quella precedente, allora muoversi con la piattaforma
        if (seguirePiattaforma && ultimaPosizioneTerreno != terreno.position)
        {
            Vector3 movimentoTerreno = terreno.position - ultimaPosizioneTerreno;

            //se la piattaforma sta salendo, lasciare lo spostamento verticale al CharacterController, o risulterà in un simil bug e il giocatore finirà per balzare in aria
            if (movimentoTerreno.y > 0)
                movimentoTerreno.y = 0;

            ultimaPosizioneTerreno = terreno.position;

            // controller.Move è moltiplicato * Time.deltaTime, quindi qua tocca dividere, avendo già il movimento da fare in questo frame
            moveDirection += movimentoTerreno / Time.deltaTime;
        }
    }

    private void CambiaTerreno(Transform nuovoTerreno)
    {
        terreno = nuovoTerreno;
        ultimaPosizioneTerreno = terreno.position;
    }

    public bool Giocatore_suTerreno(CharacterController controller, Transform transform, RaycastHit hitGround)
    {
        //per hitGround usare lo sphere cast di SuTerreno()
        //se controller.isGrounded è vero, allora isGrounded (perché è realmente a terra), 
        //altrimenti controlla se hitGround ha toccato qualcosa o se si sta eseguendo un salto automatico (quindi potrebbe essere rialzato di qualche centimetro)
        bool isGrounded = controller.isGrounded ? true : (hitGround.transform != null || saltoAutomatico.staSaltando);

        return isGrounded;
    }

    public RaycastHit SuTerreno(CharacterController controller, Transform transform)
    {
        //si crea un LayerMask, così da non colpire né il giocatore né ciò che ha layer "Ignore Raycast"
        LayerMask layerMask = ModificaLayer.LayerTuttoTranne(new string[] { "Giocatore", "Ignore Raycast" });

        RaycastHit hit;
        float distanza = (controller.height / 2) + 0.1f - (controller.radius / 2);

        // controlla sotto il giocatore di 0.1f, con il raggio del CharacterController, quindi toccherà sotto il giocatore di 0.1f
        Physics.SphereCast(transform.position + controller.center, controller.radius, Vector3.down, out hit, distanza, layerMask);

        return hit;
    }

    public virtual void SaltoAutomatico(Transform transform, bool isGrounded, float inputVertical, CharacterController controller = null)
    {
        //controller aggiunto per semplificare l'override, nel caso si volesse ad esempio usare uno sphereCast con controller.radius

        //se c'è il salto automatico e il giocatore si trova a terra e si sta muovendo in avanti, se c'è un qualcosa davanti, lo salta
        if (saltoAutomatico.isSaltoAutomatico && isGrounded && inputVertical > 0)
        {
            //si crea un LayerMask, così da non colpire né gli altri nemici né ciò che ha layer "Ignore Raycast", né se stesso
            LayerMask layerMask = ModificaLayer.LayerTuttoTranne(new string[] { "Ignore Raycast", "Giocatore", "Nemico" });

            Ray raggio = new Ray(transform.position + saltoAutomatico.offset, transform.forward);

            //controlla che non ci sia nulla davanti a bloccarlo
            //se si passa il character controller, usa lo sphereCast, altrimenti si limita al Raycast
            if (Physics.Raycast(raggio, 2, layerMask))
            {
                //se c'è qualcosa a bloccarlo, controlla che non ci sia nulla sopra
                if (Physics.Raycast(transform.position + Vector3.up, transform.forward, 1, layerMask) == false)
                {
                    //se è fattibile, salta
                    moveDirection.y = saltoAutomatico.altezzaSaltoAutomatico;

                    saltoAutomatico.staSaltando = true;
                }
            }
        }
    }
}