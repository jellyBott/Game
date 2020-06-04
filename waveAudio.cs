using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (AudioSource))] 
public class waveAudio : MonoBehaviour
{
    //GetMicrophone
    private bool micConnected = false;  
    private int minFreq;  
    private int maxFreq;  
    private AudioSource goAudioSource;  




    private const int SAMPLE_SIZE = 1024;

    public float rmsValue;
    public float dbValue;
    public float  pitchValue;

    public float visualModifier = 50.0f;
    public float smoothSpeed = 10.0f;
    //private AudioSource source;
    private float[] samples;
    private float[] spectrum;
    private float sampleRate;

    private Transform[] visualList;
    private float[] visualScale;
    private int amnVisual = 64;

    public  void  Start() {

      GetMicrophone();
        // foreach (var device in Microphone.devices)
        // {
        //     Debug.Log("Name: " + device);
        // }

        //source = GetComponent<AudioSource>();
        //source.loop = true;
        samples = new float[SAMPLE_SIZE];
        spectrum  = new float[SAMPLE_SIZE];
        sampleRate =  44100;

        SpawnLine();
    }

    public void Update(){
       AnalyzeSound();
       UpdateVisual();
    }

    private void AnalyzeSound(){


      goAudioSource.GetOutputData(samples, 0);

      //Get the RMS;

      int i = 0;
      float sum = 0;

      for (; i < SAMPLE_SIZE; i++) {

        sum = samples[i] *  samples[i];
      }

      rmsValue =  Mathf.Sqrt(sum/SAMPLE_SIZE);

      //Get the DB value

      dbValue = 20 * Mathf.Log10(rmsValue/0.1f);

      Debug.Log(dbValue);

      //Get the sounds spectrum

      goAudioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);



      //Find pitch

      float maxV = 0;
      var maxN = 0;

      for (i = 0; i < SAMPLE_SIZE; i++) {

          if (!(spectrum[i] > maxV) || !(spectrum[i] > 0.0f))
            continue;
          

          maxV = spectrum[i];
          maxN = i;
      }

      Debug.Log("MaxV: " + maxV);

      float frenqN = maxN;

      if (maxN > 0 && maxN < SAMPLE_SIZE - 1) {

        var dL =  spectrum[maxN - 1] / spectrum[maxN];
        var dR =  spectrum[maxN + 1] / spectrum[maxN];

        frenqN += 0.5f * (dR * dR - dL * dL);

      }

      pitchValue = frenqN * (sampleRate / 2) /  SAMPLE_SIZE;

      
    }

    private void SpawnLine(){

      visualScale = new float[amnVisual];
      visualList = new Transform[amnVisual];

      for (int i = 0; i < amnVisual; i++)
      {
          GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
          visualList[i] = go.transform;
          visualList[i].position = Vector2.right * i;
      }

    }
    
    private void UpdateVisual(){

      int visualIndex = 0;
      int spectrumIndex = 0;
      int averageSize = SAMPLE_SIZE / amnVisual;

      while (visualIndex < amnVisual)
      {
          int j = 0;
          float sum = 0;

          while (j < averageSize)
          {
            sum += spectrum[spectrumIndex];
            spectrumIndex++;
            j++;             
          }

          float scaleY = sum / averageSize * visualModifier;
          visualScale[visualIndex] -= Time.deltaTime * smoothSpeed;

          if (visualScale[visualIndex] < scaleY)
          {
              visualScale[visualIndex] = scaleY;
          }

          visualList[visualIndex].localScale = Vector2.one + Vector2.up * visualScale[visualIndex];
          visualIndex++;
      }
    }
    

    private void GetMicrophone(){

        //Check if there is at least one microphone connected  
        if(Microphone.devices.Length <= 0)  
        {  
            //Throw a warning message at the console if there isn't  
            Debug.LogWarning("Microphone not connected!");  
        }  
        else //At least one microphone is present  
        {  
            //Set our flag 'micConnected' to true  
            micConnected = true;  
  
            //Get the default microphone recording capabilities  
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);  
  
            //According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...  
            if(minFreq == 0 && maxFreq == 0)  
            {  
                //...meaning 44100 Hz can be used as the recording sampling rate  
                maxFreq = 44100;  
            }  
  
            //Get the attached AudioSource component  
            goAudioSource = this.GetComponent<AudioSource>();  
        }  

         //If there is a microphone  
        if(micConnected)  
        {  
            //If the audio from any microphone isn't being captured  
            if(!Microphone.IsRecording(null))  
            {  
              //Start recording and store the audio captured from the microphone at the AudioClip in the AudioSource  
              goAudioSource.clip = Microphone.Start(null, true, 20, maxFreq);  
              goAudioSource.Play();
              goAudioSource.loop = true;
            }    
        }  
        else // No microphone  
        {  
            //Print a red "Microphone not connected!" message at the center of the screen  
            GUI.contentColor = Color.red;  
            GUI.Label(new Rect(Screen.width/2-100, Screen.height/2-25, 200, 50), "Microphone not connected!");  
        }  
  
      
    }

}
