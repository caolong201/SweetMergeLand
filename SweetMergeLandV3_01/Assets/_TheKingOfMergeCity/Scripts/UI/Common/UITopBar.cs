using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheKingOfMergeCity.Enum;
using System;
using USimpFramework.Animation.DOTweenExtension;
using USimpFramework.Utility;
using USimpFramework.UIGachaEffect;
using DG.Tweening;

namespace TheKingOfMergeCity
{
    [DefaultExecutionOrder(150)]
    public class UITopBar : MonoBehaviour
    {
        [Header("Player Level")]
        [SerializeField] Image playerExpProgressImage;
        [SerializeField] TMP_Text playerLevelText;

        [Header("Star coin")]
        [SerializeField] Image _starCoinImage;
        public Image starCoinImage => _starCoinImage;
        [SerializeField] TMP_Text starCoinText;

        [Header("Gem")]
        [SerializeField] Image _gemCoinImage;
        public Image gemCoinImage => _gemCoinImage;
        [SerializeField] TMP_Text gemText;

        [Header("Energy")]
        [SerializeField] TMP_Text energyText;
        [SerializeField] Image energyImage;
        [SerializeField] RectTransform containerEnergyRegen;
        [SerializeField] TMP_Text energyCooldownTimeText;
        [SerializeField] Vector2 energyContainerHidePos;
        [SerializeField] Vector2 energyContainerShowPos;

        bool startRegenEnergy;

        Vector2 energyImageOriginalPos;
        Vector2 playerLevelTextOriginalPos;

        int currentExpReach;
        int nextExpReach;
        
        void Awake()
        {
            UserManager.Instance.onCurrencyBalanceChanged += OnCurrencyBalanceChanged;
        }
        
        void Start()
        {
            energyImageOriginalPos = energyImage.transform.position;
            playerLevelTextOriginalPos = playerLevelText.transform.position;
        }
        
        void OnEnable()
        {
            var userManager = UserManager.Instance;

            OnCurrencyBalanceChanged(CurrencyType.Star);
            OnCurrencyBalanceChanged(CurrencyType.Gem);

            if (userManager.isPlayingDecoBuildingFromInGameScene)
            {
                //Fake for only updating the old energy purpose
                energyText.text = userManager.oldEnergy.ToString();

                UpdatePlayerLevel(userManager.oldLevel);
                playerExpProgressImage.fillAmount = Mathf.InverseLerp(currentExpReach, nextExpReach, userManager.oldExp);
            }
            else //Because this UI is DDOL (child of the UIManager, so it must be reset the data when reloading new scene)
            {
                OnCurrencyBalanceChanged(CurrencyType.Energy);
                OnPlayerLevelUp();
            }

            if (HomeManager.Instance != null)
            {
                HomeManager.Instance.onPlayerLevelUp -= OnPlayerLevelUp;
                HomeManager.Instance.onPlayerLevelUp += OnPlayerLevelUp;
            }
          
        }

        void OnPlayerLevelUp()
        {
            UpdatePlayerLevel(UserManager.Instance.currentPlayerLevel);
            OnCurrencyBalanceChanged(CurrencyType.Exp);
        }

        void UpdatePlayerLevel(int level)
        {
            var userManager = UserManager.Instance;
            currentExpReach = userManager.GetExpByLevel(level, out _);
            nextExpReach = userManager.GetExpByLevel(level + 1, out var isMax);
            playerLevelText.text =level.ToString();
        }

        void OnDisable()
        {
            if (HomeManager.Instance != null)
            {
                HomeManager.Instance.onPlayerLevelUp -= OnPlayerLevelUp;
            }
        }

        void OnDestroy()
        {
            UserManager.Instance.onCurrencyBalanceChanged -= OnCurrencyBalanceChanged;
        }

        void OnCurrencyBalanceChanged(CurrencyType currencyType)
        {
            var balance = UserManager.Instance.GetCurrencyBalance(currencyType);

            switch (currencyType)
            {
                case CurrencyType.Energy:
                    {
                        energyText.text = balance.ToString();

                        if (balance < ConfigManager.Instance.configGlobal.maxEnergy)
                        {
                            //Starting regen energy
                            containerEnergyRegen.DOKill();
                            containerEnergyRegen.DOAnchorPos(energyContainerShowPos, 0.4f).SetEase(Ease.OutSine);
                            startRegenEnergy = true;
                        }
                        else
                        {
                            energyCooldownTimeText.text = "Full";                             
                            if(containerEnergyRegen.anchoredPosition == energyContainerShowPos)
                            {
                                containerEnergyRegen.DOKill();
                                containerEnergyRegen.DOAnchorPos(energyContainerHidePos, 0.4f).SetEase(Ease.InSine).SetDelay(1f);
                            }
                            startRegenEnergy = false;
                        }

                        break;
                    }
                case CurrencyType.Star: starCoinText.text = balance.ToString("N0"); break;
                case CurrencyType.Exp:
                    {
                        playerExpProgressImage.DOKill();
                        playerExpProgressImage.fillAmount = Mathf.InverseLerp(currentExpReach, nextExpReach, balance);
                        //Debug.Log($"On currency balance changed, current exp reach {currentExpReach}, next exp reach {nextExpReach}, balance {balance}, progress: {Mathf.InverseLerp(currentExpReach, nextExpReach, balance)}");
                        break;
                    }
                case CurrencyType.Gem: gemText.text = balance.ToString("N0"); break;
            }
        }

        void Update()
        {
            if (startRegenEnergy)
            {
                long cooldownMs = UserManager.Instance.nextTimeAddEnergyMs - TimeUtils.GetServerUtcNowMs();

                energyCooldownTimeText.text = DateTimeOffset.FromUnixTimeMilliseconds(cooldownMs).ToString("mm:ss");
            }
        }

        public void IncreaseCurrency(CurrencyType currencyType, float duration = 0.5f)
        {
            int balance = UserManager.Instance.GetCurrencyBalance(currencyType);

            switch (currencyType)
            {
                case CurrencyType.Star:
                    starCoinText.DOIncrease(balance, duration); break;
                case CurrencyType.Energy:
                    energyText.DOIncrease(balance, duration); break;
                case CurrencyType.Gem:
                    gemText.DOIncrease(balance, duration); break;
                case CurrencyType.Exp:
                    {
                        float newValue = Mathf.InverseLerp(currentExpReach, nextExpReach, balance);
                        playerExpProgressImage.DOFillAmount(newValue, duration);
                        break;
                    }
            }
        }


        public void PlayCurrencyGachaEffect(CurrencyType currencyType,int amount, Vector2 screenPosition, float startScale = 1, Action onCompleted = null)
        {
            string settingId = "";
            var endPos = Vector2.zero;

            switch (currencyType)
            {
                case CurrencyType.Star: 
                    settingId = "star_coin";
                    endPos = starCoinImage.transform.position;
                    break;
                case CurrencyType.Energy: 
                    settingId = "energy";
                    endPos = energyImageOriginalPos;
                    break;
                case CurrencyType.Gem:
                    settingId = "gem";
                    endPos = gemCoinImage.transform.position;
                    break;
                case CurrencyType.Exp: 
                    settingId = "exp";
                    endPos = playerLevelTextOriginalPos;
                    break;
            }

            if (!string.IsNullOrEmpty(settingId))
            {
                UIGachaEffect.Instance.PlayGachaEffect(settingId, amount, screenPosition, endPos, startScale, onFirstItemCompleted: () =>
                 {
                     IncreaseCurrency(currencyType);
                     DOVirtual.DelayedCall(0.2f, () => onCompleted?.Invoke());
                 });
            }
        }
    }
}
