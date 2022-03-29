using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using Newtonsoft.Json.Linq;

/// <summary>
/// Controls the displayment of different support info panels.
/// </summary>
public class SupportInfoSummaryPanel : MonoBehaviour
{
    public VideoPlayerPanel videoPlayerPanel;
    public SuccessCriteriaPanel successCriteriaPanel;
    public QualityCriteriaPanel qualityCriteriaPanel;
    public TextsPanel textsPanel;
    public ImagesPanel imagesPanel;
    public AudioClip visualVerificationAudio;
    public VideoClip visualVerificationVideo;
    private UnityAction returnToPanelAction;

    public void ShowSupportInfoPanel(SupportInfo supportInfo, bool stopEducationMaster = true)
    {
        if (stopEducationMaster)
            VirtualInstructorController.Instance.Stop();
        if (supportInfo.type == "Visual Verification")
        {
            LearningScenariosMonitorController.Instance.ChangePanel(videoPlayerPanel.gameObject);
            VirtualInstructorController.Instance.Init(visualVerificationAudio);
            videoPlayerPanel.onSeek.AddListener(VirtualInstructorController.Instance.Play);
            videoPlayerPanel.onPause.AddListener(VirtualInstructorController.Instance.Pause);
            videoPlayerPanel.PlayClip(visualVerificationVideo);
        }
        else if (supportInfo.type == "Video")
        {
            LearningScenariosMonitorController.Instance.ChangePanel(videoPlayerPanel.gameObject);
            JObject jsonObject = JObject.Parse(supportInfo.properties);
            Media video = DataController.Instance.media[(long) jsonObject.GetValue("videoId")];
            videoPlayerPanel.PlayClip(video.GetPath());
        }
        else
        {
            if (supportInfo.type == "Success Criteria")
            {
                JObject jsonObject = JObject.Parse(supportInfo.properties);
                successCriteriaPanel.coatUsage = (string) jsonObject.GetValue("coatUsage");
                successCriteriaPanel.targetThickness = (string) jsonObject.GetValue("targetThickness");
            }
            else if (supportInfo.type == "Quality Criteria")
            {
                JObject jsonObject = JObject.Parse(supportInfo.properties);
                qualityCriteriaPanel.sequential = (bool) jsonObject.GetValue("sequential");
            }
            else if (supportInfo.type == "Texts")
            {
                JObject jsonObject = JObject.Parse(supportInfo.properties);
                List<string> texts = new List<string>();
                foreach (JToken text in jsonObject.GetValue("texts"))
                    texts.Add((string)text);
                textsPanel.texts = texts;
            }
            else if (supportInfo.type == "Images")
            {
                JObject jsonObject = JObject.Parse(supportInfo.properties);
                List<Tuple<Media,Media>> images = new List<Tuple<Media,Media>>();
                foreach (JToken imageJson in jsonObject.GetValue("images"))
                {
                    Media image = DataController.Instance.media[(long) imageJson["imageId"]];
                    Media audio = imageJson["audioId"] != null
                        ? DataController.Instance.media[(long) imageJson["audioId"]]
                        : null;
                    images.Add(new Tuple<Media, Media>(image,audio));
                }
                imagesPanel.images = images;
            }

            LearningScenariosMonitorController.Instance.ChangePanel(supportInfo.type);
        }
    }
}