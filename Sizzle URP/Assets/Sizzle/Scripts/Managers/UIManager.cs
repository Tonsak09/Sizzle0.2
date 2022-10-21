using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{

    [SerializeField] KeyCode menuKey;
    [SerializeField] float speed;
    [SerializeField] GameObject coloredBackground;
    [SerializeField] TextMeshProUGUI currentDisplay;
    [SerializeField] GameObject currentScreen;

    private RectTransform rt;
    private GameManager gm;
    private DialogueManager dm;

    private float sensitivity;
    private float volume;

    // Whether or not the screen is moving
    private bool moving;
    private Vector2 offScreen { get { return onScreen + Vector2.up * rt.rect.height; } }
    private Vector2 onScreen { get { return Vector2.zero; } }
    

    private void Awake()
    {
        // Get necessary references 
        rt = this.GetComponent<RectTransform>();
        gm = GameObject.FindObjectOfType<GameManager>();
        dm = GameObject.FindObjectOfType<DialogueManager>();

    }

    private void Start()
    {
        // Set position to proper position
        rt.anchoredPosition = offScreen;
        coloredBackground.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Switches menu from on to off screen
        if(Input.GetKeyDown(menuKey) && !moving)
        {
            if(rt.anchoredPosition == offScreen)
            {
                StartCoroutine(Appear());
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Continue();
            }
        }
    }

    /// <summary>
    /// Continue button that returns to game 
    /// </summary>
    public void Continue()
    {
        if (rt.anchoredPosition != offScreen)
        {
            Time.timeScale = 1;
            StartCoroutine(Dissapear());
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void LoadLevel(int index)
    {
        LevelManager.LoadScene(index);
    }

    /// <summary>
    /// Changes to a new screen that is passed 
    /// </summary>
    /// <param name="screen"></param>
    public void LoadScreen(GameObject screen)
    {
        currentScreen.SetActive(false);

        currentScreen = screen;
        currentDisplay.text = screen.name;

        currentScreen.SetActive(true);
    }

    /// <summary>
    /// Function that closes the game 
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slider"></param>
    public void UpdateSliderValue(Slider slider)
    {
        //output.text = ((int)slider.value).ToString();
        float value = slider.value;
        print(value);

        Transform parent = slider.transform.parent;    

        // Get what is going to display the new value 
        TextMeshProUGUI display = parent.GetComponentInChildren<TextMeshProUGUI>();
        display.text = ((int)Mathf.Clamp(value * 10, 1, 10)).ToString();

        StoreSliderValueToSave(parent.gameObject.name, value);
    }

    /// <summary>
    /// Passes specific value, based on key, to the game manager 
    /// </summary>
    /// <param name="key">The name of the data you want to store</param>
    /// <param name="value">The actual value you want to store</param>
    private void StoreSliderValueToSave(string key, float value)
    {
        switch(key.ToLower())
        {
            case "sensitivity":

                sensitivity = (int)Mathf.Clamp(value * 10, 1, 10); ;

                // Changes to 0 - 1 range 
                gm.CamSensitivity = sensitivity / 10.0f; 
                break;
            case "volume":
                gm.Volume = value;

                break;
            default:
                Debug.LogError($"{key} is an invalid value");
                break;
        }

        gm.UpdateValues();
    }

    /// <summary>
    /// Slides the UI to the middle of the screen 
    /// </summary>
    /// <returns></returns>
    private IEnumerator Appear()
    {
        moving = true;
        float lerp = 0; 
        
        while(lerp <= 1)
        {
            lerp += speed * Time.deltaTime;

            rt.anchoredPosition = Vector3.Lerp(offScreen, onScreen, lerp);

            yield return null;
        }

        moving = false;
        Time.timeScale = 0;
    }

    /// <summary>
    /// Slides the UI off the screen 
    /// </summary>
    /// <returns></returns>
    private IEnumerator Dissapear()
    {
        moving = true;
        float lerp = 0;

        while (lerp <= 1)
        {
            lerp += speed * Time.deltaTime;

            rt.anchoredPosition = Vector3.Lerp(onScreen, offScreen, lerp);

            yield return null;
        }

        moving = false;
    }
}