using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.UI;

using USimpFramework.UI;
using USimpFramework.Animation.DOTweenExtension;
using USimpFramework.UIGachaEffect;
using USimpFramework.Utility;

using TMPro;
using DG.Tweening;

using Random = UnityEngine.Random;

namespace TheKingOfMergeCity
{
    using Config;
    using Enum;
    
    public class UIRoulettePopup : UIScalePopup
    {
        public enum RouletteSpinState
        {
            None = 0,
            Spin = 1,
            SlowDown = 2,    
        }

        [Header("Animation")]
        [SerializeField] List<Transform> bubbleTransforms;
        [SerializeField] Transform handleTrans;
        [SerializeField] Transform turningTrans;

        [Header("References")]
        [SerializeField] List<UIRewardItem> uiRewardItems;
        [SerializeField] Button spinButton;
        [SerializeField] Button closeButton;
        [SerializeField] Button stopButton;
        [SerializeField] TMP_Text spinText;
        [SerializeField] TMP_Text recoverText;
        [SerializeField] UIRouletteRewardPopup uiRewardPopup;
        [SerializeField] Image flyingPuzzleImagePrefab;

        [Header("Settings")]
        [SerializeField] Sprite availableSpinButtonSprite;
        [SerializeField] Sprite unAvailableSpinButtonSprite;
        [SerializeField] Material availableTextMaterial;
        [SerializeField] Material unvailableTextMaterial;

        ConfigRewardItem spinReward;


        int curRewardIndex;

        float originalBoardScale = -1;
        float currentSpeed;

        bool isRotateHandle;
        bool isSpinning;

        bool isGettingReward;

        void Start()
        {
            uiRewardPopup.gameObject.SetActive(false);
            flyingPuzzleImagePrefab.gameObject.SetActive(false);
          
            PlaySpinAnimation();
            UpdateRewards();
        }

        void OnEnable()
        {
            UserManager.Instance.rouletteData.value.onValueChanged += OnRouletteChanged;

            spinButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);

            OnRouletteChanged(UserManager.Instance.rouletteData.value, 0);

            if (UserManager.Instance.CheckResetRouletteConfigRewards(true))
                UpdateRewards();
        }

        void OnDisable()
        {
            UserManager.Instance.rouletteData.value.onValueChanged -= OnRouletteChanged;
        }

        void OnRouletteChanged(Model.ConsumableItemCooldown item, float oldValue)
        {
            spinText.text = $"Spin {item.currentValue}/{item.maxValue}";

            if (item.isCooldown)
            {
                spinButton.interactable = false;
                spinButton.image.sprite = unAvailableSpinButtonSprite;
                spinText.fontMaterial = unvailableTextMaterial;
                recoverText.gameObject.SetActive(true);
            }
            else
            {
                spinButton.interactable = true;
                spinButton.image.sprite = availableSpinButtonSprite;
                spinText.fontMaterial = availableTextMaterial;
                recoverText.gameObject.SetActive(false);
            }
        }
       
        void PlaySpinAnimation()
        {
            float dur = 0.2f;
            var mainSequence = DOTween.Sequence();
            for (int i = 0; i < bubbleTransforms.Count; i++)
            {
                var bubbleTrans = bubbleTransforms[i];
                var tween = bubbleTrans.DOScale(0.5f, dur).SetEase(Ease.Linear).SetDelay(i * 0.1f).OnComplete(() => bubbleTrans.DOScale(1f, dur).SetEase(Ease.Linear));
                mainSequence.Append(tween);
            }

            mainSequence.AppendInterval(0.4f);
            mainSequence.SetLoops(-1);
        }

        void UpdateRewards()
        {
            var rewards = UserManager.Instance.rouletteData.value.configRewards;
            int count = 0;
            foreach (var uiReward in uiRewardItems)
            {
                uiReward.Setup(rewards[count]);

                count++;
                if (count == rewards.Count)
                {
                    count = 0;
                }
            }
        }

        public override void Show(bool withTransition = true, Action onCompleted = null)
        {
            gameObject.SetActive(true);

            if (withTransition)
            {
                if (originalBoardScale < 0)
                {
                    originalBoardScale = boardTrans.localScale.x;
                }

                spinButton.transform.localScale = Vector3.zero;
                closeButton.transform.localScale = Vector3.zero;

                boardTrans.DOScale(originalBoardScale, duration).SetEase(Ease.OutBack).From(originalBoardScale * 0.7f);

                spinButton.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.3f);
                closeButton.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.3f);

