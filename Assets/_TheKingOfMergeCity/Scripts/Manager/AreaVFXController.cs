using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USimpFramework.Utility;

namespace TheKingOfMergeCity
{
    [DisallowMultipleComponent]
    public class AreaVFXController : MonoBehaviour
    {
        [Header("VFX")]
        [SerializeField] ParticleSystem buildingVfxPrefab;
        [SerializeField] ParticleSystem buildingCompleteVfxPrefab;

        public void PlayBuildingVfx(Vector3 position)
        {
            var buildingVfx = SimpleObjectPool.Spawn(buildingVfxPrefab);
            buildingVfx.transform.position = position;
            buildingVfx.Play();

            DOVirtual.DelayedCall(0.6f, () => SimpleObjectPool.Despawn(buildingVfx));
        }

        public void PlayBuildingCompletedVfx(Vector3 position)
        {
            var buildingCompleteVfx = SimpleObjectPool.Spawn(buildingCompleteVfxPrefab);
            buildingCompleteVfx.transform.position = position;
            buildingCompleteVfx.Play();

            DOVirtual.DelayedCall(1.5f, () => SimpleObjectPool.Despawn(buildingCompleteVfx));
        }
    }
}
