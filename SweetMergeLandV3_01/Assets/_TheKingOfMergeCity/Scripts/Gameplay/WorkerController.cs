using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity
{
    public class WorkerController : MonoBehaviour
    {
        [SerializeField] GameObject model;
        [SerializeField] float modelAnimatorSpeed;

        Animator modelAnimator;

        public void Setup(GameObject modelPrefab)
        {
            
            if (model != null)
            {
                modelAnimator = model.GetComponent<Animator>();

                if (model.name != modelPrefab.name)
                {
                    Destroy(model);
                    model = Instantiate(modelPrefab, transform);
                    model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    modelAnimator = model.GetComponent<Animator>();
                }
            }

            modelAnimator.speed = modelAnimatorSpeed;

            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void Build()
        {
            modelAnimator.SetTrigger("Build");
        }

        public void Idle()
        {
            modelAnimator.SetTrigger("Idle");
        }

    }
}
