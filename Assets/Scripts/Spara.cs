using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Spara : RompiPiazzaScript
{
    //il punto da cui deve partire l'effetto particellare del fucile
    public Transform cannaFucile;

    //l'effetto particellare dello sparo
    public GameObject fucileParticles;

    //l'effetto particellare quando si colpisce qualcosa
    public GameObject hitParticles;
    
    public void StartFunction(Camera cam)
    {
        //sistema se non è stato aggiunto dall'inspector
        if(cannaFucile == null)
        {
            cannaFucile = cam.transform.parent.Find("CannaFucile");
        }
        if(fucileParticles == null)
        {
            fucileParticles = (GameObject)Resources.Load("Particle Effects/GunParticles/GunParticles");
        }
        if(hitParticles == null)
        {
            hitParticles = (GameObject)Resources.Load("Particle Effects/HitParticles/HitParticles");
        }
    }

    public override void UpdateFunction(Camera cam, bool InputRompi, bool InputPiazza)
    {        
        //se spara e può sparare
        if(InputRompi && Time.time < canFire)
        {
            //instanzia l'effetto particellare alla canna del fucile
            GameObject effetto_particellare = GameObject.Instantiate(fucileParticles, cannaFucile.position, cam.transform.rotation);
            GameObject.Destroy(effetto_particellare, 1);
            effetto_particellare.GetComponent<ParticleSystem>().Play(true);
        }

        //esegue la funzione normale
        base.UpdateFunction(cam, InputRompi, InputPiazza);
    }

	public override void Rompi_Blocco(RaycastHit hit)
    {
        base.Rompi_Blocco(hit);
           
        //instanzia l'effetto particellare sul blocco colpito
        GameObject effetto_particellare = GameObject.Instantiate(hitParticles, hit.point, Quaternion.Euler(hit.normal));
        GameObject.Destroy(effetto_particellare, 1);
        effetto_particellare.GetComponent<ParticleSystem>().Play(true);
    }

    public override void Rompi_Nemico(RaycastHit hit)
    {
        //instanzia l'effetto particellare sul nemico colpito
        GameObject effetto_particellare = GameObject.Instantiate(hitParticles, hit.point, Quaternion.Euler(hit.normal));
        GameObject.Destroy(effetto_particellare, 1);
        effetto_particellare.GetComponent<ParticleSystem>().Play(true);

        NemicoScript nemico = hit.collider.GetComponent<NemicoScript>();
        if(nemico)
        {
            nemico.SubisciDanni(danno);
        }
    }
}
