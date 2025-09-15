using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using USimpFramework.Utility;
using UnityEngine;

namespace USimpFramework.UIGachaEffect
{
    [System.Serializable]
    public struct GachaEffectSetting
    {
        public string id;
        public GameObject itemPrefab;
        public List<ItemCountPerAmountSetting> itemCountPerAmountSettings;
        public Vector2 spreadRadiusRange;
        public float spreadDuration;
        public float moveToTargetDuration;
        public Vector2 offsetMoveToTargetDurationRange;
        public bool scaleDown;
    }


    [System.Serializable]
    public struct ItemCountPerAmountSetting
    {
        public int minAmount;
        public Vector2Int countRange;
    }

    [System.Serializable]
    public struct ParabolaEffectSetting
    {
        public string id;
        public GameObject itemPrefab;

        //Move module
        public float moveDuration;
        public List<ItemCountPerAmountSetting> itemCountPerAmountSettings;
        public float delayBetweenSpawn;
        public float height;
        
        //Rotate
        public RotateOvertimeModuleSetting rotateOvertimeModuleSetting;
    }

    [System.Serializable]
    public struct RotateOvertimeModuleSetting
    {
        public bool enable;
        public float speed;
        public bool counterClockwise;
    }

    public class UIGachaEffect : SimpleSingleton<UIGachaEffect>
    {

        [SerializeField] List<GachaEffectSetting> settings;
        [SerializeField] List<ParabolaEffectSetting> parabolaEffectSettings;

        /// <summary>
        /// Play gacha effect with id defined in the UI gahca effect game object
        /// </summary>
        /// <param name="settingId">The setting id, defined in UI gacha effet game object</param>
        /// <param name="onCompleted">Completed is call when the first item reach the target transform</param>
        public void PlayGachaEffect(string settingId, int amount, Vector3 startPosition, Vector3 endPosition, float startScale = 1, System.Action onFirstItemCompleted = null, System.Action onAllCompleted = null)
        {
            var setting = settings.Find(s => s.id == settingId);
            if (string.IsNullOrEmpty(setting.id))
                throw new UnityException("Dont find the setting for " + settingId);

            PlayGachaEffect(setting, amount, startPosition, endPosition, startScale, onFirstItemCompleted, onAllCompleted);
        }

