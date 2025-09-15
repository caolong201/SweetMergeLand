using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheKingOfMergeCity
{
    using Model;

    public class UIPuzzleItemHolderController : MonoBehaviour
    {
        public UIPuzzleItemController item { get; private set; }

        public BoardPosition boardPosition { get; private set; }


        public bool isEmpty => item == null;


        public void Setup(BoardPosition boardPosition)
        {
            this.boardPosition = boardPosition;
        }

        public void SetItem(UIPuzzleItemController item)
        {
            this.item = item;
        }

    }
}
