// /////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audio Manager.
//
// This code is release under the MIT licence. It is provided as-is and without any warranty.
//
// Developed by Daniel Rodríguez (Seth Illgard) in April 2010
// http://www.silentkraken.com
//
// /////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[System.Serializable]
public class Sound
{
    public AudioClip clip;
    public float fPitchAjust = 0.0f;
    public float fVolumeAdjust = 0.0f;
    public bool  bLoop = false;
    public float fDopplerLevel = 0.0f;
    public float fMaxDistance = 100.0f;
	
    public void Apply( AudioSource audio )
    {
        if(audio.clip==clip)return;
		if(audio.isPlaying)audio.Stop();
		audio.clip=clip;
		audio.loop=bLoop;
		audio.dopplerLevel=fDopplerLevel;
		if(fMaxDistance==0.0) fMaxDistance=100;
		audio.maxDistance=fMaxDistance;
		audio.volume=1+fVolumeAdjust;
		audio.pitch=1+fPitchAjust;
		audio.rolloffMode = AudioRolloffMode.Linear;
		audio.Play();
    }
	
	public void setPitch( float amount, AudioSource audio ){
		audio.pitch=amount + fPitchAjust;
	}
	
	public void setVolume( float amount , AudioSource audio){
		audio.volume=amount + fVolumeAdjust;
	}
}

public class AudioCtr : MonoBehaviour
{
	public static float BGM_VALUME = 0.1f;
	
    public static AudioSource Play(AudioClip clip, Transform emitter)
    {
         return Play(clip, emitter, 1f, 1f);
    }

    public static AudioSource Play(AudioClip clip, Transform emitter, float volume)
    {
         return Play(clip, emitter, volume, 1f);
    }

    /// <summary>
    /// Plays a sound by creating an empty game object with an AudioSource
    /// and attaching it to the given transform (so it moves with the transform). Destroys it after it finished playing.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="emitter"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <returns></returns>
    public static AudioSource Play(AudioClip clip, Transform emitter, float volume, float pitch)
    {
        //Create an empty game object
        GameObject go = new GameObject("Audio: " + clip.name);
        go.transform.position = emitter.position;
        go.transform.parent = emitter;

        //Create the source
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;        
        source.pitch = pitch;
        source.Play();
        Destroy(go, clip.length);
        return source;
    }
    public static AudioSource Play(AudioClip clip)
    {
        return Play(clip, Vector3.zero, 1f, 1f, false);
    }

    public static AudioSource Play(AudioClip clip, float volume)
    {
        return Play(clip, Vector3.zero, volume, 1f, false);
    }

    public static AudioSource Play(AudioClip clip, float volume, bool loop)
    {
        return Play(clip, Vector3.zero, volume, 1f, loop);
    }
    public static AudioSource Play(AudioClip clip, bool loop)
    {
        return Play(clip, Vector3.zero, 1f, 1f, loop);
    }

    public static AudioSource Play(AudioClip clip, Vector3 point)
    {
         return Play(clip, point, 1f, 1f, false);
    }

    public static AudioSource Play(AudioClip clip, Vector3 point, float volume)
    {
        return Play(clip, point, volume, 1f, false);
    }

    /// <summary>
    /// Plays a sound at the given point in space by creating an empty game object with an AudioSource
    /// in that place and destroys it after it finished playing.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="point"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <returns></returns>
    public static AudioSource Play(AudioClip clip, Vector3 point, float volume, float pitch, bool loop)
    {
        // 중복 재생 방지
        //if( isPlaying( clip ) ) return null;

        //Debug.Log("Audio Play : " + clip.name);
        //Create an empty game object
        GameObject go = new GameObject("Audio: " + clip.name);
        go.transform.position = point;

        //Create the source
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.Play();

        if( !loop )
            Destroy(go, clip.length);

        return source;
    }

    public static bool Stop(AudioClip clip)
    {
        //Debug.Log("Audio Stop : " + clip.name);
        GameObject go = GameObject.Find("Audio: " + clip.name);
        if (go)
        {
            Destroy(go);
            return true;
        }
        else
            return false;
    }

    public static bool isPlaying(AudioClip clip)
    {
       try
       {
           return GameObject.Find("Audio: " + clip.name).GetComponent<AudioSource>().isPlaying;
       } catch
       {
           return false;
       }
    }

