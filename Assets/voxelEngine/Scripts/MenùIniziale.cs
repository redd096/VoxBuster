using UnityEngine;
using UnityEngine.SceneManagement;

//serializable per poterlo salvare
[System.Serializable]
public class ValoriMondo
{
    //da passare a Mondo.cs
    
    //il nome che viene dato al mondo
    public string nomeMondo;
    //il seed che viene dato al mondo
    public string seed;
    //la grandezza di un blocco (0.25m, 0.5m, 1m)
    public int grandezzaBlocco;
    //il numero di blocchi presente in un chunk (4, 8, 16, 32)
    public int grandezzaChunk;
    //moltiplicatore del numero di blocchi che deve avere un chunk sull'asse y
    public int moltiplicatoreY;
}

[ExecuteInEditMode]
public class MenùIniziale : MonoBehaviour
{
    //Le modifiche che vanno apportate dall'editor
    void SistemaInEditMode()
    {
        //ricordo da subito di aggiungere il layer per il giocatore
        if (LayerMask.LayerToName(8) == "Giocatore")
        {
            gameObject.layer = LayerMask.NameToLayer("Giocatore");
        }
        else
        {
            Debug.LogError("Manca il layer 'Giocatore'");
        }

        //ricordo di aggiungere le scene alla lista del build settings
        if(SceneManager.sceneCountInBuildSettings < 2)
        {
            Debug.LogError("Aggiungere le scene nella lista del build settings");
        }
    }

    //quando si crea il mondo e ne esiste uno con lo stesso nome: se è false, crea comunque il mondo, mettendo un numero alla fine del nome, se è true, da errore e non lo crea.
    public bool erroreCreaMondoEsistente = false;

    public ValoriMondo valoriMondo = new ValoriMondo();

    //sapere se la scena è attiva e deve visualizzare il menù o se si ha già cambiato scena e si aspetta solo di passare i valori e cancellare lo script
    bool scenaAttiva;
    //per sapere se si sta creando un mondo o se si deve mostrare la lista dei mondi già esistenti
    bool creazioneMondo;

    //per la visualizzazione dei mondi da caricare
    string[] ListaMondi;
    Vector2 scrollListaMondi;
    int mondoSelezionato;

    //per mostrare al giocatore, in caso di errore
    string errore;

    void Start()
    {      
        //non eseguire lo Start se non si è avviato il gioco
        if(Application.isEditor && !Application.isPlaying)
        {
            return;
        }

        //non distruggere al cambio di scena. È da distruggere una volta passati i valori a Mondo.cs
        DontDestroyOnLoad(this);

        //Setto la Camera per mostrare un colore solido di background invece che lo skybox
        GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

        //semplice reset delle variabili
        {
            erroreCreaMondoEsistente = false;
            scenaAttiva = true;
            creazioneMondo = false;
            scrollListaMondi = Vector2.zero;
            mondoSelezionato = -1;
            errore = "";
            //reset valoriMondo
            {
                valoriMondo.nomeMondo = "Voxel World";
                valoriMondo.seed = "";
                valoriMondo.grandezzaBlocco = 2;
                valoriMondo.grandezzaChunk = 2;
                valoriMondo.moltiplicatoreY = 0;
            }
        }

        //ottiene i nomi delle cartelle all'interno della directory di salvataggio, ovvero i nomi dei mondi creati fin ora
        ListaMondi = System.IO.Directory.GetDirectories(SaveAndLoad.CartellaDeiSalvataggi(""));
        //SaveAndLoad.CartellaDeiSalvataggi(""), non passando il nome del mondo (""), allora si ottiene solo il percorso in cui vengono salvati tutti i mondi
        //percorso_del_progetto/Assets/nomeCartella
        //con GetDirectories si ottengono i percorsi delle cartelle, per cui:
        //percorso_del_progetto\Assets\nomeCartella\nomeMondo1
        //percorso_del_progetto\Assets\nomeCartella\nomeMondo2, ecc...
        //va rimossa la cartella di salvataggio per ottenere solo il nome del mondo, ma si ottiene \nomeMondo1, \nomeMondo2, ecc...
        //bisogna quindi rimpiazzare "\", ma un singolo backslash all'interno delle stringhe, viene considerato come una chiusura delle virgolette
        //bisogna quindi scrivere "\\", per riferirsi ad un solo backslash
        //Quindi da percorso_del_progetto\Assets\nomeCartella\nomeMondo, ri rimpiazza 
        //percorso_del_progetto\Assets\nomeCartella + "\" con un carattere vuoto, 
        //per ottenere semplicemente "nomeMondo"
        for (int i = 0; i < ListaMondi.Length; i++)
            ListaMondi[i] = ListaMondi[i].Replace(SaveAndLoad.CartellaDeiSalvataggi("") + "\\", "");
    }

