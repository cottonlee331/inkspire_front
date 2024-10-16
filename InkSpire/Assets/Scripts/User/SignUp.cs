using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class SignUp : MonoBehaviour
{
    [SerializeField] TMP_InputField input_email;
    [SerializeField] TMP_InputField input_nickname;
    [SerializeField] TMP_InputField input_pw;
    [SerializeField] TMP_InputField input_pwcheck;
    [SerializeField] TextMeshProUGUI wrong_pw;

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                OnClickBack();
            }
        }
    }

    public void SignupButton(){
        string user_email = input_email.text;
        string user_nickname = input_nickname.text;
        string user_pw = input_pw.text;
        string user_pwcheck = input_pwcheck.text;

        if(string.IsNullOrEmpty(user_email)) {
            wrong_pw.text = "이메일을 입력해주세요.";
            return;
        }

        if(string.IsNullOrEmpty(user_nickname)) {
            wrong_pw.text = "닉네임을 입력해주세요.";
            return;
        }

        if(string.IsNullOrEmpty(user_pw)) {
            wrong_pw.text = "비밀번호를 입력해주세요.";
            return;
        }
        
        if(user_pw != user_pwcheck){
            wrong_pw.text = "비밀번호가 일치하지 않습니다.";
            return;
        }

        NewAccount account = new()
        {
            email = user_email,
            nickname = user_nickname,
            password = user_pw
        };
        string account_json = JsonUtility.ToJson(account);
        StartCoroutine(APIManager.api.PostRequest<Null>("/users/signup", account_json, ProcessResponse));
    }

    private void ProcessResponse(Response<Null> response){
        if(response.success){
            SceneManager.LoadScene("1_Start");
        }
        else {
            wrong_pw.text = response.message;
        }
    }

    public void OnClickBack(){
        SceneManager.LoadScene("1_Start");
    }
}
