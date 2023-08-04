using UnityEngine;

[RequireComponent(typeof(CharacterController))]

[ExecuteInEditMode]
public class GiocatoreBase : MonoBehaviour
{
    //Le modifiche che vanno apportate dall'editor
    public virtual void SistemaInEditMode()
    {
        //Modifica Giocatore Base
        {
            //rinomina il giocatore
            gameObject.name = "Giocatore";

            /*
            //se il layer non viene trovato, restituisce -1
            {
                if (LayerMask.NameToLayer("Giocatore") > -1)
                {
                    gameObject.layer = LayerMask.NameToLayer("Giocatore");
                }
                else
                {
                    Debug.LogError("Manca il layer 'Giocatore'");
                }
            }
            */
        }

        //Modifica Character Controller
        {
            CharacterController cc = GetComponent<CharacterController>();
            cc.skinWidth = 0.03f;
        }

        //Crea Camera
        {
            Transform t = transform.Find("Camera");
            if (t == null)
            {
                GameObject go = new GameObject();
                go.name = "Camera";
                t = go.transform;
                t.parent = transform;
            }

            t.localScale = Vector3.one;
            t.localPosition = new Vector3(0, 0.7f, 0);
            t.localRotation = Quaternion.identity;

            //Crea il componente Camera
            {
                Camera c = t.GetComponent<Camera>();
                if (c == null)
                {
                    c = t.gameObject.AddComponent<Camera>();
                }

                c.nearClipPlane = 0.01f;
            }

            //Crea Audio Listener
            {
                AudioListener al = t.GetComponent<AudioListener>();
                if(al == null)
                {
                    al = t.gameObject.AddComponent<AudioListener>();
                }
            }
        }

        //Crea gameObject Grafica
        {
            Transform t = transform.Find("Grafica");
            if (t == null)
            {
                MeshFilter mf = GetComponentInChildren<MeshFilter>();
                GameObject go;
                
                //se ci sono mesh, crea l'oggetto vuoto "Grafica" sotto cui metterle - altrimenti, crea la capsula base come mesh
                if(mf != null)
                {
                    go = new GameObject();
                }
                else
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    DestroyImmediate(go.GetComponent<CapsuleCollider>());
                }

                go.name = "Grafica";
                t = go.transform;
                t.parent = transform;
            }

            t.localScale = Vector3.one;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            //Se c'è qualche Mesh per il giocatore, va sotto Grafica
            //Non viene fatto via script, per evitare problemi
        }

        
        //Crea Luce
        {
            Light l = GetComponentInChildren<Light>();
            if (l == null)
            {
                l = transform.Find("Camera").gameObject.AddComponent<Light>();
            }

            l.enabled = false;
            l.color = new Color32(255, 193, 132, 255);

            //per avere il calore sotto forma di codice Hex (per colore schermi) http://www.vendian.org/mncharity/dir3/blackbody/
        }
    }

    
    public bool Attivo = true;  //non disattivare lo script, così continuerà a funzionare la Sistemazione nell'Editor

    public ControlloCamera controlloCamera = new ControlloCamera();
    public MovimentoGiocatore movimentoGiocatore = new MovimentoGiocatore();
    public MirinoIntelligente mirinoIntelligente = new MirinoIntelligente();

    [HideInInspector]
    public CharacterController controller;

    [HideInInspector]
    public Camera cam;
    
    [HideInInspector]
    public Light Luce;


    public virtual void Start()
    {
        //se in editor e non si ha premuto il play -> non va considerato ciò che si trova al di sotto
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }

        controller = GetComponent<CharacterController>();
        cam = transform.Find("Camera").GetComponent<Camera>();
        Luce = GetComponentInChildren<Light>();

        //blocca il mouse al centro dello schermo e rendilo invisibile
        BloccaMouse(CursorLockMode.Locked);
        //crea la texture del mirino
        mirinoIntelligente.CreaMirino();
    }

    public virtual void Update()
    {        
        //se in editor e non si ha premuto il play -> sistemare le cose fuori posto
        if (Application.isEditor && !Application.isPlaying)
        {
            SistemaInEditMode();
            return;
        }
        //=========================================================================

        //se è disabilitato, deve limitarsi ad eseguire le funzioni sopra
        if (Attivo == false)
        {
            return;
        }

        AzioniInput();
    }

    public virtual void AzioniInput()
    {
        MovimentoCamera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        MovimentoGiocatore(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetButton("Jump"));

        AllargaRestringi_Mirino(Input.GetButton("Fire1"));

        AccendiSpegniLuce(Input.GetKeyDown(KeyCode.F));
    }

    public virtual void OnGUI()
    {
        //se in editor e non si ha premuto il play -> non va considerato ciò che si trova nello Start()
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }

        mirinoIntelligente.DisegnaMirino();
    }

    // funzioni da richiamare

    public virtual void MovimentoCamera(float mouseX, float mouseY)
    {
        controlloCamera.UpdateFunction(transform, cam.transform, mouseX, mouseY);
    }

    public virtual void MovimentoGiocatore(float inputHorizontal, float inputVertical, bool inputSalto)
    {
        movimentoGiocatore.UpdateFunction(controller, transform, inputHorizontal, inputVertical, inputSalto);
    }

    public virtual void AllargaRestringi_Mirino(bool inputAllargaMirino)
    {
        mirinoIntelligente.AllargaRestringi_Mirino(inputAllargaMirino);
    }

    public virtual void AccendiSpegniLuce(bool inputLuce)
    {
        //accende e spegne la luce ¯\_(ツ)_/¯
        if (inputLuce)
        {
            Luce.enabled = !Luce.enabled;
        }
    }

    public virtual void BloccaMouse(CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;

        // nascondi il cursore se è bloccato
        Cursor.visible = (CursorLockMode.Locked != lockMode);
    }

    public virtual void AttivaDisattiva(bool attivato)
    {
        //funzione per disabilitare lo script, ma continuare ad eseguire determinate funzioni
        Attivo = attivato;

        //in caso di disattivazione, qui è dove decidere cosa modificare
        if(attivato == false)
        {
            AccendiSpegniLuce (false);
        }
    }
}
