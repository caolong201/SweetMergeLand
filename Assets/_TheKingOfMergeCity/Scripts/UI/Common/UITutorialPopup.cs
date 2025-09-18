using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using USimpFramework.UI;
using USimpFramework.Utility;
using TMPro;
using DG.Tweening;

namespace TheKingOfMergeCity
{
    using System;
    using Config;

    public class UITutorialPopup : UIPopupBase
    {
        [SerializeField] Transform tutorialInfoTrans;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] Transform avatarTrans;
        [SerializeField] Transform descriptionContainerTrans;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform handTrans;
        [SerializeField] Image handImage;
        [SerializeField] Image holeImage;
        [SerializeField] Image bgMaskImage;
        [SerializeField] Image blockTouchImage;
        
        Sequence handSequence;
        float originalBgMaskAlpha;

        public override void Show(bool withTransition = true, Action onCompleted = null)
        {
            gameObject.SetActive(true);
            if (originalBgMaskAlpha == 0)
                originalBgMaskAlpha = bgMaskImage.color.a;

            ResetData();

        }

        public void ShowBlockTouch(bool isShow)
        {
            blockTouchImage.enabled = isShow;
        }

        public void ShowMaskBg(bool isShow)
        {
            bgMaskImage.transform.parent.gameObject.SetActive(isShow);
        }

        public void ShowEffect(Action onCompleted = null)
        {
            //Debug.Log("Show effect for step: " + TutorialManager.Instance.currentStep.config.id);

            StartCoroutine(CR_ShowEffect(onCompleted));

            transform.SetAsLastSibling();

            IEnumerator CR_ShowEffect(Action onCompleted = null)
            {
                yield return new WaitWhile(() => isHiding);
                ShowMaskBg(true);

                var configStep = TutorialManager.Instance.currentStep.config;

                var pos = (Vector2)Camera.main.WorldToScreenPoint(Vector2.zero);
                pos += configStep.offsetBetweenUITutorialInfoAndCenter;
                tutorialInfoTrans.position = pos;
                
                descriptionText.text = configStep.description;
                //Init
                ResetData();

                if (configStep.showDescription)
                {
                    bgMaskImage.SetAlpha(originalBgMaskAlpha);

                    var fadeTween = canvasGroup.DOFade(1, 0.35f).SetEase(Ease.OutSine).From(0);
                    yield return fadeTween.WaitForCompletion();
                    avatarTrans.DOScale(1, 0.4f).SetEase(Ease.OutBack);
                    yield return new WaitForSeconds(0.3f);
                    descriptionContainerTrans.DOScale(1, 0.4f).SetEase(Ease.OutBack);

                    OnCompleteShowEffect();

                }
                else
                {
                    bgMaskImage.SetAlpha(0);
                    canvasGroup.alpha = 1;

                    OnCompleteShowEffect();
                }

                onCompleted?.Invoke();
            }
        }

        void ResetData()
        {
            avatarTrans.localScale = Vector3.zero;
            descriptionContainerTrans.localScale = Vector3.zero;
            holeImage.gameObject.SetActive(false);
            handTrans.gameObject.SetActive(false);
            canvasGroup.alpha = 0;
        }

