using UnityEngine;

public class GiocatoreVoxel : GiocatoreBase
{
    public CaricaChunk caricaChunk = new CaricaChunk();
    public RompiPiazzaScript rompiPiazza_script = new RompiPiazzaScript();
    public Transform altraCamera;

    //variabili per modificare il codice in game
    Vector2 scrollBar = Vector2.zero;

    public override void SistemaInEditMode()
    {
        base.SistemaInEditMode();

        //per non far vedere le linee tra i blocchi
        transform.Find("Camera").GetComponent<Camera>().allowMSAA = false;
    }

    public override void Start()
    {
        base.Start();

        //se in editor e non si ha premuto il play -> non va considerato ciò che si trova al di sotto
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }
        //=========================================================================

        //setto la variabile Mondo per CaricaChunk
        caricaChunk.mondo = FindObjectOfType<Mondo>();

        //setto il salto automatico
        movimentoGiocatore.saltoAutomatico.isSaltoAutomatico = true;
        //setta l'offset per il salto automatico
        movimentoGiocatore.saltoAutomatico.offset = new Vector3(0, - controller.height /2, 0);

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

        //passa dal giocatore alla creative
        CambiaCamera(Input.GetKeyDown(KeyCode.G));
    }

    public override void OnGUI()
    {
        base.OnGUI();

        //se in editor e non si ha premuto il play -> non va considerato ciò che si trova al di sotto
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }
        //=========================================================================

        //Scritta in alto al centro
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        
        GUI.Box(new Rect(Screen.width / 2 - (Screen.width / 10 /2), 0, Screen.width / 10, Screen.height / 12), "F - Light On/Off" + "\n"
            + "Left Click - Destroy Block" + "\n"
            + "Right Click - Place Block" + "\n"
            + "G - Change to Creative");

        //mostra a destra le opzioni di modifica
        GUILayout.BeginArea(new Rect(Screen.width - (Screen.width / 5), Screen.height / 10, Screen.width / 5, Screen.height - Screen.height / 10));
        GUILayout.BeginVertical();
        GUILayout.Space(25);
        scrollBar = GUILayout.BeginScrollView(scrollBar);

        MostraVariabiliInPlay();

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    public virtual void CaricaChunks()
    {
        caricaChunk.UpdateFunction(transform);
    }

    public virtual void RompiPiazza_Function(bool InputRompi, bool InputPiazza)
    {
        rompiPiazza_script.UpdateFunction(cam, InputRompi, InputPiazza);
    }

    public virtual void CambiaCamera(bool inputCambiaCamera)
    {
        if (inputCambiaCamera && altraCamera != null)
        {
            altraCamera.position = cam.transform.position;
            altraCamera.gameObject.SetActive(true);
            caricaChunk.AzzeraListe();
            BloccaMouse(CursorLockMode.None);
            gameObject.SetActive(false);
        }
    }
	

    public virtual void MostraVariabiliInPlay()
    {
        //Limita Frame Rate
        {
            GUILayout.Label("Limita Frame Rate (-1 = nessun limite)");
            GUILayout.BeginHorizontal();
            GUILayout.Label("" + Application.targetFrameRate);
            Application.targetFrameRate = Mathf.FloorToInt(GUILayout.HorizontalSlider(Application.targetFrameRate, -1, 100));
            GUILayout.EndHorizontal();
        }
        //DistanzaChunk
        {
            GUILayout.Label("Distanza a cui caricare i Chunk");
            GUILayout.BeginHorizontal();
            GUILayout.Label("" + caricaChunk.distanzaChunk);
            caricaChunk.distanzaChunk = Mathf.FloorToInt(GUILayout.HorizontalSlider(caricaChunk.distanzaChunk, 10, 100));
            GUILayout.EndHorizontal();

            if (caricaChunk.distanzaRimuovereChunk < caricaChunk.distanzaChunk)
                caricaChunk.distanzaRimuovereChunk = caricaChunk.distanzaChunk + 10;
        }
        //DistanzaY
        {
            GUILayout.Label("Distanza sull'asse y (verticale) a cui caricare i Chunk");
            GUILayout.BeginHorizontal();
            GUILayout.Label("" + caricaChunk.distanzaY);
            caricaChunk.distanzaY = Mathf.FloorToInt(GUILayout.HorizontalSlider(caricaChunk.distanzaY, 4, 10));
            GUILayout.EndHorizontal();
        }
        //DistanzaRimuovereChunk
        {
            GUILayout.Label("Distanza a cui rimuovere i Chunk");
            GUILayout.BeginHorizontal();
            GUILayout.Label("" + caricaChunk.distanzaRimuovereChunk);
            caricaChunk.distanzaRimuovereChunk = Mathf.FloorToInt(GUILayout.HorizontalSlider(caricaChunk.distanzaRimuovereChunk, caricaChunk.distanzaChunk + 10, 110));
            GUILayout.EndHorizontal();
        }
        //ChunkDaCaricare
        {
            GUILayout.Label("Numero di Chunk da caricare ad ogni frame");
            GUILayout.BeginHorizontal();
            GUILayout.Label("" + caricaChunk.chunkDaCaricare);
            caricaChunk.chunkDaCaricare = Mathf.FloorToInt(GUILayout.HorizontalSlider(caricaChunk.chunkDaCaricare, 1, 20));
            GUILayout.EndHorizontal();
        }
        //ChunkDaAggiornare
        {
            GUILayout.Label("Numero di Chunk da aggiornare ad ogni frame (finiti quelli da caricare)");
            GUILayout.BeginHorizontal();
            GUILayout.Label("" + caricaChunk.chunkDaAggiornare);
            caricaChunk.chunkDaAggiornare = Mathf.FloorToInt(GUILayout.HorizontalSlider(caricaChunk.chunkDaAggiornare, 1, 20));
            GUILayout.EndHorizontal();
        }
    }
}

