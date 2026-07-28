using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsentBox : MonoBehaviour
{
    public string privacyPolicyLink, EULAlink;
    public event Action<bool> OnConsent;

    public Button button_Accept;
    public Button button_Decline;
    public Button button_PrivacyPolicy;
    public Button button_EULA;
    
    private void Start()
    {
        button_Accept.onClick.AddListener(() => OnConsent(true));
        button_Decline.onClick.AddListener(() => OnConsent(false));
        button_PrivacyPolicy.onClick.AddListener(() => Application.OpenURL(privacyPolicyLink));
        button_EULA.onClick.AddListener(() => Application.OpenURL(EULAlink));

        EventSystem.current.SetSelectedGameObject(button_Accept.gameObject);
    }
}