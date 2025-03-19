using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialSound
{
    public PhysicsMaterial material;
    public AudioClip clip;
}
//basically creates list to play based on material hit
public class BounceSoundPlayer : MonoBehaviour
{

    public AudioSource audioSource;

    public List<MaterialSound> materialSounds;

    public AudioClip defaultClip;

    void OnCollisionEnter(Collision collision)
    {
        PhysicsMaterial hitMaterial = collision.collider.sharedMaterial;

        AudioClip clipToPlay = null;

        if (hitMaterial != null)
        {
            foreach (var materialSound in materialSounds)
            {
                if (materialSound.material == hitMaterial)
                {
                    clipToPlay = materialSound.clip;
                    break;
                }
            }
        }

        if (clipToPlay == null && defaultClip != null)
        {
            clipToPlay = defaultClip;
        }

        if (clipToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}
