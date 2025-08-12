using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaNotifications.Notifications
{
    internal class OnScreenNotifications : MonoBehaviour
    {
        public static OnScreenNotifications Instance { get; private set; }

        private List<string> notifications = new();
        
        private string currentNotification = "";
        
        private Coroutine notificationCoroutine;
        
        private GUIStyle labelStyle;
        
        private void Start()
        {
            labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.richText = true;
            labelStyle.fontSize = 40;
            labelStyle.normal.textColor = Color.white;
        }

        public void SendNotification(string notification, float duration)
        {
            notifications.Add(notification);
            UpdateNotification();
            StartCoroutine(DisplayNotification(duration, notification));
        }

        private IEnumerator DisplayNotification(float duration, string messageToRemove)
        {
            yield return new WaitForSeconds(duration);
            notifications.Remove(messageToRemove);
            UpdateNotification();
        }

        private void UpdateNotification()
        {
            foreach (string notificationMessage in notifications)
                currentNotification += notificationMessage + "\n";
            
            currentNotification = currentNotification.Substring(0, currentNotification.Length - 1);
        }

        private void OnGUI() => GUI.Label(new Rect(new Vector2(Screen.width/4f, Screen.height - 55 * (currentNotification.Split("\n").ToArray().Length + 1)), new Vector2(Screen.width/2f, 100f)), currentNotification, labelStyle);
        private void Awake() => Instance = this;
    }
}