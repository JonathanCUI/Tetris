using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

    public Text GameMessageText;

    //singlton
    private static UIManager _instance;

    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    void Awake()
    {
        _instance = this;
    }

    public static UIManager Instance
    {
        get
        {
            return _instance;
        }
    }
}
