using UnityEngine;

namespace PhysicsProject.Objects
{
    ///<summary>
    /// Contrôleur pour le verre transparent
    /// Gère les propriétés physiques et les réactions aux collisions
    ///</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GlassController : MonoBehaviour
    {
        [Header("Propriétés du Verre")]
        [SerializeField] private float mass = 0.2f;
        [SerializeField][Range(0f, 1f)] private float friction = 0.3f;
        [SerializeField][Range(0f, 0.5f)] private float bounciness = 0.1f;

        [Header("Stabilité")]
        [SerializeField] private float centerOfMassOffset = -0.05f; // Plus bas = plus stable
        [SerializeField] private bool isStaticObject = false; // Verre fixe sur la table (false = peut bouger/casser)

        [Header("Effets Visuels")]
        [SerializeField] private Material glassMaterial;
        [SerializeField][Range(0f, 1f)] private float transparency = 0.3f;
        [SerializeField] private Color glassColor = new Color(1f, 1f, 1f, 0.3f);

        [Header("Sons (Optionnel)")]
        [SerializeField] private AudioClip glassClinkSound;
        [SerializeField] private float soundVolume = 0.4f;

        [Header("Casse du Verre")]
        [SerializeField] private float breakForceThreshold = 2.0f; // Force nécessaire pour casser
        [SerializeField] private GameObject[] glassFragments; // Fragments Blender (cell fracture)
        [SerializeField] private float explosionForce = 3.0f; // Force d'explosion des fragments
        [SerializeField] private float fragmentLifetime = 5.0f; // Durée de vie des fragments

        private Rigidbody rb;
        private AudioSource audioSource;
        private MeshRenderer meshRenderer;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool isBroken = false;

        void Start()
        {
            InitializeComponents();
            SetupPhysics();
            SetupVisuals();
        }

        void InitializeComponents()
        {
            rb = GetComponent<Rigidbody>();
            meshRenderer = GetComponent<MeshRenderer>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            // Audio optionnel
            if (glassClinkSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = glassClinkSound;
                audioSource.volume = soundVolume;
                audioSource.playOnAwake = false;
            }
        }

        void SetupPhysics()
        {
            rb.mass = mass;
            rb.useGravity = true; // Gravité activée
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.5f;

            // Centre de masse plus bas pour stabilité
            rb.centerOfMass = new Vector3(0, centerOfMassOffset, 0);

            // Verre statique ou dynamique
            if (isStaticObject)
            {
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                // Mode dynamique avec physique réaliste
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.mass = 5.0f; // Masse raisonnable pour stabilité et chute réaliste
                // Aucune contrainte - le verre se comporte naturellement
                rb.constraints = RigidbodyConstraints.None;
            }

            // Matériau physique du verre
            PhysicsMaterial glassMaterial = new PhysicsMaterial("Glass")
            {
                dynamicFriction = friction,
                staticFriction = friction,
                bounciness = bounciness,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };

            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.material = glassMaterial;
            }
        }

