using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Music : MonoBehaviour
{
    private enum Track
    {
        NONE = -1,
        MENU = 0,
        GAME = 1
    }

    public AudioSource[] startSources, loopSources;
    private Track currentTrack, desiredTrack;
    private float baseVolume = 0.5f;
    private float volumePercentage = 1;
    private bool fading, looping;

    // Start is called before the first frame update
    void Start()
    {
        if(GameObject.FindObjectsByType<Music>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(this);
            return;
        }
        else
        {
            DontDestroyOnLoad(this);
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            looping = false;
            currentTrack = Track.NONE;
            CheckCurrentScene();
        }
    }

    private void SceneManager_activeSceneChanged (Scene previous, Scene current)
    {
        CheckCurrentScene();
    }

    private void CheckCurrentScene()
    {
        switch(SceneManager.GetActiveScene().name)
        {
            case "Main Menu":
            case "HowToPlay":
            case "Credits":
            case "Level Select":
                TryPlayMenuTheme();
                break;
            default:
                TryPlayGameTheme();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(fading)
        {
            if(currentTrack != desiredTrack)
            {
                //Fading out

                volumePercentage -= 2f * Time.deltaTime; //2 seconds to fade out
                if(volumePercentage < 0)
                {
                    volumePercentage = 0;

                    if(currentTrack != Track.NONE)
                    {
                        startSources[(int)currentTrack].Stop();
                        loopSources[(int)currentTrack].Stop();
                    }

                    currentTrack = desiredTrack;
                    startSources[(int) currentTrack].Play();
                }
            }
            else
            {
                //Fading in
                volumePercentage += 2f * Time.deltaTime; //2 seconds to fade in
                if(volumePercentage > 1)
                {
                    volumePercentage = 1;
                    fading = false;
                }
            }
        }

        if(currentTrack != Track.NONE)
        {
            if(currentTrack == desiredTrack && !looping && !startSources[(int)currentTrack].isPlaying)
            {
                looping = true;
                startSources[(int)currentTrack].Stop();
            }

            float audioTime = startSources[(int)currentTrack].time;
            switch(currentTrack)
            {
                case Track.MENU:
                    if(audioTime >= 7.2 && !loopSources[(int)currentTrack].isPlaying) loopSources[(int)currentTrack].Play();
                    break;
                case Track.GAME:
                    if(audioTime >= 12.73 && !loopSources[(int)currentTrack].isPlaying) loopSources[(int)currentTrack].Play();
                    break;
            }

            startSources[(int)currentTrack].volume = baseVolume * volumePercentage;
            loopSources[(int)currentTrack].volume = baseVolume * volumePercentage;
        }
        

        
    }

    void TryPlayMenuTheme()
    {
        if(currentTrack == Track.MENU) return;
        fading = true;
        looping = false;
        desiredTrack = Track.MENU;
    }

    void TryPlayGameTheme()
    {
        if(currentTrack == Track.GAME) return;
        fading = true;
        looping = false;
        desiredTrack = Track.GAME;
    }
}
