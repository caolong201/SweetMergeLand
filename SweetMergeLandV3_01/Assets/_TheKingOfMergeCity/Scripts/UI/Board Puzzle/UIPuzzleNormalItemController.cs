using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using USimpFramework.Animation.DOTweenExtension;

namespace TheKingOfMergeCity
{
    public class UIPuzzleNormalItemController : UIPuzzleItemController
    {
        [SerializeField] Image tickImage;
        [SerializeField] Image serveBackgroundImage;    
        
        public bool hasOrder { get; private set; }
        public bool pendingServe { get; private set; }

        protected override void ResetData()
        {
            base.ResetData();
            hasOrder = false;
            pendingServe = false;

            tickImage.gameObject.SetActive(false);
            serveBackgroundImage.gameObject.SetActive(false);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            tickImage.gameObject.SetActive(false);
            serveBackgroundImage.gameObject.SetActive(false);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            base.OnDrop(eventData);

            //Debug.Log($"item {transform.GetInstanceID()} at ({holder.boardPosition.y}, {holder.boardPosition.x}) On Dropped, has order {hasOrder} , pending serve {pendingServe}");
            tickImage.gameObject.SetActive(hasOrder);
            serveBackgroundImage.gameObject.SetActive(pendingServe);

        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            //Todo: Double click to make customer serve (if able to serve)

            base.OnPointerClick(eventData);

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            tickImage.transform.DOKill();
            serveBackgroundImage.transform.DOKill();
        }

        public virtual void SetCompletedOrder(bool hasOrder)
        {
            if (this.hasOrder == hasOrder)
                return;
            //Need to check one more time if hasOrder input is false
            if (!hasOrder)
            {
                //Check again in the customer
                if (InGameManager.Instance.customersController.HasFoodOrder(this))
                    return;
            }

            //Debug.Log($"item {transform.GetInstanceID()} at ({holder.boardPosition.y}, {holder.boardPosition.x}) set has order {hasOrder}");
            this.hasOrder = hasOrder;
            tickImage.transform.DOKill();
            if (hasOrder)
            {
                tickImage.transform.DOPopIn(0.4f);
            }
            else
            {
                tickImage.transform.DOPopOut(0.4f);
            }
        }

        public void HideTickAndBackground()
        {
            tickImage.gameObject.SetActive(false);
            serveBackgroundImage.gameObject.SetActive(false);
        }

        public virtual void SetPendingServe(bool pendingServe)
        {
            if (this.pendingServe == pendingServe)
                return;

            if (!pendingServe)
            {
                //Need to check if another customer is pending serve by this puzzle
                if (InGameManager.Instance.customersController.HasPendingServeCustomer(this))
                    return;
            }

            // Debug.Log($"item at {transform.GetInstanceID()} at ({holder.boardPosition.y},{holder.boardPosition.x}) set pending serve {pendingServe}");
            
            this.pendingServe = pendingServe;
            serveBackgroundImage.gameObject.SetActive(this.pendingServe);
        }
    }
}