    void Update()
    {
        //se in editor e non si ha premuto il play -> sistemare le cose fuori posto
        if (Application.isEditor && !Application.isPlaying)
        {
            SistemaInEditMode();
            return;
        }
    }

    void OnGUI()
    {
        //quando si cambia scena, non deve più visualizzare niente - deve solo aspettare che Mondo.cs prenda le variabili necessarie e cancelli questo gameObject
        if (scenaAttiva == false)
            return;

        //crea nuovo mondo o visualizza quelli da caricare
        if (creazioneMondo)
        {
            CreazioneMondo();
        }
        else
        {
            VisualizzaMondi();
        }
    }

    void CreazioneMondo()
    {
        //se si preme Invio, ed è il tasto invio
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            //se si è posizionati sul seed, fa partire la creazione del mondo
            //if (GUI.GetNameOfFocusedControl() == "seed")
            //    CreaMondo();

            //se si è in qualche textfield, disattiva il focus
            if (GUI.GetNameOfFocusedControl() != "")
                GUI.FocusControl("");            
        }

        //per mettere tutto in centro schermo
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
                
        //per avere una grandezza minima per i textfield, e quindi, anche per i bottoni
        float size = GUI.skin.textField.CalcSize(new GUIContent("Write World Name")).x;
        //per impostare un numero massimo di caratteri da usare nei textField
        int maxLengthString = 50;

        //visualizza in caso di errore
        if (errore.Length > 0)
            GUILayout.Box("<color=red>" + errore + "</color>");

        //modifica nome e vari valori del mondo
        GUILayout.Label("World Name:");
        GUI.SetNextControlName("nomeMondo");
        valoriMondo.nomeMondo = GUILayout.TextField(valoriMondo.nomeMondo, maxLengthString, GUILayout.MinWidth(size));

        GUILayout.Label("Seed:");
        GUI.SetNextControlName("seed");
        valoriMondo.seed = GUILayout.TextField(valoriMondo.seed, maxLengthString, GUILayout.MinWidth(size));

        GUILayout.Label("Size Block:");
        string[] s = new string[] { "0.25", "0.5", "1" };
        valoriMondo.grandezzaBlocco = GUILayout.Toolbar(valoriMondo.grandezzaBlocco, s, GUILayout.MinWidth(size));

        GUILayout.Label("Size Chunk:");
        s = new string[] { "4", "8", "16", "32" };
        valoriMondo.grandezzaChunk = GUILayout.Toolbar(valoriMondo.grandezzaChunk, s, GUILayout.MinWidth(size));

        GUILayout.Label("Multiplier Y Chunk:");
        s = new string[] { "1", "2", "4", "8" };
        valoriMondo.moltiplicatoreY = GUILayout.Toolbar(valoriMondo.moltiplicatoreY, s, GUILayout.MinWidth(size));

        GUILayout.Space(20);  //uno spazio tra i dati del mondo e i tasti
        GUILayout.BeginHorizontal();

        //annulla la creazione del mondo e torna a visualizzare i mondi caricabili
        if (GUILayout.Button("Cancel"))
        {
            creazioneMondo = false;
            errore = "";
        }

