using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{

    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start() {
        bgmSlider.value = AudioManager.instance.bgmVolume;
        sfxSlider.value = AudioManager.instance.sfxVolume;
    }

    public void CloseSettingsPanel(GameObject settingsPanel){
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);
        settingsPanel.SetActive(false);
    }

    public void SignUpAction(){

    }

    public void SignInAction(){
        DataManager.instance.SignInWithGoogle();
    }

    public void SignOutAction(){

    }

    public void DeleteIdAction(){

    }


    /** 오디오 세팅 */
    public void SetBGMScale(){
        AudioManager.instance.SetBGMVolme(bgmSlider.value);
    }

    public void SetSFXScale(){
        AudioManager.instance.SetSFXVolme(sfxSlider.value);
    }
}