        void SetupVisuals()
        {
            if (meshRenderer != null)
            {
                if (glassMaterial != null)
                {
                    meshRenderer.material = glassMaterial;
                }

                // Configuration de la transparence
                Material mat = meshRenderer.material;
                if (mat.HasProperty("_Color"))
                {
                    mat.color = glassColor;
                }

                // Mode transparent
                if (mat.HasProperty("_Mode"))
                {
                    mat.SetFloat("_Mode", 3); // Transparent mode
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (isBroken) return;

            float impactForce = collision.relativeVelocity.magnitude;

            // Son de verre lors de l'impact
            if (audioSource != null && impactForce > 0.3f)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.Play();
            }

            // Vérifier si la force est suffisante pour casser le verre
            if (impactForce >= breakForceThreshold)
            {
                BreakGlass(collision.contacts[0].point);
            }
        }

        /// <summary>
        /// Casse le verre en fragments (Cell Fracture de Blender)
        /// </summary>
        void BreakGlass(Vector3 impactPoint)
        {
            isBroken = true;

            // Vérifier que les fragments Blender existent
            if (glassFragments == null || glassFragments.Length == 0)
            {
                Debug.LogWarning("Aucun fragment de verre assigné ! Assignez les fragments dans l'Inspector.");
                Destroy(gameObject);
                return;
            }

            // Instancier tous les fragments de Blender
            ActivateBlenderFragments(impactPoint);

            // Cacher le verre intact (ou le détruire)
            meshRenderer.enabled = false;
            GetComponent<Collider>().enabled = false;

            // Détruire le verre parent après un court délai
            Destroy(gameObject, 0.1f);
        }

        /// <summary>
        /// Active les fragments Blender - ils s'éparpillent sur la table
        /// </summary>
        void ActivateBlenderFragments(Vector3 impactPoint)
        {
            // Calculer le centre du verre pour la direction radiale
            Vector3 glassCenter = transform.position;

            foreach (GameObject fragmentPrefab in glassFragments)
            {
                if (fragmentPrefab == null) continue;

                // Instancier le fragment à la position du verre parent
                GameObject fragment = Instantiate(fragmentPrefab, transform.position, transform.rotation);

                // Appliquer le matériau transparent
                MeshRenderer fragRenderer = fragment.GetComponent<MeshRenderer>();
                if (fragRenderer != null && glassMaterial != null)
                {
                    fragRenderer.material = glassMaterial;
                }

                // Ajouter Rigidbody si absent
                Rigidbody fragRb = fragment.GetComponent<Rigidbody>();
                if (fragRb == null)
                {
                    fragRb = fragment.AddComponent<Rigidbody>();
                }

                // Configurer la physique - TRÈS léger pour ne pas faire bouger la table
                fragRb.mass = 0.005f; // Masse réduite de moitié
                fragRb.useGravity = true;
                fragRb.linearDamping = 1.0f; // Résistance augmentée
                fragRb.angularDamping = 0.5f;
                fragRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Ajouter collider si absent
                if (fragment.GetComponent<Collider>() == null)
                {
                    MeshCollider meshCol = fragment.AddComponent<MeshCollider>();
                    meshCol.convex = true;
                }

                // IMPORTANT : Utiliser le centre du fragment RELATIF au parent
                // Ceci donne la vraie position de chaque morceau dans le verre cassé
                Vector3 fragmentLocalCenter = Vector3.zero;
                Renderer fragRend = fragment.GetComponent<Renderer>();
                if (fragRend != null)
                {
                    fragmentLocalCenter = fragment.transform.InverseTransformPoint(fragRend.bounds.center);
                }

                // Position world du centre du fragment
                Vector3 fragmentWorldCenter = fragment.transform.TransformPoint(fragmentLocalCenter);

                // Direction depuis le centre du verre vers ce fragment spécifique
                Vector3 directionFromCenter = (fragmentWorldCenter - glassCenter);

                // Si trop proche du centre, direction aléatoire
                if (directionFromCenter.sqrMagnitude < 0.0001f)
                {
                    directionFromCenter = new Vector3(
                        Random.Range(-1f, 1f),
                        0f,
                        Random.Range(-1f, 1f)
                    );
                }

                // Projection horizontale uniquement
                directionFromCenter.y = 0f;
                directionFromCenter.Normalize();

                // Force réduite pour éparpillement doux
                float spreadForce = Random.Range(0.1f, 0.25f); // Force encore plus douce
                Vector3 force = directionFromCenter * spreadForce;

                // Très petite force verticale
                force.y = Random.Range(0.02f, 0.08f);

                fragRb.AddForce(force, ForceMode.Impulse);

                // Rotation douce
                Vector3 randomTorque = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f)
                );
                fragRb.AddTorque(randomTorque, ForceMode.Impulse);

                // Détruire après la durée spécifiée
                Destroy(fragment, fragmentLifetime);
            }
        }

        /// <summary>
        /// Active/désactive le mode statique du verre
        /// </summary>
        public void SetStatic(bool isStatic)
        {
            isStaticObject = isStatic;
            rb.isKinematic = isStatic;

            if (isStatic)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.None;
            }
        }

        /// <summary>
        /// Réinitialise le verre à sa position initiale
        /// </summary>
        public void ResetToInitialPosition()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (isStaticObject)
            {
                rb.isKinematic = true;
            }
        }

        /// <summary>
        /// Modifie la transparence du verre dynamiquement
        /// </summary>
        public void SetTransparency(float alpha)
        {
            transparency = Mathf.Clamp01(alpha);
            if (meshRenderer != null && meshRenderer.material.HasProperty("_Color"))
            {
                Color color = meshRenderer.material.color;
                color.a = transparency;
                meshRenderer.material.color = color;
            }
        }

        void OnDrawGizmosSelected()
        {
            // Visualisation du centre de masse
            Gizmos.color = Color.red;
            Vector3 centerOfMassWorld = transform.TransformPoint(
                GetComponent<Rigidbody>() != null ?
                GetComponent<Rigidbody>().centerOfMass :
                new Vector3(0, centerOfMassOffset, 0)
            );
            Gizmos.DrawWireSphere(centerOfMassWorld, 0.02f);
        }
    }
}
