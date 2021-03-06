﻿using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SoundDesigner : EditorWindow
{
    private AudioManager am;
    private Vector2 listScroll;
    private string output;
    private readonly List<int> sounds = new List<int>();
    private readonly List<float> soundVolumes = new List<float>();

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        if (am)
        {
            listScroll = GUILayout.BeginScrollView(listScroll);
            EditorGUILayout.Space();

            for (var i = 0; i < am.effects.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(am.effects[i].name, EditorStyles.boldLabel, GUILayout.MaxWidth(150.0f));
                if (GUILayout.Button("Play", GUILayout.MaxWidth(70.0f))) Play(i);
                if (GUILayout.Button("Add", GUILayout.MaxWidth(70.0f)))
                {
                    sounds.Add(i);
                    soundVolumes.Add(1f);
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            GUILayout.EndScrollView();
        }
        else
        {
            if (GUILayout.Button("Find AudioManager")) FindAudioManager();
        }

        if (sounds.Count > 0)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            output = "";

            for (var i = 0; i < sounds.Count; i++)
            {
                var sb = sounds[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label(am.effects[sounds[i]].name, EditorStyles.boldLabel, GUILayout.MaxWidth(150.0f));
                soundVolumes[i] = EditorGUILayout.Slider("", soundVolumes[i], 0f, 2f);
                if (GUILayout.Button("Play", GUILayout.MaxWidth(70.0f))) Play(sounds[i]);
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(70.0f)))
                {
                    sounds.RemoveAt(i);
                    soundVolumes.RemoveAt(i);
                }

                GUILayout.EndHorizontal();

                var pars = sounds[i] + ", transform.position, " + soundVolumes[i];
                output += "AudioManager.Instance.PlayEffectAt(" + pars + "f);\n";
            }

            EditorGUILayout.Space();

            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Play in play mode", GUILayout.MaxHeight(50f)))
                    for (var i = 0; i < sounds.Count; i++)
                        am.PlayEffectAt(sounds[i], Camera.main.transform.position, soundVolumes[i]);
            }
            else
            {
                if (GUILayout.Button("Play", GUILayout.MaxHeight(50f)))
                    for (var i = 0; i < sounds.Count; i++)
                        Play(sounds[i]);
            }

            EditorGUILayout.Space();
            GUILayout.TextArea(output, GUILayout.MinHeight(100f));
            EditorGUILayout.Space();

            if (GUILayout.Button("Clear"))
            {
                sounds.Clear();
                soundVolumes.Clear();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();
    }

    [MenuItem("Window/SoundDesigner")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SoundDesigner));
    }

    public void FindAudioManager()
    {
        am = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    private void Play(int i)
    {
        if (EditorApplication.isPlaying)
        {
            am.PlayEffectAt(i, Camera.main.transform.position, 1f);
            return;
        }

        var path = "Assets/Sounds/" + am.effects[i].name + ".wav";
        var c = (AudioClip) EditorGUIUtility.Load(path);
        PlayClip(c);
    }

    public static void PlayClip(AudioClip clip)
    {
        var unityEditorAssembly = typeof(AudioImporter).Assembly;
        var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        var method = audioUtilClass.GetMethod(
            "PlayClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new[]
            {
                typeof(AudioClip)
            },
            null
        );
        method.Invoke(
            null,
            new object[]
            {
                clip
            }
        );
    }
}