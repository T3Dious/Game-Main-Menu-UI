using UnityEngine;
using System.IO;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] UIDocument uIDocument;
    private VisualElement rootElement;
    // public string saveFileName = "gamestate.json";
    GameManager gameManager;

    void Start()
    {
        rootElement = uIDocument.rootVisualElement;
        var playButton = rootElement.Q<Button>("PlayButton");
        var resumeButton = rootElement.Q<Button>("ResumeButton");
        var exitButton = rootElement.Q<Button>("ExitButton");
        var optionsButton = rootElement.Q<Button>("OptionsButton");
        playButton.clicked += OnPlayButtonClicked;
        resumeButton.clicked += OnResumeButtonClicked;
        exitButton.clicked += OnExitButtonClicked;
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked. Starting game...");
        // Load the game scene or initialize the game state
        StartNewGame();
    }

    private void OnResumeButtonClicked()
    {
        Debug.Log("Resume button clicked. Resuming game...");
        // Load the saved game state
        GameState loadedState = Resume();
        // Use loadedState to continue the game
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked. Quitting game...");
        Time.timeScale = 1f; // Ensure time scale is normal before quitting
        Application.Quit();
    }

    void StartNewGame()
    {
        // Get fresh instances of PlayerStats and WeaponData
        PlayerStats playerStats = new PlayerStats(); // Or use your initialization method
        WeaponData weaponData = new WeaponData();    // Or use your initialization method

        // Create a dictionary to hold all data
        var saveData = new Dictionary<string, object>();

        // Serialize fresh PlayerStats
        string playerStatsJson = JsonConvert.SerializeObject(playerStats, Formatting.Indented);
        var playerStatsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(playerStatsJson);
        saveData["PlayerStats"] = playerStatsDict;

        // Serialize fresh WeaponData
        string weaponDataJson = JsonConvert.SerializeObject(weaponData, Formatting.Indented);
        var weaponDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(weaponDataJson);
        saveData["WeaponData"] = weaponDataDict;

        // Determine save path
        string path = gameManager != null
            ? Path.Combine(Application.persistentDataPath, gameManager.fileName)
            : Path.Combine(Application.persistentDataPath, "gamestate.json");

        // Write fresh data (will overwrite any existing file)
        File.WriteAllText(path, JsonConvert.SerializeObject(saveData, Formatting.Indented));
        Debug.Log("New game state created at: " + path);

        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
    }

    GameState Resume()
    {
        string path = Path.Combine(Application.persistentDataPath, gameManager.fileName);
        var resumeButton = rootElement.Q<Button>("ResumeButton");
        if (File.Exists(path))
        {
            resumeButton.style.display = DisplayStyle.Flex;
            string json = File.ReadAllText(path);
            GameState loadedState = JsonConvert.DeserializeObject<GameState>(json);
            // Apply loadedState to your game as needed
            // Move to the next scene or update game state here
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
            return loadedState;
        }
        else
        {
            resumeButton.style.display = DisplayStyle.None;
            Debug.LogWarning("No saved game state found. Starting new game.");
            return new GameState(); // Return default state if no save exists
        }
    }

    void OnEnable()
    {
        if (uIDocument == null)
        {
            Debug.LogError("MainMenuUI: UIDocument is not assigned in the Inspector!");
            return;
        }

        string path;
        if (gameManager != null)
            path = Path.Combine(Application.persistentDataPath, gameManager.fileName);
        else
            path = Path.Combine(Application.persistentDataPath, "gamestate.json");
        var resumeButton = uIDocument.rootVisualElement.Q<Button>("ResumeButton");
        resumeButton.style.display = File.Exists(path) ? DisplayStyle.Flex : DisplayStyle.None;
    }


}

[System.Serializable]
public class GameState
{
    public int level;
    public int score;
    public List<string> inventory;

    public GameState()
    {
        level = 1; // Default level
        score = 0; // Default score
        inventory = new List<string>(); // Default empty inventory
    }
}

