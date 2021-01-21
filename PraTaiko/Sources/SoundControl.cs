using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PraTaiko
{
    class SoundControl
    {
        static SoundControl SC;
        const int MAX = 100;
        const int MIN = 0;
        const int DISPLACEMENT = 5;

        public int VolumePer = MAX;
        public bool Mute { get; private set; } = false;
        List<Sound> Sounds;

        public static bool KeyVolumeMute
        {
            get
            {
                return Key.GetCount(Key.INPUT_T) == 1;
            }
        }
        public static bool KeyVolumeUp
        {
            get
            {
                return Key.GetCount(Key.R) % 10 == 1;
            }
        }
        public static bool KeyVolumeDown
        {
            get
            {
                return Key.GetCount(Key.E) % 10 == 1;
            }
        }

        public string State
        {
            get
            {
                switch (Mute)
                {
                    case false:
                        return VolumePer.ToString();
                    default:
                        return "Mute";
                }
            }
        }
        public int Volume { get; private set; }

        void SetVolume()
        {
            switch (Mute)
            {
                case false:
                    Volume = 255 * VolumePer / 100;
                    break;
                case true:
                    Volume = 0;
                    break;
            }
        }

        void VolumePerMute()
        {
            Mute = !Mute;
            SetVolume();
            Sounds.ForEach(s => s.ChangeVolume(Volume));
        }
        void VolumePerUp()
        {
            Mute = false;
            VolumePer += DISPLACEMENT;
            if (VolumePer > MAX)
            {
                VolumePer = MAX;
            }
            SetVolume();
            Sounds.ForEach(s => s.ChangeVolume(Volume));
        }
        void VolumePerDown()
        {
            Mute = false;
            VolumePer -= DISPLACEMENT;
            if (VolumePer < MIN)
            {
                VolumePer = MIN;
            }
            SetVolume();
            Sounds.ForEach(s => s.ChangeVolume(Volume));
        }
        
        public static void VolumeMute()
        {
            SC.VolumePerMute();
        }
        public static void VolumeUp()
        {
            SC.VolumePerUp();
        }
        public static void VolumeDown()
        {
            SC.VolumePerDown();
        }

        public void Add(Sound s)
        {
            Sounds.Add(s);
        }
        public static SoundControl Get()
        {
            return SC;
        }
        public static void Construct()
        {
            SC = new SoundControl();
        }
        SoundControl()
        {
            Sounds = new List<Sound>();
            SetVolume();
        }
    }
}
