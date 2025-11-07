using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public RiddlesProgress currentState;
    private bool[] riddlesSolved;
    //public FadeScreen fadeScreen;
    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            riddlesSolved = new bool[(int)RiddlesProgress.DoorCodeInserted + 1];
            DontDestroyOnLoad(gameObject);
            UpdateGameState(RiddlesProgress.Start);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateGameState(RiddlesProgress riddle)
    {

        int stateIndex = (int)riddle;
        riddlesSolved[stateIndex] = true;
        if ((int)riddle > (int)currentState)
        {
            currentState = riddle;
        }
        //checkForSceneChange();
    }

    public void changeScene(string sceneName)
    {
        //SceneManager.LoadScene(sceneName);
    }

    public void endGame()
    {
        //if (riddlesSolved[(int)RiddlesProgress.DoorCodeInserted] && !gameEnded)
        //{
        //    gameEnded = true;
        //    StartCoroutine(capsuleLaunch());
        //}


    }

}



public enum RiddlesProgress
{
    Start,
    PowerPlugged,
    WiresPattern,
    TrapDoorButton,
    UVLightGrabbed,
    OpenBoxSign,
    ScrewsRemoved,
    BrakeWindow,
    TurnOnEngine,
    ThreeDigitsCode,
    SwitchesRiddle,
    RocketLaunched,
    FireEstinguished,
    FrameFallen,
    FrameAttached,
    ExitPushed,
    DoorCodeInserted
}
