using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DxLibDLL;
using static DxLibDLL.DX;

namespace PraTaiko
{
    public class Sound
    {
        static SoundControl SC;
        public static int TYPE_BACK = DX_PLAYTYPE_BACK;
        public int Handle { get; private set; }
        public int TopPositionFlag { get; private set; } = 1;
        public void SetTopPositionFlag(int f)
        {
            TopPositionFlag = f;
        }
        public void SetHandle(int handle, string fp)
        {
            DeleteSoundMem(Handle);

            filePath = fp;
            Handle = handle;
            ChangeVolumeSoundMem(SC.Volume, Handle);
        }
        public string filePath { get; private set; }
        public int PlayType { get; private set; }
        public void Play()
        {
            PlaySoundMem(Handle, PlayType, TopPositionFlag);
        }
        public int CheckNow()
        {
            return CheckSoundMem(Handle);
        }
        public void ChangeVolume(int value)
        {
            ChangeVolumeSoundMem(value, Handle);
        }
        public void SetCurrentTime(int time)
        {
            SetSoundCurrentTime(time, Handle);
        }
        static Sound()
        {
            SC = SoundControl.Get();
        } 
        public Sound(string fp)
        {
            filePath = fp;
            Handle = LoadSoundMem(fp);
            ChangeVolumeSoundMem(SC.Volume, Handle);
            PlayType = TYPE_BACK;
            SC.Add(this);
        }        
        ~Sound()
        {
            DeleteSoundMem(Handle);
        }
    }
}