        void OnCompleteShowEffect()
        {
            ShowBlockTouch(false);

            var configStep = TutorialManager.Instance.currentStep.config;

            if (configStep is ConfigTutorialPuzzleMove configPuzzleMove)
            {
                handTrans.gameObject.SetActive(true);
                handImage.SetAlpha(1);
                holeImage.gameObject.SetActive(true);

                var puzzleController = InGameManager.Instance.puzzlesController;
                UIPuzzleItemController firstItem = null;

                var secondItem = puzzleController.GetPuzzleItem(configPuzzleMove.endPosition);
                if (secondItem == null)
                {
                    Debug.LogError($"This board position {configPuzzleMove.endPosition.y}-{configPuzzleMove.endPosition.x} has no item!");
                    return;
                }

                if (configPuzzleMove.autoDetectStartItem)
                {
                    firstItem = puzzleController.GetNearestMergableItem(configPuzzleMove.endPosition, secondItem);
                    if (firstItem == null)
                    {
                        Debug.LogError($"step:{configStep.id}, Cannot find mergable item, at position {configPuzzleMove.endPosition}, compare item {secondItem.data.puzzleId}, level {secondItem.data.level} ");
                        return;
                    }
                }
                else
                {
                    firstItem = puzzleController.GetPuzzleItem(configPuzzleMove.startPosition);
                    if (firstItem == null)
                    {
                        Debug.LogError($"step: {configStep.id} This board position {configPuzzleMove.startPosition} has no item");
                        return;

                    }
                }




                var start = firstItem.itemPosition;
                var end = secondItem.itemPosition;

                holeImage.transform.position = (start + end) / 2;
                holeImage.rectTransform.SetSize(configPuzzleMove.holeSize);

                handSequence = DOTween.Sequence();
                handSequence.Append(handImage.DOFade(1, 0.3f).SetEase(Ease.OutQuad).From(0));
                handSequence.Append(handTrans.DOMove(end, 0.4f).SetEase(Ease.OutQuad).From(start));
                handSequence.AppendInterval(0.3f);
                handSequence.Append(handImage.DOFade(0, 0.3f).SetEase(Ease.InQuad));
                handSequence.SetLoops(-1, LoopType.Restart);

            }
            else if (configStep is ConfigTutorialClickProducePuzzle configClickProducePuzzle)
            {
                handTrans.gameObject.SetActive(true);
                handImage.SetAlpha(1);
                holeImage.gameObject.SetActive(true);

             

                var puzzleController = InGameManager.Instance.puzzlesController;
                var item = puzzleController.GetPuzzleItem(configClickProducePuzzle.clickPosition);
                if (item == null || item is not UIPuzzleProducerController producerItem || !producerItem.canProduce)
                {
                    Debug.LogError($"This board position {configClickProducePuzzle.clickPosition} can have producer item!");
                    return;
                }

                handTrans.position = item.itemPosition;

                var holeBoardPosition = configClickProducePuzzle.clickPosition;
                holeBoardPosition.x -= 1;
                var holePosition = puzzleController.BoardToScreenPos(holeBoardPosition);

                holeImage.transform.position = holePosition;
                holeImage.rectTransform.SetSize(configClickProducePuzzle.holeSize);

                PlayHandClickingAnimation(item.itemPosition);

            }
            else if (configStep is ConfigServeCustomer configServeCustomer)
            {
                handTrans.gameObject.SetActive(true);
                handImage.SetAlpha(1);
                holeImage.gameObject.SetActive(true);

              



                if (configServeCustomer.autoDetectServeCustomer)
                {
                    var pendingServeCustomer = InGameManager.Instance.customersController.GetPendingServeCustomer();

                    if (pendingServeCustomer == null)
                    {
                        Debug.LogError("Cannot find any pending serve customer!");
                    }
                    else
                    {
                        //Highligh it (by override canvas sorting) | or move the mask to it?
                        holeImage.transform.position = pendingServeCustomer.serveButton.transform.position;
                        holeImage.rectTransform.SetSize(configServeCustomer.holeSize);

                        pendingServeCustomer.onStartServe -= OnStartServe;
                        pendingServeCustomer.onStartServe += OnStartServe;

                        PlayHandClickingAnimation(pendingServeCustomer.serveButton.transform.position);
                    }
                }
            }
            else if (configStep is ConfigTutorialClickBuildDeco configClickToBuildDeco)
            {
                handTrans.gameObject.SetActive(true);
                holeImage.SetAlpha(1);
                holeImage.gameObject.SetActive(true);

                //Click on build deco button sub step
                var uiHomeView = UIManager.Instance.currentView as UIInGameView;

                PlayHandClickingAnimation(uiHomeView.buildButton.transform.position);

                holeImage.transform.position = uiHomeView.buildButton.transform.position;
                holeImage.rectTransform.SetSize(configClickToBuildDeco.holeSize);
            }
            else if (configStep is ConfigTutorialClickStartInBuildPopup configClickStartInBuildPopup)
            {
                handTrans.gameObject.SetActive(true);
                holeImage.SetAlpha(1);
                holeImage.gameObject.SetActive(true);

                var uiBuildDecoPopup = UIManager.Instance.GetPopup<UIDecoBuildPopup>();
                holeImage.transform.position = uiBuildDecoPopup.startButton.transform.position;
                holeImage.rectTransform.SetSize(configClickStartInBuildPopup.holeSize);
                PlayHandClickingAnimation(holeImage.transform.position);
            }


            else if (configStep is ConfigTutorialClickBuyEnergy configClickBuyEnergy)
            {
                // Bật tay + vòng tròn highlight
                handTrans.gameObject.SetActive(true);
                handImage.SetAlpha(1);
                holeImage.gameObject.SetActive(true);

                // Lấy UI hiện tại
                var uiHomeView = UIManager.Instance.currentView as UIInGameView;
                if (uiHomeView == null || uiHomeView.energyButton == null)
                {
                    Debug.LogError("Không tìm thấy EnergyButton trong UIInGameView!");
                    return;
                }

                // Hiển thị animation tay click vào nút Energy
                PlayHandClickingAnimation(uiHomeView.energyButton.transform.position);

                // Đặt vòng highlight vào nút Energy
                holeImage.transform.position = uiHomeView.energyButton.transform.position;
                holeImage.rectTransform.SetSize(configClickBuyEnergy.holeSize);
            }
        }
        void OnClickBuyEnergyTutorial()
        {
            TutorialManager.Instance.CheckCompleteStep<ConfigTutorialClickBuyEnergy>();
        }
        void PlayHandClickingAnimation(Vector2 position)
        {
            handTrans.position = position;

            //Scale or move?
            handSequence = DOTween.Sequence();
            handSequence.Append(handTrans.DOScale(0.9f, 0.2f).SetEase(Ease.Linear).From(1));
            handSequence.Append(handTrans.DOScale(1, 0.2f).SetEase(Ease.Linear));
            handSequence.AppendInterval(0.5f);
            handSequence.SetLoops(-1, LoopType.Restart);
        }

        void OnStartServe(UICustomerOrderItem customer)
        {
            customer.onStartServe -= OnStartServe;

            HideEffect();
        }

       

        bool isHiding = false;

        public void HideEffect(Action onCompleted = null)
        {
            isHiding = true;
            StartCoroutine(CR_HideEffect(() =>
            {
                handTrans.DOKill();
                handImage.DOKill();

                handSequence.Complete();
                handSequence.Kill();
                isHiding = false;
                onCompleted?.Invoke();
            }));
        }

        public override void Hide(bool withTransition = true, Action onCompleted = null)
        {
            HideEffect(() =>
            {
                gameObject.SetActive(false);
            });
        }

        IEnumerator CR_HideEffect(Action onCompleted = null)
        {
            ShowBlockTouch(true);
            descriptionContainerTrans.DOScale(0, 0.4f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.3f);
            avatarTrans.DOScale(0, 0.4f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.3f);
            var fadeTween = canvasGroup.DOFade(0, 0.35f).SetEase(Ease.InSine);
            yield return fadeTween.WaitForCompletion();
            onCompleted?.Invoke();
        }
    }
}
