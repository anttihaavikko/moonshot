using UnityEngine;

public class StartView : MonoBehaviour
{
    private bool disableStart;
    private bool hasStarted;

    private void Start()
    {
        AudioManager.Instance.BackToDefaultMusic();
    }

    // Update is called once per frame
    private void Update()
    {
        if (disableStart) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !hasStarted && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            hasStarted = true;
            Application.Quit();
        }

        if (Input.anyKeyDown && !hasStarted)
        {
            hasStarted = true;

            if (SaveManager.Instance.ShouldShowDemo("Intro"))
            {
                SaveManager.Instance.MarkDemoSeen("Intro");
                SceneChanger.Instance.ChangeScene("Intro");
            }
            else
            {
                SceneChanger.Instance.ChangeScene("LevelSelect");
            }
        }
    }

    public void SetClicksDisabled(bool state)
    {
        disableStart = state;
    }
}