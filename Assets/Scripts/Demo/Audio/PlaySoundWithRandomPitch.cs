using System;
using UnityEngine;

public class PlaySoundWithRandomPitch : MonoBehaviour
{
    [SerializeField] private AudioSource _audio;
    [SerializeField] private RandomPitchRange _randomPitchRange;

    public void Play()
    {
        _audio.pitch = UnityEngine.Random.Range(_randomPitchRange.Min, _randomPitchRange.Max);
        _audio.Play();
    }
}

[Serializable]
public struct RandomPitchRange
{
    public float Min;
    public float Max;
}