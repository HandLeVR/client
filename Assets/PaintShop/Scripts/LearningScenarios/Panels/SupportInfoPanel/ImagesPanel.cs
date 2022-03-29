using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel that can display multiple images.
/// </summary>
public class ImagesPanel : SlidePanel
{
    public Image image;
    public List<Tuple<Media,Media>> images;
    
    protected override int GetSlideCount()
    {
        return images.Count;
    }

    protected override void SetSlideContent()
    {
        headerField.text = images[currentPage].Item1.name;
        image.sprite = TaskPreparationController.Instance.loadedImages[images[currentPage].Item1.id];

        if (images[currentPage].Item2 != null)
        {
            AudioClip audioClip = TaskPreparationController.Instance.loadedAudioClips[images[currentPage].Item2.id];
            VirtualInstructorController.Instance.Init(audioClip);
            VirtualInstructorController.Instance.active = true;
            VirtualInstructorController.Instance.Speak();
        }
    }
}