    // bgm    
    public static AudioClip[] snd_bgm = { (AudioClip)(Resources.Load("sound/BGM/1_title")),
                                   (AudioClip)(Resources.Load("sound/BGM/2_TRACK1_BGM")),
                                   (AudioClip)(Resources.Load("sound/BGM/3_TRACK2_BGM")),
                                   (AudioClip)(Resources.Load("sound/BGM/4_TRACK3_BGM")),
                                   (AudioClip)(Resources.Load("sound/BGM/5_SELECT_RACING_BGM")),
                                   (AudioClip)(Resources.Load("sound/BGM/6_RACING_MENU_BGM")),
                                   (AudioClip)(Resources.Load("sound/BGM/ResultWnd"))                                   
                                 };
    // bike
    public static AudioClip[] snd_pedal = { (AudioClip)(Resources.Load("sound/bike/6_PEDAL_01")),
                                            (AudioClip)(Resources.Load("sound/bike/6_PEDAL_02")),
                                            (AudioClip)(Resources.Load("sound/bike/6_PEDAL_03")),
                                            (AudioClip)(Resources.Load("sound/bike/6_PEDAL_04"))
                                          };
    public static AudioClip[] snd_chain = { (AudioClip)(Resources.Load("sound/bike/7_CHAIN_01")),
                                            (AudioClip)(Resources.Load("sound/bike/7_CHAIN_02")),
                                            (AudioClip)(Resources.Load("sound/bike/7_CHAIN_03"))                                            
                                          };



    public static AudioClip[] snd_landing = { (AudioClip)(Resources.Load("sound/bike/8_LANDING_01")),
                                            (AudioClip)(Resources.Load("sound/bike/8_LANDING_02")),
                                            (AudioClip)(Resources.Load("sound/bike/8_LANDING_03"))                                            
                                          };
    public static AudioClip[] snd_crash_voice = { (AudioClip)(Resources.Load("sound/bike/10_CRASH_VOICE_01")),
                                            (AudioClip)(Resources.Load("sound/bike/10_CRASH_VOICE_02")),
                                            (AudioClip)(Resources.Load("sound/bike/10_CRASH_VOICE_03")),
                                            (AudioClip)(Resources.Load("sound/bike/10_CRASH_VOICE_04")),
                                            (AudioClip)(Resources.Load("sound/bike/10_CRASH_VOICE_05")),
                                            (AudioClip)(Resources.Load("sound/bike/10_CRASH_VOICE_06"))
                                          };
    public static AudioClip[] snd_crash = { (AudioClip)(Resources.Load("sound/bike/11_CRASH_01")),
                                            (AudioClip)(Resources.Load("sound/bike/11_CRASH_02")),
                                            (AudioClip)(Resources.Load("sound/bike/11_CRASH_03"))                                            
                                          };
    public static AudioClip[] snd_fall = { (AudioClip)(Resources.Load("sound/bike/12_FALL_01")),
                                            (AudioClip)(Resources.Load("sound/bike/12_FALL_02"))
                                          };
    public static AudioClip[] snd_break = { (AudioClip)(Resources.Load("sound/bike/16_BREAK_01")),
                                            (AudioClip)(Resources.Load("sound/bike/16_BREAK_02")),
                                            (AudioClip)(Resources.Load("sound/bike/16_BREAK_03")),
                                            (AudioClip)(Resources.Load("sound/bike/16_BREAK_04")),
                                            (AudioClip)(Resources.Load("sound/bike/16_BREAK_05"))
                                          };
    public static AudioClip[] snd_drift = { (AudioClip)(Resources.Load("sound/bike/17_DRIFT_01")),
                                              (AudioClip)(Resources.Load("sound/bike/17_DRIFT_02")),
                                              (AudioClip)(Resources.Load("sound/bike/17_DRIFT_03"))
                                          };
    public static AudioClip[] snd_jump = { (AudioClip)(Resources.Load("sound/bike/29_JUMP_01")),
                                             (AudioClip)(Resources.Load("sound/bike/29_JUMP_02")),
                                             (AudioClip)(Resources.Load("sound/bike/29_JUMP_03"))
                                          };
    public static AudioClip[] snd_gravel = { (AudioClip)(Resources.Load("sound/bike/30_GRAVEL_01")),
                                               (AudioClip)(Resources.Load("sound/bike/30_GRAVEL_02")),
                                               (AudioClip)(Resources.Load("sound/bike/30_GRAVEL_03"))
                                          };
    public static AudioClip[] snd_yahoo = { (AudioClip)(Resources.Load("sound/bike/32_YAHOO_01")),
                                              (AudioClip)(Resources.Load("sound/bike/32_YAHOO_02"))
                                          };

