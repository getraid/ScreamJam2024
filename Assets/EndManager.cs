using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndManager : MonoBehaviour
{
    
    public static void ReplayGame() 
    {
        SceneManager.LoadScene(0);
    }


}