                canvasGroup.DOFade(1, duration).SetEase(Ease.Linear).From(0).OnComplete(() => onCompleted?.Invoke());
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                onCompleted?.Invoke();
            }
        }

        public override void Hide(bool withTransition = true, Action onCompleted = null)
        {
            if (withTransition)
            {
                boardTrans.DOScale(originalBoardScale * 0.7f, duration).SetEase(Ease.InBack).From(originalBoardScale);
                canvasGroup.DOFade(0, duration).SetEase(Ease.Linear).From(1f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onCompleted?.Invoke();
                });

                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                gameObject.SetActive(false);
                onCompleted?.Invoke();
            }
        }


        void Update()
        {
            var roulette = UserManager.Instance.rouletteData.value;
            if (roulette.isCooldown)
            {
                var remainingTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(roulette.GetRemainingCooldownResetMs());
                recoverText.text = $"Reset in {remainingTimeOffset.Hour}h {remainingTimeOffset.Minute}m {remainingTimeOffset.Second}s";
            }
          


            var configRoulette = ConfigManager.Instance.configRoulette;

            if (isSpinning)
            {
                currentSpeed += Time.deltaTime * configRoulette.acceleration;
                currentSpeed = Mathf.Min(currentSpeed, configRoulette.angularSpeed);
                turningTrans.Rotate(new Vector3(0, 0, -currentSpeed * Time.deltaTime * configRoulette.angleFactor), Space.Self);

                var rewardIndex = Mathf.FloorToInt((turningTrans.localEulerAngles.z + 360) / 45f);
                if (rewardIndex != curRewardIndex)
                {
                    curRewardIndex = rewardIndex;

                    if (!isRotateHandle)
                    {
                        isRotateHandle = true;
                        handleTrans.DORotate(new Vector3(0, 0, configRoulette.rotateHandle), configRoulette.handleRotateSpeed, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear).SetSpeedBased(true)
                    .OnComplete(() =>
                    {
                        handleTrans.DORotate(Vector3.zero, configRoulette.handleRotateSpeed).SetEase(Ease.Linear).SetSpeedBased(true)
                        .OnComplete(() => isRotateHandle = false);
                    });
                    }
                }
            }


            #region Just For Testing

            // if (Input.GetKeyDown(KeyCode.A))
            // {
            //     PlaySpinAnimationOnItem(uiRewardItems[Random.Range(0,uiRewardItems.Count)]);
            // }

            /*if (Input.GetKeyDown(KeyCode.B))
            {
                uiRewardPopup.Show(uiRewardItems[Random.Range(0, uiRewardItems.Count)].config);
            }
            */

            #endregion
        }

        public void PressClose()
        {
            if (isGettingReward)
                return;

            UIManager.Instance.HidePopup(this);
        }

        void OnDestroy()
        {
            foreach (var trans in bubbleTransforms)
            {
                trans.DOKill();
            }
        }

        public void PressSpin()
        {
            if (isSpinning)
                return;

            if (UserManager.Instance.rouletteData.value.currentValue == 0)
                return;

            ApplovinManager.Instance.ShowRewardedAd(isSuccess =>
            {
                if (!isSuccess)
                {
                    UIManager.Instance.ShowFloatingText("Cannot spin! Please check your internet connection!");
                    return;
                }

               
                spinReward = UserManager.Instance.MakeRouletteSpin();
                if (spinReward == null)
                {
                    Debug.LogError("Cannot spin! Invalid reward");
                    return;
                }


                isSpinning = true;
                isGettingReward = true;

                currentSpeed = 0;
                spinButton.gameObject.SetActive(false);
                stopButton.transform.DOPopIn(0.4f);
            });

        }

        void PlaySpinAnimationOnItem(UIRewardItem rouletteItem, Action onCompleted = null)
        {
            var configRoulette = ConfigManager.Instance.configRoulette;

            float unit = (float)360 / uiRewardItems.Count;
            var index = uiRewardItems.IndexOf(rouletteItem);

            var minAngle = index * unit - 360;
            var maxAngle = (index + 1) * unit - 360;

            var randomAngle = Random.Range(minAngle,maxAngle);
            int spinCount = Random.Range(configRoulette.spinCountRange.x, configRoulette.spinCountRange.y + 1);
            float targetAngle =  randomAngle - spinCount * 360 - turningTrans.eulerAngles.z;

            //Debug.Log($"{rouletteItem.name} Min {minAngle}, max {maxAngle}, random {randomAngle}, target {targetAngle}, spin count {spinCount}");

            int curRewardIndex = 0;
            bool isRotateHandle = false;
            turningTrans.DORotate(new Vector3(0,0, targetAngle), configRoulette.angularSpeed, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic)
                .SetSpeedBased(true)
                .OnUpdate(() =>
                {
                    var rewardIndex = Mathf.FloorToInt((turningTrans.localEulerAngles.z + 360) / 45f);
                    if (rewardIndex != curRewardIndex)
                    {
                        curRewardIndex = rewardIndex;

                        if (!isRotateHandle)
                        {
                            isRotateHandle = true;
                            handleTrans.DORotate(new Vector3(0, 0, configRoulette.rotateHandle), configRoulette.handleRotateSpeed, RotateMode.LocalAxisAdd)
                        .SetEase(Ease.Linear).SetSpeedBased(true)
                        .OnComplete(() =>
                        {
                            handleTrans.DORotate(Vector3.zero, configRoulette.handleRotateSpeed).SetEase(Ease.Linear).SetSpeedBased(true)
                            .OnComplete(() => isRotateHandle = false);
                        }); 
                        }
                     
                    }
                })
                .OnComplete(() =>
            {                
                onCompleted?.Invoke();
            });
        }

        public void PressClaimReward(Button button)
        {
            button.interactable = false;

            uiRewardPopup.Hide();

            var configReward = uiRewardPopup.configReward;

            spinButton.transform.DOPopIn(0.5f);

            if (configReward.rewardType == RewardType.Currency)
            {
                var uiTopBar = (UIManager.Instance.currentView as UIHomeView).uiTopBar;

                float sizeScale = configReward.currencyType == CurrencyType.Energy ? 1 : 1.5f;

                uiTopBar.PlayCurrencyGachaEffect(configReward.currencyType, configReward.amount, uiRewardPopup.uiRewardItem.transform.position, sizeScale);
            }
            else if (configReward.rewardType == RewardType.PuzzleItem)
            {

                //Fly the puzzle item to the play button
                var flyingPuzzle = SimpleObjectPool.Spawn(flyingPuzzleImagePrefab, UIGachaEffect.Instance.transform);
                flyingPuzzle.sprite = uiRewardPopup.uiRewardItem.GetRewardImage().sprite;
                flyingPuzzle.rectTransform.SetSize(uiRewardPopup.uiRewardItem.GetRewardImage().rectTransform.GetSize());
                flyingPuzzle.gameObject.SetActive(true);
                flyingPuzzle.transform.position = uiRewardPopup.uiRewardItem.transform.position;

                float moveDuration = 0.4f;

                var seq = DOTween.Sequence();
                var startPos = flyingPuzzle.transform.position + new Vector3(80, 80);
                var playButton = (UIManager.Instance.currentView as UIHomeView).playButton;
                UIManager.Instance.SetInteraction(false);
                seq.Append(flyingPuzzle.transform.DOMove(startPos, moveDuration).SetEase(Ease.OutQuad));
                seq.Append(flyingPuzzle.transform.DOMove(playButton.transform.position, 0.8f).SetEase(Ease.InQuad).OnStart(() =>
                {
                    flyingPuzzle.transform.DOScale(0.4f, 0.8f).SetEase(Ease.InQuad);
                }));
                seq.OnComplete(() =>
                {
                    playButton.transform.DOJellyPop(1.2f, 0.2f);
                    SimpleObjectPool.Despawn(flyingPuzzle, moveToPoolContainer: false);
                    UIManager.Instance.SetInteraction(true);
                });
            }
        }




        public void PressStop()
        {
            isSpinning = false;
            stopButton.gameObject.SetActive(false);

            var uiRouletteItem = uiRewardItems.Find(ui => ui.config == spinReward);
            if (uiRouletteItem != null)
            {
                PlaySpinAnimationOnItem(uiRouletteItem, () =>
                {
                    UIManager.Instance.SetInteraction(true);
                    isGettingReward = false;
                    uiRewardPopup.Show(spinReward);
                });
            }
            else
            {
                Debug.LogError("Cannot find ui reward: " + spinReward.rewardType);
            }
        }

    }




}