    // effect_2d    
    public static AudioClip[] snd_count = { (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_01")),                                              
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_02")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_03")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_04")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_05")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_06")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_07")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_08")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_09")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_10")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_11")),
                                              (AudioClip)(Resources.Load("sound/effect_2d/9_COUNTE_12"))
                                          };
    public static AudioClip[] snd_loadout_alarm = { (AudioClip)(Resources.Load("sound/effect_2d/13_LOAD_OUT_ALARM_01")),
                                                      (AudioClip)(Resources.Load("sound/effect_2d/13_LOAD_OUT_ALARM_02")),
                                                      (AudioClip)(Resources.Load("sound/effect_2d/13_LOAD_OUT_ALARM_03"))
                                                    };
    public static AudioClip[] snd_regen_alarm = { (AudioClip)(Resources.Load("sound/effect_2d/14_REGEN_ALARM_01")),
                                                    (AudioClip)(Resources.Load("sound/effect_2d/14_REGEN_ALARM_02")),
                                                    (AudioClip)(Resources.Load("sound/effect_2d/14_REGEN_ALARM_03"))
                                          };
    public static AudioClip[] snd_sign_alarm = { (AudioClip)(Resources.Load("sound/effect_2d/15_SIGN_ALARM_01")),
                                                   (AudioClip)(Resources.Load("sound/effect_2d/15_SIGN_ALARM_02")),
                                                   (AudioClip)(Resources.Load("sound/effect_2d/15_SIGN_ALARM_03"))
                                          };
    public static AudioClip[] snd_gameover = { (AudioClip)(Resources.Load("sound/effect_2d/19_GAME_OVER_01")),
                                                 (AudioClip)(Resources.Load("sound/effect_2d/19_GAME_OVER_02")),
                                                 (AudioClip)(Resources.Load("sound/effect_2d/19_GAME_OVER_03"))
                                          };
    public static AudioClip[] snd_finish = { (AudioClip)(Resources.Load("sound/effect_2d/20_FINISH01"))
                                          };
    public static AudioClip[] snd_retire = { (AudioClip)(Resources.Load("sound/effect_2d/21_RETIRE_01")),
                                               (AudioClip)(Resources.Load("sound/effect_2d/21_RETIRE_02"))
                                          };
    public static AudioClip[] snd_checkoint = { (AudioClip)(Resources.Load("sound/effect_2d/22_CHECK POINT_01"))
                                          };
    public static AudioClip[] snd_time_alarm = { (AudioClip)(Resources.Load("sound/effect_2d/26_TIME_ALARM_01")),
                                                   (AudioClip)(Resources.Load("sound/effect_2d/26_TIME_ALARM_02"))
                                          };
    public static AudioClip[] snd_hurryup = { (AudioClip)(Resources.Load("sound/effect_2d/27_HURRY_UP_01")),
                                                 (AudioClip)(Resources.Load("sound/effect_2d/27_HURRY_UP_02")),
                                                 (AudioClip)(Resources.Load("sound/effect_2d/27_HURRY_UP_03"))
                                          };
    public static AudioClip[] snd_ranknum = { (AudioClip)(Resources.Load("sound/effect_2d/31_RANK_NUM_01")),
                                                (AudioClip)(Resources.Load("sound/effect_2d/31_RANK_NUM_02"))
                                          };
    public static AudioClip snd_flash = (AudioClip)(Resources.Load("sound/effect_2d/flash"));

    // gui
    public static AudioClip[] snd_coin = { (AudioClip)(Resources.Load("sound/gui/1_INSERT COIN_01")),
                                             (AudioClip)(Resources.Load("sound/gui/1_INSERT COIN_02")),
                                             (AudioClip)(Resources.Load("sound/gui/1_INSERT COIN_03"))
                                          };
    public static AudioClip[] snd_bt_start = { (AudioClip)(Resources.Load("sound/gui/2_START_BUTTON_01")),
                                                 (AudioClip)(Resources.Load("sound/gui/2_START_BUTTON_02")),
                                                 (AudioClip)(Resources.Load("sound/gui/2_START_BUTTON_03"))
                                          };
    public static AudioClip[] snd_bt_move = { (AudioClip)(Resources.Load("sound/gui/3_MENU_MOVE_01")),
                                                (AudioClip)(Resources.Load("sound/gui/3_MENU_MOVE_02")),
                                                (AudioClip)(Resources.Load("sound/gui/3_MENU_MOVE_03")),
                                                (AudioClip)(Resources.Load("sound/gui/3_MENU_MOVE_04")),
                                                (AudioClip)(Resources.Load("sound/gui/3_MENU_MOVE_05"))
                                          };
    public static AudioClip[] snd_bt_select = { (AudioClip)(Resources.Load("sound/gui/4_MENU_SELECT_01")),
                                                  (AudioClip)(Resources.Load("sound/gui/4_MENU_SELECT_02")),
                                                  (AudioClip)(Resources.Load("sound/gui/4_MENU_SELECT_03"))
                                          };
    public static AudioClip[] snd_bt_cancle = { (AudioClip)(Resources.Load("sound/gui/5_CANCLE_BUTTON_01")),
                                                  (AudioClip)(Resources.Load("sound/gui/5_CANCLE_BUTTON_02")),
                                                  (AudioClip)(Resources.Load("sound/gui/5_CANCLE_BUTTON_03"))
                                          };

    
}