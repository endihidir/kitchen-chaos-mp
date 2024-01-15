using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryFeedbackUI : MonoBehaviour
{
    [SerializeField] private Color _successColor, _failColor;

    [SerializeField] private Sprite _succcessIcon, _failIcon;

    [SerializeField] private string _successMessage, _failMessage;

    [SerializeField] private Image _backgroundImage, _iconImage;

    [SerializeField] private TextMeshProUGUI _feedBackMessageTxt;

    [SerializeField] private Animator _feedbackAnimator;
    
    private static readonly int DeliveryFeedback = Animator.StringToHash("DeliveryFeedback");
    public void PlaySuccess()
    {
        _backgroundImage.color = _successColor;
        _iconImage.sprite = _succcessIcon;
        _feedBackMessageTxt.text = _successMessage;
        _feedbackAnimator.Play(DeliveryFeedback, -1, 0f);
    }

    public void PlayFail()
    {
        _backgroundImage.color = _failColor;
        _iconImage.sprite = _failIcon;
        _feedBackMessageTxt.text = _failMessage;
        _feedbackAnimator.Play(DeliveryFeedback, -1, 0f);
    }
}