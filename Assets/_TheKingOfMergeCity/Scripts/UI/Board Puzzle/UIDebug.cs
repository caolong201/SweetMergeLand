using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TheKingOfMergeCity
{
    using System;
    using Enum;

    [DefaultExecutionOrder(150)]
    public class UIDebug : MonoBehaviour
    {

        [SerializeField] GameObject groupGridIndexText;
        [SerializeField] TMP_Text textPrefab;
        [SerializeField] Transform colContainer;
        [SerializeField] Transform rowContainer;
        [SerializeField] Toggle revealItemToggle;

        void Start()
        {
            textPrefab.gameObject.SetActive(false);

            var boardSize = ConfigManager.Instance.configPuzzle.boardSize;
            for (int i = 0; i < boardSize.x; i++)
            {
                var text = Instantiate(textPrefab, colContainer);
                text.text = i.ToString();
                text.gameObject.SetActive(true);
            }

            for (int j = 0; j < boardSize.y; j++)
            {
                var text = Instantiate(textPrefab, rowContainer);
                text.text = j.ToString();
                text.gameObject.SetActive(true);
            }

            revealItemToggle.onValueChanged.AddListener(OnRevealItemToggleValueChanged);
            revealItemToggle.isOn = false;
        }

        void OnRevealItemToggleValueChanged(bool isOn)
        {
            groupGridIndexText.SetActive(isOn);

            //Review item hide hunder the puzzle
            var allPuzzles = InGameManager.Instance.puzzlesController.GetAllPuzzles();
            allPuzzles.ForEach(p => p.RevealItem(isOn));
        }

        public void PressCheatAddEnergy(int amount)
        {
            UserManager.Instance.AddCurrencyAmount(CurrencyType.Energy, amount, true, true);
        }

        public void PressCheatAddStar(int amount)
        {
            UserManager.Instance.AddCurrencyAmount(CurrencyType.Star, amount, true, true);
        }

        public void PressCheatAddGem(int amount)
        {
            UserManager.Instance.AddCurrencyAmount(CurrencyType.Gem, amount, true, true);
        }

        public void PressTestAd()
        {
            ApplovinManager.Instance.ShowRewardedAd(isSuccess =>
            {
                if (isSuccess)
                    Debug.Log("Show rewarded ad success");
            });
        }
    }
}
