using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class _SceneController : MonoBehaviour
{
    public void GoScene(string nameScene){
        SceneManager.LoadScene(nameScene);
    }

}
