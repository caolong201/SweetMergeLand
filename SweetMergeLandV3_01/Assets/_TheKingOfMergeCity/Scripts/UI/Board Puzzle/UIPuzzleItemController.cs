using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using USimpFramework.UI;
using USimpFramework.Utility;
using DG.Tweening;
using TMPro;



namespace TheKingOfMergeCity
{
    using Config;
    using Model;

    public abstract class UIPuzzleItemController : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Serializable]
        public struct ScaleSetting
        {
            public float duration;
            public AnimationCurve curve;
            public float endScale;
        }

        public event Action<UIPuzzleItemController> onBeginDragItem;
        public event Action<UIPuzzleItemController> onDragItem;
        public event Action<UIPuzzleItemController> onDropItem; 
        public event Action<UIPuzzleItemController> onLevelUpgraded;
        public event Action<UIPuzzleItemController> onHoldItem;

        [SerializeField] Image _itemImage;
        public Image itemImage => _itemImage;
        
        [SerializeField] Image imageFullBlock;
        [SerializeField] Image imageHalfBlock;
        [SerializeField] TMP_Text levelText;
        [SerializeField] Image maxLevelImage;

        [Header("Animation")]
        [SerializeField] Transform containerScaleTrans;
        [SerializeField] ScaleSetting clickingScaleSetting;
        [SerializeField] ScaleSetting mergeUpgradeScaleSetting;

        public ConfigPuzzleItem config { get; private set; }

        public ConfigPuzzleType configPuzzleType { get; private set; }

        /// <summary>  The item image's position in world space </summary>
        public Vector2 itemPosition => itemImage.transform.position;

        public bool isMaxLevel => config != null && data.level == config.configPerLevel.Count - 1;

        public bool isBlock => data.blockingLevel >= 0 && data.blockingLevel <= 1;

        public bool isDragging { get; protected set; }

        public bool isMovingSelf { get; private set; }

        public int currentSiblingIndex { get; private set; }


        public UserPuzzleDataItem data { get; private set; }
        
        protected Image interactableImage;
        protected Vector2? originalItemImageSize;
        protected bool isDraggable = true;

        bool isPointerDown;

        public virtual void Setup(ConfigPuzzleItem config, ConfigPuzzleType configPuzzleType, UserPuzzleDataItem data, Vector2 spawnPosition, Vector2 itemSize)
        {
            if (interactableImage == null)
                interactableImage = GetComponent<Image>();

            originalItemImageSize ??= itemSize;

            ResetData();

            this.config = config;
            this.configPuzzleType = configPuzzleType;
            this.data = data;

            levelText.gameObject.SetActive(false);

            transform.position = spawnPosition;
            (transform as RectTransform).SetSize(itemSize);

            currentSiblingIndex = transform.GetSiblingIndex();

            if (config == null)
            {
                itemImage.gameObject.SetActive(false);
                return;
            }

            OnLevelUpdated();
        }

        protected virtual void ResetData()
        {
            isDragging = false;
            isMovingSelf = false;
            transform.localScale = Vector3.one;
            itemImage.rectTransform.sizeDelta = originalItemImageSize.GetValueOrDefault();
            SetRaycastTarget(true);
            SetDraggable(true);
        }

        protected virtual void OnDestroy()
        {
            transform.DOKill();
        }

        public void SetRaycastTarget(bool isActive)
        {
            interactableImage.raycastTarget = isActive;
        }

        public void SetDraggable(bool isDraggable)
        {
            this.isDraggable = isDraggable;
        }

        public void RevealItem(bool isReveal)
        {
            if (isReveal)
            {
                levelText.gameObject.SetActive(true);
                levelText.text = data.level.ToString();
            }
            else
            {
                levelText.gameObject.SetActive(false);
            }

            if (data.blockingLevel > 0) //Don't need to handle logic for item has half block| unblock
                return;

            if (isReveal)
            {
                itemImage.gameObject.SetActive(true);
                imageFullBlock.SetAlpha(0.3f);
            }
            else
            {
                itemImage.gameObject.SetActive(false);
                imageFullBlock.SetAlpha(1f);
            }
        }


        Coroutine crPlayScale;
        void PlayScale(ScaleSetting scaleSetting, Action onCompleted = null)
        {
            if (crPlayScale != null)
                StopCoroutine(crPlayScale);

            crPlayScale = StartCoroutine(CR_PlayScale(scaleSetting, onCompleted));
        }

        IEnumerator CR_PlayScale(ScaleSetting scaleSetting, Action onCompleted)
        {
            float elapse = 0;
            containerScaleTrans.localScale = Vector3.one;
            var curScale = containerScaleTrans.localScale;
            while (elapse < scaleSetting.duration)
            {
                var fraction = elapse / scaleSetting.duration;
                curScale.x = scaleSetting.curve.Evaluate(fraction) * scaleSetting.endScale;
                curScale.y = curScale.z = curScale.x;
                containerScaleTrans.localScale = curScale;
                elapse += Time.deltaTime;
                yield return null;
            }

            containerScaleTrans.localScale = Vector3.one * scaleSetting.endScale;
            onCompleted?.Invoke();
        }

        
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!canInteract)
                return;

            if (!isDraggable)
                return;

            BringToFront();
            isDragging = true;

            var uiInGame = UIManager.Instance.currentView as UIInGameView;
            uiInGame.CheckHideSelector(transform);

            onBeginDragItem?.Invoke(this);
        }

        public bool canInteract => !isMovingSelf && !isBlock;

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!canInteract)
                return;

            if (!isDraggable)
                return;

            transform.position = Input.mousePosition;
            onDragItem?.Invoke(this);
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (!canInteract)
                return;

            isPointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isDragging && isPointerDown)
            {
                OnDrop(eventData);
            }

            isPointerDown = false;
            //Debug.Log("pointer up!: " + transform.GetInstanceID());
        }

        void Update()
        {
            if (isPointerDown)
            {
                //Debug.Log("On Hold:" + transform.GetInstanceID());
                onHoldItem?.Invoke(this);
            }
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            DOVirtual.DelayedCall(0.0001f, () => isDragging = false);

            onDropItem?.Invoke(this);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (data.blockingLevel == 0 || !canInteract)
                return;

            var uiInGame = UIManager.Instance.currentView as UIInGameView;
            uiInGame.ShowSelectorIndicator(true, transform);

            if (!isDragging)
            {
                PlayScale(clickingScaleSetting);
            }
        }


        public virtual void Upgrade()
        {
            data.level++;
            OnLevelUpdated();

            var uiInGame = UIManager.Instance.currentView as UIInGameView;
            uiInGame.ShowSelectorIndicator(true, transform);

            if (data.level >= 3)
            {
                uiInGame.ShowItemMergeFx(transform.position, true);
            }
            else
            {
                uiInGame.ShowItemMergeFx(transform.position, false);
            }

            PlayScale(mergeUpgradeScaleSetting);

            onLevelUpgraded?.Invoke(this);
        }

        public void BreakBlock()
        {
            //Play vfx break block
            if (data.blockingLevel == 0)
            {
                var uiInGameView = UIManager.Instance.currentView as UIInGameView;
                uiInGameView.ShowBreakBlockFx(transform.position);
            }

            data.blockingLevel++;
            OnLevelUpdated();
        }

        public void BackToOriginal()
        {
            var pos = InGameManager.Instance.puzzlesController.BoardToScreenPos(data.boardPosition);

            //If it's can merge
            isMovingSelf = true;
            transform.DOMove(pos, 0.2f).SetEase(Ease.OutQuad)
                 .OnComplete(() =>
                 {
                     isMovingSelf = false;
                     transform.SetSiblingIndex(currentSiblingIndex);
                 });
        }


        public void MoveItemToTile(PuzzlesController.TileInfo newTile, Ease ease = Ease.OutQuad, float duration = 0.1f, Action onCompleted = null)
        {
            Move(newTile.worldPosition, true, false, ease, duration, onCompleted);
            newTile.puzzleItem = this;
            data.boardPosition = newTile.boardPosition;
        }

        public void Move(Vector2 endPosition, bool setParentTrans, bool bringToFront, Ease ease = Ease.OutQuad, float duration = 0.1f, Action onCompleted = null)
        {
            if (bringToFront)
                BringToFront();

            isMovingSelf = true;
            transform.DOMove(endPosition, duration).SetEase(ease).OnComplete(() =>
            {
                isMovingSelf = false;
                onCompleted?.Invoke();
            });
        }


        public void SwapItemInTile(PuzzlesController.TileInfo targetTile)
        {
            var puzzlesController = InGameManager.Instance.puzzlesController;

            var swappedItem = targetTile.puzzleItem;
            var thisTile = puzzlesController.GetTileInfo(data.boardPosition);

            MoveItemToTile(targetTile);

            swappedItem.BringToFront();
            swappedItem.MoveItemToTile(thisTile, Ease.OutQuad, 0.3f);
        }

        public void BringToFront()
        {
            transform.SetAsLastSibling();
        }

        public void HideItem(bool withTransition)
        {
            if (withTransition)
            {

            }
            else
            {
                transform.localPosition = Vector2.zero;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Call manually if we want to update the level without setting item
        /// </summary>
        public virtual void OnLevelUpdated()
        {
            var configPuzzle = ConfigManager.Instance.configPuzzle;

            //Debug.Log($"{config.id} update level ${data.level} at {holder.boardPosition}");

            var configDetail = config.configPerLevel[data.level];
            itemImage.sprite = configDetail.itemSprite;
            itemImage.rectTransform.SetSize(originalItemImageSize.GetValueOrDefault() * (1 + configDetail.sizeScale));

            itemImage.gameObject.SetActive(false);
            imageFullBlock.gameObject.SetActive(false);
            imageHalfBlock.gameObject.SetActive(false);

            if (data.blockingLevel == 0)//Full block
            {
                imageFullBlock.gameObject.SetActive(true);
                imageFullBlock.sprite = configPuzzle.configBlockingItems[data.blockingLevel].GetRandomSprite(); 
            }

            else if (data.blockingLevel == 1) //Half block
            {
                imageHalfBlock.gameObject.SetActive(true);
                itemImage.gameObject.SetActive(true);
                imageHalfBlock.sprite = configPuzzle.configBlockingItems[data.blockingLevel].GetRandomSprite();
            }
            else
            {
                itemImage.gameObject.SetActive(true);
            }
            
            maxLevelImage.gameObject.SetActive(!isBlock && isMaxLevel);
        }
    }
}
