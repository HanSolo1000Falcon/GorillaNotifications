using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GorillaLocomotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaNotifications.Core;

public class NotificationController : MonoBehaviour
{
    private const float HeightOffset = 62f;

    internal static NotificationController Instance;

    internal readonly Dictionary<FontType, (Font, TMP_FontAsset)> FontCache           = [];
    internal readonly List<NotificationEntry>                     NotificationEntries = [];
    internal readonly List<TextPoolEntry>                         TextPool            = [];

    private void Awake() => Instance = this;

    private void Start()
    {
        Stream bundleStream = Assembly.GetExecutingAssembly()
                                      .GetManifestResourceStream("GorillaNotifications.Resources.gnotifications");

        AssetBundle bundle = AssetBundle.LoadFromStream(bundleStream);
        bundleStream?.Close();
        GameObject worldSpaceCanvas = Instantiate(bundle.LoadAsset<GameObject>("WorldSpaceCanvas"),
                GTPlayer.Instance.headCollider.transform);

        worldSpaceCanvas.transform.localScale    = Vector3.one * 0.001f;
        worldSpaceCanvas.transform.localPosition = new Vector3(0f, 0f, 1f);
        worldSpaceCanvas.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);

        worldSpaceCanvas.SetLayer(UnityLayer.FirstPersonOnly);

        GameObject pcScreenCanvas = Instantiate(bundle.LoadAsset<GameObject>("PCScreenCanvas"));

        TextPoolEntry textPoolEntry = new(worldSpaceCanvas.GetComponentInChildren<Text>(),
                pcScreenCanvas.GetComponentInChildren<TextMeshProUGUI>());

        TextPool.Add(textPoolEntry);

        FontCache.Add(FontType.JetBrains_Mono,
                (bundle.LoadAsset<Font>("JetBrainsMono-ExtraBold"),
                 bundle.LoadAsset<TMP_FontAsset>("JetBrainsMono-ExtraBold SDF")));

        FontCache.Add(FontType.Bit_Cell,
                (bundle.LoadAsset<Font>("bitcell_memesbruh03"),
                 bundle.LoadAsset<TMP_FontAsset>("bitcell_memesbruh03 SDF")));

