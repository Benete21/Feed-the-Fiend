using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("----------Audio Source----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] public AudioSource SFXSource;

    [Header("----------Monster Audio Source----------")]
    public AudioSource monsterMoveSource;

    [Header("----------Audio Clip----------")]
    public AudioClip background;
    public AudioClip winScreenMusic;
    public AudioClip monsterAttackingWaiter;
    public AudioClip preparingFood;
    public AudioClip monsterMoving;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public AudioSource preparingFoodSource;

    public void PlayPreparingFood()
    {
        if (!preparingFoodSource.isPlaying)
        {
            preparingFoodSource.clip = preparingFood;
            preparingFoodSource.loop = true;
            preparingFoodSource.Play();
        }
    }

    public void StopPreparingFood()
    {
        if (preparingFoodSource.isPlaying)
        {
            preparingFoodSource.Stop();
        }
    }

    public void PlayMonsterMove()
    {
        // Only play if the previous clip has completely finished
        if (!monsterMoveSource.isPlaying)
        {
            monsterMoveSource.clip = monsterMoving;
            monsterMoveSource.loop = false;
            monsterMoveSource.Play();
        }
    }

    public void StopMonsterMove()
    {
        if (monsterMoveSource.isPlaying)
        {
            monsterMoveSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}