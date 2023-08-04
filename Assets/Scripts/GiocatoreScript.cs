using UnityEngine;
using UnityEngine.UI;

public class GiocatoreScript : GiocatoreBase
{
	public override void SistemaInEditMode()
	{
		base.SistemaInEditMode();

		//imposta il salto un po' più basso, adatto giusto a saltare un blocco da 1m
		movimentoGiocatore.altezzaSalto = 6;

		//imposta la luce per partire da accesa
		GetComponentInChildren<Light>().enabled = true;

		//Modifica Character Controller
        {
            CharacterController cc = GetComponent<CharacterController>();
            cc.center = new Vector3(0.2f, 0.7f, 0);
			cc.radius = 0.6f;
			cc.height = 1.5f;
        }

		//Modifica la Camera
		{
			Transform camera = transform.Find("Camera");

			//per non far vedere le linee tra i blocchi
			camera.GetComponent<Camera>().allowMSAA = false;

			//mettere la camera in terza persona
			camera.localScale = Vector3.one;
			camera.localPosition = new Vector3(1.5f, 1.3f, -2.5f);
			camera.localRotation = Quaternion.identity;
		}

		//Crea il punto da cui far partire l'effetto particellare del fucile
		{						
            Transform cannaFucile = transform.Find("CannaFucile");
			if(cannaFucile == null)
			{
				GameObject cf = new GameObject();
				cf.transform.parent = this.transform;
				cf.transform.localPosition = new Vector3(0.4f, 0.3f, 0.7f);
				cf.name = "CannaFucile";
			}
		}
	}

	
    public CaricaChunk caricaChunk = new CaricaChunk();
    public Spara rompiPiazza_script = new Spara();

	public float vita = 100;	
	public Slider barraVita;

	Animator anim;


    public override void Start()
    {
        base.Start();

        //se in editor e non si ha premuto il play -> sistemare le cose fuori posto
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }
        //=========================================================================

        //setto la variabile Mondo per CaricaChunk
        caricaChunk.mondo = FindObjectOfType<Mondo>();

		//setta la barra della vita
		barraVita = FindObjectOfType<Slider>();
		barraVita.value = 1;

        //si imposta l'altezza del salto
        movimentoGiocatore.saltoAutomatico.altezzaSaltoAutomatico = Blocco.grandezzaBlocco * 6;

		anim = GetComponentInChildren<Animator>();

		//setta le variabili della cameraTerzaPersona
		controlloCamera.AvviaInTerzaPersona(this.transform, cam.transform);

		//setta le variabili dello script per rompere e piazzare blocchi
		rompiPiazza_script.StartFunction(cam);

        //piazza il giocatore sopra tutti i chunk
        RaycastHit hit;
        if (Physics.Raycast(transform.position + (Vector3.up * 100), Vector3.down, out hit))
        {
            transform.position = hit.point + Vector3.up * 50;
        }
    }

    public override void Update()
    {
        base.Update();

        //se in editor e non si ha premuto il play -> non va considerato ciò che si trova al di sotto
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }
        //=========================================================================

        //se è disabilitato, deve limitarsi ad eseguire le funzioni sopra
        if (Attivo == false)
        {
            return;
        }
        
        //carica i chunk intorno al giocatore
        CaricaChunks();
    }

	public override void AzioniInput()
	{
		base.AzioniInput();
		
        //gli input con il tasto per rompere e quello per piazzare i blocchi. Viene usato anche per quando si colpisce qualcosa che non sia un chunk
        RompiPiazza_Function(Input.GetButton("Fire1"), Input.GetButton("Fire2"));

		//attiva e disattiva la visualizzazione degli fps
		if(Input.GetKeyDown(KeyCode.F2))
			FindObjectOfType<FpsDisplay>().mostrareFPS = !FindObjectOfType<FpsDisplay>().mostrareFPS;

		if(Input.GetButtonDown("Cancel"))
			SubisciDanni(vita);
	}

	public override void MovimentoGiocatore(float inputHorizontal, float inputVertical, bool inputSalto)
	{	
		base.MovimentoGiocatore(inputHorizontal, inputVertical, inputSalto);

		//setta le animazioni di movimento
		if(inputHorizontal != 0 || inputVertical != 0)
		{
			anim.SetBool("IsWalking", true);
		}
		else
		{
			anim.SetBool("IsWalking", false);			
		}
	}

	public void CaricaChunks()
    {
        caricaChunk.UpdateFunction(transform);
    }

    public void RompiPiazza_Function(bool InputRompi, bool InputPiazza)
    {
        rompiPiazza_script.UpdateFunction(cam, InputRompi, InputPiazza);
    }

	public void SubisciDanni(float danno)
	{
		vita -= danno;

		barraVita.value = vita /100;

		if(vita <= 0)
		{
			anim.SetTrigger("Die");
			Invoke("ConcluderePartita", 3);
		}
	}

	void ConcluderePartita()
	{	
		//sblocca il mouse
		BloccaMouse(CursorLockMode.None);
		//ricarica la scena del menù iniziale
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenùIniziale", UnityEngine.SceneManagement.LoadSceneMode.Single);
	}
}
