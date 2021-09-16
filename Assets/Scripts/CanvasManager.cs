using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CanvasManager controls the activation and deactivation of Canvases to maintain the logical flow of
/// the login system.
/// </summary>
public class CanvasManager : MonoBehaviour
{

    #region Serialized Private Fields
    [SerializeField]
    [Tooltip("The Canvas containing the login UI.")]
    public GameObject LoginCanvas;

    [SerializeField]
    [Tooltip("The Canvas containing the account creation UI.")]
    public GameObject CreateAccountCanvas;

    [SerializeField]
    [Tooltip("The Canvas containing the account information, post-login.")]
    public GameObject AccountViewerCanvas;
    #endregion

    #region True Private Fields
    private Text _loginErrorLabel;
    private Text _createAccountErrorLabel;
    private Text _accountViewerInformationLabel;
    #endregion

    #region MonoBehaviour Callbacks

    // Start is called before the first frame update
    void Start()
    {

        //need to get Text label components for later modification on first initializing the CanvasManager
        _loginErrorLabel = LoginCanvas.transform
            .Find("LoginErrorLabel")
            .GetComponent<Text>();
        _createAccountErrorLabel = CreateAccountCanvas.transform
            .Find("CreateAccountErrorLabel")
            .GetComponent<Text>();
        _accountViewerInformationLabel = AccountViewerCanvas.transform
            .Find("AccountViewerInformationLabel")
            .GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region UI OnClick Functions

    public void CreateAccountButtonOnClick()
    {
        LoginCanvas.SetActive(false);
        CreateAccountCanvas.SetActive(true);
    }

    public void BackButtonOnClick()
    {
        CreateAccountCanvas.SetActive(false);
        LoginCanvas.SetActive(true);
    }

    #endregion

    #region Canvas Switcher
    // On callback from async methods, these will be called.
    /// <summary>
    /// Changes the active Canvas on the main scene to be the AccountViewer. Intended to be used in successful
    /// account creation callback.
    /// </summary>
    public void CallbackFromAccountCreation(string userEmail, string userId, bool isLoggedIn)
    {
        Debug.Log("CallbackFromAccountCreation called.");
        LoginCanvas.SetActive(false);
        AccountViewerCanvas.SetActive(true);

        _accountViewerInformationLabel.text =
            "Username / Email: " + userEmail + "\n" +
            "UserId: " + userId + "\n" +
            "Logged in? " + isLoggedIn + "\n";
        CreateAccountCanvas.SetActive(false);
        Debug.Log("Reached the end of CallbackFromAccountCreation?");
    }

    /// <summary>
    /// Changes the active Canvas on the main scene to be the AccountViewerCanvas and fills the information box
    /// on the AccountViewer Canvas. Intended to be used in successful account login callback.
    /// </summary>
    public void CallbackFromLogin(string userEmail, string userId, bool isLoggedIn)
    {
        Debug.Log("CallbackFromLogin called.");
        LoginCanvas.SetActive(false);
        Debug.Log("LoginCanvas is active? " + LoginCanvas.active);
        AccountViewerCanvas.SetActive(true);
        Debug.Log("AccountViewerCanvas is active? " + AccountViewerCanvas.active);

        _accountViewerInformationLabel.text =
            "Username / Email: " + userEmail + "\n" +
            "UserId: " + userId + "\n" +
            "Logged in? " + isLoggedIn + "\n";

        CreateAccountCanvas.SetActive(false);
        Debug.Log("CreateAccountCanvas is active? " + CreateAccountCanvas.active);
    }

    /// <summary>
    /// Changes the active Canvas on the main scene to be the LoginCanvas. Intended to be used upon successful
    /// user log out.
    /// </summary>
    public void CallbackFromLogOut()
    {
        Debug.Log("CallbackFromLogout called.");
        AccountViewerCanvas.SetActive(false);
        LoginCanvas.SetActive(true);
    }

    #endregion

    #region Text Field Updates
    /// <summary>
    /// Updates error Text label on LoginCanvas to show message denoted by string parameter message.
    /// Used to update label from async callbacks.
    /// </summary>
    /// <param name="message"></param>
    public void UpdateLoginErrorLabel(string message)
    {
        _loginErrorLabel.text = message;
    }

    /// <summary>
    /// Updates error Text label on CreateAccountCanvas to show message denoted by string parameter message.
    /// Used to update label from async callbacks.
    /// </summary>
    /// <param name="message"></param>
    public void UpdateCreateAccountErrorLabel(string message)
    {
        _createAccountErrorLabel.text = message;
    }

    #endregion
}
