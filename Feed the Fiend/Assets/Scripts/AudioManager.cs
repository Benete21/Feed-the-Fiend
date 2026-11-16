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
    public AudioClip cookingFood;
    public AudioClip monsterMoving;
    public AudioClip monsterEnter;
    public AudioClip dayComplete;
    public AudioClip waiterStunned;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public AudioSource preparingFoodSource;
    public AudioSource cookingFoodSource;
    public AudioSource monsterEnterSource;
    public AudioSource dayCompleteSource;
    public AudioSource waiterStunnedSource;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayWaiterStunned();
        }
    }

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

    public void PlayCookingFood()
    {
        if (cookingFoodSource != null && cookingFood != null)
        {
            cookingFoodSource.clip = cookingFood;
            cookingFoodSource.loop = true;

            if (!cookingFoodSource.isPlaying)
            {
                cookingFoodSource.Play();
            }
        }
    }
    public void StopCookingFood()
    {
        if (cookingFoodSource.isPlaying)
        {
            cookingFoodSource.Stop();
        }
    }

    public void playMonsterEnter()
    {
        if (!preparingFoodSource.isPlaying)
        {
            monsterEnterSource.clip = monsterEnter;
            monsterEnterSource.loop = false;
            monsterEnterSource.Play();
        }
    }

    public void PlayDayComplete()
    {
        if (!dayCompleteSource.isPlaying)
        {
            dayCompleteSource.clip = dayComplete;
            dayCompleteSource.loop = false;
            dayCompleteSource.Play();
        }
    }

    public void PlayWaiterStunned()
    {
        if (waiterStunnedSource != null && waiterStunned != null)
        {
            waiterStunnedSource.PlayOneShot(waiterStunned);
            Debug.Log("Waiter stunned audio playing!");
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