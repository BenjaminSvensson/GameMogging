using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Playermotor))]
public class PlayerAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Playermotor motor;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] walkFootstepClips;
    [SerializeField] private AudioClip[] runFootstepClips;
    [SerializeField, Min(0f)] private float minFootstepSpeed = 0.2f;
    [SerializeField, Min(0.01f)] private float walkStepInterval = 0.48f;
    [SerializeField, Min(0.01f)] private float runStepInterval = 0.32f;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.8f;
    [SerializeField, Range(0f, 0.5f)] private float footstepPitchVariance = 0.08f;

    [Header("Jump")]
    [SerializeField] private AudioClip[] jumpClips;
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.9f;

    [Header("Landing")]
    [SerializeField] private AudioClip[] landClips;
    [SerializeField, Range(0f, 1f)] private float landVolume = 0.9f;

    private float footstepTimer;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (motor == null)
        {
            motor = GetComponent<Playermotor>();
        }

        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        if (motor == null)
        {
            return;
        }

        motor.Jumped += PlayJump;
        motor.Landed += PlayLanding;
    }

    private void OnDisable()
    {
        if (motor == null)
        {
            return;
        }

        motor.Jumped -= PlayJump;
        motor.Landed -= PlayLanding;
    }

    private void Update()
    {
        UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        if (motor == null || !motor.IsGrounded || motor.HorizontalSpeed < minFootstepSpeed)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer > 0f)
        {
            return;
        }

        AudioClip[] clips = motor.IsRunning ? runFootstepClips : walkFootstepClips;
        PlayRandomClip(clips, footstepVolume, footstepPitchVariance);
        footstepTimer = motor.IsRunning ? runStepInterval : walkStepInterval;
    }

    private void PlayJump()
    {
        PlayRandomClip(jumpClips, jumpVolume, 0f);
    }

    private void PlayLanding()
    {
        PlayRandomClip(landClips, landVolume, 0f);
    }

    private void PlayRandomClip(AudioClip[] clips, float volume, float pitchVariance)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
        {
            return;
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null)
        {
            return;
        }

        audioSource.pitch = Random.Range(1f - pitchVariance, 1f + pitchVariance);
        audioSource.PlayOneShot(clip, volume);
    }
}
