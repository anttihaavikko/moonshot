using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {
    public int level = -1;
    public Vector3 startPos = Vector3.zero;
    public bool showInfo = true;

    private List<string> shownMessages;

	private static Manager instance = null;
	public static Manager Instance {
		get { return instance; }
	}

	void Awake() {
		if (instance != null && instance != this) {
			Destroy (this.gameObject);
			return;
		} else {
			instance = this;
		}

        shownMessages = new List<string>();

        DontDestroyOnLoad(gameObject);
	}

    public void Add(string message)
    {
        shownMessages.Add(message);
    }

    public bool IsShown(string message)
    {
        return shownMessages.Contains(message);
    }
}
