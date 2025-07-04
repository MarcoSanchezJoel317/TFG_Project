using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CrowAnimation : MonoBehaviour
{
    [Header("Transform settings")]
    [SerializeField] float maxAltitude = 6;
    [SerializeField] float minAltitude = 1;

    [Header("Animation controler")]
    Animator animator;

    [Header("AIconection")]
    [SerializeField] CrowAI crowAI;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        crowAI = transform.parent.gameObject.GetComponent<CrowAI>();
    }

    private void Update()
    {
        if (crowAI.detected)
        {
            //animator.SetBool("Attacking", true);
            if (crowAI.agent.remainingDistance < 5f)
                ChangeAltitude(minAltitude);

            PlayAttack();
        }
        else if (animator.GetBool("Attacking"))
        {
            //animator.SetBool("Attacking", false);
            ChangeAltitude(maxAltitude);
        }
    }

    void ChangeAltitude(float targetAltitude)
    {
        StopAllCoroutines(); // Detiene cualquier cambio anterior en progreso       

        StartCoroutine(ChangeAltitudeCoroutine(targetAltitude));
    }

    IEnumerator ChangeAltitudeCoroutine(float targetAltitude)
    {
        float speed = 2f;

        if (transform.localPosition.y < minAltitude)
            transform.localPosition = new Vector3(transform.localPosition.x, minAltitude, transform.localPosition.z);
        else if (transform.localPosition.y > maxAltitude)

            transform.localPosition = new Vector3(transform.localPosition.x, maxAltitude, transform.localPosition.z);
        while (transform.localPosition.y >= minAltitude && transform.localPosition.y <= maxAltitude)
        {
            float newY = Mathf.MoveTowards(transform.localPosition.y, targetAltitude, speed * Time.deltaTime);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);

            yield return null;
        }        
    }


    [Header("AudioSettings")]
    public AudioSource audioSource;
    public AudioSource attackSource;
    public AudioClip fly;
    public AudioClip attack;

    public void PlayFly()
    {
        if (audioSource && fly)
        {
            if (audioSource.clip != fly || !audioSource.isPlaying)
            {
                audioSource.clip = fly;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }

    public void PlayAttack()
    {
        if (attackSource && attack)
        {
            if (!attackSource.isPlaying)
                attackSource.PlayOneShot(attack);
        }
    }
    public void Stop()
    {
        if (audioSource && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

}
