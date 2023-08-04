using UnityEngine;

 [System.Serializable]
 public class MirinoIntelligente
 {
    //per decidere se raffigurare il mirino
    public bool disegnaMirino = true;

	//colore del mirino
    public Color coloreMirino = Color.white;
	//larghezza e altezza delle linee che compongono il mirino
    public float larghezza = 2;
    public float altezza = 10;
 
	//una sottoclasse con le variabili necessarie per allargare il mirino.
	//da modificare per avere un mirino più o meno largo (setta la grandezza del centro, quindi la distanza tra le linee che compongono il mirino)
    public class spreading
    {
        public float spread = 5;
        public float minSpread = 5;
        public float maxSpread = 20;
        public float spreadPerSecond = 30;
        public float decreasePerSecond = 25;
    }
 
    public spreading allargamento = new spreading();
 
	//le variabili che vengono usate nello script
    Texture2D tex;
    GUIStyle stileLinea;
 

    public void CreaMirino () 
	{
		//crea la texture
        tex = new Texture2D(1, 1);
		SetColor(tex, coloreMirino);

		//crea un guiStyle con la texture come background
        stileLinea = new GUIStyle();
        stileLinea.normal.background = tex;
    }

	public void AllargaRestringi_Mirino (bool inputAllargaMirino, float minSpread = 5, float maxSpread = 20, float spreadPerSecond = 30, float decreasePerSecond = 25)
	{
		//quando si preme l'input (ad esempio si spara), si allarga il mirino, altrimenti si restringe
    	if(inputAllargaMirino)
		{
			//allarga il mirino
        	allargamento.spread += spreadPerSecond * Time.deltaTime;
    	}
		else
		{
			//restringi il mirino
        	allargamento.spread -= decreasePerSecond * Time.deltaTime;     
    	}
    
		//limita lo spread in base al min/max
    	allargamento.spread = Mathf.Clamp(allargamento.spread, minSpread, maxSpread);  
 	}
     
    public void DisegnaMirino () 
	{
		//viene settato come punto centrale il centro dello schermo
        Vector2 puntoCentrale = new Vector2(Screen.width / 2, Screen.height / 2);
 
        if (disegnaMirino)
        {
            //sotto e sopra
            GUI.Box(new Rect(puntoCentrale.x - (larghezza / 2), puntoCentrale.y - (altezza + allargamento.spread), larghezza, altezza), "", stileLinea);
            GUI.Box(new Rect(puntoCentrale.x - (larghezza / 2), puntoCentrale.y + allargamento.spread, larghezza, altezza), "", stileLinea);
            //destra e sinistra
            GUI.Box(new Rect(puntoCentrale.x + allargamento.spread, puntoCentrale.y - (larghezza / 2), altezza, larghezza), "", stileLinea);
            GUI.Box(new Rect(puntoCentrale.x - (altezza + allargamento.spread), puntoCentrale.y - (larghezza / 2), altezza, larghezza), "", stileLinea);
        }
    }
 
    void SetColor(Texture2D myTexture, Color myColor) 
	{
        for (int y = 0; y < myTexture.height; y++) 
		{
            for (int x = 0; x < myTexture.width; x++)
			{
                myTexture.SetPixel(x, y, myColor);
			}
        }

        myTexture.Apply();
    }
}
