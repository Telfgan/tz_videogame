
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class PhysKitEditor
    {
        [InitializeOnLoadMethod]
        public static void InitOnLoadMethod()
        {
            StartCoroutine(InitializeAndLoad());
        }

        [MenuItem("Help/Heathen/Phys Kit/Update System Core (Package Manager)", priority = 1)]
        public static void InstallSysCoreMenuItem()
        {
            if (!SessionState.GetBool("SysCoreInstall", false))
            {
                StartCoroutine(InstallSystemCore());
            }
        }

        [MenuItem("Help/Heathen/Phys Kit/Documentation", priority = 2)]
        public static void Documentation()
        {
            Application.OpenURL("https://kb.heathenengineering.com/assets/physkit");
        }

        [MenuItem("Help/Heathen/Phys Kit/Support", priority = 3)]
        public static void Support()
        {
            Application.OpenURL("https://discord.gg/RMGtDXV");
        }

        private static IEnumerator InitializeAndLoad()
        {
            yield return null;

            if (!SessionState.GetBool("PhsyKitInstallCheck", false))
            {
                SessionState.SetBool("PhsyKitInstallCheck", true);
#if !HE_SYSCORE
                if (EditorUtility.DisplayDialog("Heathen Installer", "System Core does not appear to be installed, and is a requirement for PhysKit to work properly. Would you like to install System Core?", "Install", "No"))
                {
                    yield return null;
                    AddRequest sysProc = null;

                    if (!SessionState.GetBool("SysCoreInstall", false))
                    {
                        SessionState.SetBool("SysCoreInstall", true);
                        sysProc = Client.Add("https://github.com/heathen-engineering/SystemCore.git?path=/com.heathen.systemcore");
                    }

                    if (sysProc.Status == StatusCode.Failure)
                        Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
                    else if (sysProc.Status == StatusCode.Success)
                        Debug.Log("System Core " + sysProc.Result.version + " installation complete");
                    else
                    {
                        Debug.Log("Installing System Core ...");
                        while (sysProc.Status == StatusCode.InProgress)
                        {
                            yield return null;
                        }
                    }

                    if (sysProc.Status == StatusCode.Failure)
                        Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
                    else if (sysProc.Status == StatusCode.Success)
                        Debug.Log("System Core " + sysProc.Result.version + " installation complete");

                    SessionState.SetBool("SysCoreInstall", false);
                }
#endif
            }
        }

        private static IEnumerator InstallSystemCore()
        {
            yield return null;
            AddRequest sysProc = null;

            if (!SessionState.GetBool("SysCoreInstall", false))
            {
                SessionState.SetBool("SysCoreInstall", true);
                sysProc = Client.Add("https://github.com/heathen-engineering/SystemCore.git?path=/com.heathen.systemcore");
            }

            if (sysProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
            else if (sysProc.Status == StatusCode.Success)
                Debug.Log("System Core " + sysProc.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing System Core ...");
                while (sysProc.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (sysProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
            else if (sysProc.Status == StatusCode.Success)
                Debug.Log("System Core " + sysProc.Result.version + " installation complete");

            SessionState.SetBool("SysCoreInstall", false);
        }

        private static List<IEnumerator> cooroutines;

        private static void StartCoroutine(IEnumerator handle)
        {
            if (cooroutines == null)
            {
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                cooroutines = new List<IEnumerator>();
            }

            cooroutines.Add(handle);
        }


        private static void EditorUpdate()
        {
            List<IEnumerator> done = new List<IEnumerator>();

            if (cooroutines != null)
            {
                foreach (var e in cooroutines)
                {
                    if (!e.MoveNext())
                        done.Add(e);
                    else
                    {
                        if (e.Current != null)
                            Debug.Log(e.Current.ToString());
                    }
                }
            }

            foreach (var d in done)
                cooroutines.Remove(d);
        }
    }
}