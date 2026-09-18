using UnityEngine;

namespace GnomeGuard
{
    public class GameSfx : MonoBehaviour
    {
        static GameSfx _instance;
        AudioSource _oneShot;
        AudioSource _music;
        AudioSource _wind;
        AudioClip _throw;
        AudioClip _throwHeavy;
        AudioClip _hit;
        AudioClip _thump;
        AudioClip _pickup;
        AudioClip _hurt;
        AudioClip _wave;
        AudioClip _over;
        AudioClip _kick;
        AudioClip _jump;
        AudioClip _step;
        AudioClip _boss;
        AudioClip _perfect;
        AudioClip _combo;

        void Awake()
        {
            _instance = this;
            _oneShot = gameObject.AddComponent<AudioSource>();
            _oneShot.playOnAwake = false;
            _oneShot.spatialBlend = 0f;

            _music = gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.loop = true;
            _music.volume = 0.18f;
            _music.clip = MakeMusic();
            _music.Play();

            _wind = gameObject.AddComponent<AudioSource>();
            _wind.playOnAwake = false;
            _wind.loop = true;
            _wind.volume = 0.07f;
            _wind.clip = MakeNoise(1.6f, 0.35f);
            _wind.Play();

            _throw = MakeTone(620f, 0.07f, 0.28f, true);
            _throwHeavy = MakeTone(280f, 0.12f, 0.4f, true);
            _hit = MakeNoise(0.09f, 0.35f);
            _thump = MakeTone(140f, 0.12f, 0.4f, false);
            _pickup = MakeArp(new[] { 523f, 659f, 784f }, 0.07f);
            _hurt = MakeTone(90f, 0.22f, 0.5f, false);
            _wave = MakeArp(new[] { 392f, 523f, 659f, 784f }, 0.09f);
            _over = MakeTone(70f, 0.45f, 0.55f, false);
            _kick = MakeTone(180f, 0.1f, 0.42f, true);
            _jump = MakeTone(340f, 0.08f, 0.22f, true);
            _step = MakeNoise(0.05f, 0.18f);
            _boss = MakeArp(new[] { 196f, 247f, 311f, 392f }, 0.12f);
            _perfect = MakeArp(new[] { 523f, 659f, 784f, 1046f }, 0.08f);
            _combo = MakeArp(new[] { 659f, 784f, 988f }, 0.06f);
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public static void Throw(bool heavy) => Play(heavy ? Clip(_instance?._throwHeavy) : Clip(_instance?._throw), heavy ? 0.6f : 0.45f);
        public static void Hit() => Play(Clip(_instance?._hit), 0.5f);
        public static void Thump() => Play(Clip(_instance?._thump), 0.55f);
        public static void Pickup() => Play(Clip(_instance?._pickup), 0.55f);
        public static void Hurt() => Play(Clip(_instance?._hurt), 0.7f);
        public static void Wave() => Play(Clip(_instance?._wave), 0.5f);
        public static void GameOver() => Play(Clip(_instance?._over), 0.8f);
        public static void Kick() => Play(Clip(_instance?._kick), 0.55f);
        public static void Jump() => Play(Clip(_instance?._jump), 0.35f);
        public static void Footstep() => Play(Clip(_instance?._step), 0.22f);
        public static void Boss() => Play(Clip(_instance?._boss), 0.7f);
        public static void Perfect() => Play(Clip(_instance?._perfect), 0.6f);
        public static void Combo() => Play(Clip(_instance?._combo), 0.45f);

        static AudioClip Clip(AudioClip clip) => clip;

        static void Play(AudioClip clip, float volume)
        {
            if (_instance == null || clip == null) return;
            _instance._oneShot.PlayOneShot(clip, volume);
        }

        static AudioClip MakeTone(float freq, float duration, float volume, bool whoosh)
        {
            int rate = 22050;
            int samples = Mathf.CeilToInt(rate * duration);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = 1f - i / (float)samples;
                float f = whoosh ? freq * (1f - t * 0.6f) : freq;
                data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * env * volume;
            }

            var clip = AudioClip.Create("tone", samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip MakeNoise(float duration, float volume)
        {
            int rate = 22050;
            int samples = Mathf.CeilToInt(rate * duration);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float env = 1f - i / (float)samples;
                data[i] = (Random.value * 2f - 1f) * env * volume;
            }

            var clip = AudioClip.Create("noise", samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip MakeArp(float[] notes, float noteLen)
        {
            int rate = 22050;
            int noteSamples = Mathf.CeilToInt(rate * noteLen);
            int samples = noteSamples * notes.Length;
            var data = new float[samples];
            int w = 0;
            foreach (float freq in notes)
            {
                for (int i = 0; i < noteSamples; i++, w++)
                {
                    float env = 1f - i / (float)noteSamples;
                    data[w] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)rate)) * env * 0.38f;
                }
            }

            var clip = AudioClip.Create("arp", samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip MakeMusic()
        {
            float[] notes =
            {
                392f, 523f, 659f, 523f,
                392f, 494f, 587f, 523f,
                349f, 523f, 659f, 784f,
                659f, 523f, 392f, 330f
            };
            int rate = 22050;
            float noteLen = 0.28f;
            int noteSamples = Mathf.CeilToInt(rate * noteLen);
            int samples = noteSamples * notes.Length;
            var data = new float[samples];
            int w = 0;
            foreach (float freq in notes)
            {
                for (int i = 0; i < noteSamples; i++, w++)
                {
                    float t = i / (float)rate;
                    float env = Mathf.Sin(Mathf.PI * (i / (float)noteSamples)) * 0.22f;
                    float sample = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f
                                   + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.18f;
                    data[w] = sample * env;
                }
            }

            var clip = AudioClip.Create("music", samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
