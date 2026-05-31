using UnityEngine;

public class FireBall : Projectile
{
    protected override void OnTriggerEnter(Collider other)
    {
        // Aqui podrian ir cosas como efectos, reacciones especials , y no se cosas :v.
        
        // Efectos de explosion, particulas, etc.
        Destroy(gameObject);
    }
}
