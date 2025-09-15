using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity
{
    public class ElasticScale : MonoBehaviour
    {
        public event System.Action<ElasticScale> onPlayCompleted;

        [SerializeField] bool playOnStart;
        [SerializeField] float duration = 0.5f;
        [SerializeField] float endScale = 1;
        [SerializeField] AnimationCurve curveX;
        [SerializeField] AnimationCurve curveY;
        [SerializeField] AnimationCurve curveZ;
        [SerializeField] bool looping;
        [SerializeField] float loopInterval = 1f;

        Coroutine crPlay;
        float nextTimePlay;

        void Start()
        {
            if (playOnStart)
            {
                Play();
                nextTimePlay = Time.time + loopInterval + duration;
            }
        }
       
        void Update()
        {
            if (looping)
            {
                if (Time.time >= nextTimePlay)
                {
                    Play();

                    nextTimePlay = Time.time + loopInterval + duration;
                }
            }
        }

        public void Play(System.Action onCompleted = null)
        {
            if (crPlay != null)
                StopCoroutine(crPlay);

            crPlay = StartCoroutine(CR_Play(() =>
            {
                onPlayCompleted?.Invoke(this);
                onCompleted?.Invoke();
            }));
        }

        IEnumerator CR_Play(System.Action onCompleted)
        {
            float elapse = 0;
            var curScale = transform.localScale;
            while (elapse < duration)
            {
                var fraction = elapse / duration;
                curScale.x = curveX.Evaluate(fraction) * endScale;
                curScale.y = curveY.Evaluate(fraction) * endScale;
                curScale.z = curveZ.Evaluate(fraction) * endScale;

                transform.localScale = curScale;

                elapse += Time.deltaTime;
                yield return null;
            }

            transform.localScale = Vector3.one * endScale;
            onCompleted?.Invoke();
        }
    }
}
