using UnityEngine;
using XFlow.P2P;

public static class Config
{
    public static readonly string DEFAULT_ROOM_A = "sandbox_";
    public static readonly string DEFAULT_ROOM_B = P2P.GetDevRoom();
      
#if UNITY_EDITOR //unity editor domain reload feature
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void _reset()
    {
        ROOM_A = DEFAULT_ROOM_A;
        ROOM_B = DEFAULT_ROOM_B;
    }
#endif

    
#if CLIENT
    public static string TMP_HASHES_PATH = "../tmp";
#else
    public static string TMP_HASHES_PATH = "../../tmp";
#endif
    
    public static string ROOM_A = DEFAULT_ROOM_A;
    public static string ROOM_B = DEFAULT_ROOM_B;


    //public static string url = $"wss://dev1.ecs.fbpub.net/XsZubnMOTHC0JRDTS95S/{ROOM}";
    //public static string url = $"ws://localhost:9096/XsZubnMOTHC0JRDTS95S/{ROOM}";
    public static string URL => $"ws://dev1.ecs.fbpub.net:9096/XsZubnMOTHC0JRDTS95S/{ROOM_A}{ROOM_B}";
}
