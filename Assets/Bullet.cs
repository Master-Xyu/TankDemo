using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet: MonoBehaviour
{
    public float speed = 0.06f;
    public GameObject explode;
    public float maxLiftTime = 2f;
    public float instantiateTime = 0f;
    int i = 0;
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
            i = 1;
            if (null != this)
            {
                Explode();
            }
        }
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        i = 2;
        Explode();
    }

    void Explode()
    {
        var expl = Instantiate(explode, transform.position, transform.rotation);
        Destroy(gameObject);
        Destroy(expl, 2);
        print(i);
    }
}
