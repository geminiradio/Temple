using UnityEngine;
using System.Collections;



public class RandomizeSFX : MonoBehaviour {

    public AudioSource audioSource;
    public AudioClip[] clips;  
    public float[] clips_randomWeights;
    public MinMaxValues[] clips_randPitchRanges;
    public float defaultVolume = 1f; // this is the default volume if no volume is passed in externally
    public float dontRetriggerTime = 0.15f;  // how many seconds must pass after SFX before another SFX will play
    public float dontTriggerUntil = 2f;  // how many second must pass after the game starts before SFX are allowed to play

    private float lastTriggerTime; 

    // intended to be called by external classes
    public bool PlaySFX ()
    {
        // use default volume if no volume is passed in
        return PlaySFX(defaultVolume);
    }

    public bool PlaySFX (float vol)
    {
        if ((Time.time - lastTriggerTime) < dontRetriggerTime)
            return false;

        lastTriggerTime = Time.time;

        int index = CodeTools.WeightedRandomSelection(clips_randomWeights);

        audioSource.pitch = Random.Range(clips_randPitchRanges[index].min, clips_randPitchRanges[index].max);

        audioSource.clip = clips[index];  // this stricly shouldn't be necessary

        audioSource.PlayOneShot(clips[index], vol);

        return true;
    }

    void Start() {

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            Debug.LogError("No AudioSource assigned or detected.");

        if (clips.Length == 0)
            Debug.LogError("No AudioClips assigned.");

        if (
            (clips.Length != clips_randomWeights.Length) ||
            (clips.Length != clips_randPitchRanges.Length) ||
            (clips_randomWeights.Length != clips_randPitchRanges.Length) 
            )
            Debug.LogError("Arrays whose names start with the word 'clips' are parallel arrays and must have the name number of elements.");


        CodeTools.ValidateWeightedRandomArray(clips_randomWeights, clips.Length);

        MinMaxValues minmax = new MinMaxValues();
        minmax.min = -3f;
        minmax.max = 3f;

        CodeTools.ValidateMinMaxArray(clips_randPitchRanges, minmax, minmax);

        lastTriggerTime = dontTriggerUntil;  // this is how dontTriggerUntil is implemented

    }
	


}
