using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet: MonoBehaviour
{
    public float speed = 0.06f;
    public GameObject explode;
    public float maxLiftTime = 2f;
    public float instantiateTime = 0f;
    public GameObject attackTank;
    public AudioClip explodeClip;
    // Start is called before the first frame update
    void Start()
    {
        instantiateTime = Time.time;
        //explode = GameObject.Find("explodeEffect");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.time;
        if (Time.time - instantiateTime > maxLiftTime)
        {
            if (null != this)
            {
                Explode();
            }
        }
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.gameObject == attackTank)
            return;
        Explode();
        
        Tank tank = collisionInfo.gameObject.GetComponent<Tank>();
        if(tank != null)
        {
            float att = GetAtt();
            tank.BeAttacked(att, attackTank);
        }
    }

    void Explode()
    {
        GameObject explodeObj = (GameObject) Instantiate(explode, transform.position, transform.rotation);
        Destroy(gameObject);
        AudioSource audioSource = explodeObj.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1;
        audioSource.PlayOneShot(explodeClip);
        Destroy(explodeObj, 2);
    }

    private float GetAtt()
    {
        float att = 100 - (Time.time - instantiateTime) * 40;
        if (att < 1)
            att = 1;
        return att;
    }
}
