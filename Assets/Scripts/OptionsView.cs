using UnityEngine;
using UnityEngine.UI;

public class OptionsView : MonoBehaviour
{
    public Slider musicSlider, soundSlider;
    
    private float prevSoundStep;
    private bool isInit;

    private void Start()
    {
        AudioManager.Instance.LoadVolumes ();
        soundSlider.value = AudioManager.Instance.volume;
        musicSlider.value = AudioManager.Instance.GetMusicVolume();
        prevSoundStep = AudioManager.Instance.volume;
        isInit = true;
    }

    public void ChangeMusicVolume()
    {
        var value = musicSlider.value;
        AudioManager.Instance.curMusic.volume = value;
        AudioManager.Instance.ChangeMusicVolume(value);
		AudioManager.Instance.SaveVolumes ();
    }

    public void ChangeSoundVolume()
    {
        if (!isInit) return;
        
        if (Mathf.Abs(soundSlider.value - prevSoundStep) > 0.1f)
        {
            AudioManager.Instance.PlayEffectAt(0, Vector3.zero, 1.5f);
            prevSoundStep = soundSlider.value;
        }

        AudioManager.Instance.volume = soundSlider.value;
        AudioManager.Instance.SaveVolumes ();
    }
}