        FontCache.Add(FontType.VCR_OSD_Mono,
                (bundle.LoadAsset<Font>("VCR_OSD_MONO"), bundle.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO SDF")));

        FontCache.Add(FontType.Pix32, (bundle.LoadAsset<Font>("Pix32"), bundle.LoadAsset<TMP_FontAsset>("Pix32 SDF")));
        FontCache.Add(FontType.Pixellari,
                (bundle.LoadAsset<Font>("Pixellari"), bundle.LoadAsset<TMP_FontAsset>("Pixellari SDF")));
    }

    /// <summary>
    ///     Sends a notification with the specified source, message, duration, and font type.
    /// </summary>
    /// <param name="source">The source of the notification, indicating where or who the notification originated from.</param>
    /// <param name="notification">The message or content of the notification.</param>
    /// <param name="duration">The duration (in seconds) that the notification will remain visible.</param>
    /// <returns>
    ///     A new instance of <see cref="NotificationController.NotificationEntry" /> representing the created
    ///     notification.
    /// </returns>
    public static NotificationEntry SendNotification(string source, string notification, float duration) =>
            new(source, notification, duration, Instance.GetAvailableTextPoolEntry(), FontType.JetBrains_Mono, []);

    /// <summary>
    ///     Sends a notification with the specified source, message, and duration.
    /// </summary>
    /// <param name="source">The origin or identifier of the notification.</param>
    /// <param name="notification">The content or message to be displayed in the notification.</param>
    /// <param name="duration">The time, in seconds, for which the notification will be visible.</param>
    /// <param name="fontType">The font type to use for the notification.</param>
    /// <returns>
    ///     A new instance of <see cref="NotificationController.NotificationEntry" /> representing the created notification.
    /// </returns>
    public static NotificationEntry SendNotification(string   source, string notification, float duration,
                                                     FontType fontType) => new(source, notification, duration,
            Instance.GetAvailableTextPoolEntry(), fontType,
            []);

    /// <summary>
    ///     Sends a notification with the specified source, message, duration, and optional font style or styling options.
    /// </summary>
    /// <param name="source">The source of the notification, indicating where or who the notification originated from.</param>
    /// <param name="notification">The message or content of the notification.</param>
    /// <param name="duration">The duration (in seconds) that the notification will remain visible.</param>
    /// <param name="stylingOptions">Optional styling options to apply to the notification.</param>
    /// <returns>
    ///     A new instance of <see cref="NotificationController.NotificationEntry" /> representing the created notification.
    /// </returns>
    public static NotificationEntry SendNotification(string source, string notification, float duration,
                                                     params StylingOptions[] stylingOptions) => new(source,
            notification, duration, Instance.GetAvailableTextPoolEntry(), FontType.JetBrains_Mono,
            stylingOptions);

    /// <summary>
    ///     Sends a notification with the specified source, message, duration, font type, and styling options.
    /// </summary>
    /// <param name="source">The source of the notification, indicating where or who the notification originated from.</param>
    /// <param name="notification">The message or content of the notification.</param>
    /// <param name="duration">The duration (in seconds) that the notification will remain visible.</param>
    /// <param name="fontType">The font type to be used for the notification text.</param>
    /// <param name="stylingOptions">An array of optional styling settings to apply to the appearance of the notification.</param>
    /// <returns>
    ///     A new instance of <see cref="NotificationController.NotificationEntry" /> representing the created
    ///     notification.
    /// </returns>
    public static NotificationEntry SendNotification(string   source,   string notification, float duration,
                                                     FontType fontType, params StylingOptions[] stylingOptions) => new(
            source, notification, duration, Instance.GetAvailableTextPoolEntry(), fontType,
            stylingOptions);

    private TextPoolEntry GetAvailableTextPoolEntry()
    {
        if (TextPool.Count > 1)
        {
            TextPoolEntry entry = TextPool[^1];
            TextPool.RemoveAt(TextPool.Count - 1);

            return entry;
        }

        Text vrText = Instantiate(TextPool[0].Text.transform.parent, TextPool[0].Text.transform.parent.parent)
               .GetComponentInChildren<Text>();

        TextMeshProUGUI pcText =
                Instantiate(TextPool[0].TextMesh.transform.parent, TextPool[0].TextMesh.transform.parent.parent)
                       .GetComponentInChildren<TextMeshProUGUI>();

        return new TextPoolEntry(vrText, pcText);
    }

    internal void ReorderTextLines()
    {
        int desktopIndex = 0;
        int vrIndex      = 0;

        for (int i = NotificationEntries.Count - 1; i >= 0; i--)
        {
            NotificationEntry entry   = NotificationEntries[i];
            StylingOptions[]  styling = entry.CachedStylingOptions;

            if (styling == null)
            {
                entry.AssociatedTextPoolEntry.TextMesh.transform.parent.position =
                        new Vector3(0f, desktopIndex * HeightOffset, 0f);

                entry.AssociatedTextPoolEntry.Text.transform.parent.localPosition =
                        new Vector3(-500f, vrIndex * HeightOffset - 515f, 0f);

                desktopIndex++;
                vrIndex++;
            }
            else
            {
                if (styling.Contains(StylingOptions.OnlyDesktop))
                {
                    entry.AssociatedTextPoolEntry.TextMesh.transform.parent.position =
                            new Vector3(0f, desktopIndex * HeightOffset, 0f);

                    desktopIndex++;
                }
                else if (styling.Contains(StylingOptions.OnlyVR))
                {
                    entry.AssociatedTextPoolEntry.Text.transform.parent.localPosition =
                            new Vector3(-500f, vrIndex * HeightOffset - 515f, 0f);

                    vrIndex++;
                }
                else
                {
                    entry.AssociatedTextPoolEntry.TextMesh.transform.parent.position =
                            new Vector3(0f, desktopIndex * HeightOffset, 0f);

                    entry.AssociatedTextPoolEntry.Text.transform.parent.localPosition =
                            new Vector3(-500f, vrIndex * HeightOffset - 515f, 0f);

                    desktopIndex++;
                    vrIndex++;
                }
            }
        }
    }

    internal IEnumerator NotificationLifetimeRoutine(NotificationEntry entry)
    {
        while (entry.Duration > -1f)
        {
            if (entry.HasBeenDestroyed)
                yield break;

            entry.Duration -= Time.deltaTime;

            if (entry.Duration <= 0f)
            {
                entry.AssociatedTextPoolEntry.Text.color     = new Color(1f, 1f, 1f, 1f + entry.Duration);
                entry.AssociatedTextPoolEntry.TextMesh.color = new Color(1f, 1f, 1f, 1f + entry.Duration);
            }
            else
            {
                entry.AssociatedTextPoolEntry.Text.color     = Color.white;
                entry.AssociatedTextPoolEntry.TextMesh.color = Color.white;
            }

            yield return null;
        }

        entry.RemoveNotification();
    }

    internal struct TextPoolEntry(Text text, TextMeshProUGUI textMesh)
    {
        public readonly Text            Text     = text;
        public readonly TextMeshProUGUI TextMesh = textMesh;
    }
}