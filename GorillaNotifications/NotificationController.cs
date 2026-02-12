using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GorillaLocomotion;
using GorillaNotifications.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaNotifications;

public class NotificationController : MonoBehaviour
{
    private static   NotificationController  instance;
    private readonly List<NotificationEntry> notificationEntries = [];
    private          TextMeshProUGUI         pcScreenNotificationText;

    private Text worldSpaceNotificationText;

    private void Awake() => instance = this;

    private void Start()
    {
        Stream bundleStream = Assembly.GetExecutingAssembly()
                                      .GetManifestResourceStream("GorillaNotifications.Resources.gnotifications");

        AssetBundle bundle = AssetBundle.LoadFromStream(bundleStream);
        bundleStream?.Close();
        GameObject worldSpaceCanvas = Instantiate(bundle.LoadAsset<GameObject>("WorldSpaceCanvas"),
                GTPlayer.Instance.headCollider.transform);

        worldSpaceCanvas.transform.localPosition = new Vector3(-0.25f, -0.2f, 1f);
        worldSpaceCanvas.transform.localRotation = Quaternion.Euler(20f, -20f, 0f);
        worldSpaceCanvas.SetLayer(UnityLayer.FirstPersonOnly);
        worldSpaceNotificationText               = worldSpaceCanvas.GetComponentInChildren<Text>();

        GameObject pcScreenCanvas = Instantiate(bundle.LoadAsset<GameObject>("PCScreenCanvas"));
        pcScreenNotificationText = pcScreenCanvas.GetComponentInChildren<TextMeshProUGUI>();
    }

    private IEnumerator RemoveNotification(NotificationEntry entry)
    {
        yield return new WaitForSeconds(entry.Duration);
        notificationEntries.Remove(entry);
        RefreshText();
    }

    private void RefreshText()
    {
        worldSpaceNotificationText.text = notificationEntries
                                         .Select(entry =>
                                                         $"[{entry.Source}] {entry.Notification}".NormaliseNotification(
                                                                 50)).Join("\n");

        pcScreenNotificationText.text = notificationEntries
                                       .Select(entry => $"[{entry.Source}] {entry.Notification}".NormaliseNotification(
                                                       Screen.width.MapForScreenWidth())).Join("\n");
    }

    /// <summary>
    ///     Sends a notification to be displayed with the specified source, message, and duration.
    /// </summary>
    /// <param name="source">The source of the notification, indicating where or who the notification originated from.</param>
    /// <param name="notification">The message or content of the notification.</param>
    /// <param name="duration">The duration (in seconds) that the notification will remain visible.</param>
    /// <returns>
    ///     A <see cref="NotificationEntry" /> object that represents the notification and allows further updates or
    ///     removal.
    /// </returns>
    public static NotificationEntry SendNotification(string source, string notification, float duration) =>
            new(source, notification, duration);

    public sealed class NotificationEntry
    {
        private Coroutine removeRoutine;

        internal NotificationEntry(string source, string notification, float duration)
        {
            if (notification.IsNullOrEmpty())
                return;

            Source        = source;
            Notification  = notification;
            Duration      = duration;
            removeRoutine = instance.StartCoroutine(instance.RemoveNotification(this));
            instance.notificationEntries.Add(this);
            instance.RefreshText();
        }

        public string Source       { get; private set; }
        public string Notification { get; private set; }
        public float  Duration     { get; private set; }

        /// <summary>
        ///     Updates the notification with new source, message, and duration values.
        /// </summary>
        /// <param name="newSource">The new source of the notification, indicating where or who the notification originated from.</param>
        /// <param name="newNotification">The new message or content to update the notification with.</param>
        /// <param name="newDuration">The new duration (in seconds) that the notification will remain visible.</param>
        public void UpdateNotification(string newSource, string newNotification, float newDuration)
        {
            instance.StopCoroutine(removeRoutine);
            Source        = newSource;
            Notification  = newNotification;
            Duration      = newDuration;
            instance.RefreshText();
            removeRoutine = instance.StartCoroutine(instance.RemoveNotification(this));
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
            instance.StopCoroutine(removeRoutine);
            instance.notificationEntries.Remove(this);
            instance.RefreshText();
        }
    }
}