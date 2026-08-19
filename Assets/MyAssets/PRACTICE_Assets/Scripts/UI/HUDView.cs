using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image hpFillImage;

    private int _score = 0;
    private float _remainTime = 60f;
    private float _health = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpFillImage.fillAmount = _health;
    }

    // Update is called once per frame
    void Update()
    {
        _remainTime = Mathf.Max(0f, _remainTime - Time.deltaTime);
        Refresh();
    }

    public void AddScoreAndDamage()
    {
        _score += 10;
        _health = Mathf.Max(0f, _health - 0.1f);
        Refresh();
    }

    private void Refresh()
    {
        scoreText.text = _score.ToString();
        timerText.text = _remainTime.ToString("F1");
        hpFillImage.fillAmount = _health;
    }
}


