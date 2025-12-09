using UnityEngine;

public class BallController : MonoBehaviour
{
    /*
     * Section des paramètres ajustables dans l'éditeur (Inspector)
     * Ces variables définissent le comportement physique initial de la balle.
     */
    [Header("Propriétés physiques")]
    [Range(0.5f, 5f)]
    public float ballMass = 2.0f; // Masse de la balle (influence l'inertie et la force d'impact)

    [Range(0.1f, 1f)]
    public float bounciness = 0.7f; // Coefficient de restitution (élasticité) du matériau

    [Range(0f, 0.5f)]
    public float maxDeformation = 0.3f; // Limite maximale de réduction du rayon du Collider à l'impact

    [Header("Références")]
    public PhysicsMaterial ballPhysicsMaterial; // Référence à l'Asset de matériau physique (facultatif)

    // Variables privées pour stocker les références aux composants et l'état
    private Rigidbody rb;
    private SphereCollider sphereCollider;
    private float originalRadius; // Rayon initial du Collider avant toute déformation

    void Start()
    {
        // 1. Initialisation des composants Rigidbody et Collider
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();

        // 2. Configuration des propriétés cinématiques du Rigidbody
        rb.mass = ballMass;
        rb.linearDamping = 0.05f; // Amortissement linéaire (résistance de l'air)
        rb.angularDamping = 0.05f; // Amortissement angulaire (freinage de la rotation)

        // 3. Attribution d'un Tag pour une détection de collision simplifiée
        gameObject.tag = "Ball";

        // 4. Stockage du rayon initial pour le cycle de déformation
        originalRadius = sphereCollider.radius;

        // 5. Configuration et application du matériau physique
        if (ballPhysicsMaterial == null)
        {
            CreatePhysicsMaterial(); // Crée un matériau par défaut si non assigné
        }
        sphereCollider.material = ballPhysicsMaterial;

        Debug.Log($"BallController: Balle initialisée avec masse={ballMass}kg et rebond={bounciness}");
    }

    // Fonction pour créer un PhysicsMaterial en runtime
    void CreatePhysicsMaterial()
    {
        ballPhysicsMaterial = new PhysicsMaterial("BallPhysics");
        ballPhysicsMaterial.bounciness = bounciness;
        ballPhysicsMaterial.dynamicFriction = 0.1f;
        ballPhysicsMaterial.staticFriction = 0.2f;
        // Utilise la valeur de rebond maximale entre les deux surfaces en collision
        ballPhysicsMaterial.bounceCombine = UnityEngine.PhysicsMaterialCombine.Maximum;
        // Utilise la valeur de friction minimale entre les deux surfaces
        ballPhysicsMaterial.frictionCombine = UnityEngine.PhysicsMaterialCombine.Minimum;
    }

    // Fonction appelée par le moteur physique lors d'une collision
    void OnCollisionEnter(Collision collision)
    {
        // impactForce représente le changement de moment (masse * vélocité) à l'impact
        float impactForce = collision.impulse.magnitude;
        // impactSpeed est la vitesse relative des deux objets au moment du contact
        float impactSpeed = collision.relativeVelocity.magnitude;

        // Condition de déformation: déclenche l'animation si la force d'impact est suffisante
        if (impactForce > 10f)
        {
            // Calcule la réduction du rayon, clampée entre 0 et maxDeformation
            float deformation = Mathf.Clamp(impactForce * 0.005f, 0f, maxDeformation);

            // Démarre la coroutine pour l'animation d'écrasement du Collider
            StartCoroutine(DeformBall(deformation));
        }

        // Affichage des informations d'impact dans la console (pour le débogage)
        if (collision.gameObject.CompareTag("Glass"))
        {
            Debug.Log($"Collision: Balle contre Verre | Force: {impactForce:F1}N | Vitesse: {impactSpeed:F1}m/s");
        }
        else if (collision.gameObject.CompareTag("Table"))
        {
            Debug.Log($"Collision: Balle contre Table | Force: {impactForce:F1}N");
        }
    }

    // Coroutine pour animer la déformation du SphereCollider
    System.Collections.IEnumerator DeformBall(float deformation)
    {
        float duration = 0.1f; // Durée de l'animation (écrasement ou récupération)
        float elapsed = 0f;
        // Calcul du rayon cible après écrasement
        float targetRadius = originalRadius * (1f - deformation);

        // Phase de Compression (réduction du rayon)
        while (elapsed < duration)
        {
            // Interpolation linéaire du rayon (du rayon original au rayon cible)
            sphereCollider.radius = Mathf.Lerp(originalRadius, targetRadius, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null; // Attend la prochaine frame
        }

        // Phase de Récupération (retour au rayon original)
        elapsed = 0f;
        while (elapsed < duration)
        {
            // Interpolation linéaire du rayon (du rayon cible au rayon original)
            sphereCollider.radius = Mathf.Lerp(targetRadius, originalRadius, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null; // Attend la prochaine frame
        }

        // Assure que le rayon est exactement à la valeur initiale à la fin
        sphereCollider.radius = originalRadius;
    }

    // Méthode de test (accessible via le menu contextuel du composant dans l'Inspector)
    [ContextMenu("Test Déformation")]
    public void TestDeformation()
    {
        StartCoroutine(DeformBall(0.2f));
    }
}