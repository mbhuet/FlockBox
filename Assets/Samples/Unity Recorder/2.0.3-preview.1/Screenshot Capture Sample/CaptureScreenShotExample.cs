#if UNITY_EDITOR

using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

namespace UnityEngine.Recorder.Examples
{
    /// <summary>
    /// This example shows how to setup a recording session via script.
    /// To use this example. Simply add the CaptureScreenShotExample component to a GameObject.
    /// 
    /// Entering playmode will display a "Capture ScreenShot" button.
    /// 
    /// Recorded images are saved in [Project Folder]/SampleRecordings
    /// </summary>
    public class CaptureScreenShotExample : MonoBehaviour
    {
       RecorderController m_RecorderController;

        public int width = 1920;
        public int height = 1080;
       void OnEnable()
       {
           var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
           m_RecorderController = new RecorderController(controllerSettings);
 
           var mediaOutputFolder = Path.Combine(Application.dataPath, "..", "SampleRecordings");

           // Image
           var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
           imageRecorder.name = "My Image Recorder";
           imageRecorder.enabled = true;
           imageRecorder.outputFormat = ImageRecorderOutputFormat.PNG;
           imageRecorder.captureAlpha = false;
           
           imageRecorder.outputFile = Path.Combine(mediaOutputFolder, "image_" + width + "x" + height + "_" + System.DateTime.Now.ToLongTimeString());
    
           imageRecorder.imageInputSettings = new GameViewInputSettings
           {
               outputWidth = width,
               outputHeight = height,
           };
    
           // Setup Recording
           controllerSettings.AddRecorderSettings(imageRecorder);
           controllerSettings.SetRecordModeToSingleFrame(0);
       }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Screenshot Captured");
                m_RecorderController.StartRecording();
            }
        }


    }
 }
    
 #endif
