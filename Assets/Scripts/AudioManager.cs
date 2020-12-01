using UnityEngine;

public class AudioManager : MyObjectPool<SoundEffect>
{
    /******/

    public AudioSource curMusic;
    public AudioSource[] musics;

    public float volume = 0.5f;
    public AudioClip[] effects;

    public AudioLowPassFilter lowpass;
    public AudioHighPassFilter highpass;

    // private AudioReverbFilter reverb;
    // private AudioReverbPreset fromReverb, toReverb;

    private Animator anim;

    private bool doingLowpass, doingHighpass;
    private float fadeOutDuration = 1f, fadeInDuration = 3f;

    private float fadeOutPos, fadeInPos;
    private float musVolume = 0.5f;
    private AudioSource prevMusic;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // reverb = GetComponent<AudioReverbFilter> ();
        //
        // fromReverb = AudioReverbPreset.Hallway;
        // toReverb = AudioReverbPreset.Off;

        DontDestroyOnLoad(Instance.gameObject);
    }

    private void Start()
    {
        LoadVolumes();
    }

    private void Update()
    {
        var targetPitch = 1f;
        var targetLowpass = doingLowpass ? 5000f : 22000;
        var targetHighpass = doingHighpass ? 600f : 10f;
        var changeSpeed = 0.075f;

        curMusic.pitch = Mathf.MoveTowards(curMusic.pitch, targetPitch, 0.01f * changeSpeed * Time.deltaTime * 60f);
        lowpass.cutoffFrequency = Mathf.MoveTowards(lowpass.cutoffFrequency, targetLowpass,
            1500f * changeSpeed * Time.deltaTime * 60f);
        highpass.cutoffFrequency = Mathf.MoveTowards(highpass.cutoffFrequency, targetHighpass,
            200f * changeSpeed * Time.deltaTime * 60f);

        if (fadeInPos < 1f) fadeInPos += Time.unscaledDeltaTime / fadeInDuration;

        if (fadeOutPos < 1f) fadeOutPos += Time.unscaledDeltaTime / fadeOutDuration;

        if (curMusic && fadeInPos >= 0f) curMusic.volume = Mathf.Lerp(0f, musVolume * 1.5f, fadeInPos);

        if (prevMusic)
        {
            prevMusic.volume = Mathf.Lerp(musVolume * 1.5f, 0f, fadeOutPos);

            if (prevMusic.volume <= 0f) prevMusic.Stop();
        }
    }

    public void BackToDefaultMusic()
    {
        if (curMusic != musics[0]) ChangeMusic(0, 0.5f, 2f, 1f);
    }

    public void Lowpass(bool state = true)
    {
        doingLowpass = state;
        doingHighpass = false;
    }

    public void Highpass(bool state = true)
    {
        doingHighpass = state;
        doingLowpass = false;
    }

    public void ChangeMusic(int next, float fadeOutDur, float fadeInDur, float startDelay)
    {
        fadeOutPos = 0f;
        fadeInPos = -1f;

        fadeOutDuration = fadeOutDur;
        fadeInDuration = fadeInDur;

        prevMusic = curMusic;
        curMusic = musics[next];

        prevMusic.time = 0f;

        Invoke(nameof(StartNext), startDelay);
    }

    private void StartNext()
    {
        fadeInPos = 0f;
        curMusic.time = 0f;
        curMusic.volume = 0f;
        curMusic.Play();
    }

    public void PlayEffectAt(AudioClip clip, Vector3 pos, float volume, bool pitchShift = true)
    {
        var se = Get();
        se.transform.position = pos;
        se.Play(clip, volume, pitchShift);
        se.transform.parent = transform;
    }

    public void PlayEffectAt(AudioClip clip, Vector3 pos, bool pitchShift = true)
    {
        PlayEffectAt(clip, pos, 1f, pitchShift);
    }

    public void PlayEffectAt(int effect, Vector3 pos, bool pitchShift = true)
    {
        PlayEffectAt(effects[effect], pos, 1f, pitchShift);
    }

    public void PlayEffectAt(int effect, Vector3 pos, float volume, bool pitchShift = true)
    {
        PlayEffectAt(effects[effect], pos, volume, pitchShift);
    }

    public float GetMusicVolume()
    {
        return musVolume;
    }

    public void ChangeMusicVolume(float vol)
    {
        curMusic.volume = vol * 1.5f;
        musVolume = vol;
    }

    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat("MusicVolume", musVolume);
        PlayerPrefs.SetFloat("SoundVolume", volume);
    }

    public void LoadVolumes()
    {
        ChangeMusicVolume(PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 0.5f);
        volume = PlayerPrefs.HasKey("SoundVolume") ? PlayerPrefs.GetFloat("SoundVolume") : 0.5f;
    }
}