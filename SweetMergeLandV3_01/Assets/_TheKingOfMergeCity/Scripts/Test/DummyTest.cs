using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity
{
    public class DummyTest : MonoBehaviour
    {
        /*  [SerializeField] ElasticScale elasticScale;

          [SerializeField] Transform move;
          [SerializeField] Transform end;
          [SerializeField] Transform target;
          [SerializeField] AnimationCurve scaleCurveX;
          [SerializeField] AnimationCurve scaleCurveY;
          [SerializeField] AnimationCurve scaleCurveZ;

          void Update()
          {
              if (Input.GetKeyDown(KeyCode.A))
              {
                  elasticScale.Play();
              }

              if (Input.GetKeyDown(KeyCode.J))
              {
                  move.DOJump(end.position, 10, 1, 0.5f).SetEase(Ease.Linear);
              }

              if (Input.GetKeyDown(KeyCode.S))
              {

                  StartCoroutine(CR_Test());
              }

              IEnumerator CR_Test()
              {
                  target.transform.localScale = Vector3.zero;
                  for (int i = 0; i < 4; i++)
                  {
                      target.DOKill();
                      target.DOScaleX(1, 0.2f).SetEase(scaleCurveX);
                      target.DOScaleY(1, 0.2f).SetEase(scaleCurveY);
                      target.DOScaleZ(1, 0.2f).SetEase(scaleCurveZ);

                      yield return new WaitForSeconds(1f);
                  }
              }
          }*/

        [SerializeField] Transform target;
        [SerializeField] float rotateSpeed;
        [SerializeField] bool counterClockwise;

        void Start()
        {
            target.DORotate(new Vector3(0, 0, (counterClockwise ? -1 : 1) * 360f), rotateSpeed, RotateMode.LocalAxisAdd).SetSpeedBased(true).SetLoops(-1).SetEase(Ease.Linear);
        }
        
    }
}
