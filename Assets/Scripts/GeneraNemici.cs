using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneraNemici : MonoBehaviour 
{
	class Presettati
	{
		public Transform giocatore;
		public Mondo mondo;

		public GameObject Hellephant;
		public GameObject ZomBear;
		public GameObject Zombunny;
	}
	Presettati oggettiPresettati = new Presettati();

	public int possibilitàHellephant = 10;
	public int possibilitàZombunny = 40;
	public int possibilitàZomBear = 50;

	public float tempoSpawn = 1;	// in secondi
	//la distanza massima, in chunk, che può avere un nemico dal giocatore, quando viene spawnato
	public int maxDistanzaDaGiocatore = 3;	
	//la distanza minima, in chunk, che può avere un nemico dal giocatore, quando viene spawnato
	public int minDistanzaDaGiocatore = 2;

	float lastspawn;

	void Start()
	{
		//trova il giocatore
		oggettiPresettati.giocatore = FindObjectOfType<GiocatoreScript>().transform;

		//trova Mondo.cs
		oggettiPresettati.mondo = FindObjectOfType<Mondo>();

		//setta i prefab
		if(oggettiPresettati.Hellephant == null)
			oggettiPresettati.Hellephant = (GameObject)Resources.Load("Enemy/Hellephant");
		
		if(oggettiPresettati.ZomBear == null)			
			oggettiPresettati.ZomBear = (GameObject)Resources.Load("Enemy/ZomBear");

		if(oggettiPresettati.Zombunny == null)			
			oggettiPresettati.Zombunny = (GameObject)Resources.Load("Enemy/Zombunny");

		//sistema per evitare problemi
		if(maxDistanzaDaGiocatore < minDistanzaDaGiocatore)
			maxDistanzaDaGiocatore = minDistanzaDaGiocatore;
	}
	
	void Update () 
	{
		//se manca il giocatore o non è ancora tempo di spawnare, non eseguire la funzione
		if(oggettiPresettati.giocatore == null || Time.time < lastspawn)
		{
			return;
		}

		//si sceglie il chunk in cui spawnare il nemico
		Chunk chunkSpawn = TrovaChunk();

		//se non si è trovato il chunk, annulla la funzione, si ricerca nel prossimo frame, senza aspettare il tempo di spawn
		if(chunkSpawn == null)
			return;

		//se si spawna il nemico, si imposta il tempo di spawn
		lastspawn = Time.time + tempoSpawn;

		Vector3 posizioneNemico = new Vector3( chunkSpawn.chunkPosition.x, chunkSpawn.chunkPosition.y, chunkSpawn.chunkPosition.z );
		
		//dalla posizione ottenuta, piazza il nemico sopra il chunk
		RaycastHit hit;

		if(Physics.Raycast(posizioneNemico + (Vector3.up * 100), Vector3.down, out hit))
		{
			posizioneNemico = hit.point + Vector3.up;
		}

		//in base al valore da 0 a 100, decide cosa instanziare
		int percentuale = Random.Range(0, 100);

		if(percentuale < possibilitàHellephant)
			Instantiate(oggettiPresettati.Hellephant, posizioneNemico, Quaternion.identity);
		else if(percentuale < possibilitàZombunny)
			Instantiate(oggettiPresettati.Zombunny, posizioneNemico, Quaternion.identity);
		else
			Instantiate(oggettiPresettati.ZomBear, posizioneNemico, Quaternion.identity);
	}

	Chunk TrovaChunk()
	{
		//ottiene il chunk in cui si trova il giocatore
		Vector3Int chunkGiocatore = FunzioniMondo.OttieniChunkPlayer(oggettiPresettati.giocatore.position);

		//si ottiene per ogni coordinata, il numero di chunk cui dovrà essere distante (in positivo o negativo)
		int x = (int)RandomPositivoNegativo(Random.Range(minDistanzaDaGiocatore, maxDistanzaDaGiocatore) * Chunk.grandezzaChunk * Blocco.grandezzaBlocco);
		int y = (int)RandomPositivoNegativo(Random.Range(minDistanzaDaGiocatore, maxDistanzaDaGiocatore) * Chunk.grandezzaChunk * Blocco.grandezzaBlocco);
		int z = (int)RandomPositivoNegativo(Random.Range(minDistanzaDaGiocatore, maxDistanzaDaGiocatore) * Chunk.grandezzaChunk * Blocco.grandezzaBlocco);
		
		return oggettiPresettati.mondo.OttieniChunk(new Vector3Int(chunkGiocatore.x + x, chunkGiocatore.y + y, chunkGiocatore.z + z));
	}

	float RandomPositivoNegativo(float num)
	{
		//restituisce a random il valore in positivo o in negativo
		int random = Random.Range(0, 2);

		if(random <= 0)
			return num;
		else 
			return -num;
	}
}
