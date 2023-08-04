using UnityEngine;

public class NemicoScript : MonoBehaviour 
{
	public float vita = 4;
	public float velocità = 4;
	public float danno = 5;
	public  float rateoAttacco = 1;

	public Transform giocatore;
	Animator anim;

	float lastAttacco;

	BoxCollider boxCollider;

	void Start () 
	{
		giocatore = FindObjectOfType<GiocatoreScript>().transform;
		anim = GetComponent<Animator>();

		boxCollider = GetComponent<BoxCollider>();
	}
	
	void Update () 
	{
		if(giocatore != null)
		{
			SeQualcosaDavanti();

			transform.LookAt(new Vector3(giocatore.position.x, transform.position.y, giocatore.position.z));

			Vector3 direzione = (giocatore.position - transform.position).normalized;
			transform.Translate(direzione * velocità * Time.deltaTime);


			anim.SetBool("PlayerDead", false);
		}
		else
		{
			anim.SetBool("PlayerDead", true);
		}

		//se troppo in basso, uccidi il nemico
		if(transform.position.y < -1000)
			MorteNemico();
	}

	void SeQualcosaDavanti()
	{
		//si crea un LayerMask, così da non colpire né gli altri nemici né ciò che ha layer "Ignore Raycast"
        LayerMask layerMask = ModificaLayer.LayerTuttoTranne(new string[] { "Ignore Raycast", "Nemico" });

		RaycastHit hit;
		Vector3 colliderCenter = new Vector3(boxCollider.center.x / boxCollider.size.x, boxCollider.center.y / boxCollider.size.y, boxCollider.center.z / boxCollider.size.z);

		//controlla con un raycast davanti a sé che non ci sia nulla a bloccarlo, o che ci sia il giocatore davanti
		if(Physics.Raycast(transform.position + colliderCenter, transform.forward, out hit,  2, layerMask))
		{
			//se è il giocatore, lo attacca
			if(hit.collider.gameObject.name == "Giocatore")
			{
				if(Time.time < lastAttacco)
				{
					return;
				}

				lastAttacco = Time.time + rateoAttacco;

				hit.collider.GetComponent<GiocatoreScript>().SubisciDanni(danno);					
			}
			//altrimenti, se si trova a terra e può quindi saltare
			else if (Physics.Raycast(transform.position + colliderCenter, Vector3.down, 1, layerMask))
			{
				//se c'è qualcosa davanti, lo salta
				transform.Translate(Vector3.up * 10 * Time.deltaTime);
			}
		}
	}

	public void SubisciDanni(float danno)
	{
		vita -= danno;
		
		if(vita <= 0)
		{
			MorteNemico();
		}
	}

	void MorteNemico()
	{
		anim.SetTrigger("Dead");
		giocatore = null;

		Destroy(gameObject, 3);
	}
}
