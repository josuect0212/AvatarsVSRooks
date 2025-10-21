using UnityEngine;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text errorText;

    /// <summary>
    /// Validates the user credentials.
    /// </summary>
    public void OnSubmitButtonPressed()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        Debug.Log("Username: " + username);
        Debug.Log("Password: " + password);


        if (ValidateCredentials(username, password))
        {
            Debug.Log("Login successful!");
            // Proceed to the next scene or functionality
            SceneManager.LoadScene("LoggedIn");
        }
        else
        {
            if (IsInputFieldEmpty(usernameInput) || IsInputFieldEmpty(passwordInput))
            {
                Debug.Log("Please fill in all fields.");
                // Show error message to the user
                errorText.text = "Error! Please fill in all fields.";
            }
            else
            {
                Debug.Log("Invalid username or password.");
                // Show error message to the user
                errorText.text = "Error! Invalid username or password.";
                OnClearButtonPressed();
            }
        }
    }

    /// <summary>
    /// Validates the provided username and password.
    /// </summary>
    private bool ValidateCredentials(string username, string password)
    {
        // Simple validation logic (for demonstration purposes)
        return username == "admin" && password == "password123";
    }

    /// <summary>
    /// Checks if the input field is empty or contains only whitespace.
    /// </summary>
    private bool IsInputFieldEmpty(TMP_InputField inputField)
    {
        return string.IsNullOrWhiteSpace(inputField.text);
    }

    /// <summary>
    /// Clears the input fields.
    /// </summary>
    public void OnClearButtonPressed()
    {
        usernameInput.text = "";
        passwordInput.text = "";
    }
}