        /// <summary>
        /// Play gacha effect with particular setting
        /// </summary>
        /// <param name="setting">The setting  defined in UI gacha effet game object</param>
        /// <param name="onCompleted">Completed is call when the first item reach the target transform</param>
        public void PlayGachaEffect(GachaEffectSetting setting, int amount, Vector3 startPosition, Vector3 endPosition, float startScale = 1, System.Action onFirstItemCompleted = null, System.Action onAllCompleted = null)
        {
            StartCoroutine(CR_Play());

            IEnumerator CR_Play()
            {
                yield return CR_PlayGachaEffect(setting, amount, startPosition, endPosition, startScale);
                onFirstItemCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Play gacha effect with setting id
        /// </summary>
        /// <param name="settingId">The setting id, defined in UI gacha effet game object</param>
        /// <returns></returns>
        public IEnumerator CR_PlayGachaEffect(string settingId, int amount, Vector3 startPosition, Vector3 endPosition, float startScale)
        {
            var setting = settings.Find(s => s.id == settingId);
            if (string.IsNullOrEmpty(setting.id))
                throw new UnityException("Dont find the setting for " + settingId);

            return CR_PlayGachaEffect(setting, amount, startPosition, endPosition, startScale);
        }

        /// <summary>
        /// Play gacha effect with particular setting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public IEnumerator CR_PlayGachaEffect(GachaEffectSetting setting, int amount, Vector3 startPosition, Vector3 endPosition, float startScale)
        {
            var itemCountPerAmountSetting = GetItemCountPerAmount(amount, setting.itemCountPerAmountSettings);
            int itemCount = Random.Range(itemCountPerAmountSetting.countRange.x,itemCountPerAmountSetting.countRange.y + 1);
            float spreadDuration = setting.spreadDuration;

            for (int i = 0; i < itemCount; i++)
            {
                var spreadPos = (Vector2)startPosition + Random.insideUnitCircle * Random.Range(setting.spreadRadiusRange.x, setting.spreadRadiusRange.y);
                var itemIns = SimpleObjectPool.Spawn(setting.itemPrefab, transform);
                itemIns.SetActive(true);
                itemIns.transform.position = startPosition;
                itemIns.transform.localScale = Vector3.one * startScale;

                var sequence = DOTween.Sequence();

                sequence.Append(itemIns.transform.DOMove(spreadPos, spreadDuration).SetEase(Ease.OutBack).
                    OnStart(() =>
                    {
                        if (startScale != 1 && setting.scaleDown)
                            itemIns.transform.DOScale(1, spreadDuration).SetEase(Ease.InOutBack);
                    }));
                sequence.Append(itemIns.transform.DOMove(endPosition, setting.moveToTargetDuration + Random.Range(setting.offsetMoveToTargetDurationRange.x, setting.offsetMoveToTargetDurationRange.y)).SetEase(Ease.InBack));
                sequence.OnComplete(() => SimpleObjectPool.Despawn(itemIns, moveToPoolContainer: false));
            }

            yield return new WaitForSeconds(spreadDuration + setting.moveToTargetDuration);
        }

        public void PlayParabolaEffect(string settingId, Vector3 startPosition, Vector3 endPosition, int amount, System.Action onItemCompleted = null, System.Action onAllCompleted = null)
        {
            var setting = parabolaEffectSettings.Find(s => s.id == settingId);

            if (setting.id == "")
                throw new UnityException($"Play parabolaf effect failed! Invalid setting {settingId}");

            StartCoroutine(CR_PlayParabolaEffectSequencely(setting, startPosition, endPosition, amount, onItemCompleted, onAllCompleted));
        }

        public IEnumerator CR_PlayParabolaEffectSequencely(ParabolaEffectSetting setting,Vector3 startPosition, Vector3 endPosition, int amount,  System.Action onItemCompleted  = null, System.Action onAllCompleted = null)
        {
            var itemCountPerAmountSetting = GetItemCountPerAmount(amount, setting.itemCountPerAmountSettings);
            var itemCount = Random.Range(itemCountPerAmountSetting.countRange.x, itemCountPerAmountSetting.countRange.y + 1);
            int count = 0;
            var waitBetweenSpawn = new WaitForSeconds(setting.delayBetweenSpawn);
            for (int i = 0; i < itemCount; i++)
            {
                var itemIns = SimpleObjectPool.Spawn(setting.itemPrefab, transform);
                itemIns.transform.position = startPosition;
                itemIns.transform.rotation = Quaternion.identity;
                //Move
                StartCoroutine(CR_MoveParabola(itemIns.transform, endPosition, setting.moveDuration, setting.height, () =>
                {
                    itemIns.transform.DOKill();
                    SimpleObjectPool.Despawn(itemIns, moveToPoolContainer: false);

                    onItemCompleted?.Invoke();
                    count++;
                }));

                //Rotate
                var rotateSetting = setting.rotateOvertimeModuleSetting;
                if (rotateSetting.enable)
                {
                    itemIns.transform.DORotate(new Vector3(0, 0, rotateSetting.counterClockwise ? -1 : 1) * 360, 
                        rotateSetting.speed, RotateMode.LocalAxisAdd).SetLoops(-1).SetEase(Ease.Linear).SetSpeedBased(true);
                }

                yield return waitBetweenSpawn;
            }
            yield return new WaitUntil(() => count == itemCount);
            onAllCompleted?.Invoke();

            IEnumerator CR_MoveParabola(Transform trans, Vector3 endPos, float duration, float height, System.Action onCompleted)
            {
                float elapsedTime = 0;
                Vector3 startPos = trans.position;
                while (elapsedTime < duration)
                {
                    float fraction = elapsedTime / duration;
                    var curPos = Vector3.Lerp(startPos, endPos, fraction);
                    float xHeight = Mathf.Sin(Mathf.PI * fraction) * height;
                    curPos.x -= xHeight;
                    trans.position = curPos;
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                onCompleted?.Invoke();
            }
        }

        ItemCountPerAmountSetting GetItemCountPerAmount(int amount, List<ItemCountPerAmountSetting> settings)
        {
            ItemCountPerAmountSetting itemCountPerAmountSetting = new() { countRange = new() };
            for (int i = 0; i < settings.Count; i++)
            {
                var itemSetting = settings[i];

                if (amount <= itemSetting.minAmount)
                {
                    itemCountPerAmountSetting = itemSetting;
                    break;
                }
            }

            if (itemCountPerAmountSetting.minAmount == 0)
                itemCountPerAmountSetting = settings[^1];

            return itemCountPerAmountSetting;
        }

    }
}
