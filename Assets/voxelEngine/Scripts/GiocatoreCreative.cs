using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(AudioListener))]

public class GiocatoreCreative : MonoBehaviour
{
    Vector2 rot;
    public float speed = 0.5f;

    private Camera cam;
    public CaricaChunk caricaChunk = new CaricaChunk();
    public RompiPiazzaScript rompiPiazza_script = new RompiPiazzaScript();

    private Light Luce;
    public Transform altraCamera;

    //variabili per modificare il codice in game
    Vector2 scrollBar = Vector2.zero;

    Light OttieniCreaLuce()
    {
        Light l = GetComponentInChildren<Light>();
        if (l == null)
        {
            l = transform.GetComponentInChildren<Camera>().gameObject.AddComponent<Light>();
        }

        l.enabled = false;
        l.color = new Color32(255, 193, 132, 255);

        //per avere il calore sotto forma di codice Hex (per colore schermi) http://www.vendian.org/mncharity/dir3/blackbody/

        return l;
    }

    void Start()
    {
        cam = GetComponent<Camera>();

        cam.allowMSAA = false;  //per non far vedere le linee tra i blocchi

        caricaChunk.mondo = FindObjectOfType<Mondo>();

        Luce = OttieniCreaLuce();

        //per rompere tutti i blocchi
        rompiPiazza_script.nessunBloccoIndistruttibile = true;
        //per rompere i blocchi con un colpo solo
        rompiPiazza_script.danno = 100;
        //per non romperli troppo velocemente
        rompiPiazza_script.rateoDiFuoco = 0.2f;

        //piazza il giocatore sopra tutti i chunk
        RaycastHit hit;
        if (Physics.Raycast(transform.position + (Vector3.up * 100), Vector3.down, out hit))
        {
            transform.position = hit.point + Vector3.up * 50;
        }
    }

    void Update()
    {
        CaricaChunks();

        MovimentoCamera(Input.GetKey(KeyCode.Space), Input.GetKey(KeyCode.LeftShift), Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"),
            Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        PiazzaORompiBlocchi(Input.GetButton("Fire1"), Input.GetButton("Fire2"));

        AccendiSpegniLuce(Input.GetKeyDown(KeyCode.F));

        CambiaCamera(Input.GetKeyDown(KeyCode.G));
    }

    void OnGUI()
    {
        //Scritta in alto al centro
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;

        GUI.Box(new Rect(Screen.width / 2 - (Screen.width / 10 / 2), 0, Screen.width / 10, Screen.height / 12), "F - Light On/Off" + "\n"
            + "Left Click - Destroy Block" + "\n"
            + "Right Click - Place Block" + "\n"
            + "G - Change to Player");

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

    void CaricaChunks()
    {
        caricaChunk.UpdateFunction(transform);
    }

    void MovimentoCamera(bool InputAlzarsi, bool InputAbbassarsi, float inputHorizontal, float inputVertical, float mouseX, float mouseY)
    {
        if (InputAlzarsi)
            transform.position += Vector3.up * speed / 2;

        if (InputAbbassarsi)
            transform.position -= Vector3.up * speed / 2;

        rot = new Vector2(
            rot.x + mouseX * 3,
            rot.y + mouseY * 3);

        transform.localRotation = Quaternion.AngleAxis(rot.x, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rot.y, Vector3.left);

        transform.position += transform.forward * speed * inputVertical;
        transform.position += transform.right * speed * inputHorizontal;

    }

    void PiazzaORompiBlocchi(bool InputRompi, bool InputPiazza)
    {
        rompiPiazza_script.UpdateFunction(cam, InputRompi, InputPiazza);
    }

    void AccendiSpegniLuce(bool inputLuce)
    {
        if (inputLuce && Luce != null)
        {
            Luce.enabled = !Luce.enabled;
        }
    }

    void CambiaCamera(bool inputCambiaCamera)
    {
        if (inputCambiaCamera && altraCamera != null)
        {
            altraCamera.position = cam.transform.position;
            altraCamera.gameObject.SetActive(true);
            caricaChunk.AzzeraListe();
            altraCamera.GetComponent<GiocatoreVoxel>().BloccaMouse(CursorLockMode.Locked);
            altraCamera.GetComponent<GiocatoreVoxel>().caricaChunk = caricaChunk;
            gameObject.SetActive(false);
        }
    }


    void MostraVariabiliInPlay()
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