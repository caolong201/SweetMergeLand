using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;

namespace TheKingOfMergeCity
{
    public class FloatingObject : MonoBehaviour
    {
        [SerializeField] float heightOffset = 5f;
        [SerializeField] float speed = 10f;
        public float delay;

        float timeSinceFloating;
        float timeStartFloating;
        float firstPosY = -1;
        
        void Start()
        {
            timeStartFloating = Time.time + delay;
        }

        void OnEnable()
        { 
            timeStartFloating = Time.time + delay;
            timeSinceFloating = 0;
        }

        public void BackDefault()
        {
            var pos = transform.position;
            pos.y = firstPosY;
            transform.position = pos;
        }

        void Update()
        {
            if (Time.time > timeStartFloating)
            {
                if (Mathf.Approximately(firstPosY, -1))
                {
                    firstPosY = transform.position.y;
                }
                
                var pos = transform.position;
                pos.y = firstPosY + heightOffset * Mathf.Sin(timeSinceFloating * speed);
                transform.position = pos;
                timeSinceFloating += Time.deltaTime * speed;
            }
        }

    }
}