        //se è stato inserito il nome del mondo, allora il mondo è creabile,
        //altrimenti il tasto Crea Mondo, viene mostrato oscurato e non fa partire la creazione, ma mostra un errore se lo si preme
        bool creabile = valoriMondo.nomeMondo.Length > 0;
        if (!creabile)
        {
            GUI.backgroundColor = Color.black;
            GUI.skin.button.normal.textColor = Color.grey;
        }

        if (GUILayout.Button("Create World"))
        {
            if (creabile)
                CreaMondo();
            else
                errore = "Write World Name";
        }

        //reset delle skin
        GUI.backgroundColor = Color.white;
        GUI.skin.button.normal.textColor = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void VisualizzaMondi()
    {
        //per averli al centro, che lascino un quarto di schermo come bordo da ogni lato
        GUILayout.BeginArea(new Rect(Screen.width /4, Screen.height /4, Screen.width /2, Screen.height /2));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        //visualizza in caso di errore
        if (errore.Length > 0)
            GUILayout.Box("<color=red>" + errore + "</color>");

        //crea uno scrollview per la visualizzazione dei mondi
        scrollListaMondi = GUILayout.BeginScrollView(scrollListaMondi, GUILayout.Width(Screen.width /2));
        
        for(int i = 0; i < ListaMondi.Length; i++)
        {
            //se è stato selezionato questo mondo, allora il bottone avrà la scritta in giallo (sia normalmente che passandoci sopra col mouse)
            if (i == mondoSelezionato)
            {
                GUI.skin.button.normal.textColor = Color.yellow;
                GUI.skin.button.hover.textColor = Color.yellow;
            }

            //mostra i nomi di tutti i mondi e la possibilità di cliccare per selezionarlo
            if (GUILayout.Button(ListaMondi[i]))
            {
                mondoSelezionato = i;
            }

            //reset delle skin
            GUI.skin.button.normal.textColor = Color.white;
            GUI.skin.button.hover.textColor = Color.white;
        }

        GUILayout.EndScrollView();
        GUILayout.Space(20);  //uno spazio tra la lista dei mondi e i tasti
        GUILayout.BeginHorizontal();

        //se è stato selezionato un mondo, allora il mondo è caricabile,
        //altrimenti il tasto Carica Mondo, viene mostrato oscurato e non fa partire il caricamento, ma mostra un errore se lo si preme
        bool caricabile = mondoSelezionato > -1 && mondoSelezionato < ListaMondi.Length;
        if (!caricabile)
        {
            GUI.backgroundColor = Color.black;
            GUI.skin.button.normal.textColor = Color.grey;
        }

        if (GUILayout.Button("Load World"))
        {
            if (caricabile)
                CaricaMondo(ListaMondi[mondoSelezionato]);
            else
                errore = "Select a World";
        }

        //reset delle skin
        GUI.backgroundColor = Color.white;
        GUI.skin.button.normal.textColor = Color.white;

        //crea un nuovo mondo, andando nel menù in cui mostra la scelta del nome e degli altri valori
        if (GUILayout.Button("Create New World"))
        {
            creazioneMondo = true;
            mondoSelezionato = -1;
            errore = "";
        }

        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Quit", GUILayout.MinWidth(100)))
            Application.Quit();

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void CreaMondo()
    {
        //In caso di problemi
        {
            //nomeMondo.Length viene già controllata alla creazione del mondo, nel menù iniziale
            //se il seed è lasciato vuoto o supera la lunghezza di 50 caratteri, allora viene impostato in base all'orario
            //gli altri valori vengono impostati per rientrare nei requisiti necessari agli script
            float time = (float)System.DateTime.Now.Hour + ((float)System.DateTime.Now.Minute /100);     //minuti diviso cento per averli dietro la virgola -> (float)ore.minuti
            string timeTicks = System.DateTime.Now.Ticks.ToString();    //ticks è un long, ed ha un valore troppo alto per essere usato come float
            float.TryParse(timeTicks.Substring(timeTicks.Length -7, 7), out time);  //quindi lo trasformo in string, uso solo le ultime 7 cifre e lo riporto in float
            valoriMondo.seed = FunzioniMondo.BugFixIniziale(valoriMondo.seed, valoriMondo.seed.Length, 1, 50, time, "Reimpostato seed, da: "+ valoriMondo.seed +" a: "+ time).ToString();
            valoriMondo.grandezzaBlocco = (int)FunzioniMondo.BugFixIniziale(valoriMondo.grandezzaBlocco, valoriMondo.grandezzaBlocco, 0, 2, 2, "Reimpostata grandezza blocchi, da: "+ valoriMondo.grandezzaBlocco +" a: "+ 2);
            valoriMondo.grandezzaChunk = (int)FunzioniMondo.BugFixIniziale(valoriMondo.grandezzaChunk, valoriMondo.grandezzaChunk, 0, 3, 2, "Reimpostata grandezza chunk, da: "+ valoriMondo.grandezzaChunk +" a: "+ 2);
            valoriMondo.moltiplicatoreY = (int)FunzioniMondo.BugFixIniziale(valoriMondo.moltiplicatoreY, valoriMondo.moltiplicatoreY, 0, 3, 0, "Reimpostato MoltiplicatoreY, da: "+ valoriMondo.moltiplicatoreY +" a:"+ 0);
        }

        //Salva i dati del mondo
        errore = Salva();

        if(errore == "Name already exists")
            return;
        else if(valoriMondo.nomeMondo != errore)
        {
            valoriMondo.nomeMondo = errore;
            Debug.Log("Il nome del mondo è stato cambiato in: " + errore);
        }

        //carica la scena in cui si gioca
        CaricaScena();
    }

    void CaricaMondo(string nomeMondoDaCaricare)
    {
        //se esiste un mondo con quel nome, allora viene caricato, altrimenti mostra un errore
        if(Carica(nomeMondoDaCaricare))
        {
            //carica la scena in cui si gioca
            CaricaScena();
        }
        else
        {
            errore = "Impossible to Load";
        }
    }

    string Salva()
    {
        //Salva i dati del mondo
        return SaveAndLoad.SalvaMondo(valoriMondo.nomeMondo, valoriMondo, erroreCreaMondoEsistente);
    }

    bool Carica(string nomeMondoDaCaricare)
    {
        //vengono caricati i dati del mondo
        ValoriMondo datiCaricati = SaveAndLoad.CaricaMondo(nomeMondoDaCaricare);

        //se non si trovato i dati salvati, allora ritorna false
        if (datiCaricati == null)
            return false;
        else
        {
            //se si trovano, li passa a valoriMondo e ritorna true
            valoriMondo = datiCaricati;
            return true;
        }
    }

    void CaricaScena()
    {        
        //fa sì che questo script non visualizzi più niente, perché si sta per cambiare scena
        scenaAttiva = false;

        //carica la scena successiva, presente nella lista delle scene al momento del build 
        //(LoadSceneMode.Single per chiudere questa scena e aprirne un'altra, invece di crearla su questa)
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);

        //per poter utilizzare la funziona al cambio di scena e passare i valori a Mondo.cs
        SceneManager.sceneLoaded += ScenaCaricata;  
    }    

	void ScenaCaricata(Scene scene, LoadSceneMode sceneMode)
	{
        //per non poter più richiamare questa funzione
        SceneManager.sceneLoaded -= ScenaCaricata;

        //trova Mondo.cs e avvia la StartFunction, che farà ottenere i valori e distruggere questo gameObject
		Mondo mondo = FindObjectOfType<Mondo>();
    	mondo.StartFunction(valoriMondo.nomeMondo, valoriMondo.seed, valoriMondo.grandezzaBlocco, valoriMondo.grandezzaChunk, valoriMondo.moltiplicatoreY, this.gameObject);            
	}
}