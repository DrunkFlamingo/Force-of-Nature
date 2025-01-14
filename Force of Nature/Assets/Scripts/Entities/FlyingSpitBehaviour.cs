using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingSpitBehaviour : MonoBehaviour
{

    [SerializeField] int damage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        

        if (collision.transform.gameObject.layer == 8)
        {
            AudioManager.instance.PlaySFX("EggCrack");
            PlayerController pc = collision.GetComponent<PlayerController>();
            pc.TakeDamage(15);
        }
        Destroy(gameObject);
    }

}
