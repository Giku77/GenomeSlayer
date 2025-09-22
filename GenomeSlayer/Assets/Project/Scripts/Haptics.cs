
using UnityEngine;

public static class Haptics
{
#if UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaObject GetVibrator()
    {
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using var context = activity.Call<AndroidJavaObject>("getApplicationContext");
        return context.Call<AndroidJavaObject>("getSystemService", "vibrator");
    }
#endif

    public static void Light() { Vibrate(20, 50); }   // 20ms, 강도 50/255
    public static void Medium() { Vibrate(35, 150); }
    public static void Heavy() { Vibrate(50, 255); }

    public static void Vibrate(long millis, int amplitude = 255)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            var vibrator = GetVibrator();
            if (vibrator == null) return;

            using var veClass = new AndroidJavaClass("android.os.VibrationEffect");
            AndroidJavaObject effect;

            int sdk = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
            if (sdk >= 26)
                effect = veClass.CallStatic<AndroidJavaObject>("createOneShot", millis, amplitude);
            else
                effect = null;

            if (effect != null)
                vibrator.Call("vibrate", effect);
            else
                vibrator.Call("vibrate", millis);
        }
        catch { /* no-op */ }
#else
        // 에디터/기타 플랫폼: 무시
#endif
    }
}
