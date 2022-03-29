using translator;

/// <summary>
/// Extends the "normal" tooltip to allow usage of a translation key as a tooltip text.
/// The key is translated at runtime.
/// </summary>
public class TooltipObjectTranslation : TooltipObject
{
    /// <summary>
    /// Translates the given key and shows the tooltip.
    /// </summary>
    protected override void ShowTooltip(string text)
    {
        base.ShowTooltip(TranslationController.Instance.Translate(text));
    }
}
