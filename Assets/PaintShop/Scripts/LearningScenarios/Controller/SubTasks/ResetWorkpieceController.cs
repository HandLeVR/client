using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Controller for the reset workpiece sub task. This task has no user interaction and used resets the task on base
/// of a recording or custom settings.
/// </summary>
public class ResetWorkpieceController : VRSubTaskController
{
    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        string type = GetStringFromJSON("type", jsonObject);
            
        // save state of the workpiece to allow resetting the workpiece to the state before this sub task
        RenderTexture heightmapOutput = ApplicationController.Instance.currentWorkpieceGameObject
            .GetComponentInChildren<CustomDrawable>().heightmapOutput;
        LearningScenariosTaskController.Instance.workpieceData.Add(new WorkpieceData
        {
            workpiece = ApplicationController.Instance.currentSelectedWorkpiece,
            coat = PaintController.Instance.chosenCoat,
            baseCoat = PaintController.Instance.chosenBaseCoat,
            texture = TextureSaver.ToTexture2D(heightmapOutput),
            evaluationData = EvaluationController.Instance.GetAsEvaluationData()
        });

        if (type == "recording")
        {
            long recordingId = GetLongFromJSON("recordingId", jsonObject);
            Recording recording = TaskPreparationController.Instance.loadedRecordings[recordingId];
            PlayRecordingController.Instance.LoadRecording(recording, ExecuteAfterLoading, false);
        }
        else
        {
            int workpieceId = (int) jsonObject.GetValue("workpieceId");
            
            int coatId = (int) jsonObject.GetValue("coatId");
            string condition = GetStringFromJSON("coatCondition", jsonObject);
            LoadBaseCoat(coatId, condition == "dry");
            ApplicationController.Instance.SpawnWorkpiece(DataController.Instance.workpieces[workpieceId]);
            // needs to wait one frames to finish workpiece creation
            StartCoroutine(WaitFor.Frames(1,FinishSubTask));
        }
    }

    private void ExecuteAfterLoading()
    {
        PlayRecordingController.Instance.LoadResultFromRecording();
        // needs to wait one frames to finish workpiece creation
        StartCoroutine(WaitFor.Frames(1,FinishSubTask));
    }

}

/// <summary>
/// Used to save the data of the workpiece before the reset workpiece sub task is executed. This allows to reset the
/// state of the workpiece if the user selects the return coin. If the selection of a return coin would result in
/// returning to the reset workpiece sub task, the application automatically returns one sub task further back and
/// resets the state of the workpiece with the help of the saved data.
/// </summary>
public class WorkpieceData
{
    public Workpiece workpiece;
    public Texture2D texture;
    public Coat baseCoat;
    public Coat coat;
    public EvaluationData evaluationData;
}
