using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace MFPSEditor
{
    [InitializeOnLoad]
    public class MFPSEditorHTTPListener
    {
        static HttpListener listener;
        static Thread listenerThread;
        public static bool isRunning = false;
        private static readonly int port = 52888; // Port number for the HTTP server

        static MFPSEditorHTTPListener()
        {
            if (isRunning) return;
            isRunning = true;

            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add($"http://localhost:{port}/"); // see port note below
                listener.Start();

                listenerThread = new Thread(HandleRequests);
                listenerThread.Start();

                //Debug.Log($"MFPS HTTP Server started at http://localhost:{port}/");
            }
            catch (Exception)
            {
                //  Debug.LogError("MFPS HTTP Server failed to start: " + ex.Message);
            }
        }

        static void HandleRequests()
        {
            if (listener == null) return;

            while (listener.IsListening)
            {
                if (listener == null) break; // Check if listener is null to avoid NullReferenceException

                try
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    string command = request.Url.AbsolutePath;
                    if (command == "/ping") HandlePing(request, response);
                    else if (command == "/menuitem") HandleMenuItem(request, response);
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        using (var writer = new StreamWriter(response.OutputStream))
                            writer.Write("404 Not Found");
                    }

                    response.Close();
                }
                catch { break; }
            }
        }

        private static void HandlePing(HttpListenerRequest request, HttpListenerResponse response)
        {
            var query = request.QueryString["path"];
            if (!string.IsNullOrEmpty(query))
            {
                string assetPath = query.Replace("\\", "/");
                MFPSHTTPThreadDispatcher.RunOnMainThread(() =>
                {
                    var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (asset != null)
                    {
                        EditorGUIUtility.PingObject(asset);
                        Selection.activeObject = asset;
                    }
                    else
                    {
                        Debug.LogWarning($"Asset not found at path: {assetPath}");
                    }
                });

                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");

                using (var writer = new StreamWriter(response.OutputStream))
                    writer.Write("Pinged: " + assetPath);
            }
        }

        private static void HandleMenuItem(HttpListenerRequest request, HttpListenerResponse response)
        {
            var query = request.QueryString["path"];
            if (!string.IsNullOrEmpty(query))
            {
                string assetPath = query.Replace("\\", "/");
                MFPSHTTPThreadDispatcher.RunOnMainThread(() =>
                {
                    EditorApplication.ExecuteMenuItem(assetPath);
                });

                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");

                using (var writer = new StreamWriter(response.OutputStream))
                    writer.Write("Executed: " + assetPath);
            }
        }

        [InitializeOnLoadMethod]
        static void Init()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
            EditorApplication.quitting += OnBeforeQuit;
        }

        static void OnBeforeReload()
        {
            Shutdown();
        }

        static void OnBeforeQuit()
        {
            Shutdown();
        }

        static void Shutdown()
        {
            try
            {
                listener?.Stop();
                listener?.Close();
                listener = null;

                listenerThread?.Abort();
                listenerThread = null;

                isRunning = false;

                //   Debug.Log("Unity HTTP Server shut down.");
            }
            catch (Exception)
            {
                // Debug.LogError("Failed to shut down HTTP server: " + ex.Message);
            }
        }
    }

    [InitializeOnLoad]
    public static class MFPSHTTPThreadDispatcher
    {
        static readonly Queue<Action> queue = new Queue<Action>();

        static MFPSHTTPThreadDispatcher()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        public static void RunOnMainThread(Action action)
        {
            lock (queue) queue.Enqueue(action);
        }

        static void Update()
        {
            lock (queue)
            {
                while (queue.Count > 0)
                    queue.Dequeue()?.Invoke();
            }
        }
    }
}