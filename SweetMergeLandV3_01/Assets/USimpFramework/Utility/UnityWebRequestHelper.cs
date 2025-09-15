using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace USimpFramework.Utility
{
    public static class UnityWebRequestHelper
    {
        /// <summary> GET data from unity web request, note that this function will return the request, you should dispose it manually when done using  </summary>
        public static async Task<UnityWebRequest> GetDataAsync(string url, Dictionary<string, string> headers = null)
        {

#if UNITY_EDITOR
            Debug.Log($"GET: {url}");
#endif
            var request = UnityWebRequest.Get(url);

            if (headers != null)
            {
                foreach (var keyValue in headers)
                {
                    request.SetRequestHeader(keyValue.Key, keyValue.Value);
                }
            }

            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
                await Task.Yield();

#if UNITY_EDITOR
            Debug.Log($"Response: {request.downloadHandler.text}");
#endif
            return request;
        }

        /// <summary>PUT data from unity web request, note that this function will return the request, you should dispose it manually when done using  </summary>
        public static async Task<UnityWebRequest> PutDataAsync(string url, string bodyData, Dictionary<string, string> headers = null)
        {
#if UNITY_EDITOR
            Debug.Log($"PUT: {url} \n, request body: {bodyData}");
#endif
            var request = UnityWebRequest.Put(url, bodyData);
            request.SetRequestHeader("Content-Type", "application/json");
            if (headers != null)
            {
                foreach (var keyValue in headers)
                {
                    request.SetRequestHeader(keyValue.Key, keyValue.Value);
                }
            }

            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
                await Task.Yield();

#if UNITY_EDITOR
            Debug.Log($"Response: {request.downloadHandler.text}");
#endif
            return request;
        }

        /// <summary>POST data from unity web request, note that this function will return the request, you should dispose it manually when done using  </summary>
        public static async Task<UnityWebRequest> PostDataAsync(string url, string bodyData, Dictionary<string, string> headers = null)
        {
#if UNITY_EDITOR
            Debug.Log($"POST: {url} \n, request body: {bodyData}");
#endif
            var request = UnityWebRequest.Post(url, bodyData, "application/json");
            if (headers != null)
            {
                foreach (var keyValue in headers)
                {
                    request.SetRequestHeader(keyValue.Key, keyValue.Value);
                }
            }

            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
                await Task.Yield();

#if UNITY_EDITOR
            Debug.Log($"Response: {request.downloadHandler.text}");
#endif
            return request;
        }


        /// <summary>DELETE data from unity web request, note that this function will return the request, you should dispose it manually when done using  </summary>
        public static async Task<UnityWebRequest> DeleteDataAsync(string url, Dictionary<string, string> headers = null)
        {
#if UNITY_EDITOR
            Debug.Log($"DELETE: {url}");
#endif

            var request = UnityWebRequest.Delete(url);
            if (headers != null)
            {
                foreach (var keyValue in headers)
                    request.SetRequestHeader(keyValue.Key, keyValue.Value);
            }

            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
                await Task.Yield();

#if UNITY_EDITOR
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Response: OK ");
            }
            else
            {
                Debug.Log($"Response: {request.error}");
            }
#endif

            return request;
        }


        public static IEnumerator CR_GetData(string url, Action<string> completed)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException(request.error);
            }
            else
            {
                string txt = request.downloadHandler.text;
                completed?.Invoke(txt);
                request.Dispose();
            }
        }
    }
}
