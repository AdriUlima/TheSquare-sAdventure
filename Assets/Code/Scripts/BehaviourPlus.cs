using UnityEngine;

public static class BehaviourPlus
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        gameManager = null;
        inputHandler = null;
    }

#pragma warning disable IDE1006 // Estilos de nombres
    public static GameManager gameManager { get; private set; }
    public static InputHandler inputHandler { get; private set; }
#pragma warning restore IDE1006 // Estilos de nombres

    public static void SetGameManager(GameManager gm) => gameManager = gm;
    public static void SetInputHandler(InputHandler ih) => inputHandler = ih;
}