/* JFARABEE 2021
 * 
 * 
 * 
 */

namespace Firebase.Auth
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Authentication is attached to an AuthenticationManager GameObject, and will control all communication
    /// with the Firebase login server.
    /// </summary>
    public class Authentication : MonoBehaviour
    {
        #region Firebase Objects
        protected Firebase.Auth.FirebaseAuth AuthenticatorProxy;
        protected Firebase.FirebaseApp App;

        public Firebase.Auth.FirebaseUser User;
        #endregion

        #region Serialized Fields
        [SerializeField]
        [Tooltip("The username InputField from the LoginCanvas.")]
        private InputField LoginUsernameInputField;

        [SerializeField]
        [Tooltip("The password InputField from the LoginCanvas.")]
        private InputField LoginPasswordInputField;

        [SerializeField]
        [Tooltip("The Text label where error messages are posted on the LoginCanvas.")]
        private Text LoginErrorTextLabel;

        [SerializeField]
        [Tooltip("The username InputField from the CreateAccountCanvas.")]
        private InputField CreateAccountUsernameInputField;

        [SerializeField]
        [Tooltip("The password InputField from the LoginCanvas.")]
        private InputField CreateAccountPasswordInputField;

        [SerializeField]
        [Tooltip("The password confirmation InputField from the LoginCanvas.")]
        private InputField CreateAccountPasswordConfirmationInputField;

        [SerializeField]
        [Tooltip("The Text label where error messages are posted on the CreateAccountCanvas.")]
        private Text CreateAccountErrorTextLabel;

        [SerializeField]
        [Tooltip("The CanvasManager GameObject.")]
        private CanvasManager CanvasManager;
        #endregion

        #region Regular Expressions

        //email regular expression is super complicated
        readonly string EmailRegex =
            @"^(?:[a-zA-Z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+\/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9]" +
                "(?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)$";

        //password is alphanumeric, some special symbols, and at least 6 chars long
        //also, I have no idea why C# was mad about escaping the " inline, so I had to split the string in half.
        //read it as one regex.
        readonly string PasswordRegex = @"^[a-zA-Z0-9~`!@#$%^&*\(\)-_+=|\}\]\{\[""':;?/>.<,].{6,}$";

        #endregion

        #region MonoBehaviour Callbacks
        // Start is called before the first frame update
        void Start()
        {


            //First, check the status of Google Play Services, and update if necessary.
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                {
                    var dependencyStatus = task.Result;
                    if(dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        //if Google Play services are up to date and available,
                        // get the default instance of the Firebase SOAP proxy objects
                        App = Firebase.FirebaseApp.DefaultInstance;
                        AuthenticatorProxy = Firebase.Auth.FirebaseAuth.DefaultInstance;

                        //If we get to this point, we have done all the necessary checks to be able to
                        //use the Firebase SDK.

                        //I recommend setting up a variable that tracks this for all purposes, but a
                        //simple App == null check can also serve that purpose.
                    }
                    else
                    {
                        //If we enter this control flow branch, then something in the check or update of
                        //Google Play services has failed - some backup behavior should be defined.
                        Debug.LogError(System.String.Format(
                            "Firebase Dependencies Error: {0}", dependencyStatus));
                    }
                }
            );
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region OnClick Methods

        /// <summary>
        /// The method called when the Login button is clicked. This will call the async Firebase member
        /// method SignInWithEmailAndPasswordAsync.
        /// </summary>
        public void OnLoginClick()
        {
            //we clear the error text label to free space for a new error text if necessary.
            CanvasManager.UpdateLoginErrorLabel("");

            //first we need to check the validity of the entered information

            string email = LoginUsernameInputField.text;
            string password = LoginPasswordInputField.text;

            //check the username InputField contents against the Email regex
            if ( !Regex.Match(email, EmailRegex).Success )
            {
                //upon failing regex check

                //update the error text label
                CanvasManager.UpdateLoginErrorLabel("Please enter a valid (email) username.");

                //no further behavior should happen.
                return;
            }

            //now check the password InputField contents against the Password regex
            if( !Regex.Match(password, PasswordRegex).Success )
            {
                //upon failing regex check

                //update the error text label
                CanvasManager.UpdateLoginErrorLabel("Please enter a valid alphanumeric password.");

                //no further behavior should happen.
                return;
            }

            //final check - sometimes checking the Google Play dependencies can be slow, so we need
            // to make sure that check has ended before we can make calls to the authentication service
            if(AuthenticatorProxy == null)
            {
                CanvasManager.UpdateLoginErrorLabel("Connection to Firebase not yet established. Please wait.");
                return;
            }

            CanvasManager.UpdateLoginErrorLabel("Logging in...");

            //since we passed InputField validations, we can now attempt the login.
            AuthenticatorProxy.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    //cancellation is a rarer case than faulting, most errors on either end will
                    // fall under the check for IsFaulted below
                    Debug.LogError("SignInWithEmailAndPasswordAsync cancelled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    //we are going to assume here that the error encountered was an unknown username/password
                    //combination, as is most often the case.
                    // so we will update the login text accordingly
                    CanvasManager.UpdateLoginErrorLabel("Unknown username/password combination. Please enter a valid username/password.");

                    //this is where the majority of errors are caught
                    //since this is sort of a catch-all case, we want to print the error message
                    //that Firebase will send us.
                    Debug.LogError("SignInWithEmailAndPasswordAsync faulted on error: " + task.Exception);


                    return;
                }

                Debug.Log("Login complete: " + task.Result.Email + ":" + task.Result.UserId);

                /* Firebase.Auth.FirebaseUser */ User = task.Result;

                //if we make it here, user login has been successful.
                //We want to change to the account viewer screen, which will show us some basic account and
                //firebase information.
                //this is done by the CanvasManager
                CanvasManager.CallbackFromLogin(task.Result.Email, task.Result.UserId, !(User == null));
            });
        }

        /// <summary>
        /// The method called when the Create Account button is clicked. This will call the async Firebase
        /// member method CreateUserWithEmailAndPasswordAsync.
        /// </summary>
        public void OnCreateAccountClick()
        {
            //we clear the error text label to free space for a new error text if necessary.
            CreateAccountErrorTextLabel.text = "";

            //first we need to check the validity of the entered information

            string email = CreateAccountUsernameInputField.text;
            string password = CreateAccountPasswordInputField.text;
            string passwordConfirm = CreateAccountPasswordConfirmationInputField.text;

            //check that the entered passwords match
            if( !password.Equals(passwordConfirm) )
            {
                //if entered password + confirm password do not match

                //update the error text label
                CanvasManager.UpdateCreateAccountErrorLabel("Entered passwords do not match.");
            }

            //check the username InputField contents against the Email regex
            if (!Regex.Match(email, EmailRegex).Success)
            {
                //upon failing regex check

                //update the error text label
                CanvasManager.UpdateCreateAccountErrorLabel("Please enter a valid (email) username.");

                //no further behavior should happen.
                return;
            }

            //now check the password InputField contents against the Password regex
            if (!Regex.Match(password, PasswordRegex).Success)
            {
                //upon failing regex check

                //update the error text label
                CanvasManager.UpdateCreateAccountErrorLabel("Please enter a valid alphanumeric password, 6+ characters.");

                //no further behavior should happen.
                return;
            }

            //final check - sometimes checking the Google Play dependencies can be slow, so we need
            // to make sure that check has ended before we can make calls to the authentication service
            if (AuthenticatorProxy == null)
            {
                CanvasManager.UpdateCreateAccountErrorLabel("Connection to Firebase not yet established. Please wait.");

                return;
            }

            CreateAccountErrorTextLabel.text = "Creating account...";

            //since we passed all validations, we can now attempt the login.
            AuthenticatorProxy.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    //cancellation is a rarer case than faulting, most errors on either end will
                    // fall under the check for IsFaulted below
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync cancelled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    //this is where the majority of errors are caught
                    //since this is sort of a catch-all case, we want to print the error message
                    //that Firebase will send us.
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync faulted on error: " + task.Exception);



                    return;
                }

                Debug.Log("User creation complete. " + task.Result.Email + ":" + task.Result.UserId);

                //if we make it here, user creation has been successful.
                // with CreateUser..., successful creation also means automatic LOGIN.

                User = task.Result;

                //We want to change to the AccountViewer.
                //This is done by the CanvasManager
                CanvasManager.CallbackFromAccountCreation(task.Result.Email, task.Result.UserId, !(User == null));
            });
        }

        /// <summary>
        /// The method called when the Log Out button is clicked. This will call the async Firebase
        /// member method SignOut.
        /// </summary>
        public void OnLogOutClick()
        {
            //removes all local credentials
            AuthenticatorProxy.SignOut();

            //manually remove all references to filled local fields
            App = null;
            AuthenticatorProxy = null;
            User = null;

            //go back to login screen.
            CanvasManager.CallbackFromLogOut();
        }

        #endregion
    }
}
