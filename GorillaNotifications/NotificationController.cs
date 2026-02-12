using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GorillaLocomotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaNotifications;

public class NotificationController : MonoBehaviour
{
    private const float PCHeightOffset = 53.065f;
    private const float VRHeightOffset = 0.054f;
    
    private static   NotificationController  instance;
    private readonly List<NotificationEntry> notificationEntries = [];
    private readonly List<TextPoolEntry> textPool = [];

    private readonly Dictionary<FontType, (Font, TMP_FontAsset)> fontCache = [];

    private void Awake() => instance = this;

    private void Start()
    {
        Stream bundleStream = Assembly.GetExecutingAssembly()
                                      .GetManifestResourceStream("GorillaNotifications.Resources.gnotifications");

        AssetBundle bundle = AssetBundle.LoadFromStream(bundleStream);
        bundleStream?.Close();
        GameObject worldSpaceCanvas = Instantiate(bundle.LoadAsset<GameObject>("WorldSpaceCanvas"),
                GTPlayer.Instance.headCollider.transform);

        worldSpaceCanvas.transform.localPosition = new Vector3(0f, 0f, 1f);
        worldSpaceCanvas.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        
        worldSpaceCanvas.SetLayer(UnityLayer.FirstPersonOnly);

        GameObject pcScreenCanvas = Instantiate(bundle.LoadAsset<GameObject>("PCScreenCanvas"));
        
        TextPoolEntry textPoolEntry = new(worldSpaceCanvas.GetComponentInChildren<Text>(), pcScreenCanvas.GetComponentInChildren<TextMeshProUGUI>());
        textPool.Add(textPoolEntry);
        
        fontCache.Add(FontType.JetBrains_Mono, (bundle.LoadAsset<Font>("JetBrainsMono-ExtraBold"), bundle.LoadAsset<TMP_FontAsset>("JetBrainsMono-ExtraBold SDF")));
        fontCache.Add(FontType.Bit_Cell, (bundle.LoadAsset<Font>("bitcell_memesbruh03"), bundle.LoadAsset<TMP_FontAsset>("bitcell_memesbruh03 SDF")));
        fontCache.Add(FontType.VCR_OSD_Mono, (bundle.LoadAsset<Font>("VCR_OSD_MONO"), bundle.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO SDF")));
        fontCache.Add(FontType.Pix32, (bundle.LoadAsset<Font>("Pix32"), bundle.LoadAsset<TMP_FontAsset>("Pix32 SDF")));
        fontCache.Add(FontType.Pixellari, (bundle.LoadAsset<Font>("Pixellari"), bundle.LoadAsset<TMP_FontAsset>("Pixellari SDF")));
    }

    /// <summary>
    /// Sends a notification with the specified source, message, duration, and font type.
    /// </summary>
    /// <param name="source">The source of the notification, indicating where or who the notification originated from.</param>
    /// <param name="notification">The message or content of the notification.</param>
    /// <param name="duration">The duration (in seconds) that the notification will remain visible.</param>
    /// <param name="fontType">The font type used for the notification. Defaults to JetBrainsMono.</param>
    /// <returns>A new instance of <see cref="NotificationController.NotificationEntry"/> representing the created notification.</returns>
    public static NotificationEntry SendNotification(string   source, string notification, float duration, FontType fontType = FontType.JetBrains_Mono) =>
            new(source, notification, duration, instance.GetAvailableTextPoolEntry(), fontType);

    private TextPoolEntry GetAvailableTextPoolEntry()
    {
        if (textPool.Count > 1)
        {
            TextPoolEntry entry = textPool[^1];
            textPool.RemoveAt(textPool.Count - 1);
            return entry;
        }

        Text            vrText = Instantiate(textPool[0].Text,     textPool[0].Text.transform.parent);
        TextMeshProUGUI pcText = Instantiate(textPool[0].TextMesh, textPool[0].TextMesh.transform.parent);

        return new TextPoolEntry(vrText, pcText);
    }


    private void ReorderTextLines()
    {
        for (int i = notificationEntries.Count - 1; i >= 0; i--)
        {
            int iMapped = notificationEntries.Count - i - 1;
            notificationEntries[i].AssociatedTextPoolEntry.Text.transform.localPosition =
                    new Vector3(-0.5f, iMapped * VRHeightOffset - 0.5f, 0f);
            notificationEntries[i].AssociatedTextPoolEntry.TextMesh.transform.position =
                    new Vector3(0f, iMapped * PCHeightOffset, 0f);
        }
    }

    internal struct TextPoolEntry(Text text, TextMeshProUGUI textMesh)
    {
        public readonly Text Text = text;
        public readonly TextMeshProUGUI TextMesh = textMesh;
    }

    private IEnumerator NotificationLifetimeRoutine(NotificationEntry entry)
    {
        while (entry.Duration > -1f)
        {
            if (entry.HasBeenDestroyed)
                yield break;
            
            entry.Duration -= Time.deltaTime;

            if (entry.Duration <= 0f)
            {
                entry.AssociatedTextPoolEntry.Text.color = new Color(1f, 1f, 1f, 1f + entry.Duration);
                entry.AssociatedTextPoolEntry.TextMesh.color = new Color(1f, 1f, 1f, 1f + entry.Duration);
            }
            else
            {
                entry.AssociatedTextPoolEntry.Text.color = Color.white;
                entry.AssociatedTextPoolEntry.TextMesh.color = Color.white;
            }
            
            yield return null;
        }
        
        entry.RemoveNotification();
    }

    public sealed class NotificationEntry
    {
        internal NotificationEntry(string source, string notification, float duration, TextPoolEntry textPoolEntry, FontType fontType)
        {
            if (notification.IsNullOrEmpty())
                return;

            Source        = source;
            Notification  = notification;
            Duration      = duration;
            AssociatedTextPoolEntry = textPoolEntry;
            
            textPoolEntry.Text.text = $"[{Source}] {Notification}";
            textPoolEntry.TextMesh.text = $"[{Source}] {Notification}";
            
            textPoolEntry.Text.font = instance.fontCache[fontType].Item1;
            textPoolEntry.TextMesh.font = instance.fontCache[fontType].Item2;
            
            instance.notificationEntries.Add(this);
            instance.ReorderTextLines();
            instance.StartCoroutine(instance.NotificationLifetimeRoutine(this));
        }

        public string Source       { get; internal set; }
        public string Notification { get; internal set; }
        public float  Duration     { get; internal set; }

        internal bool HasBeenDestroyed;
        internal TextPoolEntry AssociatedTextPoolEntry;

        /// <summary>
        ///     Updates the notification with new source, message, and duration values.
        /// </summary>
        /// <param name="newSource">The new source of the notification, indicating where or who the notification originated from.</param>
        /// <param name="newNotification">The new message or content to update the notification with.</param>
        /// <param name="newDuration">The new duration (in seconds) that the notification will remain visible.</param>
        public void UpdateNotification(string newSource, string newNotification, float newDuration)
        {
            Source        = newSource;
            Notification  = newNotification;
            Duration      = newDuration;
            
            AssociatedTextPoolEntry.Text.text     = $"[{Source}] {Notification}";
            AssociatedTextPoolEntry.TextMesh.text = $"[{Source}] {Notification}";
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
            instance.notificationEntries.Remove(this);
            instance.textPool.Add(AssociatedTextPoolEntry);
            AssociatedTextPoolEntry.Text.text = "";
            AssociatedTextPoolEntry.TextMesh.text = "";
            HasBeenDestroyed = true;
        }
    }

    public enum FontType
    {
        JetBrains_Mono,
        Bit_Cell,
        VCR_OSD_Mono,
        Pix32,
        Pixellari,
    }
}