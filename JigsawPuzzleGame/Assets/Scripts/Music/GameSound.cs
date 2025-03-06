using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSound : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> clips;
    // Start is called before the first frame update
   public void ButtonClick(){
    audioSource.PlayOneShot(clips[0]);
   }
    public void Rotate(){
    audioSource.PlayOneShot(clips[1]);
    }

    public void Win(){
    audioSource.PlayOneShot(clips[2]);
   }

    public void saveGame(){
    audioSource.PlayOneShot(clips[3]);
   }

   public void select(){
    audioSource.PlayOneShot(clips[4]);
   }
}
