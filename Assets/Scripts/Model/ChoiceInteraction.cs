using System.Collections.Generic;

public class ChoiceInteraction
{
    public string label;
    public bool shuffle;
    public int maxChoices;
    public int minChoices;
    public string orientation;
    public List<SimpleChoice> simpleChoices = new List<SimpleChoice>();

    public ChoiceInteraction(string label, bool shuffle, int maxChoices, int minChoices, string orientation,
        List<SimpleChoice> simpleChoices)
    {
        this.label = label;
        this.shuffle = shuffle;
        this.maxChoices = maxChoices;
        this.minChoices = minChoices;
        this.orientation = orientation;
        this.simpleChoices = simpleChoices;
    }

    public ChoiceInteraction()
    {
        
    }
}
