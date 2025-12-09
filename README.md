# Projet-final-INFO-4900 : Challenge de Rendu et de Dynamique Physique

## Aperçu du Projet

Ce projet a été réalisé dans le cadre du cours INFO4900 (INFO6900) - Sujets avancés en informatique à l'UMONCTON. L'objectif est de présenter une scène 3D mettant en valeur un rendu visuel de qualité, une dynamique physique réaliste et une déformation crédible d'objet.

## Configuration Requise et Installation

### Plateforme de Développement

  * Moteur de Jeu : Unity
  * Version d'Unity utilisée : 6000.2.10f1

### Guide d'Installation

  **Ouverture du Projet :**
      * Ouvrez le Unity Hub.
      * Ajoutez le dossier cloné ou téléchargé et assurez-vous d'utiliser la version d'Unity **6000.2.10f1**.
      * Remplacer le dossier Asset avec le dossier asset téléchargé de Git
      * Ouvrez le projet.

## Commandes et Exécution

  * Lancement de la Simulation :** Appuyez sur le bouton **Play** dans l'éditeur Unity.
  * Interactions :** Aucune autre commande utilisateur n'est requise.

  * Paramètres pour:
        - Appliquer un box collider et dans le mesh_renderer, la texture est forest wallpaper
        - Balle de Volleyball: Appliquer dans mesh_renderer la texture volleball_albedo, dans rigibody: Mass: 0.27, un mesh collider avec la case convex cochée et le script ballcontroller.
          Dans les propriétés physique sous le script, il y a ballmass = 2, Bounciness= 0.7 et MaxDeformation = 0.5
        - Le DOF glassware a un mesh_collider avec la case convex cochée, un rigibody avec une masse = 1. Un mesh Render avec Glasses_BaseColor
        - Le fork avec un rigibody avec une masse de 0.05, un mesh collider avec la case convex est cochée
        - Le verre produit dans blender avec une masse de 4, les propriétés du verre sont masse = 0.2, friction= 0.3, bounciness = 0.1. Les paramètres de la casse du verre Break Force Threshold = 2, Explosion               Force = 3, Fragment lifetime = 5, un meshcollider avec la case convex cochée. On ajoute le materiel empty(qui contient les 50 morceaux de verre) qui est dans le dossier prefabs et drag and drop le               materiel GlassMat
        - ON applique la texture streaky-plywwod_albedo sur le sol
        - Les murs et le plancher sont crées grâce à des cube gameobjects 3d

## Documentation et Livrables

Tous les livrables du projet sont inclus dans ce dépôt:

  * `Rapport.pdf` (Note technique)
  * `Simulation_Unity.mp4` (Vidéo de démonstration)
  * `Presentation du projet D'info4900.pptx` (Présentation)

## Références et Licences des Assets

### Modèles 3D et Textures

  * **Modèles TurboSquid (Licence Royalty-Free) :**
      * Table : `https://www.turbosquid.com/3d-models/3d-coffee-table-2230645`
      * Ballon : `https://www.turbosquid.com/3d-models/generic-volleyball-ball-low-poly-pbr-2450512`
      * Fourchette : `https://www.turbosquid.com/3d-models/old-vintage-fork-1747461`
      * Verre : `https://www.turbosquid.com/3d-models/dof-glassware-2026510`
  * **Modèles Personnels :** Le verre à vin et les fragments de verre brisé ont été modélisés sur Blender et sont libres de droit.
  * **Textures :** La texture pour la table en bois provient de **freepbr.com**.
