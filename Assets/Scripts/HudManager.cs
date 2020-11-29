using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public Text infoDisplay;

    public Transform worldCanvas;
    public Text flyingText;
    private float deltaTime;

    private float messageAlpha;

    public static HudManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        messageAlpha = Mathf.MoveTowards(messageAlpha, 0, Time.deltaTime / Time.timeScale);

        if (messageAlpha <= 1f) infoDisplay.color = new Color(1, 1, 1, messageAlpha);
    }

    public void DisplayMessage(string msg, float delay = 1f)
    {
        if (infoDisplay)
        {
            infoDisplay.text = msg;
            infoDisplay.color = Color.white;
            messageAlpha = 1f + delay;
        }
        else
        {
            Debug.Log("Message display area not set!");
        }
    }

    public void ShowStatus(float x, float y, string str, Color c)
    {
        var t = Instantiate(flyingText, new Vector3(x, y + 1f, 0), Quaternion.identity);
        t.color = c;
        t.text = str;
        t.transform.SetParent(worldCanvas, false);
    }
}