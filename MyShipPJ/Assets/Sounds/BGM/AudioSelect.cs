using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSelect : MonoBehaviour
{
    public void OnButtonZeroClicked()
    {
        AudioManager.instance.Zero();
        Debug.Log("Zero clicked");
    }

    public void OnButtonOneClicked()
    {
        AudioManager.instance.One();
        Debug.Log("one clicked");
    }

    public void OnButtonTwoClicked()
    {
        AudioManager.instance.Two();
        Debug.Log("Two clicked");
    }
}
