using DG.Tweening;
using TheKingOfMergeCity;
using TheKingOfMergeCity.Enum;
using UnityEngine;
using UnityEngine.UI;

public class UICommercialpartPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button buttonGem;     
    [SerializeField] private Button buttonClose;  
    [SerializeField] private RectTransform board;  
    private int cost = 4;
    void Start()
    {
        buttonGem.onClick.AddListener(OnClickBuyWithGem);
        buttonClose.onClick.AddListener(CloseCommercial);
        board.localScale = Vector3.zero;
        board.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    private void OnClickBuyWithGem()
    {
        int currentGem = UserManager.Instance.GetCurrencyBalance(CurrencyType.Gem);

        if (currentGem >= cost)
        {           
            UserManager.Instance.AddCurrencyAmount(CurrencyType.Gem, -cost, true, true);   
           UserManager.Instance.GetCurrencyBalance(CurrencyType.Gem);        
            CloseCommercial();
        }
        else
        {
            Debug.Log("Không đủ gem!");          
        }
    }

    public void CloseCommercial()
    {
      
        board.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
