using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity
{
    public class UIHomeDebug : MonoBehaviour
    {
        [SerializeField] UIAreaCompleted uiAreaCompleted;
        
        public void PressOpenAreaCompleted()
        {
            uiAreaCompleted.Show(0);
        }
        
        public void PressAddExp()
        {
            UserManager.Instance.AddCurrencyAmount(Enum.CurrencyType.Exp, 5, true, true);
        }
    }
}
