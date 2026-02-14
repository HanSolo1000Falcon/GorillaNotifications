using System.Linq;
using GorillaNotifications.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaNotifications.Core;

public sealed class NotificationEntry
{
    internal NotificationController.TextPoolEntry AssociatedTextPoolEntry;

    internal readonly StylingOptions[] CachedStylingOptions;

    internal bool HasBeenDestroyed;

    internal NotificationEntry(string                               source, string notification, float duration,
                               NotificationController.TextPoolEntry textPoolEntry,
                               FontType                             fontType, StylingOptions[] stylingOptions)
    {
        if (notification.IsNullOrEmpty())
            return;

        Source                  = source;
        Notification            = notification;
        Duration                = duration;
        AssociatedTextPoolEntry = textPoolEntry;

        textPoolEntry.Text.text     = $"[{Source}] {Notification}";
        textPoolEntry.TextMesh.text = $"[{Source}] {Notification}";

        textPoolEntry.Text.font     = NotificationController.Instance.FontCache[fontType].Item1;
        textPoolEntry.TextMesh.font = NotificationController.Instance.FontCache[fontType].Item2;
        
        textPoolEntry.TextMesh.transform.parent.gameObject.SetActive(true);
        textPoolEntry.Text.transform.parent.gameObject.SetActive(true);

        CachedStylingOptions = stylingOptions;

        NotificationController.Instance.NotificationEntries.Add(this);
        NotificationController.Instance.ReorderTextLines();
        NotificationController.Instance.StartCoroutine(
                NotificationController.Instance.NotificationLifetimeRoutine(this));

        // ReSharper disable once InvertIf
        if (stylingOptions != null)
        {
            textPoolEntry.TextMesh.ForceMeshUpdate();

            if (stylingOptions.Contains(StylingOptions.BlackBox))
            {
                if (!stylingOptions.Contains(StylingOptions.OnlyVR))
                {
                    Transform textTransform = textPoolEntry.TextMesh.transform;

                    GameObject blackBox = textTransform.parent.TakeChild(0, 0).gameObject;
                    blackBox.SetActive(true);

                    textPoolEntry.TextMesh.ForceMeshUpdate();
                    Bounds textBounds = textPoolEntry.TextMesh.textBounds;

                    RectTransform rect = blackBox.GetComponent<RectTransform>();
                    rect.sizeDelta = textBounds.size + new Vector3(10f, 10f, 1f);
                    rect.position  = textBounds.center;
                }

                if (!stylingOptions.Contains(StylingOptions.OnlyDesktop))
                {
                    Transform textTransform = textPoolEntry.Text.transform;

                    GameObject blackBox = textTransform.parent.TakeChild(0, 0).gameObject;
                    blackBox.SetActive(true);

                    Text legacyText = textPoolEntry.Text;

                    TextGenerator generator = legacyText.cachedTextGenerator;
                    TextGenerationSettings settings =
                            legacyText.GetGenerationSettings(legacyText.rectTransform.rect.size);

                    generator.Populate(legacyText.text, settings);
                    Rect textRectLocal = generator.rectExtents;

                    RectTransform boxRect = blackBox.GetComponent<RectTransform>();
                    boxRect.sizeDelta = textRectLocal.size + new Vector2(10f, 10f);

                    Vector3 worldCenter = legacyText.rectTransform.TransformPoint(textRectLocal.center);
                    boxRect.position = worldCenter;
                }
            }

            if (stylingOptions.Contains(StylingOptions.OnlyVR))
                textPoolEntry.TextMesh.transform.parent.gameObject.SetActive(false);

            // ReSharper disable once InvertIf
            if (stylingOptions.Contains(StylingOptions.OnlyDesktop))
                textPoolEntry.Text.transform.parent.gameObject.SetActive(false);
        }
    }

    public string Source       { get; internal set; }
    public string Notification { get; internal set; }
    public float  Duration     { get; internal set; }

    /// <summary>
    ///     Updates the notification with new source, message, and duration values.
    /// </summary>
    /// <param name="newSource">The new source of the notification, indicating where or who the notification originated from.</param>
    /// <param name="newNotification">The new message or content to update the notification with.</param>
    /// <param name="newDuration">The new duration (in seconds) that the notification will remain visible.</param>
    public void UpdateNotification(string newSource, string newNotification, float newDuration)
    {
        Source       = newSource;
        Notification = newNotification;
        Duration     = newDuration;
        
            AssociatedTextPoolEntry.Text.text = $"[{Source}] {Notification}";
            AssociatedTextPoolEntry.TextMesh.text = $"[{Source}] {Notification}";

        NotificationController.Instance.NotificationEntries.Remove(this);
        NotificationController.Instance.NotificationEntries.Add(this);
        NotificationController.Instance.ReorderTextLines();
    }

    /// <summary>
    ///     Updates the notification with a new message value, while retaining the source and duration.
    /// </summary>
    /// <param name="newNotification">The new message or content to update the notification with.</param>
    public void UpdateNotification(string newNotification) => UpdateNotification(Source, newNotification, Duration);

    /// <summary>
    ///     Updates the duration of the notification while retaining the current source and message.
    /// </summary>
    /// <param name="newDuration">The new duration (in seconds) for the notification to remain visible.</param>
    public void UpdateDuration(float newDuration) => UpdateNotification(Source, Notification, newDuration);

    /// <summary>
    ///     Updates the source of the notification while retaining the current message and duration.
    /// </summary>
    /// <param name="newSource">The new source of the notification, indicating where or who the notification originated from.</param>
    public void UpdateSource(string newSource) => UpdateNotification(newSource, Notification, Duration);

    /// <summary>
    ///     Removes the notification from the active list and refreshes the display.
    /// </summary>
    public void RemoveNotification()
    {
        NotificationController.Instance.NotificationEntries.Remove(this);
        NotificationController.Instance.TextPool.Add(AssociatedTextPoolEntry);
        NotificationController.Instance.ReorderTextLines();

        AssociatedTextPoolEntry.Text.text     = "";
        AssociatedTextPoolEntry.TextMesh.text = "";

        // disabling optional styles
        foreach (Transform child in AssociatedTextPoolEntry.Text.transform.parent.TakeChild(0))
            child.gameObject.SetActive(false);

        foreach (Transform child in AssociatedTextPoolEntry.TextMesh.transform.parent.TakeChild(0))
            child.gameObject.SetActive(false);

        HasBeenDestroyed = true;
    }
}