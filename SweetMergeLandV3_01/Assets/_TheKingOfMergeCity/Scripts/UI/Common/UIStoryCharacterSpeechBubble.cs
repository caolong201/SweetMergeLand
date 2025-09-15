using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace TheKingOfMergeCity
{
    public class UIStoryCharacterSpeechBubble : MonoBehaviour
    {
        [SerializeField] TMP_Text speechText;
        [SerializeField] Transform avatarTrans;
        [SerializeField] Transform speechContainerTrans;

        public void Show(string speech)
        {
            gameObject.SetActive(true);
            StartCoroutine(CR_Show(speech));
        }

        IEnumerator CR_Show(string speech)
        {
            avatarTrans.localScale = Vector3.zero;
            speechContainerTrans.localScale = Vector3.zero;
            
            UpdateSpeech(speech);
                
            avatarTrans.DOScale(1, 0.4f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.3f);
            speechContainerTrans.DOScale(1, 0.4f).SetEase(Ease.OutBack);
        }

        public void UpdateSpeech(string speech)
        {
            speechText.text = speech;
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;
            
            StartCoroutine(CR_Hide());
        }
        
        IEnumerator CR_Hide()
        {
            speechContainerTrans.DOScale(0, 0.4f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.3f);
            var tween = avatarTrans.DOScale(0, 0.4f).SetEase(Ease.InBack);
            yield return tween.WaitForCompletion();
            gameObject.SetActive(false);
        }
    }
}
