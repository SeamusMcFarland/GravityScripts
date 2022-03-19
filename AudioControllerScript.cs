using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioControllerScript : MonoBehaviour
{

	public AudioSource sfxSource, musicSource;

	public string[] audioClipNames;
	public AudioClip[] audioClips; // should be same size as audioClipNames

	private List<bool> audioClipPlayed = new List<bool>(); // prevents audio clips from being played multiple times in a single frame (is automatically set to the size of audioClipNames)

	private float sfxVolume, musicVolume;

	// Start is called before the first frame update
	void Start()
	{
		musicVolume = 1f;
		sfxVolume = 1f;
		foreach (AudioClip ac in audioClips) // fills audioClipPlayed with same number of values as audioClipNames and audioClips
			audioClipPlayed.Add(ac); //
	}

	// Update is called once per frame
	void Update()
	{
		sfxSource.volume = sfxVolume;
		musicSource.volume = musicVolume;

		for (int i = 0; i < audioClipPlayed.Count; i++)  // empties each frame in order to re-enable the ability to play the associated sounds after the frame has passed
			audioClipPlayed[i] = false;     //
	}

	public void PlaySound(string requestedString)
	{
		bool found = false;
		for (int i = 0; i < audioClipNames.Length; i++)
		{
			if (audioClipNames[i] == requestedString && !audioClipPlayed[i])
			{
				sfxSource.PlayOneShot(audioClips[i]);
				audioClipPlayed[i] = true;
				break;
			}
			found = true;
		}
		if (!found)
			Debug.Log("ERROR! INVALID STRING FOR PLAYSOUND()!!!");
	}

	public void PlaySound(AudioSource requestedSource) // WARNING! THERE IS CURRENTLY NO PREVENTITIVE THINGS IMPLEMENTED TO PREVENT MULTIPLE SOUNDS BEING PLAYED ON THE SAME FRAME AND AUDIO SOURCE
	{
		requestedSource.volume = sfxVolume;
		requestedSource.PlayOneShot(requestedSource.clip);
	}

	public void PlaySound(string requestedString, AudioSource aSource) // allows for the utilization of a custom audio source, usually for the sake of physical distance drop off
	{
		bool found = false;
		for (int i = 0; i < audioClipNames.Length; i++)
		{
			if (audioClipNames[i] == requestedString) // Note that this does NOT check audioClipPlayed since many sounds could be played at once without being overwelming when using physical distance drop off
			{
				aSource.volume = sfxVolume;
				aSource.PlayOneShot(audioClips[i]);
				break;
			}
			found = true;
		}
		if (!found)
			Debug.Log("ERROR! INVALID STRING FOR PLAYSOUNDCUSTOM()!!!");
	}

	public void SetSFXVolume(float v)
	{
		sfxVolume = v;
	}

	public void SetMusicVolume(float v)
	{
		musicVolume = v;
	}

}

