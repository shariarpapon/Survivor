using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class GameUtility
{
    public static readonly HashSet<int> ObjectsCurrentlyTweening = new HashSet<int>();

    private static int DebugCount = 1000;

    #region Item Functions
    public static void SwapSlot(ItemSlot from, ItemSlot to)
    {
        if (from == to) return;

        Item triggerItem = from.slotItem;
        Item targetItem = to.slotItem;

        int triggerItemCount = from.itemCount;

        if (!from.IsEmpty)
        {

            if (to.IsEmpty || targetItem == triggerItem)
            {
                for (int i = 0; i < triggerItemCount; i++)
                {
                    if (to.TryAdd(triggerItem)) from.Remove(false);
                    else return;
                }
            }
            else
            {
                if (!SlotAndItemInterchangebale(from, to)) return;
            
                int targetItemCount = to.itemCount;

                to.Clear();
                for (int i = 0; i < triggerItemCount; i++) to.TryAdd(triggerItem);

                from.Clear();
                for (int i = 0; i < targetItemCount; i++) from.TryAdd(targetItem);
            }
        }
    }

    public static void TransferItem(ItemSlot from, ItemSlot to, int amount)
    {
        if ((!to.IsEmpty && to.slotItem != from.slotItem) || from.IsEmpty || !SlotAndItemInterchangebale(from, to)) return;

        for (int i = 0; i < amount; i++)
        {
            if (!from.IsEmpty) 
            {
                if (to.TryAdd(from.slotItem)) from.Remove(false);
                else return;
            }
        }
    }

    public static bool SlotAndItemInterchangebale(ItemSlot a, ItemSlot b) 
    {
        if (a.GetType() == typeof(ToolSlot) || b.GetType() == typeof(ToolSlot))
            if (!a.IsEmpty && !b.IsEmpty && b.slotItem.itemData.type != a.slotItem.itemData.type) return false;

        return true;
    }
    #endregion
    
    #region String Search Functions

    //returns the index of searchField that has the best string match with input.
    public static int SearchStringArray(string input, string[] searchField) 
    {
        input = input.Replace(" ", string.Empty);
        input = input.ToLower();
        float matchWeight = 0;
        int matchIndex = -1;
        for (int i = 0; i < searchField.Length; i++) 
        {
            string search = searchField[i].Replace(" ", string.Empty).ToLower();
            if (search == input) return i;
            else 
            {
                float weight = StringMatchWeight(search, input);
                if (weight >= matchWeight) 
                {
                    matchWeight = weight;
                    matchIndex = i; 
                }
            }
        }
        return matchIndex;
    }

    //returns a float weight based on hwo well they match, higher weight mean better match.
    public static float StringMatchWeight(string a, string b) 
    {
        if (a.Length == 0 || b.Length == 0) return -1;

        a = a.Replace(" ", string.Empty);
        b = b.Replace(" ", string.Empty);
        string c = a;

        if (a.Length > b.Length) {
            a = b;
            b = c;
        }
        float weightedMatches = 0;
        for (int i = 0; i < a.Length; i++) 
            if (a[i] == b[i]) weightedMatches++;

        //Normalize the match weight first
        return SigmoidVariantInterpolate(weightedMatches / a.Length);
    }

    #endregion

    #region Math Functions
    public const float E = 2.71828182845f;

    public static float LerpBell(float t, float min, float max) 
    {
        return (max - 1) * -Mathf.Pow(2*t-1, 2) + (min + max - 1);
    }

    public static float SigmoidVariantInterpolate(float t) 
    {
        return 1.0f / (1.0f + Mathf.Exp(t * -13.0f + 6.0f));
    }

    public static float SigmoidInterpolate(float t, float b, float c) 
    {
        return 1 / (1 + Mathf.Exp(-t * b + c));
    }

    public static bool InBetweenOrEqual(float value, float min, float max)
    {
        return value >= min && value <= max;
    }

    public static int Index2Dto1D(int x, int y, int sizeY) 
    {
        return y * sizeY + x;
    }

    public static float MaxVec3Component(Vector3 vec)
    {
        return Mathf.Max(Mathf.Max(vec.x, vec.y), vec.z);
    }

    public static void SmoothRotate(Transform transform, Vector3 direction, float dt)
    {
        Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
        rot = Quaternion.Lerp(transform.rotation, rot, dt);
        transform.rotation = rot;
    }

    public static bool InBetween(Vector3 current, Vector3 pointA, Vector3 pointB) 
    {
        Vector2 min = new Vector2(Mathf.Min(pointA.x, pointB.x), Mathf.Min(pointA.y, pointB.y));
        Vector2 max = new Vector2(Mathf.Max(pointA.x, pointB.x), Mathf.Max(pointA.y, pointB.y));
        return current.x < max.x && current.x > min.x && current.y < max.y && current.y > min.y;
    }

    public static float RoundDecimal(float value, int place) 
    {
        int placeFac = (int)Mathf.Pow(10, place);
        return ((int)(value * placeFac)) / (float)placeFac;
    }
    #endregion

    #region Tweening
    public static IEnumerator DelayExecute(float delay, System.Action action) 
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public static IEnumerator TweenUISlideOutRight(GameObject obj, float durationInFrames, bool destroy) 
    {
        if (obj != null) 
        {
            ObjectsCurrentlyTweening.Add(obj.GetInstanceID());
            float frame = 0;
            Vector3 pos = obj.transform.position;
            Vector3 target = new Vector3(Screen.width * 2, pos.y, 0);
            while (frame <= durationInFrames) 
            {
                obj.transform.position = Vector3.Lerp(pos, target, frame / durationInFrames);
                frame++;
                yield return null;
            }
            if (destroy) GameObject.Destroy(obj);
            ObjectsCurrentlyTweening.Remove(obj.GetInstanceID());
        }
    }

    public static IEnumerator TweenUISlideOutLeft(GameObject obj, float durationInFrames, bool destroy)
    {
        if (obj != null)
        {
            ObjectsCurrentlyTweening.Add(obj.GetInstanceID());
            float frame = 0;
            Vector3 pos = obj.transform.position;
            Vector3 target = new Vector3(-Screen.width * 2, pos.y, 0);
            while (frame <= durationInFrames)
            {
                obj.transform.position = Vector3.Lerp(pos, target, frame / durationInFrames);
                frame++;
                yield return null;
            }
            ObjectsCurrentlyTweening.Remove(obj.GetInstanceID());
            if (destroy) GameObject.Destroy(obj);
            else obj.SetActive(false);
        }
    }

    public static IEnumerator TweenScaleBell(GameObject obj, float minFac, float maxFac,  float durationInFrames)
    {
        if (!ObjectsCurrentlyTweening.Contains(obj.GetInstanceID()))
        {
            ObjectsCurrentlyTweening.Add(obj.GetInstanceID());
            float frame = 0;
            Vector3 scale = obj.transform.localScale;
            while (frame <= durationInFrames)
            {
                obj.transform.localScale = scale * LerpBell(frame / durationInFrames, minFac, maxFac);
                frame++;
                yield return null;
            }
            obj.transform.localScale = scale;
            ObjectsCurrentlyTweening.Remove(obj.GetInstanceID());
        }
    }

    public static IEnumerator TweenScaleIn(GameObject obj, float durationInFrames, Vector3 maxScale)
    {
        if (!ObjectsCurrentlyTweening.Contains(obj.GetInstanceID()))
        {
            ObjectsCurrentlyTweening.Add(obj.GetInstanceID());
            Transform tf = obj.transform;
            tf.localScale = Vector3.zero;
            tf.gameObject.SetActive(true);

            float frame = 0;
            while (frame <= durationInFrames)
            {
                tf.localScale = Vector3.Lerp(Vector3.zero, maxScale, frame / durationInFrames);
                frame++;
                yield return null;
            }
            ObjectsCurrentlyTweening.Remove(obj.GetInstanceID());
        }
    }

    public static IEnumerator TweenScaleOut(GameObject obj, float durationInFrames, bool destroy)
    {
        if (!ObjectsCurrentlyTweening.Contains(obj.GetInstanceID()))
        {
            ObjectsCurrentlyTweening.Add(obj.GetInstanceID());
            float frame = 0;
            Vector3 scale = obj.transform.localScale;
            while (frame <= durationInFrames)
            {
                if (obj != null)
                {
                    obj.transform.localScale = Vector3.Lerp(scale, Vector3.zero, frame / durationInFrames);
                }
                frame++;
                yield return null;
            }
            ObjectsCurrentlyTweening.Remove(obj.GetInstanceID());
            if (!destroy) obj.SetActive(false);
            else GameObject.Destroy(obj);
        }
    }

    public static IEnumerator TweenUIFadeIn(GameObject obj, float durationInFrames)
    {
        if (!ObjectsCurrentlyTweening.Contains(obj.GetInstanceID()))
        {
            ObjectsCurrentlyTweening.Add(obj.GetInstanceID());
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            obj.SetActive(true);
            float frame = 0;
            while (frame <= durationInFrames)
            {
                cg.alpha = Mathf.Lerp(0, 1, frame / durationInFrames);
                frame++;
                yield return null;
            }
            ObjectsCurrentlyTweening.Remove(obj.GetInstanceID());
        }
    }

    public static IEnumerator TweenUIFadeOut(GameObject obj, float durationInFrames, bool destroy)
    {
        if (!ObjectsCurrentlyTweening.Contains(obj.GetInstanceID()))
        {
            ObjectsCurrentlyTweening.Add(obj.GetInstanceID());
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            float alpha = cg.alpha;
            obj.SetActive(true);
            float frame = 0;
            while (frame <= durationInFrames)
            {
                cg.alpha = Mathf.Lerp(alpha, 0, frame / durationInFrames);
                frame++;
                yield return null;
            }
            ObjectsCurrentlyTweening.Remove(obj.GetInstanceID());
            if (destroy) GameObject.Destroy(obj);
            else obj.SetActive(false);
        }
    }

    public static IEnumerator TweenScaleUp(Transform tf, float speed, float scaleFactor, float durationInFrames)
    {
        if (!ObjectsCurrentlyTweening.Contains(tf.GetInstanceID()))
        {
            ObjectsCurrentlyTweening.Add(tf.GetInstanceID());

            Vector3 curr = tf.localScale;
            Vector3 target = tf.localScale * scaleFactor;

            float frame = 0;
            while (frame <= durationInFrames)
            {
                tf.localScale = Vector3.Lerp(curr, target, frame / durationInFrames);
                frame += speed;
                yield return null;
            }

            ObjectsCurrentlyTweening.Remove(tf.GetInstanceID());
        }
    }

    public static IEnumerator TweenScaleDown(Transform tf, float speed, float durationInFrames)
    {
        Vector3 curr = tf.localScale;
        Vector3 target = Vector3.one;

        float frame = 0;
        while (frame <= durationInFrames)
        {
            tf.localScale = Vector3.Lerp(curr, target, frame / durationInFrames);
            frame += speed;
            yield return null;
        }
    }

    #endregion

    #region Game Functionality

    public static GameObject InstantiateGameObject(GameObject gameObject, Vector3 pos, Quaternion rot, Transform parent) 
    {
        return Object.Instantiate(gameObject, pos, rot, parent);
    }

    public static void InstantiateItemGrabber(Item item, Vector3 position)
    {
        Vector3 targetSize = Vector3.one * 0.75f;
        GameObject inst = GameObject.Instantiate(item.itemData.prefab, position, Quaternion.identity);
        float maxSizeComponent = MaxVec3Component(inst.GetComponent<MeshRenderer>().bounds.size);

        inst.transform.localScale = inst.transform.localScale * (MaxVec3Component(targetSize) * maxSizeComponent);

        if (inst.TryGetComponent<Interactable>(out Interactable i)) Object.Destroy(i);

        inst.GetComponent<Collider>().isTrigger = true;
        inst.AddComponent<ItemGrabber>().Create(item);
    }

    public static void SaveScreenshot() 
    {
        string path = Application.persistentDataPath + $"/{System.DateTime.Now.ToFileTimeUtc()}.png";
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Screenshot Captured");
    }

    public static void ShakeCamera(Transform camera) 
    {
        //Sahek camera
    }

    #endregion

    #region Debugging

#if UNITY_EDITOR
    public static void LimitedDebug(string msg) 
    {
        if (DebugCount <= 0) return;
        Debug.Log(msg);
        DebugCount--;
    }

    public static void PrintArray<T>(T[] array) 
    {
        Debug.Log($"<color=#e6bc98>Printing List - {array.ToString()}:"+"</color>");
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < array.Length; i++)
            builder.AppendLine($"[{i}] {array[i]}");

        Debug.Log(builder.ToString());
    }

    public static void PrintArray<T>(List<T> list)
    {
        PrintArray(list.ToArray());
    }
#endif

    #endregion
}
