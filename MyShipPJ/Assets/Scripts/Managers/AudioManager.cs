using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameData;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public float bgmVolume;
    public float sfxVolume;
    public int curBgm;
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    public AudioClip[] sfxClips;
    public AudioClip[] bgmClips;

    public enum SFXClip { NONE = -1, FAIL, EATTING, SLIDE, BUY, CLICK, SUCCESS, MAX }
    public enum BGMClip { NONE = -1, When_I_Was_A_Boy = 0, Piano = 1, Demo = 2, MAX }

    public List<AudioSource> tempSources;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

    }

    void Start()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);

        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;

        curBgm = PlayerPrefs.GetInt("curBGM", 0);
        PlayBGM((BGMClip)curBgm);
        StartCoroutine(ClearTemp());


        // sfxSources = new List<AudioSource>();

        // for(int i = 0 ; i < (int)SFXClip.MAX ; i++){
        //     sfxSources.Add(gameObject.AddComponent<AudioSource>());
        //     sfxSources[i].clip = sfxClips[i];
        // }
        // 뭐가 더 빠른 지 모르겠네...
    }

    public void PlaySFX(SFXClip clip)
    {
        if (sfxSource.isPlaying)
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.volume = sfxVolume;
            tempSource.clip = sfxClips[(int)clip];
            tempSource.Play();
            tempSources.Add(tempSource);
        }
        else
        {
            sfxSource.clip = sfxClips[(int)clip];
            sfxSource.Play();
        }
    }

    IEnumerator ClearTemp()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            for (int i = 0; i < tempSources.Count; i++)
            {
                if (!tempSources[i].isPlaying)
                {
                    Destroy(tempSources[i]);
                    tempSources.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public void PlayBGM(BGMClip clip)
    {
        bgmSource.clip = bgmClips[(int)clip];
        bgmSource.Play();
    }

    public void SetBGMVolme(float value)
    {
        bgmVolume = value;
        bgmSource.volume = value;
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void SetSFXVolme(float value)
    {
        sfxVolume = value;
        sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }







    public void Zero()
    {
        curBgm = 0;
        PlayerPrefs.SetInt("curBGM", curBgm);
        PlayBGM((BGMClip)curBgm);
        StartCoroutine(ClearTemp());
    }
    public void One()
    {
        curBgm = 1;
        PlayerPrefs.SetInt("curBGM", curBgm);
        PlayBGM((BGMClip)curBgm);
        StartCoroutine(ClearTemp());
    }
    public void Two()
    {
        curBgm = 2;
        PlayerPrefs.SetInt("curBGM", curBgm);
        PlayBGM((BGMClip)curBgm);
        StartCoroutine(ClearTemp());
    }
}
