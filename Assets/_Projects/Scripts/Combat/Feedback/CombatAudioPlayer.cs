using UnityEngine;

namespace Expedition33.Combat
{
    [RequireComponent(typeof(AudioSource))]
    public class CombatAudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        private AudioClip _hitClip;
        private AudioClip _turnStartClip;
        private AudioClip _buttonClickClip;
        private AudioClip _victoryClip;
        private AudioClip _defeatClip;

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            _audioSource.playOnAwake = false;
            GenerateProceduralClips();
        }

        public void PlayHit()
        {
            PlayClip(_hitClip, 0.9f);
        }

        public void PlayTurnStart()
        {
            PlayClip(_turnStartClip, 0.6f);
        }

        public void PlayButtonClick()
        {
            PlayClip(_buttonClickClip, 0.5f);
        }

        public void PlayVictory()
        {
            PlayClip(_victoryClip, 0.8f);
        }

        public void PlayDefeat()
        {
            PlayClip(_defeatClip, 0.8f);
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip, volume);
            }
        }

        private void GenerateProceduralClips()
        {
            _hitClip = CreateTone(140f, 0.12f, 0.8f, true);
            _turnStartClip = CreateDualTone(440f, 660f, 0.15f);
            _buttonClickClip = CreateTone(800f, 0.05f, 0.3f, false);
            _victoryClip = CreateMelody(new[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.12f);
            _defeatClip = CreateMelody(new[] { 392.00f, 349.23f, 329.63f, 261.63f }, 0.22f);
        }

        private AudioClip CreateTone(float frequency, float duration, float volume, bool noiseDecay)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float decay = 1f - ((float)i / sampleCount);
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);

                if (noiseDecay)
                {
                    float noise = (Random.value * 2f - 1f) * 0.3f;
                    wave = (wave + noise) * 0.8f;
                }

                samples[i] = wave * decay * volume;
            }

            AudioClip clip = AudioClip.Create("ProceduralTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateDualTone(float freq1, float freq2, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float decay = 1f - ((float)i / sampleCount);
                float wave1 = Mathf.Sin(2f * Mathf.PI * freq1 * t);
                float wave2 = Mathf.Sin(2f * Mathf.PI * freq2 * t);
                samples[i] = (wave1 + wave2) * 0.5f * decay * 0.6f;
            }

            AudioClip clip = AudioClip.Create("DualTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateMelody(float[] notes, float noteDuration)
        {
            int sampleRate = 44100;
            int noteSamples = Mathf.CeilToInt(sampleRate * noteDuration);
            int totalSamples = noteSamples * notes.Length;
            float[] samples = new float[totalSamples];

            for (int noteIdx = 0; noteIdx < notes.Length; noteIdx++)
            {
                float freq = notes[noteIdx];
                int offset = noteIdx * noteSamples;

                for (int i = 0; i < noteSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float decay = 1f - ((float)i / noteSamples);
                    float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                    samples[offset + i] = wave * decay * 0.5f;
                }
            }

            AudioClip clip = AudioClip.Create("Melody", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
